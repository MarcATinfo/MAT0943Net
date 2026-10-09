using ADODB;
using MAT0943Net.TraspasFacturas.Models;
using MAT0943Net.TraspasFacturas.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Runtime.InteropServices;

namespace MAT0943Net.TraspasFacturas.Services
{
    internal static class ServeiVincleTraspasFacturesActiveX
    {
        private const string EstadoPendiente = "PENDIENTE";
        private const string EstadoCreada = "CREADA";
        private const string EstadoError = "ERROR";

        internal static bool IntentarPrepararTraspaso(
            string baseDatosOrigen,
            decimal idFacvOrigen,
            string empresaDestino,
            string baseDatosDestino)
        {
            return IntentarExecutar(
                baseDatosOrigen,
                nameof(IntentarPrepararTraspaso),
                conexion =>
                {
                    string estado = ObtenerEstado(
                        conexion,
                        baseDatosOrigen,
                        idFacvOrigen);

                    if (string.Equals(estado, EstadoCreada, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "La factura origen ja ha estat traspassada anteriorment i no es pot tornar a traspassar.");
                    }

                    if (string.Equals(estado, EstadoPendiente, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "La factura origen ja té un traspàs pendent.");
                    }

                    if (string.Equals(estado, EstadoError, StringComparison.OrdinalIgnoreCase))
                    {
                        EjecutarNoQuery(
                            conexion,
                            "UPDATE dbo.AT_TRASPAS_FACTURES SET EMPRESA_DESTI = ?, BD_DESTI = ?, IDFACV_DESTI = NULL, SERIE_DESTI = NULL, NUMDOC_DESTI = NULL, FECHA_TRASPAS = GETDATE(), ESTADO = ? WHERE BD_ORIGEN = ? AND IDFACV_ORIGEN = ?;",
                            empresaDestino.Trim(),
                            baseDatosDestino ?? string.Empty,
                            EstadoPendiente,
                            baseDatosOrigen.Trim(),
                            idFacvOrigen,
                            "No s'ha pogut reactivar el registre ERROR.");
                        return;
                    }

                    EjecutarNoQuery(
                        conexion,
                        "INSERT INTO dbo.AT_TRASPAS_FACTURES (BD_ORIGEN, IDFACV_ORIGEN, EMPRESA_DESTI, BD_DESTI, IDFACV_DESTI, SERIE_DESTI, NUMDOC_DESTI, FECHA_TRASPAS, ESTADO) VALUES (?, ?, ?, ?, NULL, NULL, NULL, GETDATE(), ?);",
                        baseDatosOrigen.Trim(),
                        idFacvOrigen,
                        empresaDestino.Trim(),
                        baseDatosDestino ?? string.Empty,
                        EstadoPendiente,
                        null);
                });
        }

        internal static bool IntentarEstaTraspassada(
            string baseDatosOrigen,
            decimal idFacvOrigen,
            out bool estaTraspassada)
        {
            bool valor = false;
            bool correcto = IntentarExecutar(
                baseDatosOrigen,
                nameof(IntentarEstaTraspassada),
                conexion => valor = string.Equals(
                    ObtenerEstado(conexion, baseDatosOrigen, idFacvOrigen),
                    EstadoCreada,
                    StringComparison.OrdinalIgnoreCase));
            estaTraspassada = valor;
            return correcto;
        }

        internal static bool IntentarGuardarIdFacvDestinoPendiente(
            string baseDatosOrigen,
            decimal idFacvOrigen,
            decimal idFacvDestino)
        {
            return IntentarExecutar(
                baseDatosOrigen,
                nameof(IntentarGuardarIdFacvDestinoPendiente),
                conexion => EjecutarNoQuery(
                    conexion,
                    "UPDATE dbo.AT_TRASPAS_FACTURES SET IDFACV_DESTI = ?, FECHA_TRASPAS = GETDATE() WHERE BD_ORIGEN = ? AND IDFACV_ORIGEN = ? AND ESTADO = ?;",
                    idFacvDestino,
                    baseDatosOrigen.Trim(),
                    idFacvOrigen,
                    EstadoPendiente,
                    "No s'ha pogut guardar l'ID de la factura destí al traspàs pendent."));
        }

        internal static bool IntentarObtenerPendiente(
            string baseDatosOrigen,
            decimal idFacvOrigen,
            out VincleTraspasPendienteDto pendiente)
        {
            VincleTraspasPendienteDto valor = null;
            bool correcto = IntentarExecutar(
                baseDatosOrigen,
                nameof(IntentarObtenerPendiente),
                conexion => valor = ObtenerPendiente(
                    conexion,
                    baseDatosOrigen,
                    idFacvOrigen));
            pendiente = valor;
            return correcto;
        }

        internal static bool IntentarReiniciarPendienteParaReintento(
            string baseDatosOrigen,
            decimal idFacvOrigen,
            string empresaDestino,
            string baseDatosDestino)
        {
            return IntentarExecutar(
                baseDatosOrigen,
                nameof(IntentarReiniciarPendienteParaReintento),
                conexion => EjecutarNoQuery(
                    conexion,
                    "UPDATE dbo.AT_TRASPAS_FACTURES SET EMPRESA_DESTI = ?, BD_DESTI = ?, IDFACV_DESTI = NULL, SERIE_DESTI = NULL, NUMDOC_DESTI = NULL, FECHA_TRASPAS = GETDATE() WHERE BD_ORIGEN = ? AND IDFACV_ORIGEN = ? AND ESTADO = ?;",
                    empresaDestino.Trim(),
                    baseDatosDestino ?? string.Empty,
                    baseDatosOrigen.Trim(),
                    idFacvOrigen,
                    EstadoPendiente,
                    "No s'ha pogut preparar el traspàs pendent per al reintent."));
        }

        internal static bool IntentarMarcarCreada(
            string baseDatosOrigen,
            decimal idFacvOrigen,
            string empresaDestino,
            string baseDatosDestino,
            decimal idFacvDestino,
            string serieDestino,
            decimal numDocDestino)
        {
            return IntentarExecutar(
                baseDatosOrigen,
                nameof(IntentarMarcarCreada),
                conexion => EjecutarNoQuery(
                    conexion,
                    "UPDATE dbo.AT_TRASPAS_FACTURES SET EMPRESA_DESTI = ?, BD_DESTI = ?, IDFACV_DESTI = ?, SERIE_DESTI = ?, NUMDOC_DESTI = ?, FECHA_TRASPAS = GETDATE(), ESTADO = ? WHERE BD_ORIGEN = ? AND IDFACV_ORIGEN = ?;",
                    empresaDestino.Trim(),
                    baseDatosDestino ?? string.Empty,
                    idFacvDestino,
                    serieDestino ?? string.Empty,
                    numDocDestino,
                    EstadoCreada,
                    baseDatosOrigen.Trim(),
                    idFacvOrigen,
                    "No s'ha pogut marcar el traspàs com a CREADA."));
        }

        internal static bool IntentarMarcarError(
            string baseDatosOrigen,
            decimal idFacvOrigen)
        {
            return IntentarExecutar(
                baseDatosOrigen,
                nameof(IntentarMarcarError),
                conexion => EjecutarNoQuery(
                    conexion,
                    "UPDATE dbo.AT_TRASPAS_FACTURES SET ESTADO = ?, FECHA_TRASPAS = GETDATE() WHERE BD_ORIGEN = ? AND IDFACV_ORIGEN = ?;",
                    EstadoError,
                    baseDatosOrigen.Trim(),
                    idFacvOrigen,
                    null));
        }

        internal static bool IntentarValidarDisponibleParaTraspaso(
            string baseDatosOrigen,
            decimal idFacvOrigen,
            string empresaDestino)
        {
            return IntentarExecutar(
                baseDatosOrigen,
                nameof(IntentarValidarDisponibleParaTraspaso),
                conexion =>
                {
                    string estado = ObtenerEstado(conexion, baseDatosOrigen, idFacvOrigen);
                    if (string.Equals(estado, EstadoCreada, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "La factura ja ha estat traspassada anteriorment.");
                    }

                    if (!string.Equals(estado, EstadoPendiente, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    VincleTraspasPendienteDto pendiente = ObtenerPendiente(
                        conexion,
                        baseDatosOrigen,
                        idFacvOrigen);
                    if (pendiente == null || string.IsNullOrWhiteSpace(pendiente.EmpresaDestino))
                    {
                        return;
                    }

                    string empresaPendiente = pendiente.EmpresaDestino.Trim();
                    string empresaSeleccionada = (empresaDestino ?? string.Empty).Trim();
                    if (!string.Equals(empresaPendiente, empresaSeleccionada, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "La factura té un traspàs pendent cap a l'empresa '" + empresaPendiente + "'." +
                            Environment.NewLine +
                            "Empresa seleccionada actualment: '" + empresaSeleccionada + "'." +
                            Environment.NewLine +
                            "Selecciona la mateixa empresa per recuperar-lo.");
                    }
                });
        }

        internal static void RegistrarConnexioOleDb(string metode, string baseDatosOrigen)
        {
            TraspasFacturasLogger.Informacio(
                "Connexió utilitzada=OleDb/.config.",
                CrearCampsLog(metode, baseDatosOrigen));
        }

        private static bool IntentarExecutar(
            string baseDatosOrigen,
            string metode,
            Action<Connection> accio)
        {
            a3ERPActiveX.Enlace enlace = null;
            try
            {
                enlace = CrearEnlaceAutenticat();
                string empresa = ResoldreEmpresaActiveX(enlace, baseDatosOrigen);
                enlace.Iniciar(empresa, string.Empty);

                if (enlace.Estado != a3ERPActiveX.EstadoEnlace.estACTIVO)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut iniciar l'empresa origen via a3ERPActiveX.");
                }

                Connection conexion = enlace.GetConexionDB("EMPRESA");
                if (conexion == null)
                {
                    throw new InvalidOperationException(
                        "a3ERPActiveX no ha retornat la connexió de l'empresa origen.");
                }

                accio(conexion);
                TraspasFacturasLogger.Informacio(
                    "Connexió utilitzada=ActiveX/ADO.",
                    CrearCampsLog(metode, baseDatosOrigen));
                return true;
            }
            catch (Exception ex)
            {
                var camps = CrearCampsLog(metode, baseDatosOrigen);
                camps.Add("Error", ex.Message);
                TraspasFacturasLogger.Advertencia(
                    "No s'ha pogut executar " + metode + " via ActiveX/ADO. S'activa el fallback OleDb/.config.",
                    camps);
                return false;
            }
            finally
            {
                FinalitzarEnlace(enlace);
            }
        }

        private static a3ERPActiveX.Enlace CrearEnlaceAutenticat()
        {
            string usuario = ConfigurationManager.AppSettings["TraspasFacturas_ActiveXUsuario"];
            if (string.IsNullOrWhiteSpace(usuario))
            {
                throw new InvalidOperationException("No s'ha configurat l'usuari ActiveX.");
            }

            var enlace = new a3ERPActiveX.Enlace { RaiseOnException = true };
            if (!enlace.LoginUsuario(
                    usuario,
                    ConfigurationManager.AppSettings["TraspasFacturas_ActiveXPassword"] ?? string.Empty))
            {
                throw new InvalidOperationException("No s'ha pogut iniciar sessió a a3ERPActiveX.");
            }

            return enlace;
        }

        private static string ResoldreEmpresaActiveX(
            a3ERPActiveX.Enlace enlace,
            string baseDatosOrigen)
        {
            Array empreses = enlace.Empresas() as Array;
            if (empreses == null || empreses.GetLowerBound(0) > 0 || empreses.GetUpperBound(0) < 0)
            {
                throw new InvalidOperationException("Empresas() no ha retornat el format esperat.");
            }

            int total;
            if (!int.TryParse(Convert.ToString(empreses.GetValue(0), CultureInfo.InvariantCulture), out total) || total < 0)
            {
                throw new InvalidOperationException("Empresas() no ha retornat un total d'empreses vàlid.");
            }

            for (int index = 1; index <= total && index <= empreses.GetUpperBound(0); index++)
            {
                string empresa = Convert.ToString(empreses.GetValue(index), CultureInfo.InvariantCulture)?.Trim();
                if (string.IsNullOrWhiteSpace(empresa))
                {
                    continue;
                }

                try
                {
                    if (string.Equals(ObtenirBaseDadesEmpresa(enlace, empresa), baseDatosOrigen.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return empresa;
                    }
                }
                catch
                {
                    // Una empresa invàlida no ha de bloquejar la resolució de l'origen.
                }
            }

            throw new InvalidOperationException(
                "No s'ha trobat cap empresa ActiveX per a la base de dades origen '" + baseDatosOrigen.Trim() + "'.");
        }

        private static string ObtenirBaseDadesEmpresa(a3ERPActiveX.Enlace enlace, string empresa)
        {
            Array valors = enlace.ParamConexion(empresa) as Array;
            if (valors == null)
            {
                throw new InvalidOperationException("ParamConexion no ha retornat el format esperat.");
            }

            int indexBaseDades = valors.GetLowerBound(0) + 5;
            if (indexBaseDades > valors.GetUpperBound(0))
            {
                throw new InvalidOperationException("ParamConexion no conté la base de dades de l'empresa.");
            }

            string baseDades = Convert.ToString(valors.GetValue(indexBaseDades), CultureInfo.InvariantCulture)?.Trim();
            if (string.IsNullOrWhiteSpace(baseDades))
            {
                throw new InvalidOperationException("ParamConexion ha retornat una base de dades buida.");
            }

            return baseDades;
        }

        private static string ObtenerEstado(Connection conexion, string baseDatosOrigen, decimal idFacvOrigen)
        {
            object valor = EjecutarEscalar(
                conexion,
                "SELECT ESTADO FROM dbo.AT_TRASPAS_FACTURES WHERE BD_ORIGEN = ? AND IDFACV_ORIGEN = ?;",
                baseDatosOrigen.Trim(),
                idFacvOrigen);
            return valor == null || valor == DBNull.Value ? string.Empty : Convert.ToString(valor)?.Trim() ?? string.Empty;
        }

        private static VincleTraspasPendienteDto ObtenerPendiente(Connection conexion, string baseDatosOrigen, decimal idFacvOrigen)
        {
            Command command = null;
            Recordset recordset = null;
            try
            {
                command = CrearComanda(
                    conexion,
                    "SELECT EMPRESA_DESTI, BD_DESTI, IDFACV_DESTI FROM dbo.AT_TRASPAS_FACTURES WHERE BD_ORIGEN = ? AND IDFACV_ORIGEN = ? AND ESTADO = ?;",
                    baseDatosOrigen.Trim(),
                    idFacvOrigen,
                    EstadoPendiente);
                object afectadas = Type.Missing;
                recordset = command.Execute(out afectadas);
                if (recordset == null || recordset.EOF)
                {
                    return null;
                }

                object idDestino = recordset.Fields["IDFACV_DESTI"].Value;
                return new VincleTraspasPendienteDto
                {
                    EmpresaDestino = LeerTexto(recordset, "EMPRESA_DESTI"),
                    BaseDatosDestino = LeerTexto(recordset, "BD_DESTI"),
                    IdFacvDestino = idDestino == null || idDestino == DBNull.Value ? (decimal?)null : Convert.ToDecimal(idDestino)
                };
            }
            finally
            {
                TancarRecordset(recordset);
                AlliberarComObject(recordset);
                AlliberarComObject(command);
            }
        }

        private static object EjecutarEscalar(Connection conexion, string sql, params object[] valors)
        {
            Command command = null;
            Recordset recordset = null;
            try
            {
                command = CrearComanda(conexion, sql, valors);
                object afectadas = Type.Missing;
                recordset = command.Execute(out afectadas);
                return recordset == null || recordset.EOF ? null : recordset.Fields[0].Value;
            }
            finally
            {
                TancarRecordset(recordset);
                AlliberarComObject(recordset);
                AlliberarComObject(command);
            }
        }

        private static void EjecutarNoQuery(
            Connection conexion,
            string sql,
            object valor1,
            object valor2,
            object valor3,
            object valor4,
            object valor5,
            object valor6,
            object valor7,
            object valor8,
            string errorSiNoUnaFila)
        {
            EjecutarNoQuery(conexion, sql, new[] { valor1, valor2, valor3, valor4, valor5, valor6, valor7, valor8 }, errorSiNoUnaFila);
        }

        private static void EjecutarNoQuery(Connection conexion, string sql, object valor1, object valor2, object valor3, object valor4, string errorSiNoUnaFila)
        {
            EjecutarNoQuery(conexion, sql, new[] { valor1, valor2, valor3, valor4 }, errorSiNoUnaFila);
        }

        private static void EjecutarNoQuery(Connection conexion, string sql, object valor1, object valor2, object valor3, string errorSiNoUnaFila)
        {
            EjecutarNoQuery(conexion, sql, new[] { valor1, valor2, valor3 }, errorSiNoUnaFila);
        }

        private static void EjecutarNoQuery(Connection conexion, string sql, object valor1, object valor2, object valor3, object valor4, object valor5, string errorSiNoUnaFila)
        {
            EjecutarNoQuery(conexion, sql, new[] { valor1, valor2, valor3, valor4, valor5 }, errorSiNoUnaFila);
        }

        private static void EjecutarNoQuery(Connection conexion, string sql, object[] valors, string errorSiNoUnaFila)
        {
            Command command = null;
            Recordset recordset = null;
            try
            {
                command = CrearComanda(conexion, sql, valors);
                object afectadas = Type.Missing;
                recordset = command.Execute(out afectadas);
                if (errorSiNoUnaFila != null && Convert.ToInt32(afectadas, CultureInfo.InvariantCulture) != 1)
                {
                    throw new InvalidOperationException(errorSiNoUnaFila);
                }
            }
            finally
            {
                TancarRecordset(recordset);
                AlliberarComObject(recordset);
                AlliberarComObject(command);
            }
        }

        private static Command CrearComanda(Connection conexion, string sql, params object[] valors)
        {
            var command = new Command
            {
                ActiveConnection = conexion,
                CommandText = sql,
                CommandType = CommandTypeEnum.adCmdText,
                CommandTimeout = 60
            };

            foreach (object valor in valors)
            {
                if (valor is decimal)
                {
                    Parameter parametre = command.CreateParameter(
                        string.Empty,
                        DataTypeEnum.adNumeric,
                        ParameterDirectionEnum.adParamInput,
                        18,
                        valor);
                    parametre.Precision = 18;
                    parametre.NumericScale = 0;
                    command.Parameters.Append(parametre);
                    continue;
                }

                string text = Convert.ToString(valor, CultureInfo.InvariantCulture) ?? string.Empty;
                command.Parameters.Append(command.CreateParameter(
                    string.Empty,
                    DataTypeEnum.adVarWChar,
                    ParameterDirectionEnum.adParamInput,
                    Math.Max(text.Length, 1),
                    text));
            }

            return command;
        }

        private static string LeerTexto(Recordset recordset, string campo)
        {
            object valor = recordset.Fields[campo].Value;
            return valor == null || valor == DBNull.Value ? string.Empty : Convert.ToString(valor)?.Trim() ?? string.Empty;
        }

        private static Dictionary<string, object> CrearCampsLog(string metode, string baseDatosOrigen)
        {
            return new Dictionary<string, object>
            {
                { "Mètode", metode },
                { "BaseDadesOrigen", baseDatosOrigen ?? string.Empty }
            };
        }

        private static void TancarRecordset(Recordset recordset)
        {
            try
            {
                if (recordset != null && recordset.State == (int)ObjectStateEnum.adStateOpen)
                {
                    recordset.Close();
                }
            }
            catch
            {
                // El tancament del Recordset no ha de bloquejar el fallback.
            }
        }

        private static void FinalitzarEnlace(a3ERPActiveX.Enlace enlace)
        {
            if (enlace == null)
            {
                return;
            }

            try
            {
                if (enlace.Estado == a3ERPActiveX.EstadoEnlace.estACTIVO)
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

        private static void AlliberarComObject(object objecteCom)
        {
            try
            {
                if (objecteCom != null && Marshal.IsComObject(objecteCom))
                {
                    Marshal.FinalReleaseComObject(objecteCom);
                }
            }
            catch
            {
                // L'alliberament COM no ha de bloquejar el fallback.
            }
        }
    }
}
