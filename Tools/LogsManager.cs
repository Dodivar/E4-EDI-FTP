using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Net.Mime;
using System.Net.Mail;

namespace Traitement_FTP_Fournisseur
{
    public class LogsManager
    {
        public static void SendLog(Exception e)
        {
            if (e != null)
            {
                EventLog.WriteEntry("Problème traitement FTP fournisseurs", e.Message + "\r\n" + e.StackTrace + "\r\n" + e.Source);
                System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient(Properties.Settings.Default.mailHost);
                smtpClient.Send(Properties.Settings.Default.mailEnvoi, Properties.Settings.Default.mailSurveillance, "[" + Properties.Settings.Default.supplierName + "] Problème traitement BAL fournisseurs (" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ")", e.Message + "\r\n" + e.StackTrace + "\r\n" + e.Source);
            }
        }


        public static void SendLog(string message)
        {
            EventLog.WriteEntry("Problème traitement FTP fournisseurs", message);
            SmtpClient smtpClient = new SmtpClient(Properties.Settings.Default.mailHost);

            // 1.1.1.0
            try
            {
                smtpClient.Send(Properties.Settings.Default.mailEnvoi, Properties.Settings.Default.mailSurveillance, "[" + Properties.Settings.Default.supplierName + "] Problème traitement FTP fournisseurs (" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ")", message);
            }
            catch(Exception ex)
            {
                //Pour que l'exploit soit notifié d'un problème on met un code retour <> 0
                EventLog.WriteEntry("[" + Properties.Settings.Default.supplierName + "] Problème traitement FTP fournisseurs", "Envoi de mail impossible, arrêt du programme en code erreur -1. " + ex.Message);
                Environment.Exit(-1);
            }
        }

        
        public static void SendMailAttachment(string recipient, string body, string attachmentFilename)
        {
            SmtpClient smtpClient = new SmtpClient(Properties.Settings.Default.mailHost);
            System.Net.NetworkCredential basicCredential = new System.Net.NetworkCredential(Properties.Settings.Default.ftpUser, Properties.Settings.Default.ftpPassword);
            MailMessage message = new MailMessage();
            MailAddress fromAddress = new MailAddress(Properties.Settings.Default.mailEnvoi);

            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = basicCredential;
 
            message.From = fromAddress;
            //1.1.1.1 Rajout du nom du fournisseur dans l'objet du mail
            //message.Subject = "Problème traitement FTP fournisseurs (" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ")";
            message.Subject = "[" + Properties.Settings.Default.supplierName + "] Problème traitement FTP fournisseurs (" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ")";
            message.IsBodyHtml = false;
            message.Body = body;
            message.To.Add(recipient);

            if (attachmentFilename != null)
            {
                using (Attachment attachment = new Attachment(attachmentFilename, MediaTypeNames.Application.Octet))
                {
                    ContentDisposition disposition = attachment.ContentDisposition;
                    disposition.CreationDate = File.GetCreationTime(attachmentFilename);
                    disposition.ModificationDate = File.GetLastWriteTime(attachmentFilename);
                    disposition.ReadDate = File.GetLastAccessTime(attachmentFilename);
                    disposition.FileName = Path.GetFileName(attachmentFilename);
                    disposition.Size = new FileInfo(attachmentFilename).Length;
                    disposition.DispositionType = DispositionTypeNames.Attachment;
                    message.Attachments.Add(attachment);
                    smtpClient.Send(message);
                }
            }
            else
            {
                smtpClient.Send(message);
            }
        }
    }


}
