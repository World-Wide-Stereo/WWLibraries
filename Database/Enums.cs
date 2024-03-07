using System;

namespace ww.Tables
{
    public class Database
    {
        public enum DataType
        {
            Unknown = 0,
            String,
            Integer,
            Decimal,
            Date,
            Boolean,
            Character,
            Long,
            Time,
        }

        public enum DatabaseType
        {
            Advantage = 1,
            SqlServer,
        }

        public enum AdvantageDatabase
        {
            Production = 0,
            Test,
        }

        public enum SqlServerDatabase
        {
            Production = 0,
            Test,
        }
    }
}
