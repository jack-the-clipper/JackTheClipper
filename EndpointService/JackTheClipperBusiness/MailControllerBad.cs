using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using JackTheClipperCommon;
using JackTheClipperCommon.Configuration;
using JackTheClipperCommon.SharedClasses;

namespace JackTheClipperBusiness
{
    /// <summary>
    /// Mails the Users with new News if the Mail is wanted.
    /// </summary>
    internal class MailControllerBAD
    {
        public static void QuerySendMailAsync(User user, string subject, string content, MemoryStream pdfStream = null)
        {
            Task.Run(delegate
            {
                try
                {
                    using (new PerfTracer(nameof(QuerySendMailAsync) + user.MailAddress))
                    {
                        var from = new MailAddress(AppConfiguration.MailConfigurationFrom,
                            "Jack The Clipper");
                        var to = new MailAddress(user.MailAddress, user.UserName);

                        using (var smtp = new SmtpClient
                        {
                            Host = AppConfiguration.MailConfigurationHost,
                            Port = AppConfiguration.MailConfigurationPort,
                            EnableSsl = AppConfiguration.MailConfigurationSSL,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            Credentials = new NetworkCredential(from.Address, AppConfiguration.MailConfigurationPassword),
                            Timeout = 10000
                        })
                        {
                            using (var message = new MailMessage(from, to) { Subject = subject, Body = content })
                            {
                                if (pdfStream != null)
                                {
                                    message.Attachments.Add(new Attachment(pdfStream, "Clipper.pdf"));
                                }

                                smtp.Send(message);
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    Console.WriteLine(error);
                }
                finally
                {
                    pdfStream?.Dispose();
                }
            });
        }
    }
}