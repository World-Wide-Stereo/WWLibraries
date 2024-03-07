using System;
using System.Data.Common;
using System.Threading;
using System.Windows.Forms;
using ww.Tables;
using ww.Utilities.Extensions;

/// <summary>
/// QueryThread is used in conjunction with <see cref="FilterListViewPanel"/> to allow interruption of a query.
/// It allows the user to change what's in any of the TextBoxes at teh top of the <see cref="FilterListViewPanel"/>,
/// causing the query to stop and rerun with the newly specified conditions.<para/>
/// Rows are returned in groups (of 50 by default) to maximize performance.<para/>
/// The default pause after a character is typed in any of the TextBoxes, but before the new query is run, is 0.5 seconds.
/// </summary>
public class QueryThread : IDisposable
{
    private FormattableString sql;
    private readonly int timeoutInSeconds;
    private readonly DatabaseConnection conn;
    private DbDataReader dr;
    private readonly Thread qt;
    private readonly ManualResetEventSlim interrupt = new ManualResetEventSlim(false);
    private volatile bool gettingDataReader;
    private volatile bool runThread = true;

    private FilterListViewPanel owner;

    public enum QueryState { NoQuery, QueryWait, QueryExec }
    public QueryState state { get; private set; }

    private delegate void UpdateListViewDelegate(bool clear);
    private UpdateListViewDelegate callback;

    private readonly int ReturnRows;
    private readonly int PauseValue;
    private const int waitValue = 1;

    public QueryThread(DatabaseConnection conn, FilterListViewPanel lv, int returnRows = 50, int pause = 500, int timeoutInSeconds = 30)
    {
        owner = lv;
        callback = new UpdateListViewDelegate(lv.UpdateListView);

        state = QueryState.NoQuery;
        this.conn = conn;
        sql = $"";
        this.timeoutInSeconds = timeoutInSeconds;

        PauseValue = pause;
        ReturnRows = returnRows;
        lv.ReturnRows = returnRows;

        qt = new Thread(RunQueryThread)
        {
            IsBackground = true,
            Name = "Query Thread",
        };
        qt.Start();
    }

    public FormattableString Query
    {
        get
        {
            return sql;
        }
        set
        {
            sql = value;
            interrupt.Set();
        }
    }

    public void Stop()
    {
        runThread = false;
        Query = $""; // Not only clears the query, but moves us past the interrupt.Wait() in RunQueryThread() so that we will check runThread and terminate.
        if (state == QueryState.QueryExec && !gettingDataReader)
        {
            // Wait while the query thread terminates.
            // This guarantees that nothing will be disposed by the calling functions, causing exceptions.
            // If the query thread isn't running, we're not at risk of accessing any disposed objects, so we can close as soon as possible.

            // No need to wait while getting the DataReader because we will terminate the query thread immediately after we finish getting it.
            // There is no way to stop the retrieval of the DataReader, so we must let it finish.
            // When we had been halting the main thread to wait for this, depending upon the complexity of the query, the user would see a noticeable unresponsive period when closing the form running the query thread.

            qt.Join();
        }
    }

    private void RunQueryThread()
    {
        do
        {
            // Wait until signaled to start running the thread.
            interrupt.Wait();
            // Initial State: No query submitted
            if (state == QueryState.NoQuery)
            {
                if (sql?.Format.Length != 0)
                {
                    // Query submitted, go to pause state
                    state = QueryState.QueryWait;
                }
                interrupt.Reset();
            }
            // QueryWait State: small delay before starting query
            if (state == QueryState.QueryWait)
            {
                if (interrupt.Wait(PauseValue))
                {
                    // Interrupted during our wait, back to the drawing board
                    state = QueryState.NoQuery;
                    continue;
                }
                // Not interrupted, time to fire off the query
                state = QueryState.QueryExec;
            }
            // QueryExec state: actually processing records
            if (state == QueryState.QueryExec)
            {
                bool clear = true;
                if (interrupt.Wait(waitValue))
                {
                    state = QueryState.NoQuery;
                    continue;
                }
                gettingDataReader = true;
                try
                {
                    dr = conn.GetDataReaderInterpolated(sql, timeoutInSeconds: timeoutInSeconds);
                }
                catch
                {
                    sql = $"";
                    state = QueryState.NoQuery;
                    gettingDataReader = false;
                    continue;
                }
                gettingDataReader = false;
                if (!runThread)
                {
                    return;
                }

                if (interrupt.Wait(waitValue))
                {
                    // Interrupted, cancel the processing, close the reader
                    DisposeDataReader();
                    state = QueryState.NoQuery;
                    if (sql?.Format.Length == 0)
                    {
                        // End the madness
                        return;
                    }
                    continue;
                }
                lock (owner.resultsLock)
                {
                    owner.results.Clear();
                }
                int count = 0;
                while (ReadData())
                {
                    if (!interrupt.Wait(waitValue))
                    {
                        lock (owner.resultsLock)
                        {
                            owner.results.Enqueue(GetListViewItem());
                        }
                        if (++count % ReturnRows == 0)
                        {
                            if (!owner.IsDisposed)
                            {
                                owner.BeginInvoke(callback, clear);
                            }
                            if (clear)
                            {
                                clear = false;
                            }
                        }

                        continue;
                    }
                    // Interrupted, cancel the processing, close the reader
                    DisposeDataReader();
                    state = QueryState.NoQuery;
                    if (sql?.Format.Length == 0)
                    {
                        // End the madness
                        return;
                    }
                }
                // If we weren't interrupted, close the reader
                lock (owner.resultsLock)
                {
                    owner.hasResults = owner.results.Count > 0;
                }
                DisposeDataReader();
                if (!owner.IsDisposed)
                {
                    owner.BeginInvoke(callback, clear);
                }
                state = QueryState.NoQuery;
            }
        } while (runThread);
    }

    private ListViewItem GetListViewItem()
    {
        var lvi = new ListViewItem(dr[0].ToString());
        for (int i = 1; i < dr.FieldCount; i++)
        {
            Type type = dr.GetFieldType(i);
            if (type == typeof(DateTime))
            {
                string value = dr[i].ToString().Trim();
                if (value.Length == 0)
                {
                    lvi.SubItems.Add("");
                }
                else
                {
                    DateTime date = DateTime.Parse(value);
                    lvi.SubItems.Add(date.TimeOfDay == default(TimeSpan) ? date.ToShortDateString() : date.ToShortDateTimeString());
                }
            }
            else
            {
                lvi.SubItems.Add(dr[i].ToString());
            }
        }
        return lvi;
    }

    private bool ReadData()
    {
        try
        {
            return dr.Read();
        }
        catch
        {
            return false;
        }
    }

    private void DisposeDataReader()
    {
        try
        {
            dr.Dispose();
        }
        catch
        {
            // This supposedly happens some of the time when the command behind the DataReader is not cancelled.
            // We do not have access to the Command object to cancel it, however.
            // https://stackoverflow.com/a/133398
        }
    }

    private bool _disposed;
    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();

            if (dr != null)
            {
                DisposeDataReader();
            }

            if (!conn.IsGlobal)
            {
                if (state == QueryState.QueryExec)
                {
                    // A running query will block connection disposal until it completes or fails.
                    new Thread(() => conn.Dispose())
                    {
                        IsBackground = true,
                        Name = "Query Thread - Connection Disposal",
                    }.Start();
                }
                else
                {
                    conn.Dispose();
                }
            }

            _disposed = true;
        }
    }
}
