using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;

namespace ww.Utilities
{
    public class DataRefreshTimer<TDataType> where TDataType : class
    {
        private readonly Func<TDataType> _refreshData;
        private readonly Action<TDataType> _refreshAction;
        private readonly Timer _dataTimer;
        private readonly Timer _uiTimer;
        private TDataType _data;

        public bool Enabled
        {
            get { return _dataTimer.Enabled; }
            set
            {
                _dataTimer.Enabled = value;
                if (_uiTimer != null) _uiTimer.Enabled = value;
            }
        }

        public DataRefreshTimer(Func<TDataType> refreshData, int interval)
        {
            _refreshData = refreshData;
            _dataTimer = new Timer(interval)
                        {
                            AutoReset = true,
                        };
            _dataTimer.Elapsed += RefreshData;
        }

        private event DataRefreshed DataRefreshedHandler;
        private delegate void DataRefreshed();

        public DataRefreshTimer(Func<TDataType> refreshData, double interval, ISynchronizeInvoke refreshTarget, Action<TDataType> refreshAction)
        {
            _refreshData = refreshData;
            _refreshAction = refreshAction;
            _dataTimer = new Timer(interval)
                             {
                                 AutoReset = true, 
                             };
            _dataTimer.Elapsed += RefreshData;

            _uiTimer = new Timer(interval)
                           {
                               AutoReset = true, 
                               SynchronizingObject = refreshTarget,
                           };
            _uiTimer.Elapsed += RefreshUI;
        }

        private void RefreshData(object sender, ElapsedEventArgs e)
        {
            var data = _refreshData.Invoke();
            lock (_dataTimer)
            {
                _data = data;
            }
        }
        private void RefreshUI(object sender, ElapsedEventArgs e)
        {
            lock (_dataTimer)
            {
                if (_data != null) _refreshAction(_data);
            }
        }

        public void TickNow()
        {
            var prior = this.Enabled;
            this.Enabled = false;
            lock (_dataTimer)
            {
                _data = _refreshData.Invoke();
                if (_data != null) _refreshAction(_data);
            }
            this.Enabled = prior;
        }
    }
}
