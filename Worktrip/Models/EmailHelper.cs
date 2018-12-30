using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;

namespace Worktrip.Models
{
    public class EmailHelper
    {
        public static int SendEmail(string fromAddress, string fromName, string toAddress, string toName, string title, string message)
        {
            try
            {
                MailMessage mailMsg = new MailMessage();

                // To
                if (toName == null)
                {
                    mailMsg.To.Add(new MailAddress(toAddress));
                }
                else
                {
                    mailMsg.To.Add(new MailAddress(toAddress, toName));
                }

                // Subject and multipart/alternative Body
                mailMsg.Subject = title;
                mailMsg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message, null, MediaTypeNames.Text.Html));
                mailMsg.From = new MailAddress(fromAddress, fromName);

                //string html = @"<p>html body</p>";

                // Init SmtpClient and send
                SmtpClient smtpClient = new SmtpClient();
                smtpClient.EnableSsl = true;
                smtpClient.Port = 587;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Host = "smtp.gmail.com";
                smtpClient.Credentials = new System.Net.NetworkCredential("noreply@worktrip.tax", "Thanksgiving23");

                smtpClient.Send(mailMsg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }

            return 0;
        }
    }
}