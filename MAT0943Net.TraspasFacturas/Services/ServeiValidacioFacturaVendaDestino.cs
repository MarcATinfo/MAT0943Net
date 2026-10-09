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
    internal sealed class ServeiValidacioFacturaVendaDestino
    {
        private const string ClauUsuari =
            "TraspasFacturas_ActiveXUsuario";

        private const string ClauPassword =
            "TraspasFacturas_ActiveXPassword";

        public ResultadoValidacionTraspasoDto Validar(
            string baseDatosOrigen,
            string empresaDestino,
            IEnumerable<FacturaOrigenDto> facturas)
        {
            if (string.IsNullOrWhiteSpace(baseDatosOrigen))
            {
                throw new InvalidOperationException(
                    "No s'ha informat la base de dades origen.");
            }

            if (string.IsNullOrWhiteSpace(empresaDestino))
            {
                throw new InvalidOperationException(
                    "No s'ha informat l'empresa de destinació.");
            }

            ResultadoValidacionTraspasoDto resultadoActiveX;

            if (IntentarValidarViaActiveX(
                    baseDatosOrigen,
                    empresaDestino,
                    facturas,
                    out resultadoActiveX))
            {
                return resultadoActiveX;
            }

            var resultado =
                new ResultadoValidacionTraspasoDto
                {
                    EmpresaDestino =
                        empresaDestino.Trim()
                };

            resultado.BaseDatosDestino =
                ResolverBaseDatosDestino(
                    empresaDestino);

            string conexionOrigen =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioBaseDades(
                        baseDatosOrigen);

            string conexionDestino =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioBaseDades(
                        resultado.BaseDatosDestino);

            using (var origen =
                   new OleDbConnection(conexionOrigen))
            using (var destino =
                   new OleDbConnection(conexionDestino))
            {
                origen.Open();
                destino.Open();

                foreach (FacturaOrigenDto factura in facturas)
                {
                    if (factura == null ||
                        !factura.Seleccionada)
                    {
                        continue;
                    }

                    ValidarFactura(
                        origen,
                        destino,
                        factura,
                        resultado);
                }
            }

            TraspasFacturasLogger.Informacio(
                "Connexió utilitzada=OleDb/.config.",
                CrearCampsLog(
                    baseDatosOrigen,
                    empresaDestino,
                    resultado.BaseDatosDestino));

            return resultado;
        }

        private static bool IntentarValidarViaActiveX(
            string baseDatosOrigen,
            string empresaDestino,
            IEnumerable<FacturaOrigenDto> facturas,
            out ResultadoValidacionTraspasoDto resultado)
        {
            a3ERPActiveX.Enlace enlaceOrigen = null;
            a3ERPActiveX.Enlace enlaceDestino = null;
            resultado = null;
            string baseDatosDestino = string.Empty;
            var facturasOrigen =
                new List<FacturaLineasOrigen>();

            try
            {
                enlaceOrigen =
                    CrearEnlaceAutenticat();

                string empresaOrigen =
                    ResoldreEmpresaActiveX(
                        enlaceOrigen,
                        baseDatosOrigen);

                enlaceOrigen.Iniciar(
                    empresaOrigen,
                    string.Empty);

                Connection conexionOrigen =
                    ObtenirConnexioEmpresa(
                        enlaceOrigen,
                        "origen");

                foreach (FacturaOrigenDto factura in facturas)
                {
                    if (factura == null ||
                        !factura.Seleccionada)
                    {
                        continue;
                    }

                    facturasOrigen.Add(
                        new FacturaLineasOrigen
                        {
                            Factura = factura,
                            Lineas = LeerLineasOrigen(
                                conexionOrigen,
                                factura.IdFacv)
                        });
                }
            }
            catch (Exception ex)
            {
                var camps =
                    CrearCampsLog(
                        baseDatosOrigen,
                        empresaDestino,
                        baseDatosDestino);

                camps.Add(
                    "Error",
                    ex.Message);

                TraspasFacturasLogger.Advertencia(
                    "No s'ha pogut validar via ActiveX/ADO. S'activa el fallback OleDb/.config.",
                    camps);

                resultado = null;
                return false;
            }
            finally
            {
                FinalitzarEnlace(enlaceOrigen);
                enlaceOrigen = null;
            }

            try
            {
                enlaceDestino =
                    CrearEnlaceDestinoAutenticat(
                        baseDatosOrigen,
                        empresaDestino,
                        baseDatosDestino);

                TraspasFacturasLogger.Informacio(
                    "ActiveX destí: abans d'Iniciar.",
                    CrearCampsLog(
                        baseDatosOrigen,
                        empresaDestino,
                        baseDatosDestino));

                enlaceDestino.Iniciar(
                    empresaDestino.Trim(),
                    string.Empty);

                TraspasFacturasLogger.Informacio(
                    "ActiveX destí: Iniciar completat.",
                    CrearCampsLog(
                        baseDatosOrigen,
                        empresaDestino,
                        baseDatosDestino));

                TraspasFacturasLogger.Informacio(
                    "ActiveX destí: abans de GetConexionDB(\"EMPRESA\").",
                    CrearCampsLog(
                        baseDatosOrigen,
                        empresaDestino,
                        baseDatosDestino));

                Connection conexionDestino =
                    ObtenirConnexioEmpresa(
                        enlaceDestino,
                        "destinació");

                TraspasFacturasLogger.Informacio(
                    "ActiveX destí: GetConexionDB(\"EMPRESA\") completat.",
                    CrearCampsLog(
                        baseDatosOrigen,
                        empresaDestino,
                        baseDatosDestino));

                baseDatosDestino =
                    ExtraerBaseDatos(
                        enlaceDestino.ParamConexion(
                            empresaDestino.Trim()),
                        empresaDestino);

                resultado =
                    new ResultadoValidacionTraspasoDto
                {
                        EmpresaDestino =
                            empresaDestino.Trim(),

                        BaseDatosDestino =
                            baseDatosDestino
                    };

                foreach (FacturaLineasOrigen facturaOrigen in facturasOrigen)
                {
                    ValidarFacturaDestino(
                        conexionDestino,
                        facturaOrigen.Factura,
                        facturaOrigen.Lineas,
                        resultado);
                }

                TraspasFacturasLogger.Informacio(
                    "Connexió utilitzada=ActiveX/ADO.",
                    CrearCampsLog(
                        baseDatosOrigen,
                        empresaDestino,
                        baseDatosDestino));

                return true;
            }
            catch (Exception ex)
            {
                var camps =
                    CrearCampsLog(
                        baseDatosOrigen,
                        empresaDestino,
                        baseDatosDestino);

                camps.Add(
                    "Error",
                    ex.Message);

                TraspasFacturasLogger.Advertencia(
                    "No s'ha pogut validar via ActiveX/ADO. S'activa el fallback OleDb/.config.",
                    camps);

                resultado = null;
                return false;
            }
            finally
            {
                FinalitzarEnlace(enlaceDestino);
            }
        }

        private static a3ERPActiveX.Enlace CrearEnlaceDestinoAutenticat(
            string baseDatosOrigen,
            string empresaDestino,
            string baseDatosDestino)
        {
            string usuario =
                ConfigurationManager.AppSettings[
                    ClauUsuari];

            if (string.IsNullOrWhiteSpace(usuario))
            {
                throw new InvalidOperationException(
                    "No s'ha configurat l'usuari ActiveX.");
            }

            var enlace =
                new a3ERPActiveX.Enlace
                {
                    RaiseOnException = true
                };

            TraspasFacturasLogger.Informacio(
                "ActiveX destí: abans de LoginUsuario.",
                CrearCampsLog(
                    baseDatosOrigen,
                    empresaDestino,
                    baseDatosDestino));

            bool loginOk =
                enlace.LoginUsuario(
                    usuario,
                    ConfigurationManager.AppSettings[
                        ClauPassword] ?? string.Empty);

            if (!loginOk)
            {
                AlliberarComObject(enlace);
                throw new InvalidOperationException(
                    "No s'ha pogut iniciar sessió a a3ERPActiveX.");
            }

            TraspasFacturasLogger.Informacio(
                "ActiveX destí: LoginUsuario completat.",
                CrearCampsLog(
                    baseDatosOrigen,
                    empresaDestino,
                    baseDatosDestino));

            return enlace;
        }

        private static a3ERPActiveX.Enlace CrearEnlaceAutenticat()
        {
            string usuario =
                ConfigurationManager.AppSettings[
                    ClauUsuari];

            if (string.IsNullOrWhiteSpace(usuario))
            {
                throw new InvalidOperationException(
                    "No s'ha configurat l'usuari ActiveX.");
            }

            var enlace =
                new a3ERPActiveX.Enlace
                {
                    RaiseOnException = true
                };

            bool loginOk =
                enlace.LoginUsuario(
                    usuario,
                    ConfigurationManager.AppSettings[
                        ClauPassword] ?? string.Empty);

            if (!loginOk)
            {
                throw new InvalidOperationException(
                    "No s'ha pogut iniciar sessió a a3ERPActiveX.");
            }

            return enlace;
        }

        private static Connection ObtenirConnexioEmpresa(
            a3ERPActiveX.Enlace enlace,
            string context)
        {
            if (enlace.Estado !=
                a3ERPActiveX.EstadoEnlace.estACTIVO)
            {
                throw new InvalidOperationException(
                    "No s'ha pogut iniciar l'empresa " +
                    context +
                    " via a3ERPActiveX.");
            }

            Connection conexion =
                enlace.GetConexionDB("EMPRESA");

            if (conexion == null)
            {
                throw new InvalidOperationException(
                    "a3ERPActiveX no ha retornat la connexió de l'empresa " +
                    context +
                    ".");
            }

            return conexion;
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
                            ExtraerBaseDatos(
                                enlace.ParamConexion(empresa),
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

        private static void ValidarFactura(
            Connection conexionOrigen,
            Connection conexionDestino,
            FacturaOrigenDto factura,
            ResultadoValidacionTraspasoDto resultado)
        {
            List<LineaValidacion> lineas =
                LeerLineasOrigen(
                    conexionOrigen,
                    factura.IdFacv);

            ValidarFacturaDestino(
                conexionDestino,
                factura,
                lineas,
                resultado);
        }

        private static void ValidarFacturaDestino(
            Connection conexionDestino,
            FacturaOrigenDto factura,
            List<LineaValidacion> lineas,
            ResultadoValidacionTraspasoDto resultado)
        {
            ValidarCliente(
                conexionDestino,
                factura,
                resultado);

            if (lineas.Count == 0)
            {
                resultado.Incidencias.Add(
                    new IncidenciaValidacionDto
                    {
                        Factura = factura.Factura,
                        Tipo = "Línies",
                        Mensaje =
                            "La factura no té línies a LINEFACT."
                    });

                return;
            }

            var articulosComprobados =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            var tiposIvaComprobados =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (LineaValidacion linea in lineas)
            {
                if (!string.IsNullOrWhiteSpace(linea.CodArt) &&
                    articulosComprobados.Add(linea.CodArt))
                {
                    ValidarArticulo(
                        conexionDestino,
                        factura,
                        linea.CodArt,
                        resultado);
                }

                if (!string.IsNullOrWhiteSpace(linea.TipIva) &&
                    tiposIvaComprobados.Add(linea.TipIva))
                {
                    ValidarTipoIva(
                        conexionDestino,
                        factura,
                        linea.TipIva,
                        resultado);
                }
            }
        }

        private static void ValidarFactura(
            OleDbConnection conexionOrigen,
            OleDbConnection conexionDestino,
            FacturaOrigenDto factura,
            ResultadoValidacionTraspasoDto resultado)
        {
            ValidarCliente(
                conexionDestino,
                factura,
                resultado);

            List<LineaValidacion> lineas =
                LeerLineasOrigen(
                    conexionOrigen,
                    factura.IdFacv);

            if (lineas.Count == 0)
            {
                resultado.Incidencias.Add(
                    new IncidenciaValidacionDto
                    {
                        Factura = factura.Factura,
                        Tipo = "Línies",
                        Mensaje =
                            "La factura no té línies a LINEFACT."
                    });

                return;
            }

            var articulosComprobados =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            var tiposIvaComprobados =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase);

            foreach (LineaValidacion linea in lineas)
            {
                if (!string.IsNullOrWhiteSpace(linea.CodArt) &&
                    articulosComprobados.Add(linea.CodArt))
                {
                    ValidarArticulo(
                        conexionDestino,
                        factura,
                        linea.CodArt,
                        resultado);
                }

                if (!string.IsNullOrWhiteSpace(linea.TipIva) &&
                    tiposIvaComprobados.Add(linea.TipIva))
                {
                    ValidarTipoIva(
                        conexionDestino,
                        factura,
                        linea.TipIva,
                        resultado);
                }
            }
        }

        private static void ValidarCliente(
            Connection conexionDestino,
            FacturaOrigenDto factura,
            ResultadoValidacionTraspasoDto resultado)
        {
            string codCli =
                factura.CodCli?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(codCli))
            {
                resultado.Incidencias.Add(
                    new IncidenciaValidacionDto
                    {
                        Factura = factura.Factura,
                        Tipo = "Client",
                        Mensaje =
                            "La factura no té CODCLI."
                    });

                return;
            }

            if (!ExisteCodigo(
                    conexionDestino,
                    @"
                SELECT COUNT(*)
                FROM dbo.CLIENTES
                WHERE LTRIM(RTRIM(CODCLI)) = ?;",
                    codCli))
            {
                resultado.Incidencias.Add(
                    new IncidenciaValidacionDto
                    {
                        Factura = factura.Factura,
                        Tipo = "Client",
                        Codigo = codCli,
                        Mensaje =
                            "El client no existeix a l'empresa de destinació."
                    });
            }
        }

        private static void ValidarCliente(
            OleDbConnection conexionDestino,
            FacturaOrigenDto factura,
            ResultadoValidacionTraspasoDto resultado)
        {
            string codCli =
                factura.CodCli?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(codCli))
            {
                resultado.Incidencias.Add(
                    new IncidenciaValidacionDto
                    {
                        Factura = factura.Factura,
                        Tipo = "Client",
                        Mensaje =
                            "La factura no té CODCLI."
                    });

                return;
            }

            bool existe =
                ExisteCodigo(
                    conexionDestino,
                    @"
                SELECT COUNT(*)
                FROM dbo.CLIENTES
                WHERE LTRIM(RTRIM(CODCLI)) = ?;",
                    codCli);

            if (!existe)
            {
                resultado.Incidencias.Add(
                    new IncidenciaValidacionDto
                    {
                        Factura = factura.Factura,
                        Tipo = "Client",
                        Codigo = codCli,
                        Mensaje =
                            "El client no existeix a l'empresa de destinació."
                    });
            }
        }

        private static void ValidarArticulo(
            Connection conexionDestino,
            FacturaOrigenDto factura,
            string codArt,
            ResultadoValidacionTraspasoDto resultado)
        {
            if (!ExisteCodigo(
                    conexionDestino,
                    @"
                    SELECT COUNT(*)
                    FROM dbo.ARTICULO
                    WHERE LTRIM(RTRIM(CODART)) = ?;",
                    codArt))
            {
                resultado.Incidencias.Add(
                    new IncidenciaValidacionDto
                    {
                        Factura = factura.Factura,
                        Tipo = "Article",
                        Codigo = codArt,
                        Mensaje =
                            "L'article no existeix a l'empresa de destinació."
                    });
            }
        }

        private static void ValidarArticulo(
            OleDbConnection conexionDestino,
            FacturaOrigenDto factura,
            string codArt,
            ResultadoValidacionTraspasoDto resultado)
        {
            bool existe =
                ExisteCodigo(
                    conexionDestino,
                    @"
                    SELECT COUNT(*)
                    FROM dbo.ARTICULO
                    WHERE LTRIM(RTRIM(CODART)) = ?;",
                    codArt);

            if (!existe)
            {
                resultado.Incidencias.Add(
                    new IncidenciaValidacionDto
                    {
                        Factura = factura.Factura,
                        Tipo = "Article",
                        Codigo = codArt,
                        Mensaje =
                            "L'article no existeix a l'empresa de destinació."
                    });
            }
        }

        private static void ValidarTipoIva(
            Connection conexionDestino,
            FacturaOrigenDto factura,
            string tipIva,
            ResultadoValidacionTraspasoDto resultado)
        {
            if (!ExisteCodigo(
                    conexionDestino,
                    @"
                SELECT COUNT(*)
                FROM dbo.TIPOIVA
                WHERE LTRIM(RTRIM(TIPIVA)) = ?;",
                    tipIva))
            {
                resultado.Incidencias.Add(
                    new IncidenciaValidacionDto
                    {
                        Factura = factura.Factura,
                        Tipo = "IVA",
                        Codigo = tipIva,
                        Mensaje =
                            "El tipus d'IVA no existeix a l'empresa de destinació."
                    });
            }
        }

        private static void ValidarTipoIva(
            OleDbConnection conexionDestino,
            FacturaOrigenDto factura,
            string tipIva,
            ResultadoValidacionTraspasoDto resultado)
        {
            bool existe =
                ExisteCodigo(
                    conexionDestino,
                    @"
                SELECT COUNT(*)
                FROM dbo.TIPOIVA
                WHERE LTRIM(RTRIM(TIPIVA)) = ?;",
                    tipIva);

            if (!existe)
            {
                resultado.Incidencias.Add(
                    new IncidenciaValidacionDto
                    {
                        Factura = factura.Factura,
                        Tipo = "IVA",
                        Codigo = tipIva,
                        Mensaje =
                            "El tipus d'IVA no existeix a l'empresa de destinació."
                    });
            }
        }

        private static List<LineaValidacion> LeerLineasOrigen(
            Connection conexion,
            decimal idFacv)
        {
            const string sql = @"
                SELECT
                    LTRIM(RTRIM(COALESCE(CODART, ''))) AS CODART,
                    LTRIM(RTRIM(COALESCE(TIPIVA, ''))) AS TIPIVA
                FROM dbo.LINEFACT
                WHERE IDFACV = ?;";

            var resultado =
                new List<LineaValidacion>();

            Command command = null;
            Recordset recordset = null;

            try
            {
                command =
                    CrearComanda(
                        conexion,
                        sql);

                AfegirParametreDecimal(
                    command,
                    "IDFACV",
                    idFacv);

                object filesAfectades =
                    Type.Missing;

                recordset =
                    command.Execute(
                        out filesAfectades);

                while (recordset != null &&
                       !recordset.EOF)
                {
                    resultado.Add(
                        new LineaValidacion
                        {
                            CodArt =
                                LeerTexto(
                                    recordset,
                                    "CODART"),

                            TipIva =
                                LeerTexto(
                                    recordset,
                                    "TIPIVA")
                        });

                    recordset.MoveNext();
                }
            }
            finally
            {
                TancarRecordset(recordset);
                AlliberarComObject(recordset);
                AlliberarComObject(command);
            }

            return resultado;
        }

        private static List<LineaValidacion> LeerLineasOrigen(
            OleDbConnection conexion,
            decimal idFacv)
        {
            const string sql = @"
                SELECT
                    LTRIM(RTRIM(COALESCE(CODART, ''))) AS CODART,
                    LTRIM(RTRIM(COALESCE(TIPIVA, ''))) AS TIPIVA
                FROM dbo.LINEFACT
                WHERE IDFACV = ?;";

            var resultado =
                new List<LineaValidacion>();

            using (var command =
                   new OleDbCommand(sql, conexion))
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

                using (OleDbDataReader reader =
                       command.ExecuteReader())
                {
                    while (reader != null &&
                           reader.Read())
                    {
                        resultado.Add(
                            new LineaValidacion
                            {
                                CodArt =
                                    LeerTexto(
                                        reader,
                                        "CODART"),

                                TipIva =
                                    LeerTexto(
                                        reader,
                                        "TIPIVA")
                            });
                    }
                }
            }

            return resultado;
        }

        private static bool ExisteCodigo(
            Connection conexion,
            string sql,
            string codigo)
        {
            Command command = null;
            Recordset recordset = null;

            try
            {
                command =
                    CrearComanda(
                        conexion,
                        sql);

                string valorCodigo =
                    codigo.Trim();

                command.Parameters.Append(
                    command.CreateParameter(
                        "CODIGO",
                        DataTypeEnum.adVarWChar,
                        ParameterDirectionEnum.adParamInput,
                        Math.Max(valorCodigo.Length, 1),
                        valorCodigo));

                object filesAfectades =
                    Type.Missing;

                recordset =
                    command.Execute(
                        out filesAfectades);

                return recordset != null &&
                       !recordset.EOF &&
                       Convert.ToInt32(
                           LeerValor(
                               recordset,
                               0)) > 0;
            }
            finally
            {
                TancarRecordset(recordset);
                AlliberarComObject(recordset);
                AlliberarComObject(command);
            }
        }

        private static bool ExisteCodigo(
            OleDbConnection conexion,
            string sql,
            string codigo)
        {
            using (var command =
                   new OleDbCommand(
                       sql,
                       conexion))
            {
                command.CommandType =
                    CommandType.Text;

                command.CommandTimeout =
                    60;

                command.Parameters
                    .Add(
                        "CODIGO",
                        OleDbType.VarWChar)
                    .Value =
                        codigo.Trim();

                object valor =
                    command.ExecuteScalar();

                return valor != null &&
                       valor != DBNull.Value &&
                       Convert.ToInt32(valor) > 0;
            }
        }

        private static Command CrearComanda(
            Connection conexion,
            string sql)
        {
            return new Command
            {
                ActiveConnection = conexion,
                CommandText = sql,
                CommandType = CommandTypeEnum.adCmdText,
                CommandTimeout = 60
            };
        }

        private static void AfegirParametreDecimal(
            Command command,
            string nom,
            decimal valor)
        {
            Parameter parametre =
                command.CreateParameter(
                    nom,
                    DataTypeEnum.adNumeric,
                    ParameterDirectionEnum.adParamInput,
                    18,
                    valor);

            parametre.Precision = 18;
            parametre.NumericScale = 0;
            command.Parameters.Append(parametre);
        }

        private static Dictionary<string, object> CrearCampsLog(
            string baseDatosOrigen,
            string empresaDestino,
            string baseDatosDestino)
        {
            return new Dictionary<string, object>
            {
                { "BaseDadesOrigen", baseDatosOrigen ?? string.Empty },
                { "EmpresaDesti", empresaDestino ?? string.Empty },
                { "BaseDadesDesti", baseDatosDestino ?? string.Empty }
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
                LeerValor(
                    reader,
                    campo);

            return valor == DBNull.Value ||
                   valor == null
                ? string.Empty
                : Convert.ToString(valor)?.Trim()
                  ?? string.Empty;
        }

        private static object LeerValor(
            Recordset reader,
            object campo)
        {
            object valor =
                reader.Fields[campo].Value;

            return valor ??
                   DBNull.Value;
        }

        private static string ResolverBaseDatosDestino(
            string empresaDestino)
        {
            string usuario =
                ConfigurationManager.AppSettings[
                    ClauUsuari];

            string password =
                ConfigurationManager.AppSettings[
                    ClauPassword];

            if (string.IsNullOrWhiteSpace(usuario))
            {
                throw new InvalidOperationException(
                    "No s'ha configurat l'usuari ActiveX.");
            }

            var enlace =
                new a3ERPActiveX.Enlace();

            try
            {
                enlace.RaiseOnException =
                    true;

                bool loginOk =
                    enlace.LoginUsuario(
                        usuario,
                        password ?? string.Empty);

                if (!loginOk)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut iniciar sessió a a3ERPActiveX.");
                }

                object paramConexion =
                    enlace.ParamConexion(
                        empresaDestino.Trim());

                return ExtraerBaseDatos(
                    paramConexion,
                    empresaDestino);
            }
            finally
            {
                FinalitzarEnlace(enlace);
            }
        }

        private static string ExtraerBaseDatos(
            object paramConexion,
            string empresaDestino)
        {
            Array valores =
                paramConexion as Array;

            if (valores == null)
            {
                throw new InvalidOperationException(
                    "ParamConexion no ha retornat el format esperat " +
                    "per a l'empresa '" +
                    empresaDestino +
                    "'.");
            }

            int lower =
                valores.GetLowerBound(0);

            int upper =
                valores.GetUpperBound(0);

            int indiceBaseDatos =
                lower + 5;

            if (indiceBaseDatos > upper)
            {
                throw new InvalidOperationException(
                    "ParamConexion no conté la base de dades " +
                    "de l'empresa '" +
                    empresaDestino +
                    "'.");
            }

            object valor =
                valores.GetValue(
                    indiceBaseDatos);

            string baseDatos =
                Convert.ToString(valor)?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(baseDatos))
            {
                throw new InvalidOperationException(
                    "La base de dades retornada per ParamConexion " +
                    "és buida per a l'empresa '" +
                    empresaDestino +
                    "'.");
            }

            return baseDatos;
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

        private sealed class LineaValidacion
        {
            public string CodArt { get; set; } =
                string.Empty;

            public string TipIva { get; set; } =
                string.Empty;
        }

        private sealed class FacturaLineasOrigen
        {
            public FacturaOrigenDto Factura { get; set; }

            public List<LineaValidacion> Lineas { get; set; }
        }
    }
}
