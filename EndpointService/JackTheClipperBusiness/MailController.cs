using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using JackTheClipperCommon;
using JackTheClipperCommon.Configuration;
using JackTheClipperCommon.Interfaces;

namespace JackTheClipperBusiness
{
    /// <summary>
    /// Mails the users with new news if mail notification is wanted.
    /// </summary>
    internal class MailController
    {
        /// <summary>
        /// Queries the send of a mail asynchronous.
        /// </summary>
        /// <param name="toNotify">The notifiable object to send the mail to.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="content">The content.</param>
        /// <param name="pdfStream">The PDF stream (optional).</param>
        /// <param name="pdfName">Name of the PDF (optional).</param>
        public static void QuerySendMailAsync(IMailNotifiable toNotify, string subject, string content, MemoryStream pdfStream = null, string pdfName = null)
        {
            Task.Run(delegate
            {
                try
                {
                    using (new PerfTracer(nameof(QuerySendMailAsync) + toNotify.UserMailAddress))
                    {
                        var from = new MailAddress(AppConfiguration.MailConfigurationFrom,
                            "Jack The Clipper");
                        var to = new MailAddress(toNotify.UserMailAddress, toNotify.UserName);

                        using (var smtp = new SmtpClient
                        {
                            Host = AppConfiguration.MailConfigurationHost,
                            Port = AppConfiguration.MailConfigurationPort,
                            EnableSsl = AppConfiguration.MailConfigurationSSL,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            Credentials = new NetworkCredential(from.Address, AppConfiguration.MailConfigurationPassword),
                            Timeout = AppConfiguration.MailConfigurationTimeout
                        })
                        {
                            using (var message = new MailMessage(from, to) { Subject = subject, Body = content })
                            {
                                if (pdfStream != null)
                                {
                                    message.Attachments.Add(new Attachment(pdfStream, pdfName));
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