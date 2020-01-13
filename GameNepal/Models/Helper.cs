using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.Configuration;

namespace GameNepal.Models
{
    public static class Helper
    {
        public static string EncodeToBase64(string password)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(password);
            byte[] inArray = HashAlgorithm.Create("SHA1").ComputeHash(bytes);
            return Convert.ToBase64String(inArray);
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

        public static void Email(string sendToEmailAddress, string messageBody)
        {
            var fromEmailAddress = ConfigurationManager.AppSettings.Get("FromEmail");
            var password = ConfigurationManager.AppSettings.Get("Password");
            var smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings.Get("SMTPPort"));
            var smtpHost = ConfigurationManager.AppSettings.Get("SMTPHost");
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
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
            catch (Exception) { }
        }
    }
}