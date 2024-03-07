using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using MailKit.Net.Smtp;
using MimeKit;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    public static class Email
    {
        public static bool Enabled = true;
        public static bool TestMode = false;
        private const string LogFile = @"\\server\logs\email.log";
        private const int MinTimeoutMilliSec = 10000;
        private const int MaxTimeoutMilliSec = 15000;
        private const int MinSleepMilliSec = 10000;
        private const int MaxSleepMilliSec = 15000;
        private const int MaxNumTries = 5;

        private static Lazy<ReadOnlyDictionary<string, KeePass.Credential>> _keePassCreds = new Lazy<ReadOnlyDictionary<string, KeePass.Credential>>(() => KeePass.GetCredentials("Email").ToReadOnlyDictionary(x => x.Title.ReadString(), x => x));
        private static ReadOnlyDictionary<string, KeePass.Credential> KeePassCreds => _keePassCreds.Value;


        public static void sendEmail(
            string to,
            string subject,
            string body,
            IEnumerable<string> attachmentFiles = null,
            bool isHTML = false,
            string onBehalfOf = "",
            MailPriority priority = MailPriority.Normal,
            IEnumerable<string> bccAddresses = null,
            IEnumerable<string> copyAddresses = null,
            IEnumerable<string> sendToOnInvalidRecipient = null,
            SendAs sendAs = SendAs.NoReply,
            bool sendInThread = true)
        {
            if (!Enabled)
            {
                return;
            }

            if (TestMode)
            {
                subject = "[Test Mode] " + subject;
            }

            // Set attachments in the current thread so that attachment files can be
            // deleted immediately after the call to this function, if desired.
            var builder = new BodyBuilder();
            if (attachmentFiles != null)
            {
                try
                {
                    foreach (string file in attachmentFiles.Where(x => !x.IsNullOrBlank()))
                    {
                        builder.Attachments.Add(file);
                    }
                }
                catch (Exception ex)
                {
                    using (var log = new Logging.LogWriter(LogFile))
                    {
                        log.WriteLine($"Failed to add attachments. To: {to} Subject: {subject} Attachment File(s): {attachmentFiles.Join(", ")}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}");
                    }
                    Email.sendAlertEmail("Email with Attachments - Send Failure", $"To: {to}{Environment.NewLine}Subject: {subject}{Environment.NewLine}Attachment File(s): {attachmentFiles.Join(", ")}{Environment.NewLine}Exception: {ex}{Environment.NewLine}{Environment.NewLine}Body: {body}", isHTML: isHTML);
                    return;
                }
            }

            if (sendInThread)
            {
                // Send the email in a foreground thread. Using a background thread would
                // allow the program to potentially terminate without running the thread.
                new Thread(() =>
                {
                    sendEmail(to, subject, body, builder, isHTML, onBehalfOf, priority, bccAddresses, copyAddresses, sendToOnInvalidRecipient, sendAs);
                }).Start();
            }
            else
            {
                sendEmail(to, subject, body, builder, isHTML, onBehalfOf, priority, bccAddresses, copyAddresses, sendToOnInvalidRecipient, sendAs);
            }
        }
        private static void sendEmail(
            string to,
            string subject,
            string body,
            BodyBuilder builder,
            bool isHTML = false,
            string onBehalfOf = "",
            MailPriority priority = MailPriority.Normal,
            IEnumerable<string> bccAddresses = null,
            IEnumerable<string> copyAddresses = null,
            IEnumerable<string> sendToOnInvalidRecipient = null,
            SendAs sendAs = SendAs.NoReply)
        {
            try
            {
                KeePass.Credential sendAsCreds = KeePassCreds[sendAs.ToString()];

                List<string> toRecipients = to.Split(new[] { ';', ',' }).Select(x => x.Trim()).ToList();
                if (toRecipients.Any(x => !UtilityFunctions.IsValidEmail(x)))
                {
                    subject = $"[Invalid Email Address {to}] {subject}";
                    toRecipients = new List<string> { "example@example.com" };
                }

                var mail = new MimeMessage();
                switch (priority)
                {
                    case MailPriority.Normal:
                        mail.Priority = MessagePriority.Normal;
                        break;
                    case MailPriority.Low:
                        mail.Priority = MessagePriority.NonUrgent;
                        break;
                    case MailPriority.High:
                        mail.Priority = MessagePriority.Urgent;
                        break;
                }
                mail.From.Add(new MailboxAddress("Display Name", sendAsCreds.Username.ReadString()));
                mail.To.AddRange(toRecipients.Select(x => new MailboxAddress(x)));
                if (copyAddresses != null)
                {
                    mail.Cc.AddRange(copyAddresses.Where(x => !x.IsNullOrBlank()).Select(x => new MailboxAddress(x)));
                }
                if (bccAddresses != null)
                {
                    mail.Bcc.AddRange(bccAddresses.Where(x => !x.IsNullOrBlank()).Select(x => new MailboxAddress(x)));
                }
                if (!subject.IsNullOrBlank())
                {
                    mail.Subject = subject;
                }
                if (!onBehalfOf.IsNullOrBlank())
                {
                    mail.ReplyTo.Add(new MailboxAddress(onBehalfOf));
                }

                if (!body.IsNullOrBlank())
                {
                    if (isHTML) builder.HtmlBody = body;
                    else builder.TextBody = body;
                }
                mail.Body = builder.ToMessageBody();

                try
                {
                    using (var smtpClient = new MailKit.Net.Smtp.SmtpClient { Timeout = RandomThreadSafe.Next(MinTimeoutMilliSec, MaxTimeoutMilliSec) })
                    {
                        bool success = false;
                        bool logFailure = true;
                        int counter = 0;
                        do
                        {
                            try
                            {
                                if (smtpClient.IsConnected)
                                {
                                    try
                                    {
                                        smtpClient.Disconnect(true);
                                    }
                                    catch (Exception ex)
                                    {
                                        using (var log = new Logging.LogWriter(LogFile))
                                        {
                                            log.WriteLine($"Failed to disconnect cleanly. To: {mail.To} Subject: {mail.Subject} (attempt #{counter}){Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}");
                                        }
                                        smtpClient.Disconnect(false);
                                    }
                                }
                                smtpClient.Connect(sendAsCreds.URL.ReadString(), 587, MailKit.Security.SecureSocketOptions.StartTls);
                                smtpClient.Authenticate(sendAsCreds.Username.ReadString(), sendAsCreds.Password.ReadString());
                                smtpClient.Send(mail);
                                success = true;
                            }
                            catch (Exception ex)
                            {
                                var smtpCmdEx = ex as SmtpCommandException;
                                // If the email address is not real, such as anything@example.com...
                                if (smtpCmdEx != null && smtpCmdEx.ErrorCode == SmtpErrorCode.RecipientNotAccepted)
                                {
                                    if (sendToOnInvalidRecipient != null)
                                    {
                                        // Send to alternative email address(es)
                                        if (counter == 0)
                                        {
                                            mail.To.Clear();
                                            mail.To.AddRange(sendToOnInvalidRecipient.Where(x => !x.IsNullOrBlank()).Select(x => new MailboxAddress(x)));

                                            if (isHTML)
                                            {
                                                int indexOfBodyTag = builder.HtmlBody.IndexOf("<body>", StringComparison.OrdinalIgnoreCase);
                                                builder.HtmlBody = builder.HtmlBody.Insert(indexOfBodyTag == -1 ? 0 : indexOfBodyTag + 6, $"<div style=\"color: red; font-weight: bold\">This email was not sent to the intended recipient due to an invalid email address. {to}</div><br/><br/>");
                                            }
                                            else
                                            {
                                                builder.TextBody = builder.TextBody.Insert(0, $"THIS EMAIL WAS NOT SENT TO THE INTENDED RECIPIENT DUE TO AN INVALID EMAIL ADDRESS. {to}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}");
                                            }
                                            mail.Body = builder.ToMessageBody();

                                            logFailure = false;
                                            counter++;
                                        }
                                        else
                                        {
                                            counter = MaxNumTries;
                                        }
                                    }
                                    else
                                    {
                                        // Do not keep retrying as the email will never succeed.
                                        counter = MaxNumTries;
                                        if (mail.To.Count == 1)
                                        {
                                            // Do not log as all recipients are invalid.
                                            logFailure = false;
                                        }
                                    }
                                }
                                else
                                {
                                    Thread.Sleep(RandomThreadSafe.Next(MinSleepMilliSec, MaxSleepMilliSec));
                                    counter++;
                                }

                                if (logFailure)
                                {
                                    using (var log = new Logging.LogWriter(LogFile))
                                    {
                                        log.WriteLine($"Error sending email. To: {mail.To} Subject: {mail.Subject} (attempt #{counter}){Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}");
                                    }
                                }
                                else if (counter < MaxNumTries)
                                {
                                    logFailure = true;
                                }
                            }
                        } while (!success && counter < MaxNumTries);
                        if (!success && logFailure)
                        {
                            string msg = $"Aborting email. To: {mail.To} Subject: {mail.Subject}";
                            Logging.WriteLine(msg);
                            using (var log = new Logging.LogWriter(LogFile))
                            {
                                log.WriteLine(msg + Environment.NewLine + mail.Body + Environment.NewLine + Environment.NewLine);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    using (var log = new Logging.LogWriter(LogFile))
                    {
                        log.WriteLine($"Email sent, failed on closing connection. To: {mail.To} Subject: {mail.Subject}{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}");
                    }
                }
            }
            catch (Exception ex)
            {
                using (var log = new Logging.LogWriter(LogFile))
                {
                    log.WriteLine($"Unexpected exception.{Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}");
                }
            }
        }

        public static void sendAlertEmail(string subject, string body, MailPriority priority = MailPriority.Normal, bool isHTML = false, IEnumerable<string> attachmentFiles = null, IEnumerable<string> bccAddresses = null, IEnumerable<string> copyAddresses = null, bool sendInThread = true)
        {
            sendEmail("example@example.com", subject, body, attachmentFiles, priority: priority, isHTML: isHTML, bccAddresses: bccAddresses, copyAddresses: copyAddresses, sendInThread: sendInThread);
        }
        public static void sendAlertEmail(string additionalEmails, string subject, string body, MailPriority priority = MailPriority.Normal, bool isHTML = false, IEnumerable<string> attachmentFiles = null, IEnumerable<string> bccAddresses = null, IEnumerable<string> copyAddresses = null, bool sendInThread = true)
        {
            sendEmail($"example@example.com,{additionalEmails}", subject, body, attachmentFiles, priority: priority, isHTML: isHTML, bccAddresses: bccAddresses, copyAddresses: copyAddresses, sendInThread: sendInThread);
        }

        public struct EmailInfo
        {
            public string Subject;
            public string Body;
        }

        public enum SendAs
        {
            NoReply = 1,
            OnlineSales,
        }
    }
}
