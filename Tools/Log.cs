using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using Salm.Fmc.LogService.Appenders;
using Salm.Fmc.LogService.Types;

[assembly: XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
namespace Traitement_FTP_Fournisseur
{
    #region Classe Log
    /// <summary>
    /// Classe dédiée au log de l'application
    /// </summary>
    internal static class Log
    {
        #region Constantes
        /// <summary>
        /// Logeur pour les informations
        /// </summary>
        private const string MONITORINGLOGGER = "MonitoringLogger";
        /// <summary>
        /// Logeur pour les exceptions
        /// </summary>
        private const string EXCEPTIONLOGGER = "ExceptionLogger";
        #endregion


        #region Membres et propriétés

        #region Log des informations ou des warnings
        /// <summary>
        /// Log des informations ou des warnings
        /// </summary>
        /// <value>Log des informations ou des warnings</value>
        public static ILog MonitoringLogger
        {
            get { return LogManager.GetLogger(MONITORINGLOGGER); }
        }
        #endregion

        #region Log des exceptions
        /// <summary>
        ///  Log des exceptions
        /// </summary>
        /// <value>Log des exceptions</value>
        public static ILog ExceptionLogger
        {
            get { return LogManager.GetLogger(EXCEPTIONLOGGER); }
        }
        #endregion

        #region Version de l'application
        /// <summary>
        /// Version de l'application
        /// </summary>
        /// <value>Version de l'application</value>
        public static Version AppVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }
        #endregion

        #region Nom de l'application
        /// <summary>
        /// Nom de l'application
        /// </summary>
        /// <value>Nom de l'application</value>
        public static string AppName
        {
            get { return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location); }
        }
        #endregion
        #endregion

        #region Méthodes publiques
        #region Log et envoi de mail lors d'une erreur
        /// <summary>
        /// Log et envoi de mail lors d'une erreur
        /// </summary>
        /// <param name="logType">Type de log</param>
        /// <param name="message">Message d'erreur</param>
        /// <param name="exception">Exception levée</param>
        public static void LogException(LogType logType, string message, Exception exception)
        {
            Log.LogException(logType, String.Empty, null, null, message, exception);
        }
        #endregion

        #region Log et envoi de mail lors d'une erreur
        /// <summary>
        /// Log et envoi de mail lors d'une erreur
        /// </summary>
        /// <param name="logType">Type de log</param>
        /// <param name="subsidiary">Code filiale</param>
        /// <param name="orderNumber">Numéro de commande</param>
        /// <param name="orderLine">Ligne de commande</param>
        /// <param name="message">Message d'erreur</param>
        /// <param name="exception">Exception levée</param>
        public static void LogException(LogType logType, string subsidiary, string orderNumber, string orderLine, string message, Exception exception)
        {
            try
            {
                // Redéfinition des paramètres
                string exceptionMessage = (exception == null) ? String.Empty : ((exception.InnerException == null) ? String.Empty : exception.InnerException.Message);
                string exceptionStackTrace = (exception == null) ? String.Empty : ((String.IsNullOrEmpty(exception.StackTrace)) ? ((exception.InnerException == null) ? String.Empty : exception.InnerException.StackTrace) : exception.StackTrace);
                string sSubsidiary = (String.IsNullOrEmpty(subsidiary)) ? String.Empty : subsidiary;
                int iOrderLine = (String.IsNullOrEmpty(orderLine)) ? 0 : Convert.ToInt32(orderLine);
            }
            catch (Exception ex)
            {
                // Log fichier
                Log.ExceptionLogger.Error("L'application n'a pas pu logguer le message d'erreur.", ex);
            }
        }
        #endregion
        #endregion
    }
    #endregion
}