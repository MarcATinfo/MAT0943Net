using A3ErpImportadorArticles.Models;
using ExcelDataReader;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace A3ErpImportadorArticles.Readers
{
    /// <summary>
    /// Llegeix fitxers d'articles en format Excel o CSV.
    ///
    /// Aquesta classe només s'encarrega d'obtenir una taula de dades.
    /// No interpreta encara els camps funcionals d'a3ERP.
    /// </summary>
    public sealed class LectorFitxerArticles
    {
        private static readonly string[] ExtensionsAdmeses =
        {
            ".xls",
            ".xlsx",
            ".csv"
        };

        /// <summary>
        /// Llegeix el fitxer indicat i retorna la primera taula
        /// que contingui columnes.
        /// </summary>
        public ResultatLecturaFitxer Llegir(string rutaFitxer)
        {
            if (string.IsNullOrWhiteSpace(rutaFitxer))
            {
                return ResultatLecturaFitxer.CrearError(
                    rutaFitxer,
                    "No s'ha indicat cap fitxer d'origen.");
            }

            if (!File.Exists(rutaFitxer))
            {
                return ResultatLecturaFitxer.CrearError(
                    rutaFitxer,
                    "El fitxer seleccionat ja no existeix o no és accessible.");
            }

            string extensio = Path
                .GetExtension(rutaFitxer)
                .ToLowerInvariant();

            if (!ExtensionsAdmeses.Contains(extensio))
            {
                return ResultatLecturaFitxer.CrearError(
                    rutaFitxer,
                    "El format del fitxer no és compatible. " +
                    "Només s'admeten fitxers XLS, XLSX i CSV.");
            }

            try
            {
                using (FileStream flux = File.Open(
                    rutaFitxer,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    using (IExcelDataReader lector =
                        CrearLector(flux, extensio))
                    {
                        DataSet conjuntDades = lector.AsDataSet(
                            CrearConfiguracioDataSet());

                        DataTable taulaOrigen =
                            ObtenirPrimeraTaulaAmbColumnes(conjuntDades);

                        if (taulaOrigen == null)
                        {
                            return ResultatLecturaFitxer.CrearError(
                                rutaFitxer,
                                "No s'ha trobat cap full o taula amb columnes.");
                        }

                        DataTable taulaNeta =
                            EliminarFilesCompletamentBuides(taulaOrigen);

                        if (taulaNeta.Rows.Count == 0)
                        {
                            return ResultatLecturaFitxer.CrearError(
                                rutaFitxer,
                                "El fitxer conté capçaleres, però no conté registres.");
                        }

                        return ResultatLecturaFitxer.CrearCorrecte(
                            rutaFitxer,
                            taulaOrigen.TableName,
                            taulaNeta);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                return ResultatLecturaFitxer.CrearError(
                    rutaFitxer,
                    "No hi ha permisos suficients per llegir el fitxer.");
            }
            catch (IOException ex)
            {
                return ResultatLecturaFitxer.CrearError(
                    rutaFitxer,
                    "No s'ha pogut obrir el fitxer. " +
                    "Pot estar bloquejat o en ús.\n\n" +
                    ex.Message);
            }
            catch (Exception ex)
            {
                return ResultatLecturaFitxer.CrearError(
                    rutaFitxer,
                    "S'ha produït un error durant la lectura del fitxer.\n\n" +
                    ex.Message);
            }
        }

        /// <summary>
        /// Crea el lector corresponent segons l'extensió.
        /// </summary>
        private static IExcelDataReader CrearLector(
            Stream flux,
            string extensio)
        {
            if (extensio == ".csv")
            {
                return ExcelReaderFactory.CreateCsvReader(
                    flux,
                    new ExcelReaderConfiguration
                    {
                        // Codificació habitual dels fitxers generats
                        // per aplicacions Windows a Espanya.
                        FallbackEncoding = Encoding.GetEncoding(1252),

                        // El separador es detecta automàticament
                        // entre els formats més habituals.
                        AutodetectSeparators = new[]
                        {
                            ';',
                            ',',
                            '\t',
                            '|'
                        },

                        QuoteChar = '"',
                        TrimWhiteSpace = true,
                        LeaveOpen = false
                    });
            }

            // Per als fitxers XLS i XLSX, la llibreria
            // detecta internament el format corresponent.
            return ExcelReaderFactory.CreateReader(
                flux,
                new ExcelReaderConfiguration
                {
                    FallbackEncoding = Encoding.GetEncoding(1252),
                    LeaveOpen = false
                });
        }

        /// <summary>
        /// Indica que la primera fila conté els noms de les columnes.
        /// No força un tipus únic per columna perquè un fitxer real
        /// pot combinar textos, números i cel·les buides.
        /// </summary>
        private static ExcelDataSetConfiguration CrearConfiguracioDataSet()
        {
            return new ExcelDataSetConfiguration
            {
                UseColumnDataType = false,

                ConfigureDataTable = delegate
                {
                    return new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true,
                        EmptyColumnNamePrefix = "COLUMNA_"
                    };
                }
            };
        }

        /// <summary>
        /// Retorna el primer full que contingui almenys una columna.
        /// En una fase futura es podrà permetre que l'usuari
        /// seleccioni manualment el full d'Excel.
        /// </summary>
        private static DataTable ObtenirPrimeraTaulaAmbColumnes(
            DataSet conjuntDades)
        {
            if (conjuntDades == null)
            {
                return null;
            }

            foreach (DataTable taula in conjuntDades.Tables)
            {
                if (taula.Columns.Count > 0)
                {
                    return taula;
                }
            }

            return null;
        }

        /// <summary>
        /// Elimina les files que no contenen cap valor.
        /// Manté intactes les files amb almenys una dada informada.
        /// </summary>
        private static DataTable EliminarFilesCompletamentBuides(
            DataTable taulaOrigen)
        {
            DataTable taulaNeta = taulaOrigen.Clone();
            taulaNeta.TableName = taulaOrigen.TableName;

            foreach (DataRow fila in taulaOrigen.Rows)
            {
                bool teAlgunValor = fila
                    .ItemArray
                    .Any(TeValor);

                if (teAlgunValor)
                {
                    taulaNeta.ImportRow(fila);
                }
            }

            return taulaNeta;
        }

        /// <summary>
        /// Determina si un valor de cel·la es considera informat.
        /// </summary>
        private static bool TeValor(object valor)
        {
            if (valor == null || valor == DBNull.Value)
            {
                return false;
            }

            string text = Convert.ToString(
                valor,
                CultureInfo.InvariantCulture);

            return !string.IsNullOrWhiteSpace(text);
        }
    }
}