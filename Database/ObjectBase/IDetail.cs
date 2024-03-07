using System;
using ww.Utilities;

namespace ww.Tables
{
    [Obsolete("Replaced by DatabaseTableDetail.")]
    public interface IDetail
    {
        void loadDetails(bool lockRecords);
        void saveDetails();
        bool hasLoadedDetails();
    }
    [Obsolete("Replaced by DatabaseTableDetail.")]
    public interface IDetail<T> : IDetail
    {
        AppendOnlyList<T> details { get; }
    }
}
