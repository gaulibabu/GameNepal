using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Configuration;
using Microsoft.Ajax.Utilities;

namespace GameNepal.Models
{
    public static class Helper
    {
        private static readonly TimeZoneInfo NepalTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Nepal Standard Time");

        public static string EncodeToBase64(string password)
        {
            var bytes = Encoding.Unicode.GetBytes(password);
            var inArray = HashAlgorithm.Create("SHA1")?.ComputeHash(bytes);
            return Convert.ToBase64String(inArray ?? throw new InvalidOperationException());
        }

        public static string GetCurrentTransactionStatus(int status)
        {
            switch (status)
            {
                case (int)TransactionStatus.New: return "New";
                case (int)TransactionStatus.Processed: return "Processed";
                case (int)TransactionStatus.Cancelled: return "Cancelled";
                default: return "Error";
            }
        }

        public static DateTime GetCurrentDateTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, NepalTimeZone);
        }

        public static DateTime GetValidPasswordResetDateTime()
        {
            var expiryTime = Convert.ToInt32(ConfigurationManager.AppSettings.Get("PasswordResetLinkExpiryInMinutes"));
            return GetCurrentDateTime().AddMinutes(expiryTime);
        }

        public static string GetFileUploadPath()
        {
            return ConfigurationManager.AppSettings.Get("AdvertisementDirectory");
        }

        public static void Email(string sendToEmailAddress, string messageBody)
        {
            var fromEmailAddress = ConfigurationManager.AppSettings.Get("FromEmail");
            var password = ConfigurationManager.AppSettings.Get("Password");
            var smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings.Get("SMTPPort"));
            var smtpHost = ConfigurationManager.AppSettings.Get("SMTPHost");
            var message = new MailMessage();
            var smtp = new SmtpClient();
            try
            {
                message.From = new MailAddress("noreply@gamenepal.com");
                message.To.Add(new MailAddress(sendToEmailAddress));
                message.Subject = "Reset your password";
                message.IsBodyHtml = true; //to make message body as html  
                message.Body = messageBody;
                smtp.Port = smtpPort;
                smtp.Host = smtpHost;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(fromEmailAddress, password);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(message);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static void LogException(string source, string message, string type, string stackTrace, int? userId = null)
        {
            using (var context = new GameNepalEntities())
            {
                var errorLog = new ErrorLog()
                {
                    message = message,
                    source = source,
                    type = type,
                    createdate = GetCurrentDateTime(),
                    stackTrace = stackTrace,
                    userid = userId
                };
                context.ErrorLogs.Add(errorLog);
                context.Entry(errorLog).State = System.Data.Entity.EntityState.Added;
                context.SaveChanges();
            }
        }
    }
}