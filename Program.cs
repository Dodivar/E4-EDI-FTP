using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Traitement_FTP_Fournisseur.Tools;
using System.Data.Common;

namespace Traitement_FTP_Fournisseur
{
    class Program
    {
        static void Main(string[] args)
        {
            String LogMonitoringAppInfos = "Démarrage du programme [" + Log.AppName + " - Ver " + Log.AppVersion + "]";
            Log.MonitoringLogger.Info("Démarrage du programme [" + Log.AppName + " - Ver " + Log.AppVersion + "]");
            
            string localDirectory = Properties.Settings.Default.tempDir;

            string ftpTravail = Properties.Settings.Default.ftpTravail;
            string ftpSauve = Properties.Settings.Default.ftpSauve;
            string ftpError = Properties.Settings.Default.ftpError;
            string storedProcedure = Properties.Settings.Default.StoredProcedure;
            string movedFileName = "";
            string lineErrorMessage = "";
            int ingoreLineCRL = 0;            

            // Init SQL Connection
            SqlConnection con = new SqlConnection(Traitement_FTP_Fournisseur.Properties.Settings.Default.SQLexpress);            

            try
            {
                if (!Directory.Exists(localDirectory))
                {
                    Directory.CreateDirectory(localDirectory);
                }

                // Init access of FTP Server
                FTPManager fm = new FTPManager(Properties.Settings.Default.ftpHost, Properties.Settings.Default.ftpUser, Properties.Settings.Default.ftpPassword);
                fm.FTPErrorRaised += new EventHandler<FTPManager.FTPErrorRaisedEventArgs>(fm_FTPErrorRaised);

                Log.MonitoringLogger.Info("Tentative de récupération des fichiers sur le FTP.");

                List<string> files = fm.GetFilesList(ftpTravail);

                 if (files != null && files.Count > 0)
                {
                    Log.MonitoringLogger.Info("Fichiers à traiter dans le serveur FTP : " + files.Count.ToString());

                    foreach (string fileToProcess in files)
                    {
                        String fileName = fileToProcess.Split('|')[0];
                        double lastModifiedDateTimeOffset = 0;
                        double.TryParse(fileToProcess.Split('|')[1], out lastModifiedDateTimeOffset);
                        int saveStatut = -1;

                        //1.1.2.1 We check if the file is old enough to consider that it's fully uploaded on the FTP server
                        if (lastModifiedDateTimeOffset > Properties.Settings.Default.temporisation)
                        {
                            // Download file in local directory
                            if (fm.DownloadFile(fileName, ftpTravail, localDirectory))
                            {
                                string tmpFilePath = Path.Combine(localDirectory, fileName);
                                //StreamReader reader = new StreamReader(tmpFilePath);

                                Log.MonitoringLogger.Info("Fichier en cours de traitement : [" + fileName + "].");
            
                                try
                                {
                                    // File start with 'M' OR text file OR kilobyte 0
                                    if (
                                            !fileName.StartsWith("M", StringComparison.InvariantCultureIgnoreCase)
                                            || !fileName.EndsWith(".txt", StringComparison.InvariantCultureIgnoreCase)
                                            || new FileInfo(tmpFilePath).Length < 1
                                        )
                                    {
                                        // Fichier vide ou ne commence pas par 'M*' ou mauvaise extension
                                        lineErrorMessage = "Erreur de fichier : [" + fileName + "] (nommage (M*.txt) ou fichier vide).";
                                        throw (new Exception(lineErrorMessage));
                                    }

                                    Log.MonitoringLogger.Info("Tentative Connexion serveur SQLexpress (DS=" + con.DataSource + ";DB=" + con.Database + ").");                                    
                                    con.Open();
                                    SqlTransaction transaction = con.BeginTransaction("InsertTransaction");
                                    Log.MonitoringLogger.Info("Connexion serveur SQLexpress OK");

                                    string line = "";
                                    Int32 lineNumber = 0;

                                    // On commence à traiter le fichier courant
                                    using (StreamReader reader = new StreamReader(tmpFilePath))
                                    {
                                        while ((line = reader.ReadLine()) != null)
                                        {
                                            lineNumber++;
                                            if (!string.IsNullOrEmpty(line.Trim()))
                                            {
                                                line = line.Replace("\0", " ");
                                                string request = "";

                                                try
                                                {
                                                    // On traite la ligne courante
                                                    request = FileParserTools.GetMCommandLocal(line);

                                                    // wasmvs
                                                    if (!string.IsNullOrEmpty(request))
                                                    {
                                                        // Création de la commande SQL
                                                        using (var cmd = new SqlCommand(request, con)) {
                                                            cmd.Transaction = transaction;
                                                            cmd.ExecuteNonQuery();
                                                        }
                                                                                                                
                                                    }
                                                    else
                                                    {
                                                        // Si ASTYP == "RCL" & ASSSCC == "" on ignore la ligne
                                                        Log.MonitoringLogger.Info("Fichier [" + fileName + "] ligne [" + lineNumber.ToString() + "] a été ignorée, ASTYP == 'RCL' & ASSSCC == ''");
                                                        ingoreLineCRL++;
                                                    }
                                                }
                                                catch (Exception exLigne)
                                                {
                                                    lineErrorMessage = "Problème détecté sur la ligne [" + lineNumber.ToString() + "].";
                                                    lineErrorMessage += "\n\rContenu de la ligne : [" + line + "].";
                                                    lineErrorMessage += "\n\rErreur : [" + exLigne.Message + "].";
                                                    lineErrorMessage += "\n\rLe fichier [" + fileName + "] va être mis en quarantaine";



                                                    try {
                                                        transaction.Rollback();
                                                    } 
                                                    catch(Exception exRollback) {
                                                        lineErrorMessage += "\n\r(Le rollback de la transaction est tombé en échec !)";
                                                        Console.WriteLine("Le rollback de la transaction est tombé en échec : [" + exRollback.Message + "]");
                                                    }
 
                                                    throw new Exception(lineErrorMessage);
                                                }
                                            }
                                        }
                                    }

                                    // Attempt to commit the transaction.
                                    transaction.Commit();     
                                    Log.MonitoringLogger.Info("Fichier [" + fileName + "] traité entièrement (" + (lineNumber - ingoreLineCRL).ToString() + " lignes insérées).");

                                    // All lines inserted, we can call the stored procedure
                                    Log.MonitoringLogger.Info("Procédure stocké [" + storedProcedure + "] va être soumise.");
                                    //saveStatut = as400m.SaveM();                                    

                                    using (var sp = new SqlCommand(storedProcedure, con) { CommandType = CommandType.StoredProcedure })
                                    {
                                        saveStatut = sp.ExecuteNonQuery();
                                    }

                                    if (saveStatut != -1)
                                    {
                                        Log.MonitoringLogger.Info("Procédure stocké [" + storedProcedure + "] soumise avec succès.");

                                        // Tout est ok, on archive le fichier
                                        string ftpErrorMessage = "";

                                        Log.MonitoringLogger.Info("Le fichier [" + fileName + "] va être déplacé dans le répertoire de sauvegarde [" + ftpSauve + "/" + movedFileName + "]");
                                        if (fm.MoveFile(fileName, ftpTravail, ftpSauve, out movedFileName, out ftpErrorMessage))
                                        {
                                            Log.MonitoringLogger.Info("Fichier [" + fileName + "] déplacé dans le répertoire de sauvegarde [" + ftpSauve + "/" + movedFileName + "]");
                                        }
                                        else
                                        {
                                            Log.MonitoringLogger.Error(ftpErrorMessage);
                                            throw new Exception(ftpErrorMessage);
                                        }
                                    }
                                    else
                                    {
                                        //Problème AS400
                                        lineErrorMessage = "Problème dans la soumission de la procédure stocké [ " + storedProcedure + " ].";
                                        lineErrorMessage += "\n\rLe fichier [" + fileName + "] va être mis en quarantaine";
                                        throw new Exception(lineErrorMessage);
                                    }

                                }
                                catch (Exception exFichier)
                                {
                                    // On gère l'anomalie sur le fichier courant
                                    lineErrorMessage = "Exception : Erreur de traitement du fichier [" + fileName + "].";
                                    lineErrorMessage += "\n\r" + exFichier.Message;

                                    // On déplace le ficher en quarantaine.
                                    string ftpErrorMessage = "";

                                    Log.MonitoringLogger.Info("Le fichier [" + fileName + "] va être déplacé dans le répertoire de quarantaine [" + ftpError + "/" + movedFileName + "]");
                                    if (fm.MoveFile(fileName, ftpTravail, ftpError, out movedFileName, out ftpErrorMessage))
                                    {
                                        Log.MonitoringLogger.Info("Fichier [" + fileName + "] déplacé dans le répertoire de quarantaine [" + ftpError + "/" + movedFileName + "]");
                                    }
                                    else
                                    {
                                        lineErrorMessage += "\n\rLe fichier [" + fileName + "] n'a pas pu être mis en quarantaine.";
                                        lineErrorMessage += "\n\r" + ftpErrorMessage;
                                    }

                                    Log.MonitoringLogger.Error(lineErrorMessage);

                                    //On envoi le mail d'erreur
                                    string erreurEnvoiMail = "Envoi du mail d'erreur à [" + Properties.Settings.Default.mailSurveillance + "]";

                                    Log.MonitoringLogger.Info("Tentative " + erreurEnvoiMail + ".");

                                    try
                                    {
                                        LogsManager.SendMailAttachment(Properties.Settings.Default.mailSurveillance, lineErrorMessage, tmpFilePath);

                                        Log.MonitoringLogger.Info(erreurEnvoiMail + " OK.");
                                    }
                                    catch (Exception exEnvoiMail)
                                    {
                                        erreurEnvoiMail += "ECHEC.";
                                        erreurEnvoiMail += "\n\rErreur : " + exEnvoiMail.Message;
                                        if (exEnvoiMail.InnerException != null)
                                        {
                                            erreurEnvoiMail += "\n\rErreur interne : " + exEnvoiMail.InnerException.Message + ".";
                                        }

                                        Log.MonitoringLogger.Error(erreurEnvoiMail);

                                        //En cas de défaillance de l'envoi de mail, on redirige vers le 1er niveau de gestion d'erreur, qui va faire planter le programme (ce qui permettra à l'exploitation de détecter le crash).
                                        throw (new Exception(erreurEnvoiMail));
                                    }

                                }

                                finally
                                {
                                    // Fermeture de la connexion
                                    con.Close();
                                    
                                    // Suppression du fichier local, on en n'a plus besoin
                                    if (File.Exists(tmpFilePath))
                                        File.Delete(tmpFilePath);
                                }

                            }
                            else
                            {
                                //Probleme au download, notifié par mail dans FTPManager, le fichier restera donc à traiter à la prochaine execution
                            }
                        }
                        else
                        {
                            // 1.1.2.2 :: Rajout du '-' dans l'écart de la date de dernière modif.
                            DateTime fileLastModificationDateTime = DateTime.Now.AddSeconds(-lastModifiedDateTimeOffset);
                            String infoMessage =
                                "Fichier [" + fileName + "] ignoré car sa date/heure de dernière modification est [" + 
                                fileLastModificationDateTime.ToShortDateString() + " " + fileLastModificationDateTime.ToShortTimeString() 
                                + "] soit [" + lastModifiedDateTimeOffset.ToString() + "] secondes d'ancienneté contre [" 
                                + Properties.Settings.Default.temporisation.ToString() + "] secondes autorisées.";
                            Log.MonitoringLogger.Info(infoMessage);
                        }
                    }
                }


                // -- DDi: Système de purge local + FTP en attente (à MEP / externaliser sur le serveur)   --
                /*  
                Log.MonitoringLogger.Info("Purge du répertoire de sauvegarde local : [" + localDirectory + "] démarrée.");
                FileManager.purgeLocalDirectory(Properties.Settings.Default.PurgeNbDays);
                Log.MonitoringLogger.Info("Purge du répertoire de sauvegarde local : [" + localDirectory + "] OK.");
                
                Log.MonitoringLogger.Info("Purge du répertoire de sauvegarde FTP : [" + ftpSauve + "] démarrée.");
                FTPManager.purgeFTPSaveDirectory(Properties.Settings.Default.PurgeNbDays);
                Log.MonitoringLogger.Info("Purge du répertoire de sauvegarde FTP : [" + ftpSauve + "] OK.");
                */


            }
            catch (Exception ex)
            {
                string errorMessage = "Exception : " + ex.Message + "\n\rStack :" + ex.StackTrace;
                Console.WriteLine(errorMessage);
                LogsManager.SendLog(errorMessage);
            }

            Log.MonitoringLogger.Info("Fin du programme [" + Log.AppName + " - Ver " + Log.AppVersion + "]");
        }

        #region FTP & SQL ErrorRaised
        static void fm_FTPErrorRaised(object sender, FTPManager.FTPErrorRaisedEventArgs e)
        {
            if (e != null)
            {
                LogsManager.SendLog("Message : " + e.Message + "\n\rException : " + e.PlainException);
            }
        }
        #endregion
    }
}