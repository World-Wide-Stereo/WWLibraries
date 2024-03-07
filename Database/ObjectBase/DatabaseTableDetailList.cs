using System;
using System.Collections.Generic;
using System.Linq;
using ww.Utilities.Extensions;

namespace ww.Tables
{
    public abstract class DatabaseTableDetailList<TParent, TDetail> : DatabaseTableDetail
    {
        protected TParent Parent { get; set; }

        protected DatabaseTableDetailList() { }
        protected DatabaseTableDetailList(TParent parent)
        {
            Parent = parent;
        }

        protected List<TDetail> _origDetails { get; private set; }
        protected List<TDetail> _details { get; set; }
        public List<TDetail> Details
        {
            get
            {
                if (_details == null)
                {
                    Load();
                    PostLoad();
                }
                return _details;
            }
            set
            {
                if (_details == null)
                {
                    Load();
                    PostLoad();
                }
                _details = value;
            }
        }

        public void PostLoad()
        {
            if (_details.Count > 0)
            {
                var integrityCheckDetail = _details[0] as DatabaseTable;
                if (integrityCheckDetail == null)
                {
                    throw new NotSupportedException("The detail class must be of type " + nameof(DatabaseTable) + ".");
                }
                if (!integrityCheckDetail.IsDatabaseTableDetail)
                {
                    throw new NotSupportedException("The detail class must have the " + nameof(DatabaseTable.IsDatabaseTableDetail) + " attribute.");
                }
            }

            if (_origDetails == null)
            {
                if (_details == null) throw new NotSupportedException("Load() must set _details.");
                _origDetails = _details.Select(x => (TDetail)((object)x).MemberwiseClone()).ToList();
            }
        }

        public override bool IsLoaded { get { return _details != null; } }

        public override void Update()
        {
            foreach (var detail in _details.Select(x => (DatabaseTable)(object)x))
            {
                detail.Update();
            }
            DeleteRemovedDetails();
        }

        public override void Delete()
        {
            foreach (var detail in _details.Select(x => (DatabaseTable)(object)x))
            {
                detail.Delete();
            }
            DeleteRemovedDetails();
        }

        private void DeleteRemovedDetails()
        {
            // Ensure that any items that were removed from _details are deleted.
            DatabaseTable.DeleteRemovedDatabaseTables(_details.Select(x => (DatabaseTable)(object)x), _origDetails.Select(x => (DatabaseTable)(object)x));
        }

        public override void UnlockWithoutUpdating()
        {
            // Unlocking _origDetails and not _details in order to unlock any items that may have been removed from _details.
            foreach (var detail in _origDetails.Select(x => (DatabaseTable)(object)x))
            {
                detail.UnlockWithoutUpdating();
            }
        }
    }
}
