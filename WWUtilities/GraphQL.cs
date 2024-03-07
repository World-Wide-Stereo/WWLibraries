using System.Collections.Generic;

namespace ww.Utilities
{
    public static class GraphQL
    {
        #region Errors
        public class Error
        {
            public string message { get; set; }
            public List<ErrorLocation> locations { get; set; }
            public List<string> path { get; set; }
            public ErrorExtensions extensions { get; set; }
        }

        public class ErrorExtensions
        {
            public string code { get; set; }
            public string typeName { get; set; }
            public string fieldName { get; set; }
        }

        public class ErrorLocation
        {
            public int? line { get; set; }
            public int? column { get; set; }
        }
        #endregion
    }
}
