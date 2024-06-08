using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using arztin.Functions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

public class EmailService
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;

    public EmailService(string smtpServer = "smtp.mailersend.net", int smtpPort = 587, string smtpUsername = "MS_1vMCIA@arztin.site", string smtpPassword = "mH0T0WChwwFEua74")
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _smtpUsername = smtpUsername;
        _smtpPassword = smtpPassword;
    }

    public void SendEmail(string toEmail, string subject, string body, bool isBodyHtml = true)
    {
        try
        {
            var fromEmail = "MS_1vMCIA@arztin.site";

            // Create a new SmtpClient
            using (SmtpClient smtpClient = new SmtpClient(_smtpServer, _smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                smtpClient.EnableSsl = true; // Set to true if your SMTP server requires SSL

                // Create a MailMessage
                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isBodyHtml
                };

                // Add recipient
                mailMessage.To.Add(toEmail);

                // Send the email
                smtpClient.Send(mailMessage);
            }

            Console.WriteLine("Email sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error sending email: " + ex.Message);
        }
    }
}
