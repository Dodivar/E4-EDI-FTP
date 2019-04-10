using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Traitement_FTP_Fournisseur.Tools
{
    class FileManager
    {
        /// <summary>
        /// Purge le dossier local
        /// </summary>
        /// <param name="byMonths">Nombre de mois</param>
        /// <returns>Etat de l'opération</returns>
        public static bool purgeLocalDirectory(Int32 PurgeNbDays)
        {
            bool status = false;
            string localDirectory = Path.GetDirectoryName(Environment.CommandLine.Replace("\"", ""));
            localDirectory = Path.Combine(localDirectory, "tmp");

            try
            {
                List<string> tmpFolder = new List<string>(Directory.GetFiles(localDirectory));

                foreach (string file in tmpFolder)
                {
                    if (File.GetCreationTime(file) < DateTime.Now.AddDays(-PurgeNbDays))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception e)
            {
                Log.MonitoringLogger.Error("Purge du répertoire de sauvegarde local NOK.");
                string lineErrorMessage = "Erreur lors de la purge du dossier local : " + localDirectory;
                lineErrorMessage += Environment.NewLine + "Exception : " + e.Message;
                throw new Exception(lineErrorMessage);
            }
            status = true;
            return status;
        }
    }
}
