using System;

namespace ww.Tables
{
    public abstract class DatabaseTableDetail
    {
        public abstract bool IsLoaded { get; }
        public abstract void Load();
        public abstract void Update();
        public abstract void Delete();
        public abstract void UnlockWithoutUpdating();
    }
}
