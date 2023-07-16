using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace HttpServerDemo
{
    class EmailSender
    {
        public static void SendEmail(string recipient)
        {
            // Sender's Gmail account credentials
            string senderEmail = "emailsenderserverapp@gmail.com";
            string senderPassword = "xvpvtkrifidmuxqw";

            // Recipient email address
            string recipientEmail = recipient;

            // Create a new MailMessage object
            MailMessage mail = new MailMessage(senderEmail, recipientEmail)
            {
                Subject = "ServerApp!", // Email subject
                Body = "This is the email body." // Email body content
            };

            // Configure the SMTP client
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587, // Gmail SMTP port
                UseDefaultCredentials = false,
                EnableSsl = true,
                Credentials = new NetworkCredential(senderEmail, senderPassword)
            };

            try
            {
                // Send the email
                smtpClient.Send(mail);
                Console.WriteLine($"Email sent successfully From {mail.From} To {mail.To}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to send email. Error message: " + ex.Message);
            }
        }
    }
}
