using MAT0943Net.TraspasFacturas.Infrastructure.Connections;
using MAT0943Net.TraspasFacturas.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;

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

            return resultado;
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
                }
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
    }
}