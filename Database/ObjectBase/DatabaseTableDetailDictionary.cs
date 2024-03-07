using System;
using System.Collections.Generic;
using System.Linq;
using ww.Utilities.Extensions;

namespace ww.Tables
{
    public abstract class DatabaseTableDetailDictionary<TParent, TDetailKey, TDetailValue> : DatabaseTableDetail
    {
        protected TParent Parent { get; set; }

        protected DatabaseTableDetailDictionary() { }
        protected DatabaseTableDetailDictionary(TParent parent)
        {
            Parent = parent;
        }

        protected Dictionary<TDetailKey, TDetailValue> _origDetails { get; private set; }
        protected Dictionary<TDetailKey, TDetailValue> _details { get; set; }
        public Dictionary<TDetailKey, TDetailValue> Details
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
                var integrityCheckDetail = _details.First().Value as DatabaseTable;
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
                _origDetails = _details.Select(x => (KeyValuePair<TDetailKey, TDetailValue>)((object)x).MemberwiseClone()).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public override bool IsLoaded { get { return _details != null; } }

        public override void Update()
        {
            if (typeof(TDetailKey).IsSubclassOf(typeof(DatabaseTable)))
            {
                foreach (var detail in _details.Keys.Select(x => (DatabaseTable)(object)x))
                {
                    detail.Update();
                }
            }
            if (typeof(TDetailValue).IsSubclassOf(typeof(DatabaseTable)))
            {
                foreach (var detail in _details.Values.Select(x => (DatabaseTable)(object)x))
                {
                    detail.Update();
                }
            }
            DeleteRemovedDetails();
        }

        public override void Delete()
        {
            if (typeof(TDetailKey).IsSubclassOf(typeof(DatabaseTable)))
            {
                foreach (var detail in _details.Keys.Select(x => (DatabaseTable)(object)x))
                {
                    detail.Delete();
                }
            }
            if (typeof(TDetailValue).IsSubclassOf(typeof(DatabaseTable)))
            {
                foreach (var detail in _details.Values.Select(x => (DatabaseTable)(object)x))
                {
                    detail.Delete();
                }
            }
            DeleteRemovedDetails();
        }

        private void DeleteRemovedDetails()
        {
            // Ensure that any items that were removed from _details are deleted.
            if (typeof(TDetailKey).IsSubclassOf(typeof(DatabaseTable)))
            {
                DatabaseTable.DeleteRemovedDatabaseTables(_details.Keys.Select(x => (DatabaseTable)(object)x), _origDetails.Keys.Select(x => (DatabaseTable)(object)x));
            }
            if (typeof(TDetailValue).IsSubclassOf(typeof(DatabaseTable)))
            {
                DatabaseTable.DeleteRemovedDatabaseTables(_details.Values.Select(x => (DatabaseTable)(object)x), _origDetails.Values.Select(x => (DatabaseTable)(object)x));
            }
        }

        public override void UnlockWithoutUpdating()
        {
            // Unlocking _origDetails and not _details in order to unlock any items that may have been removed from _details.
            if (typeof(TDetailKey).IsSubclassOf(typeof(DatabaseTable)))
            {
                foreach (var detail in _origDetails.Keys.Select(x => (DatabaseTable)(object)x))
                {
                    detail.UnlockWithoutUpdating();
                }
            }
            if (typeof(TDetailValue).IsSubclassOf(typeof(DatabaseTable)))
            {
                foreach (var detail in _origDetails.Values.Select(x => (DatabaseTable)(object)x))
                {
                    detail.UnlockWithoutUpdating();
                }
            }
        }
    }
}
