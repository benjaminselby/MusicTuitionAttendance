using System.Configuration;
using System.Net.Mail;

namespace ApplicationCode
{
    public class MailHandler
    {
        private static SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings["mailServerName"]);

        public static void SendMail(string senderAddress, string[] emailRecipients, string subject, string messageBody)
        {
            MailMessage Mail = new MailMessage();
            foreach (string recipientAddress in emailRecipients)
            {
                // ===========================================================================
                // DEBUG MODE - CHANGE FOR PRODUCTION!!!
                Mail.To.Add("selby_b@woodcroft.sa.edu.au");
                // Mail.To.Add(CType(recipient, EmailRecipient).emailAddress)
                //  ===========================================================================
            }
            Mail.From = new MailAddress(senderAddress);
            Mail.Subject = subject;
            Mail.Body = messageBody;

            smtpClient.Send(Mail);
        }

        public static void SendMail(string senderAddress, string emailRecipient, string subject, string messageBody)
        {
            string[] emailRecipients = { emailRecipient };
            SendMail(senderAddress, emailRecipients, subject, messageBody);
        }
    }
}