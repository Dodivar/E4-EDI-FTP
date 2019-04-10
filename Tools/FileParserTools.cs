using System;
using System.Collections.Generic;
using System.Text;

namespace Traitement_FTP_Fournisseur
{
    public class FileParserTools
    {

        /// <summary>
        /// Retourne la requête pour une ligne de type "M"
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string GetMCommandLocal(string line)
        {
            string[] values = new string[20];

            // ASENR
            try
            {
                values[0] = line.Substring(0, 5).Trim();
            }
            catch
            {
                values[0] = "";
            }

            // ASIDE
            try
            {
                values[1] = line.Substring(5, 9).Trim();
            }
            catch
            {
                values[1] = "";
            }

            // ASETS
            try
            {
                values[2] = line.Substring(14, 6).Trim();
            }
            catch
            {
                values[2] = "";
            }

            // ASTYP
            try
            {
                values[3] = line.Substring(20, 4).Trim();
            }
            catch
            {
                values[3] = "";
            }

            // ASART
            try
            {
                values[4] = line.Substring(24, 14).Trim();
            }
            catch
            {
                values[4] = "";
            }

            // ASDEP
            try
            {
                values[5] = line.Substring(38, 6).Trim();
            }
            catch
            {
                values[5] = "";
            }

            // ASQT
            try
            {
                values[6] = line.Substring(44, 7).Trim();
                /*if (!IsNumeric(values[6]))
                {
                    values[6] = "0";
                }*/
            }
            catch
            {
                values[6] = "0";
            }

            // ASDAT
            try
            {
                values[7] = line.Substring(51, 8).Trim();
            }
            catch
            {
                values[7] = "";
            }

            // ASCLA
            try
            {
                values[8] = line.Substring(59, 4).Trim();
            }
            catch
            {
                values[8] = "";
            }

            // ASNUM
            try
            {
                values[9] = line.Substring(63, 9).Trim();
                if (values[9].Trim() == "")
                {
                    values[9] = "0";
                }
                else
                {
                    if (!IsNumeric(values[9]))
                    {
                        values[9] = "0";
                    }
                }
            }
            catch
            {
                values[9] = "0";
            }

            // ASSNU
            try
            {
                values[10] = line.Substring(72, 6).Trim();
                if (values[10].Trim() == "")
                {
                    values[10] = "0";
                }
                else
                {
                    if (!IsNumeric(values[10]))
                    {
                        values[10] = "0";
                    }
                }
            }
            catch
            {
                values[10] = "0";
            }

            // ASCB
            try
            {
                values[11] = line.Substring(78, 10).Trim();
            }
            catch
            {
                values[11] = "";
            }

            // ASSSCC
            try
            {
                values[12] = line.Substring(88, 18).Trim();
            }
            catch
            {
                values[12] = "";
            }

            // ASSMS
            try
            {
                values[13] = line.Substring(106, 4).Trim();
            }
            catch
            {
                values[13] = "";
            }

            // ASMOT
            try
            {
                values[14] = line.Substring(110, 4).Trim();
            }
            catch
            {
                values[14] = "";
            }

            // ASAG
            try
            {
                values[15] = line.Substring(114, 3).Trim();
            }
            catch
            {
                values[15] = "";
            }

            // ASCDE
            try
            {
                values[16] = line.Substring(117, 9).Trim();
            }
            catch
            {
                values[16] = "";
            }

            // ASLIG
            try
            {
                values[17] = line.Substring(126, 5).Trim();
            }
            catch
            {
                values[17] = "";
            }

            // ASNAV
            try
            {
                values[18] = line.Substring(131, 5).Trim();
            }
            catch
            {
                values[18] = "";
            }

            // ASBL
            try
            {
                values[19] = line.Substring(136, 9).Trim();
            }
            catch
            {
                values[19] = "";
            }


            // SI ASTYP == "RCL" & ASSSCC == "" on ignore la ligne
            if (values[3].ToUpper() == "RCL" && string.IsNullOrEmpty(values[12]))
            {
                return string.Empty;
            }
            else
            {
                return "insert into WASMVS (asenr, aside, asets, astyp, asart, asdep, asqt, asdat, ascla, asnum, assnu, ascb, assscc, assms, asmot, asag, ascde, aslig, asnav, asbl) values('" +
                    values[0] + "' , '" +
                    values[1] + "' , '" +
                    values[2] + "' , '" +
                    values[3] + "' , '" +
                    values[4] + "' , '" +
                    values[5] + "' , " +
                    values[6] + " , '" +
                    values[7] + "' , '" +
                    values[8] + "' , " +
                    values[9] + " , " +
                    values[10] + " , '" +
                    values[11] + "' , '" +
                    values[12] + "' , '" +
                    values[13] + "' , '" +
                    values[14] + "' , '" +
                    values[15] + "' , '" +
                    values[16] + "' , '" +
                    values[17] + "' , '" +
                    values[18] + "' , '" +
                    values[19] + "')";
            }
        }


        /// <summary>
        /// Retourne la requête pour une ligne de type "M"
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string GetMCommand(string line)
        {
            string[] values = new string[20];

            // ASENR
            try
            {
                values[0] = line.Substring(0, 5).Trim();
            }
            catch
            {
                values[0] = "";
            }

            // ASIDE
            try
            {
                values[1] = line.Substring(5, 9).Trim();
            }
            catch
            {
                values[1] = "";
            }

            // ASETS
            try
            {
                values[2] = line.Substring(14, 6).Trim();
            }
            catch
            {
                values[2] = "";
            }

            // ASTYP
            try
            {
                values[3] = line.Substring(20, 4).Trim();
            }
            catch
            {
                values[3] = "";
            }

            // ASART
            try
            {
                values[4] = line.Substring(24, 14).Trim();
            }
            catch
            {
                values[4] = "";
            }

            // ASDEP
            try
            {
                values[5] = line.Substring(38, 6).Trim();
            }
            catch
            {
                values[5] = "";
            }

            // ASQT
            try
            {
                values[6] = line.Substring(44, 7).Trim();
                /*if (!IsNumeric(values[6]))
                {
                    values[6] = "0";
                }*/
            }
            catch
            {
                values[6] = "0";
            }

            // ASDAT
            try
            {
                values[7] = line.Substring(51, 8).Trim();
            }
            catch
            {
                values[7] = "";
            }

            // ASCLA
            try
            {
                values[8] = line.Substring(59, 4).Trim();
            }
            catch
            {
                values[8] = "";
            }

            // ASNUM
            try
            {
                values[9] = line.Substring(63, 9).Trim();
                if (values[9].Trim() == "")
                {
                    values[9] = "0";
                }
                else
                {
                    if (!IsNumeric(values[9]))
                    {
                        values[9] = "0";
                    }
                }
            }
            catch
            {
                values[9] = "0";
            }

            // ASSNU
            try
            {
                values[10] = line.Substring(72, 6).Trim();
                if (values[10].Trim() == "")
                {
                    values[10] = "0";
                }
                else
                {
                    if (!IsNumeric(values[10]))
                    {
                        values[10] = "0";
                    }
                }
            }
            catch
            {
                values[10] = "0";
            }

            // ASCB
            try
            {
                values[11] = line.Substring(78, 10).Trim();
            }
            catch
            {
                values[11] = "";
            }

            // ASSSCC
            try
            {
                values[12] = line.Substring(88, 18).Trim();
            }
            catch
            {
                values[12] = "";
            }

            // ASSMS
            try
            {
                values[13] = line.Substring(106, 4).Trim();
            }
            catch
            {
                values[13] = "";
            }

            // ASMOT
            try
            {
                values[14] = line.Substring(110, 4).Trim();
            }
            catch
            {
                values[14] = "";
            }

            // ASAG
            try
            {
                values[15] = line.Substring(114, 3).Trim();
            }
            catch
            {
                values[15] = "";
            }

            // ASCDE
            try
            {
                values[16] = line.Substring(117, 9).Trim();
            }
            catch
            {
                values[16] = "";
            }

            // ASLIG
            try
            {
                values[17] = line.Substring(126, 5).Trim();
            }
            catch
            {
                values[17] = "";
            }

            // ASNAV
            try
            {
                values[18] = line.Substring(131, 5).Trim();
            }
            catch
            {
                values[18] = "";
            }

            // ASBL
            try
            {
                values[19] = line.Substring(136, 9).Trim();
            }
            catch
            {
                values[19] = "";
            }


            // SI ASTYP == "RCL" & ASSSCC == "" on ignore la ligne
            if (values[3].ToUpper() == "RCL" && string.IsNullOrEmpty(values[12]))
            {
                return string.Empty;
            }
            else
            {
                return "insert into erfilelib.wasmvs (asenr, aside, asets, astyp, asart, asdep, asqt, asdat, ascla, asnum, assnu, ascb, assscc, assms, asmot, asag, ascde, aslig, asnav, asbl) values('" +
                    values[0] + "' , '" +
                    values[1] + "' , '" +
                    values[2] + "' , '" +
                    values[3] + "' , '" +
                    values[4] + "' , '" +
                    values[5] + "' , " +
                    values[6] + " , '" +
                    values[7] + "' , '" +
                    values[8] + "' , " +
                    values[9] + " , " +
                    values[10] + " , '" +
                    values[11] + "' , '" +
                    values[12] + "' , '" +
                    values[13] + "' , '" +
                    values[14] + "' , '" +
                    values[15] + "' , '" +
                    values[16] + "' , '" +
                    values[17] + "' , '" +
                    values[18] + "' , '" +
                    values[19] + "')";
            }
        }

        /// <summary>
        /// Retourne la requête pour une ligne de type S
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string GetSCommand(string line)
        {
            //2018-05-23 PSo : File of type S no more used.
            string[] values = new string[7];
            try
            {
                values[0] = line.Substring(0, 6).Trim();
            }
            catch
            {
                values[0] = "";
            }

            try
            {
                values[1] = line.Substring(6, 14).Trim();
            }
            catch
            {
                values[1] = "";
            }

            try
            {
                values[2] = line.Substring(20, 6).Trim();
            }
            catch
            {
                values[2] = "";
            }

            try
            {
                values[3] = line.Substring(26, 7).Trim();

                if (!IsNumeric(values[3]))
                {
                    values[3] = "0";
                }
            }
            catch
            {
                values[3] = "0";
            }

            try
            {
                values[4] = line.Substring(33, 7).Trim();

                if (!IsNumeric(values[4]))
                {
                    values[4] = "0";
                }
            }
            catch
            {
                values[4] = "0";
            }

            try
            {
                values[5] = line.Substring(40, 7).Trim();

                if (!IsNumeric(values[5]))
                {
                    values[5] = "0";
                }
            }
            catch
            {
                values[5] = "0";
            }

            try
            {
                values[6] = line.Substring(47, 7).Trim();

                if (!IsNumeric(values[6]))
                {
                    values[6] = "0";
                }
            }
            catch
            {
                values[6] = "0";
            }

            return "insert into erfilelib.wasstj (asets,asart,asdep,asqts,asqtr,asqta,asqtb) values('" + 
                values[0] + "' , '" +
                values[1] + "' , '" +
                values[2] + "' , " +
                values[3] + " , " +
                values[4] + " , " +
                values[5] + " , " +
                values[6] + " )";
        }

        /// <summary>
        /// Vérifie qu'une valeur soit de type numérique
        /// </summary>
        /// <param name="value">Valeur à tester</param>
        /// <returns>Est-ce un numérique?</returns>
        public static bool IsNumeric(string value)
        {
            try
            {
                decimal.Parse(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
