using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    /* This class uses the Graph API to communicate directly with the Exchange Server instead of needing to go through Outlook
     * Eventually this should be able to replace libmail.cs and outlook.cs */

    public static class Graph
    {
        public static bool GraphEnabled = true;

        private static Lazy<KeePass.Credential?> _keePassCreds = new Lazy<KeePass.Credential?>(() => KeePass.GetCredential("Azure", "Email"));
        private static KeePass.Credential KeePassCreds => _keePassCreds.Value.Value;

        public static string ClientId => KeePassCreds.Username.ReadString();
        public static string ClientSecret => KeePassCreds.Password.ReadString();

        private static GraphServiceClient ConnectToExchange()
        {
            ClientSecretCredential tokenCredential = new ClientSecretCredential(
                "a72bcf16-ab6a-424e-8ada-fb382e356116",
                ClientId,
                ClientSecret,
                new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud });
            return new GraphServiceClient(tokenCredential, new[] { "https://graph.microsoft.com/.default" });
        }

        public static void CreateAppointment(IEnumerable<string> emailAddresses, string subject, string body, DateTime start, DateTime end, bool? isReminderOn = null)
        {
            if (!GraphEnabled)
                return;

            var graphClient = ConnectToExchange();
            foreach (string emailAddress in emailAddresses)
            {
                Task.Run(() => graphClient.Users[emailAddress].Calendar.Events.PostAsync(new Event
                {
                    Subject = subject,
                    Body = new ItemBody() { ContentType = BodyType.Text, Content = body },
                    Start = new DateTimeTimeZone() { DateTime = start.ToString("s"), TimeZone = "Eastern Standard Time" },
                    End = new DateTimeTimeZone() { DateTime = end.ToString("s"), TimeZone = "Eastern Standard Time" },
                    ShowAs = FreeBusyStatus.Free,
                    AllowNewTimeProposals = false,
                    IsReminderOn = isReminderOn
                })).Wait();
            }
        }

        public static void CreateAllDayAppointment(IEnumerable<string> emailAddresses, string subject, string body, DateTime start)
        {
            if (!GraphEnabled)
                return;

            var graphClient = ConnectToExchange();
            foreach (string emailAddress in emailAddresses)
            {
                Task.Run(() => graphClient.Users[emailAddress].Calendar.Events.PostAsync(new Event
                {
                    Start = new DateTimeTimeZone() { DateTime = start.Date.ToString("s"), TimeZone = "Eastern Standard Time" },
                    End = new DateTimeTimeZone() { DateTime = start.Date.AddDays(1).ToString("s"), TimeZone = "Eastern Standard Time" },
                    Subject = subject,
                    Body = new ItemBody() { ContentType = BodyType.Text, Content = body },
                    IsAllDay = true,
                    ShowAs = FreeBusyStatus.Free,
                    AllowNewTimeProposals = false,
                    IsReminderOn = true
                })).Wait();
            }
        }

        public static void DeleteWorkOrderAppointments(IEnumerable<string> emailAddresses, int workOrderNumber, int receiptNumber)
        {
            if (!GraphEnabled)
                return;

            var graphClient = ConnectToExchange();
            foreach (string emailAddress in emailAddresses)
            {
                try
                {
                    EventCollectionResponse matchingEvents = Task.Run(() => graphClient.Users[emailAddress].Calendar.Events.GetAsync(rc => rc.QueryParameters.Filter = $"contains(subject, '{workOrderNumber}') and contains(subject, '{receiptNumber}')")).Result;
                    foreach (Event eventToDelete in new List<Event>(matchingEvents.Value))
                    {
                        Task.Run(() => graphClient.Users[emailAddress].Events[eventToDelete.Id].DeleteAsync()).Wait();
                    }
                }
                catch (Exception ex)
                {
                    Email.sendAlertEmail("Error Deleting Appointment From Calendar", $"Unable to delete calendar appointment for email address {emailAddress} work order {workOrderNumber} on receipt {receiptNumber}\n\n{ex}");
                }
            }
        }

        public static void SendEmail(IEnumerable<string> to, string subject, string body, bool isHTML = false)
        {
            if (!GraphEnabled) { return; }

            var graphClient = ConnectToExchange();

            var email = new Message()
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = isHTML ? BodyType.Html : BodyType.Text,
                    Content = body
                },
                ToRecipients = to.Select(x => new Recipient() { EmailAddress = new EmailAddress() { Address = x } }).ToList()
            };

            using (PrincipalContext ctx = new PrincipalContext(ContextType.Domain, "domain.example.com"))
            {
                using (UserPrincipal queryFilter = new UserPrincipal(ctx))
                {
                    using (UserPrincipal user = new PrincipalSearcher(queryFilter).FindAll()
                        .Cast<UserPrincipal>()
                        .FirstOrDefault(x => x.SamAccountName.EqualsIgnoreCase(Environment.UserName)))
                    {
                        Task.Run(() => graphClient.Users[user == null ? $"{Environment.UserName}@example.com" : user.EmailAddress].SendMail.PostAsync(new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
                        {
                            Message = email,
                            SaveToSentItems = true
                        })).Wait();
                    }
                }
            }
        }
    }
}
