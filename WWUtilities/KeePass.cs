using System.Collections.Generic;
using System.Linq;
using KeePassLib;
using KeePassLib.Keys;
using KeePassLib.Security;
using KeePassLib.Serialization;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    public static class KeePass
    {
        public struct Credential
        {
            public string Group;
            public ProtectedString Title;
            public ProtectedString Username;
            public ProtectedString Password;
            public ProtectedString URL;
        }

        public static Credential GetCredential(string group, string title)
        {
            return GetCredentials(group, title).FirstOrDefault();
        }
        public static IEnumerable<Credential> GetCredentials(string group = null)
        {
            return GetCredentials(group, null);
        }
        private static IEnumerable<Credential> GetCredentials(string group, string title)
        {
            // Using the network path rather than mapped drives so that our web apps can access the files.
            var connInfo = new IOConnectionInfo { Path = @"\\server\credentials\credentials.kdbx" };
            var compositeKey = new CompositeKey();
            compositeKey.AddUserKey(new KcpKeyFile(@"\\server\credentials\credentials.key"));

            IEnumerable<Credential> credentials;
            var keePassDB = new PwDatabase();
            try
            {
                keePassDB.Open(connInfo, compositeKey, null);
                IEnumerable<PwEntry> rawCredentials = keePassDB.RootGroup.GetEntries(true);
                if (!group.IsNullOrBlank())
                {
                    rawCredentials = rawCredentials.Where(x => x.ParentGroup.Name.EqualsIgnoreCase(group));
                }
                if (!title.IsNullOrBlank())
                {
                    rawCredentials = rawCredentials.Where(x => x.Strings.ReadSafe("Title").EqualsIgnoreCase(title));
                }
                credentials = rawCredentials.Select(x => new Credential
                {
                    Group = x.ParentGroup.Name,
                    Title = x.Strings.GetSafe("Title"),
                    Username = x.Strings.GetSafe("UserName"),
                    Password = x.Strings.GetSafe("Password"),
                    URL = x.Strings.GetSafe("URL"),
                });
                keePassDB.Close();
            }
            catch
            {
                keePassDB.Close();
                throw;
            }

            return credentials;
        }
    }
}
