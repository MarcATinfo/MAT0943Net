using MAT0943Net.TraspasFacturas.Models;
using System;
using ADODB;
using MAT0943Net.TraspasFacturas.Infrastructure.Connections;
using MAT0943Net.TraspasFacturas.Infrastructure.Logging;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Globalization;
using System.Runtime.InteropServices;

namespace MAT0943Net.TraspasFacturas.Services
{
    internal sealed class ServeiConfiguracioLogTraspasFacturas
    {
        private const string ClauLogActiu =
            "TraspasFacturas_LogActivo";

        private const string ClauRutaLog =
            "TraspasFacturas_LogRuta";

        public ConfiguracioLogTraspasFacturas Carregar(
            string baseDatosOrigen)
        {
            if (string.IsNullOrWhiteSpace(baseDatosOrigen))
            {
                throw new ArgumentException(
                    "La base de dades origen és buida.",
                    nameof(baseDatosOrigen));
            }

            ConfiguracioLogTraspasFacturas configuracio;

            if (IntentarCarregarViaActiveX(
                    baseDatosOrigen,
                    out configuracio))
            {
                return configuracio;
            }

            string cadenaConnexio =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioBaseDades(
                        baseDatosOrigen);

            configuracio =
                new ConfiguracioLogTraspasFacturas();

            const string sql =
                "SELECT " +
                "CLAVE, " +
                "VALOR " +
                "FROM dbo.AT_MAT0943NET_CONFIG " +
                "WHERE ACTIVO = 1 " +
                "AND CLAVE IN (?, ?)";

            using (OleDbConnection connexio =
                   new OleDbConnection(cadenaConnexio))
            {
                connexio.Open();

                using (OleDbCommand comanda =
                       new OleDbCommand(
                           sql,
                           connexio))
                {
                    comanda.Parameters.Add(
                            "CLAVE_1",
                            OleDbType.VarChar,
                            100)
                        .Value =
                            ClauLogActiu;

                    comanda.Parameters.Add(
                            "CLAVE_2",
                            OleDbType.VarChar,
                            100)
                        .Value =
                            ClauRutaLog;

                    using (OleDbDataReader lector =
                           comanda.ExecuteReader())
                    {
                        if (lector == null)
                        {
                            return configuracio;
                        }

                        while (lector.Read())
                        {
                            configuracio.ConfiguracioDisponible =
                                true;

                            string clau =
                                Convert.ToString(
                                    lector["CLAVE"])
                                ?.Trim()
                                ?? string.Empty;

                            string valor =
                                Convert.ToString(
                                    lector["VALOR"])
                                ?.Trim()
                                ?? string.Empty;

                            if (string.Equals(
                                    clau,
                                    ClauLogActiu,
                                    StringComparison.OrdinalIgnoreCase))
                            {
                                configuracio.LogActiu =
                                    InterpretarBoolea(
                                        valor,
                                        false);
                            }
                            else if (string.Equals(
                                         clau,
                                         ClauRutaLog,
                                         StringComparison.OrdinalIgnoreCase))
                            {
                                if (!string.IsNullOrWhiteSpace(valor))
                                {
                                    configuracio.RutaLog =
                                        valor;
                                }
                            }
                        }
                    }
                }
            }

            TraspasFacturasLogger.Informacio(
                "Configuració de log llegida via OleDb/.config.",
                CrearCampsLog(
                    baseDatosOrigen));

            return configuracio;
        }

        private static bool IntentarCarregarViaActiveX(
            string baseDatosOrigen,
            out ConfiguracioLogTraspasFacturas configuracio)
        {
            a3ERPActiveX.Enlace enlace = null;
            Command command = null;
            Recordset recordset = null;
            configuracio = null;

            try
            {
                enlace =
                    CrearEnlaceAutenticat();

                enlace.Iniciar(
                    ResoldreEmpresaActiveX(
                        enlace,
                        baseDatosOrigen),
                    string.Empty);

                if (enlace.Estado !=
                    a3ERPActiveX.EstadoEnlace.estACTIVO)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut iniciar l'empresa origen via a3ERPActiveX.");
                }

                Connection conexion =
                    enlace.GetConexionDB("EMPRESA");

                if (conexion == null)
                {
                    throw new InvalidOperationException(
                        "a3ERPActiveX no ha retornat la connexió de l'empresa origen.");
                }

                command =
                    new Command
                    {
                        ActiveConnection = conexion,
                        CommandText =
                            "SELECT CLAVE, VALOR " +
                            "FROM dbo.AT_MAT0943NET_CONFIG " +
                            "WHERE ACTIVO = 1 " +
                            "AND CLAVE IN (?, ?)",
                        CommandType = CommandTypeEnum.adCmdText,
                        CommandTimeout = 60
                    };

                AfegirParametreText(
                    command,
                    "CLAVE_1",
                    ClauLogActiu);

                AfegirParametreText(
                    command,
                    "CLAVE_2",
                    ClauRutaLog);

                object filesAfectades =
                    Type.Missing;

                recordset =
                    command.Execute(
                        out filesAfectades);

                configuracio =
                    new ConfiguracioLogTraspasFacturas();

                while (recordset != null &&
                       !recordset.EOF)
                {
                    AplicarValorConfiguracio(
                        configuracio,
                        LeerTexto(recordset, "CLAVE"),
                        LeerTexto(recordset, "VALOR"));

                    recordset.MoveNext();
                }

                TraspasFacturasLogger.Informacio(
                    "Configuració de log llegida via ActiveX/ADO.",
                    CrearCampsLog(
                        baseDatosOrigen));

                return true;
            }
            catch (Exception ex)
            {
                var camps =
                    CrearCampsLog(
                        baseDatosOrigen);

                camps.Add(
                    "Error",
                    ex.Message);

                TraspasFacturasLogger.Advertencia(
                    "No s'ha pogut llegir la configuració de log via ActiveX/ADO. S'activa el fallback OleDb/.config.",
                    camps);

                configuracio = null;
                return false;
            }
            finally
            {
                TancarRecordset(recordset);
                AlliberarComObject(recordset);
                AlliberarComObject(command);
                FinalitzarEnlace(enlace);
            }
        }

        private static a3ERPActiveX.Enlace CrearEnlaceAutenticat()
        {
            string usuari =
                ConfigurationManager.AppSettings[
                    "TraspasFacturas_ActiveXUsuario"];

            if (string.IsNullOrWhiteSpace(usuari))
            {
                throw new InvalidOperationException(
                    "No s'ha configurat l'usuari ActiveX.");
            }

            var enlace =
                new a3ERPActiveX.Enlace
                {
                    RaiseOnException = true
                };

            if (!enlace.LoginUsuario(
                    usuari,
                    ConfigurationManager.AppSettings[
                        "TraspasFacturas_ActiveXPassword"] ?? string.Empty))
            {
                throw new InvalidOperationException(
                    "No s'ha pogut iniciar sessió a a3ERPActiveX.");
            }

            return enlace;
        }

        private static string ResoldreEmpresaActiveX(
            a3ERPActiveX.Enlace enlace,
            string baseDatosOrigen)
        {
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

        private static void AfegirParametreText(
            Command command,
            string nom,
            string valor)
        {
            command.Parameters.Append(
                command.CreateParameter(
                    nom,
                    DataTypeEnum.adVarWChar,
                    ParameterDirectionEnum.adParamInput,
                    Math.Max(valor.Length, 1),
                    valor));
        }

        private static void AplicarValorConfiguracio(
            ConfiguracioLogTraspasFacturas configuracio,
            string clau,
            string valor)
        {
            configuracio.ConfiguracioDisponible =
                true;

            if (string.Equals(
                    clau,
                    ClauLogActiu,
                    StringComparison.OrdinalIgnoreCase))
            {
                configuracio.LogActiu =
                    InterpretarBoolea(
                        valor,
                        false);
            }
            else if (string.Equals(
                         clau,
                         ClauRutaLog,
                         StringComparison.OrdinalIgnoreCase) &&
                     !string.IsNullOrWhiteSpace(valor))
            {
                configuracio.RutaLog =
                    valor;
            }
        }

        private static Dictionary<string, object> CrearCampsLog(
            string baseDatosOrigen)
        {
            return new Dictionary<string, object>
            {
                { "BaseDadesOrigen", baseDatosOrigen ?? string.Empty }
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

        private static void FinalitzarEnlace(
            a3ERPActiveX.Enlace enlace)
        {
            if (enlace == null)
            {
                return;
            }

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
                // El tancament de l'Enlace no ha de bloquejar el fallback.
            }
            finally
            {
                AlliberarComObject(enlace);
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
            Recordset reader,
            string campo)
        {
            object valor =
                reader.Fields[campo].Value;

            return valor == null ||
                   valor == DBNull.Value
                ? string.Empty
                : Convert.ToString(valor)?.Trim()
                  ?? string.Empty;
        }

        private static bool InterpretarBoolea(
            string valor,
            bool valorPerDefecte)
        {
            string valorNormalitzat =
                (valor ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            switch (valorNormalitzat)
            {
                case "TRUE":
                case "T":
                case "1":
                case "SI":
                case "SÍ":
                case "S":
                    return true;

                case "FALSE":
                case "F":
                case "0":
                case "NO":
                case "N":
                    return false;

                default:
                    return valorPerDefecte;
            }
        }
    }
}
