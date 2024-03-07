using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using ww.Utilities.Extensions;

namespace ww.WebUtilities
{
    [DebuggerStepThrough]
    public static class WebUtilityFunctions
    {
        public static Assembly GetEntryAssembly()
        {
            if (HttpContext.Current == null || HttpContext.Current.ApplicationInstance == null)
            {
                return null;
            }

            Type type = HttpContext.Current.ApplicationInstance.GetType();
            while (type != null && type.Namespace == "ASP")
            {
                type = type.BaseType;
            }
            return type?.Assembly;
        }

        public static bool IsBasicAuthenticationValid(HttpRequestHeaders headers, IDictionary<string, string> validUsernamesAndPasswords)
        {
            headers.TryGetValues("Authorization", out IEnumerable<string> auths);
            string authHeader = auths?.FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string usernameAndPassword;
            try
            {
                usernameAndPassword = authHeader.Substring("Basic ".Length).DecodeBase64String();
            }
            catch (FormatException)
            {
                // This occurs when the string being decoded is not actually Base64.
                return false;
            }

            int separatorIndex = usernameAndPassword.IndexOf(':');
            if (separatorIndex == -1)
            {
                return false;
            }

            return validUsernamesAndPasswords.GetValueOrDefault(usernameAndPassword.Substring(0, separatorIndex)) == usernameAndPassword.Substring(separatorIndex + 1);
        }
    }
}
