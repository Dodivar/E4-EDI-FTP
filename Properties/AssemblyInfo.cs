using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Les informations générales relatives à un assembly dépendent de 
// l'ensemble d'attributs suivant. Changez les valeurs de ces attributs pour modifier les informations
// associées à un assembly.
[assembly: AssemblyTitle("Traitement_FTP_Fournisseur_Local-BTS-E4")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Schmidt Groupe")]
[assembly: AssemblyProduct("Traitement_FTP_Fournisseur")]
[assembly: AssemblyCopyright("Copyright SG 2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// L'affectation de la valeur false à ComVisible rend les types invisibles dans cet assembly 
// aux composants COM. Si vous devez accéder à un type dans cet assembly à partir de 
// COM, affectez la valeur true à l'attribut ComVisible sur ce type.
[assembly: ComVisible(false)]

// Le GUID suivant est pour l'ID de la typelib si ce projet est exposé à COM
[assembly: Guid("4e384004-1946-439d-9bbd-d210f3daafbe")]

// Les informations de version pour un assembly se composent des quatre valeurs suivantes :
//
//      Version principale
//      Version secondaire 
//      Numéro de build
//      Révision
//
// Vous pouvez spécifier toutes les valeurs ou indiquer les numéros de build et de révision par défaut 
// en utilisant '*', comme indiqué ci-dessous :
// [assembly: AssemblyVersion("1.0.*")]

// 1.0.0.0 DDi : Récupère un fichier Mxxx.txt sur un serveur FTP pour extraire ses données dans une table puis lance le programme AS400 et sauvegarde le fichier dans un répertoire sur le serveur. Si erreur de donnée/connexion, envoi de mail aux personnes en charges de la surveillance avec fichier en PJ puis mise en quarantaine et renommage du fichier dans le répertoire "Quarantaine/ERROR" du FTP. 
// 1.1.0.0 DDi 2018-05-23 : Mise en Production
// 1.1.1.0 DDi 2018-05-24 : Gestion d'un probleme sur envoi de mail dans SendLog, pour quitter le programme avec code retour -1 pour que Opcon détècte l'anomalie
// 1.1.1.1 DDi 2018-09-27 : Rajout d'un paramètre de configuration contenant le nom du fournisseur pour pouvoir le notifier dans les mails
// 1.1.2.1 DDi 2018-12-04 : Modif du FTP Manager pour récupérer la date/heure de modif des fichiers et ignorer les sousdossiers de travail et système
// 1.1.2.2 DDi 2018-01-15 : Correction de la chaîne de caractère transmise au log
// 
//TODO :: Rajout d'un identificateur du fournisseur à la fin de fichier EOF 
//TODO :: Gérer d'une meilleure facon le cas où une ligne est CRL donc à ignorée

// PENSE-BETE : Pour debug, commenter la section <runtime> en bas du fichier app.config du projet + Référence:IBM.Data.DB2.iSeries en 12.0
// REMARQUE : FileParser.cs : Si ASTYP == "RCL" & ASSSCC == "" on ignore la ligne
[assembly: AssemblyVersion("1.1.2.2")]
[assembly: AssemblyFileVersion("1.1.2.2")]
