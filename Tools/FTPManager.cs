using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Traitement_FTP_Fournisseur
{
    public class FTPManager
    {

        #region SQLErrorRaisedEventArgs
        public class FTPErrorRaisedEventArgs : EventArgs
        {
            public string Message { get; private set; } // Message
            public Exception PlainException { get; private set; } // Exception
            public DateTime Date { get; private set; } // Date de création

            public FTPErrorRaisedEventArgs(string message, Exception plainException)
            {
                Message = message;
                PlainException = plainException;
                Date = DateTime.Now;
            }
        }

        public event EventHandler<FTPErrorRaisedEventArgs> FTPErrorRaised;
        #endregion

        #region Fields
        private string _server; // IP ou Nom du serveur
        private string _user; // Nom d'utilisateur
        private string _password; // Mot de passe
        private NetworkCredential _nc; // Credentials utilisés
        #endregion

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="server">Ip ou Nom du serveur</param>
        /// <param name="user">Nom d'utilisateur</param>
        /// <param name="password">Mot de passe</param>
        public FTPManager(string server, string user, string password)
        {
            this._server = server;
            this._user = user;
            this._password = password;
            this._nc = new NetworkCredential(user, password);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Retourne la liste des fichiers d'un dossier
        /// </summary>
        /// <param name="folder">Dossier à explorer</param>
        /// <returns>Liste des fichiers</returns>
        public List<string> GetFilesList(string folder)
        {
            List<string> files = new List<string>();
            String errorMessage = "";
            try
            {
                errorMessage = "Connexion au chemin FTP [" + "ftp://" + this._server + "/" + folder + "/] pour récupération de la liste des fichiers.";
                Log.MonitoringLogger.Error(errorMessage);

                FtpWebRequest fwr = (FtpWebRequest)FtpWebRequest.Create("ftp://" + this._server + "/" + folder + "/");
                fwr.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                fwr.UseBinary = true;
                fwr.Credentials = this._nc; //new NetworkCredential(this._user, this._password);

                FtpWebResponse response = (FtpWebResponse)fwr.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);

                errorMessage = "Récupération de la liste des fichiers/dossiers du FTP OK.";
                Log.MonitoringLogger.Error("Récupération de la liste des fichiers/dossiers du FTP OK.");

                string line = string.Empty;
                string currentFolder = folder + "/";

                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    //1.1.2.1 : We Exclude all folders
                    bool isFile = line.ToUpper().StartsWith("D") ? false : true;

                    if (isFile)
                    {
                        errorMessage = "Detail du fichier FTP : [" + line + "]";
                        Log.MonitoringLogger.Error(errorMessage);

                        //1.1.2.1 : We extract the information of the file using a regex
                        // Warning : be careful, if the pattern is not correct, the split will not work and we'll not find a valid information

                        string regex =
                             @"^" +                          //# Start of line
                             @"(?<dir>[\-ld])" +             //# File size          
                             @"(?<permission>[\-rwx]{9})" +  //# Whitespace          \n
                             @"\s+" +                        //# Whitespace          \n
                             @"(?<filecode>\d+)" +
                             @"\s+" +                        //# Whitespace          \n
                             @"(?<owner>[a-zA-Z0-9_+-]*)" +
                             @"\s+" +                        //# Whitespace          \n
                             @"(?<group>[a-zA-Z0-9_+-]*)" +
                             @"\s+" +                        //# Whitespace          \n
                             @"(?<size>\d+)" +
                             @"\s+" +                        //# Whitespace          \n
                             @"(?<month>\w{3})" +            //# Month (3 letters)   \n
                             @"\s+" +                        //# Whitespace          \n
                             @"(?<day>\d{1,2})" +            //# Day (1 or 2 digits) \n
                             @"\s+" +                        //# Whitespace          \n
                             @"(?<timeyear>[\d:]{4,5})" +    //# Time or year        \n
                             @"\s+" +                        //# Whitespace          \n
                             @"(?<filename>(.*))" +          //# Filename            \n
                             @"$";                           //# End of line

                        var split = new Regex(regex).Match(line);
                        string fileName = split.Groups["filename"].ToString();

                        //#if DEBUG
                        //Log.MonitoringLogger.Error("Dir du fichier trouvé : [" + split.Groups["dir"].ToString() + "]");
                        //Log.MonitoringLogger.Error("permission du fichier trouvé : [" + split.Groups["permission"].ToString() + "]");
                        //Log.MonitoringLogger.Error("filecode du fichier trouvé : [" + split.Groups["filecode"].ToString() + "]");
                        //Log.MonitoringLogger.Error("owner du fichier trouvé : [" + split.Groups["owner"].ToString() + "]");
                        //Log.MonitoringLogger.Error("group du fichier trouvé : [" + split.Groups["group"].ToString() + "]");
                        //Log.MonitoringLogger.Error("size du fichier trouvé : [" + split.Groups["size"].ToString() + "]");
                        //Log.MonitoringLogger.Error("month du fichier trouvé : [" + split.Groups["month"].ToString() + "]");
                        //Log.MonitoringLogger.Error("day du fichier trouvé : [" + split.Groups["day"].ToString() + "]");
                        //Log.MonitoringLogger.Error("timeyear du fichier trouvé : [" + split.Groups["timeyear"].ToString() + "]");
                        //Log.MonitoringLogger.Error("filename trouvé : [" + fileName + "]");
                        //#endif

                        errorMessage = "Nom de fichier trouvé par regexp : [" + fileName + "]";
                        Log.MonitoringLogger.Error(errorMessage);

                        //1.1.2.1 We search the offset in seconds between the modification date/time on the FTP Server and local date time
                        if (fileName != "")
                        {
                            errorMessage = "Récupération de la date/heure de modification du fichier [" + "ftp://" + this._server + "/" + folder + "/" + fileName + "]";
                            Log.MonitoringLogger.Error(errorMessage);
                            FtpWebRequest fwr_GetDateTimestamp = (FtpWebRequest)FtpWebRequest.Create("ftp://" + this._server + "/" + folder + "/" + fileName);
                            fwr_GetDateTimestamp.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                            fwr_GetDateTimestamp.UseBinary = true;
                            fwr_GetDateTimestamp.Credentials = this._nc; //new NetworkCredential(this._user, this._password);

                            using (FtpWebResponse resp = (FtpWebResponse)fwr_GetDateTimestamp.GetResponse())
                            {
                                files.Add(fileName + "|" + (DateTime.Now - resp.LastModified).TotalSeconds.ToString());
                            }
                        }
                        else
                        {
                            errorMessage = "Le nom de fichier n'a pas pu être trouvé à partir des details du FTP [" + line + "]. Arrêt";
                            Log.MonitoringLogger.Error(errorMessage);
                            throw new Exception(errorMessage);
                        }
                    }
                }

                Log.MonitoringLogger.Error("Récupération de la liste des fichiers du FTP et des dates de modifications OK.");

                if (files.Count == 0)
                {
                    Log.MonitoringLogger.Error("0 fichiers à traiter trouvés sur le serveur FTP.");
                }

                response.Close();
                reader.Close();
            }
            catch (Exception e)
            {
                Log.MonitoringLogger.Error("ERREUR dans l'étape : " + errorMessage);
                RaiseError("ERREUR dans l'étape : " + errorMessage, e);
                files = new List<string>();
            }

            return files;
        }

        /// <summary>
        /// Retourne la liste des fichiers d'un dossier qui doivent être purgés
        /// </summary>
        /// <param name="folder">Dossier à explorer</param>
        /// <param name="PurgeNbDays">Nb de jours de différence à laquelle on purge</param>
        /// <returns>Liste des fichiers</returns>
        public List<string> GetFilesListToPurge(string folder, Int32 PurgeNbDays)
        {
            List<string> files = new List<string>();

            try
            {
                string rootURL = "ftp://" + this._server + "/" + folder + "/";
                string fileURL = "";

                FtpWebRequest fwr = (FtpWebRequest)FtpWebRequest.Create(rootURL);
                fwr.Method = WebRequestMethods.Ftp.ListDirectory;
                fwr.UseBinary = true;
                fwr.Credentials = this._nc; //new NetworkCredential(this._user, this._password);

                FtpWebResponse response = (FtpWebResponse)fwr.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);

                string line = string.Empty;
                string currentFolder = folder + "/";

                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    if (!line.StartsWith("."))
                    {
                        fileURL = rootURL + line.Replace(currentFolder, "");

                        FtpWebRequest wr = (FtpWebRequest)WebRequest.Create(fileURL);
                        wr.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                        wr.UseBinary = true;
                        wr.Credentials = this._nc;

                        response = (FtpWebResponse)wr.GetResponse();
                        Console.WriteLine("{0} {1}", fileURL, response.LastModified);

                        if (response.LastModified < DateTime.Now.AddDays(-PurgeNbDays))
                        {
                            files.Add(line.Replace(currentFolder, ""));
                        }

                    }
                }

                reader.Close();
                response.Close();
            }
            catch (Exception e)
            {
                RaiseError("", e);
                files = new List<string>();
            }

            return files;
        }

        /// <summary>
        /// Télécharge un fichier
        /// </summary>
        /// <param name="fileName">Nom du fichier</param>
        /// <param name="folderFrom">Dossier source</param>
        /// <param name="folderTo">Dossier de destination</param>
        /// <returns>Etat de l'opération</returns>
        public bool DownloadFile(string fileName, string folderFrom, string folderTo)
        {
            bool status = false;

            try
            {
                string tempFilePath = Path.Combine(folderTo, fileName);
                string filePath = Path.Combine(folderFrom, fileName);

                FileStream outputStream = new FileStream(tempFilePath, FileMode.Create);
                FtpWebRequest fwr = (FtpWebRequest)FtpWebRequest.Create("ftp://" + this._server + "/" + filePath);
                fwr.Method = WebRequestMethods.Ftp.DownloadFile;
                fwr.UseBinary = true;
                fwr.UsePassive = true;
                fwr.Credentials = this._nc; //new NetworkCredential(this._user, this._password);

                FtpWebResponse response = (FtpWebResponse)fwr.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[2048];

                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }

                status = true;

                ftpStream.Close();
                outputStream.Close();
                response.Close();
            }
            catch (Exception e)
            {
                String receivedArgs = "Error downloading file from FTP server [" + this._server + "] with the following parameters :";

                receivedArgs += "\n\rfileName=[" + fileName + "].";
                receivedArgs += "\n\rfolderFrom=[" + folderFrom + "].";
                receivedArgs += "\n\rfolderTo=[" + folderTo + "].";

                RaiseError(receivedArgs, e);
                status = false;
            }

            return status;
        }

        /// <summary>
        /// Déplace un fichier
        /// </summary>
        /// <param name="fileName">Nom du fichier</param>
        /// <param name="folderFrom">Dossier source</param>
        /// <param name="folderTo">Dossier de destination</param>
        /// <returns>Etat de l'opération</returns>
        public bool MoveFile(string fileName, string folderFrom, string folderTo, out string movedFileName, out string errorMessage)
        {
            string localDirectory = Path.GetDirectoryName(Environment.CommandLine.Replace("\"", ""));
            localDirectory = Path.Combine(localDirectory, "tmp");
            errorMessage = "";

            bool status = false;
            movedFileName = DateTime.Now.ToString("yyyyMMdd-HHmmss_") + fileName;

            try
            {
                FtpWebRequest fwr = (FtpWebRequest)FtpWebRequest.Create("ftp://" + this._server + "/" + Path.Combine(folderFrom, fileName));
                fwr.Method = WebRequestMethods.Ftp.Rename;
                fwr.UseBinary = true;
                fwr.Credentials = this._nc;

                //For DEBGU only :
                //fwr.RenameTo = folderTo.Remove(0, folderFrom.Length + 1) + "123/" + movedFileName;
                fwr.RenameTo = folderTo.Remove(0, folderFrom.Length + 1) + "/" + movedFileName;

                FtpWebResponse response = (FtpWebResponse)fwr.GetResponse();
                Stream responseStream = response.GetResponseStream();

                status = true;
                response.Close();
            }
            catch (Exception e)
            {
                errorMessage = "Déplacement de fichier [" + fileName + "] sur le FTP [" + this._server + "] de [" + folderFrom + "] vers [" + folderTo + "/" + movedFileName + "] en erreur.";
                errorMessage += "\n\rErreur : " + e.Message;

                //RaiseError(errorMessage, e);
                status = false;
            }

            return status;
        }

        /// <summary>
        /// Supprime un fichier
        /// </summary>
        /// <param name="fileName">Nom du fichier</param>
        /// <returns>Etat de l'opération</returns>
        public bool DeleteFile(string fileName)
        {
            bool status = false;

            try
            {
                FtpWebRequest fwr = (FtpWebRequest)FtpWebRequest.Create("ftp://" + this._server + "/" + fileName);
                fwr.Method = WebRequestMethods.Ftp.DeleteFile;
                fwr.UseBinary = true;
                fwr.Credentials = this._nc; //new NetworkCredential(this._user, this._password);
                //fwr.KeepAlive = true;

                FtpWebResponse response = (FtpWebResponse)fwr.GetResponse();
                Stream responseStream = response.GetResponseStream();

                status = true;
                response.Close();
            }
            catch (Exception e)
            {
                RaiseError("", e);
                status = false;
            }

            return status;
        }

        /// <summary>
        /// Purge le dossier de sauvegarde sur le FTP
        /// </summary>
        /// <param name="byMonths">Nombre de mois</param>
        /// <returns>Etat de l'opération</returns>
        public static bool purgeFTPSaveDirectory(Int32 PurgeNbDays)
        {
            bool status = false;

            try
            {
                // Init access of FTP Server
                FTPManager fm = new FTPManager(Properties.Settings.Default.ftpHost, Properties.Settings.Default.ftpUser, Properties.Settings.Default.ftpPassword);
                List<string> files = fm.GetFilesListToPurge(Properties.Settings.Default.ftpSauve, PurgeNbDays);

                foreach (string file in files)
                {
                    try
                    {
                        fm.DeleteFile(Properties.Settings.Default.ftpSauve + "/" + file);
                    }
                    catch (Exception ex)
                    {
                        string lineErrorMessage = "Probleme de purge FTP pour le fichier : [" + Properties.Settings.Default.ftpSauve + "/" + file + "].\n\r";
                        Log.MonitoringLogger.Error(lineErrorMessage + ex.Message);
                        throw new Exception(lineErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.MonitoringLogger.Error("Purge du répertoire de sauvegarde FTP NOK.");
                string lineErrorMessage = "Erreur lors de la purge du dossier de sauvegarde sur le FTP: " + Properties.Settings.Default.ftpSauve;
                lineErrorMessage += Environment.NewLine + "Exception : " + ex.Message;
                throw new Exception(lineErrorMessage);
            }
            status = true;
            return status;
        }
        #endregion

        #region Private Methods
        private void RaiseError(string message, Exception plainException)
        {
            if (FTPErrorRaised != null)
            {
                FTPErrorRaised(this, new FTPErrorRaisedEventArgs(message, plainException));
            }
        }
        #endregion
    }
}
