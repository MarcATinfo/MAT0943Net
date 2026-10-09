using MAT0943Net.TraspasFacturas.Infrastructure.Connections;
using MAT0943Net.TraspasFacturas.Models;
using ADODB;
using MAT0943Net.TraspasFacturas.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Runtime.InteropServices;

namespace MAT0943Net.TraspasFacturas.Services
{
    internal sealed class ConsultaLineasFacturaOrigenService
    {
        private const string ClauUsuariActiveX =
            "TraspasFacturas_ActiveXUsuario";

        private const string ClauPasswordActiveX =
            "TraspasFacturas_ActiveXPassword";

        public List<LineaFacturaOrigenDto> Consultar(
            string baseDatosOrigen,
            decimal idFacv)
        {
            const string sql = @"
                SELECT
                    LTRIM(RTRIM(COALESCE(CODART, ''))) AS CODART,
                    UNIDADES,
                    COALESCE(DESCLIN, '') AS DESCLIN,
                    PRCMONEDA,
                    DESC1,
                    DESC2,
                    DESC3,
                    DESC4,
                    LTRIM(RTRIM(COALESCE(TIPIVA, ''))) AS TIPIVA
                FROM dbo.LINEFACT
                WHERE IDFACV = ?;";

            List<LineaFacturaOrigenDto> resultado;

            if (IntentarConsultarViaActiveX(
                    baseDatosOrigen,
                    idFacv,
                    sql,
                    out resultado))
            {
                return resultado;
            }

            string conexion =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioBaseDades(
                        baseDatosOrigen);

            resultado =
                new List<LineaFacturaOrigenDto>();

            using (var connection =
                   new OleDbConnection(conexion))
            using (var command =
                   new OleDbCommand(sql, connection))
            {
                command.CommandType =
                    CommandType.Text;

                command.CommandTimeout =
                    60;

                command.Parameters
                    .Add(
                        "IDFACV",
                        OleDbType.Decimal)
                    .Value = idFacv;

                connection.Open();

                using (OleDbDataReader reader =
                       command.ExecuteReader())
                {
                    while (reader != null &&
                           reader.Read())
                    {
                        resultado.Add(
                            new LineaFacturaOrigenDto
                            {
                                CodArt =
                                    LeerTexto(
                                        reader,
                                        "CODART"),

                                Unidades =
                                    LeerDecimal(
                                        reader,
                                        "UNIDADES"),

                                DescLin =
                                    LeerTexto(
                                        reader,
                                        "DESCLIN"),

                                PrcMoneda =
                                    LeerDecimal(
                                        reader,
                                        "PRCMONEDA"),

                                Desc1 =
                                    LeerDecimal(
                                        reader,
                                        "DESC1"),

                                Desc2 =
                                    LeerDecimal(
                                        reader,
                                        "DESC2"),

                                Desc3 =
                                    LeerDecimal(
                                        reader,
                                        "DESC3"),

                                Desc4 =
                                    LeerDecimal(
                                        reader,
                                        "DESC4"),

                                TipIva =
                                    LeerTexto(
                                        reader,
                                        "TIPIVA")
                            });
                    }
                }
            }

            TraspasFacturasLogger.Informacio(
                "Connexió utilitzada=OleDb/.config.",
                new Dictionary<string, object>
                {
                    { "BaseDadesOrigen", baseDatosOrigen ?? string.Empty },
                    { "IDFACVOrigen", idFacv },
                    { "NumLinies", resultado.Count }
                });

            return resultado;
        }

        private static bool IntentarConsultarViaActiveX(
            string baseDatosOrigen,
            decimal idFacv,
            string sql,
            out List<LineaFacturaOrigenDto> resultado)
        {
            a3ERPActiveX.Enlace enlace = null;
            Command command = null;
            Recordset recordset = null;
            resultado = null;

            try
            {
                string usuari =
                    ConfigurationManager.AppSettings[
                        ClauUsuariActiveX];

                if (string.IsNullOrWhiteSpace(usuari))
                {
                    throw new InvalidOperationException(
                        "No s'ha configurat l'usuari ActiveX.");
                }

                enlace =
                    new a3ERPActiveX.Enlace();

                enlace.RaiseOnException = true;

                bool loginCorrecte =
                    enlace.LoginUsuario(
                        usuari,
                        ConfigurationManager.AppSettings[
                            ClauPasswordActiveX] ?? string.Empty);

                if (!loginCorrecte)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut iniciar sessió a a3ERP ActiveX.");
                }

                enlace.Iniciar(
                    ResoldreEmpresaActiveX(
                        enlace,
                        baseDatosOrigen),
                    string.Empty);

                if (enlace.Estado !=
                    a3ERPActiveX.EstadoEnlace.estACTIVO)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut iniciar l'empresa origen via a3ERP ActiveX.");
                }

                Connection connexio =
                    enlace.GetConexionDB("EMPRESA");

                if (connexio == null)
                {
                    throw new InvalidOperationException(
                        "a3ERP ActiveX no ha retornat la connexió de l'empresa origen.");
                }

                command =
                    new Command
                    {
                        ActiveConnection = connexio,
                        CommandText = sql,
                        CommandType = CommandTypeEnum.adCmdText,
                        CommandTimeout = 60
                    };

                Parameter parametre =
                    command.CreateParameter(
                        "IDFACV",
                        DataTypeEnum.adNumeric,
                        ParameterDirectionEnum.adParamInput,
                        18,
                        idFacv);

                parametre.Precision = 18;
                parametre.NumericScale = 0;
                command.Parameters.Append(parametre);

                object filesAfectades =
                    Type.Missing;

                recordset =
                    command.Execute(
                        out filesAfectades);

                resultado =
                    new List<LineaFacturaOrigenDto>();

                while (recordset != null &&
                       !recordset.EOF)
                {
                    resultado.Add(
                        MapearLinea(
                            recordset));

                    recordset.MoveNext();
                }

                TraspasFacturasLogger.Informacio(
                    "Connexió utilitzada=ActiveX/ADO.",
                    new Dictionary<string, object>
                    {
                        { "BaseDadesOrigen", baseDatosOrigen ?? string.Empty },
                        { "IDFACVOrigen", idFacv },
                        { "NumLinies", resultado.Count }
                    });

                return true;
            }
            catch (Exception ex)
            {
                TraspasFacturasLogger.Advertencia(
                    "No s'han pogut carregar les línies via ActiveX/ADO. S'activa el fallback OleDb/.config.",
                    new Dictionary<string, object>
                    {
                        { "BaseDadesOrigen", baseDatosOrigen ?? string.Empty },
                        { "IDFACVOrigen", idFacv },
                        { "Error", ex.Message }
                    });

                resultado = null;
                return false;
            }
            finally
            {
                TancarRecordset(recordset);
                AlliberarComObject(recordset);
                AlliberarComObject(command);

                if (enlace != null)
                {
                    try
                    {
                        if (enlace.Estado ==
                            a3ERPActiveX.EstadoEnlace.estACTIVO)
                        {
                            enlace.Acabar();
                        }
                    }
                    catch
                    {
                        // No bloquegem el fallback si no es pot tancar l'Enlace.
                    }

                    AlliberarComObject(enlace);
                }
            }
        }

        private static string ResoldreEmpresaActiveX(
            a3ERPActiveX.Enlace enlace,
            string baseDatosOrigen)
        {
            if (string.IsNullOrWhiteSpace(baseDatosOrigen))
            {
                throw new InvalidOperationException(
                    "No s'ha informat la base de dades origen.");
            }

            Array empreses =
                enlace.Empresas() as Array;

            if (empreses == null ||
                empreses.GetLowerBound(0) > 0 ||
                empreses.GetUpperBound(0) < 0)
            {
                throw new InvalidOperationException(
                    "Empresas() no ha retornat el format esperat.");
            }

            int total;

            if (!int.TryParse(
                    Convert.ToString(
                        empreses.GetValue(0),
                        CultureInfo.InvariantCulture),
                    out total) ||
                total < 0)
            {
                throw new InvalidOperationException(
                    "Empresas() no ha retornat un total d'empreses vàlid.");
            }

            int maxim =
                empreses.GetUpperBound(0);

            for (int index = 1;
                 index <= total && index <= maxim;
                 index++)
            {
                string empresa =
                    Convert.ToString(
                        empreses.GetValue(index),
                        CultureInfo.InvariantCulture)
                    ?.Trim();

                if (string.IsNullOrWhiteSpace(empresa))
                {
                    continue;
                }

                try
                {
                    if (string.Equals(
                            ObtenirBaseDadesEmpresa(
                                enlace,
                                empresa),
                            baseDatosOrigen.Trim(),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return empresa;
                    }
                }
                catch
                {
                    // Una empresa invalida no ha de bloquejar la resolucio de l'origen.
                }
            }

            throw new InvalidOperationException(
                "No s'ha trobat cap empresa ActiveX per a la base de dades origen '" +
                baseDatosOrigen.Trim() +
                "'.");
        }

        private static string ObtenirBaseDadesEmpresa(
            a3ERPActiveX.Enlace enlace,
            string empresa)
        {
            Array valors =
                enlace.ParamConexion(empresa) as Array;

            if (valors == null)
            {
                throw new InvalidOperationException(
                    "ParamConexion no ha retornat el format esperat.");
            }

            int indexBaseDades =
                valors.GetLowerBound(0) + 5;

            if (indexBaseDades > valors.GetUpperBound(0))
            {
                throw new InvalidOperationException(
                    "ParamConexion no conté la base de dades de l'empresa.");
            }

            string baseDades =
                Convert.ToString(
                    valors.GetValue(indexBaseDades),
                    CultureInfo.InvariantCulture)
                ?.Trim();

            if (string.IsNullOrWhiteSpace(baseDades))
            {
                throw new InvalidOperationException(
                    "ParamConexion ha retornat una base de dades buida.");
            }

            return baseDades;
        }

        private static LineaFacturaOrigenDto MapearLinea(
            Recordset reader)
        {
            return new LineaFacturaOrigenDto
            {
                CodArt = LeerTexto(reader, "CODART"),
                Unidades = LeerDecimal(reader, "UNIDADES"),
                DescLin = LeerTexto(reader, "DESCLIN"),
                PrcMoneda = LeerDecimal(reader, "PRCMONEDA"),
                Desc1 = LeerDecimal(reader, "DESC1"),
                Desc2 = LeerDecimal(reader, "DESC2"),
                Desc3 = LeerDecimal(reader, "DESC3"),
                Desc4 = LeerDecimal(reader, "DESC4"),
                TipIva = LeerTexto(reader, "TIPIVA")
            };
        }

        private static void TancarRecordset(
            Recordset recordset)
        {
            if (recordset == null)
            {
                return;
            }

            try
            {
                if (recordset.State ==
                    (int)ObjectStateEnum.adStateOpen)
                {
                    recordset.Close();
                }
            }
            catch
            {
                // El tancament del Recordset no ha de bloquejar el fallback.
            }
        }

        private static void AlliberarComObject(
            object objecteCom)
        {
            try
            {
                if (objecteCom != null &&
                    Marshal.IsComObject(objecteCom))
                {
                    Marshal.FinalReleaseComObject(objecteCom);
                }
            }
            catch
            {
                // L'alliberament COM no ha de bloquejar el fallback.
            }
        }

        private static string LeerTexto(
            OleDbDataReader reader,
            string campo)
        {
            object valor =
                reader[campo];

            return valor == DBNull.Value
                ? string.Empty
                : Convert.ToString(valor)?.Trim()
                  ?? string.Empty;
        }

        private static string LeerTexto(
            Recordset reader,
            string campo)
        {
            object valor =
                LeerValor(
                    reader,
                    campo);

            return valor == DBNull.Value ||
                   valor == null
                ? string.Empty
                : Convert.ToString(valor)?.Trim()
                  ?? string.Empty;
        }

        private static decimal LeerDecimal(
            OleDbDataReader reader,
            string campo)
        {
            object valor =
                reader[campo];

            return valor == DBNull.Value
                ? 0m
                : Convert.ToDecimal(valor);
        }

        private static decimal LeerDecimal(
            Recordset reader,
            string campo)
        {
            object valor =
                LeerValor(
                    reader,
                    campo);

            return valor == DBNull.Value ||
                   valor == null
                ? 0m
                : Convert.ToDecimal(valor);
        }

        private static object LeerValor(
            Recordset reader,
            string campo)
        {
            object valor =
                reader.Fields[campo].Value;

            return valor ??
                   DBNull.Value;
        }
    }
}
