using MAT0943Net.TraspasFacturas.Infrastructure.Connections;
using MAT0943Net.TraspasFacturas.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace MAT0943Net.TraspasFacturas.Services
{
    internal sealed class ConsultaFacturasOrigenService
    {
        public const int LimiteFacturas =
            200;

        public List<FacturaOrigenDto> Consultar(
            string baseDatosOrigen,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            string texto,
            string tipo,
            string estado)
        {
            string conexion =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioOrigen(
                        baseDatosOrigen);

            string filtroTexto =
                NormalizarFiltroTexto(
                    texto);

            string filtroTipo =
                NormalizarFiltroTipo(
                    tipo);

            string filtroEstado =
                NormalizarFiltroEstado(
                    estado);

            string sql =
                CrearSql(
                    fechaDesde.HasValue,
                    fechaHasta.HasValue,
                    !string.IsNullOrWhiteSpace(
                        filtroTexto),
                    filtroTipo,
                    filtroEstado);

            var facturas =
                new List<FacturaOrigenDto>();

            using (var connection =
                   new OleDbConnection(conexion))
            using (var command =
                   new OleDbCommand(
                       sql,
                       connection))
            {
                command.CommandType =
                    CommandType.Text;

                command.CommandTimeout =
                    60;

                /*
                 * IMPORTANT:
                 * OleDb associa els paràmetres
                 * estrictament per posició.
                 *
                 * L'ordre d'aquests Add ha de coincidir
                 * amb l'ordre dels ? de CrearSql().
                 */

                if (fechaDesde.HasValue)
                {
                    command.Parameters
                        .Add(
                            "FECHA_DESDE",
                            OleDbType.Date)
                        .Value =
                            fechaDesde.Value.Date;
                }

                if (fechaHasta.HasValue)
                {
                    command.Parameters
                        .Add(
                            "FECHA_HASTA",
                            OleDbType.Date)
                        .Value =
                            fechaHasta.Value.Date
                                .AddDays(1);
                }

                if (!string.IsNullOrWhiteSpace(
                        filtroTexto))
                {
                    string textoLike =
                        "%" +
                        filtroTexto +
                        "%";

                    /*
                     * 1. Codi client
                     */
                    command.Parameters
                        .Add(
                            "CODCLI",
                            OleDbType.VarWChar)
                        .Value =
                            textoLike;

                    /*
                     * 2. Nom client
                     */
                    command.Parameters
                        .Add(
                            "NOMCLI",
                            OleDbType.VarWChar)
                        .Value =
                            textoLike;

                    /*
                     * 3. Número factura
                     */
                    command.Parameters
                        .Add(
                            "NUMDOC",
                            OleDbType.VarWChar)
                        .Value =
                            textoLike;

                    /*
                     * 4. Sèrie
                     */
                    command.Parameters
                        .Add(
                            "SERIE",
                            OleDbType.VarWChar)
                        .Value =
                            textoLike;

                    /*
                     * 5. Factura completa.
                     * Exemple: 2026-836
                     */
                    command.Parameters
                        .Add(
                            "FACTURA",
                            OleDbType.VarWChar)
                        .Value =
                            textoLike;
                }

                connection.Open();

                using (OleDbDataReader reader =
                       command.ExecuteReader())
                {
                    while (reader != null &&
                           reader.Read())
                    {
                        facturas.Add(
                            MapearFactura(
                                reader));
                    }
                }
            }

            return facturas;
        }

        private static string CrearSql(
            bool aplicaFechaDesde,
            bool aplicaFechaHasta,
            bool aplicaTexto,
            string tipo,
            string estado)
        {
            string sql = @"
SELECT TOP 200
    F.IDFACV,
    LTRIM(RTRIM(COALESCE(F.SERIE, ''))) AS SERIE,
    F.NUMDOC,
    F.FECHA,
    LTRIM(RTRIM(F.CODCLI)) AS CODCLI,
    F.NOMCLI,
    F.BASE,
    F.TOTIVA,
    F.TOTDOC,
    F.RECTIFICATIVA,
    F.BORRADOR,
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM dbo.AT_TRASPAS_FACTURES T
            WHERE
                T.IDFACV_ORIGEN = F.IDFACV
                AND T.ESTADO = 'CREADA'
        )
        THEN 1
        ELSE 0
    END AS TRASPASSADA
FROM dbo.CABEFACV F
WHERE 1 = 1";

            /*
             * FILTRES DE DATA
             */

            if (aplicaFechaDesde)
            {
                sql += @"
  AND F.FECHA >= ?";
            }

            if (aplicaFechaHasta)
            {
                sql += @"
  AND F.FECHA < ?";
            }

            /*
             * CERCA GENERAL
             *
             * Busca simultàniament per:
             * - codi client
             * - nom client
             * - número factura
             * - sèrie
             * - factura completa, ex. 2026-836
             */

            if (aplicaTexto)
            {
                sql += @"
  AND
  (
      LTRIM(RTRIM(COALESCE(F.CODCLI, ''))) LIKE ?
      OR COALESCE(F.NOMCLI, '') LIKE ?
      OR CONVERT(varchar(30), F.NUMDOC) LIKE ?
      OR LTRIM(RTRIM(COALESCE(F.SERIE, ''))) LIKE ?
      OR
      (
          LTRIM(RTRIM(COALESCE(F.SERIE, '')))
          + '-'
          + CONVERT(
                varchar(30),
                CONVERT(bigint, F.NUMDOC)
            )
      ) LIKE ?
  )";
            }

            /*
             * TIPUS FACTURA
             */

            if (string.Equals(
                    tipo,
                    "Normal",
                    StringComparison.OrdinalIgnoreCase))
            {
                sql += @"
  AND
  (
      F.RECTIFICATIVA IS NULL
      OR LTRIM(RTRIM(F.RECTIFICATIVA)) <> 'T'
  )";
            }
            else if (string.Equals(
                         tipo,
                         "Rectificativa",
                         StringComparison.OrdinalIgnoreCase))
            {
                sql += @"
  AND LTRIM(RTRIM(COALESCE(F.RECTIFICATIVA, ''))) = 'T'";
            }

            /*
             * ESTAT DEL TRASPÀS
             */

            if (string.Equals(
                    estado,
                    "Pendents",
                    StringComparison.OrdinalIgnoreCase))
            {
                sql += @"
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.AT_TRASPAS_FACTURES T
      WHERE
          T.IDFACV_ORIGEN = F.IDFACV
          AND T.ESTADO = 'CREADA'
  )";
            }
            else if (string.Equals(
                         estado,
                         "Traspassades",
                         StringComparison.OrdinalIgnoreCase))
            {
                sql += @"
  AND EXISTS
  (
      SELECT 1
      FROM dbo.AT_TRASPAS_FACTURES T
      WHERE
          T.IDFACV_ORIGEN = F.IDFACV
          AND T.ESTADO = 'CREADA'
  )";
            }

            sql += @"
ORDER BY
    F.FECHA DESC,
    F.IDFACV DESC;";

            return sql;
        }

        private static FacturaOrigenDto MapearFactura(
            OleDbDataReader reader)
        {
            return new FacturaOrigenDto
            {
                IdFacv =
                    LeerDecimal(
                        reader,
                        "IDFACV"),

                Serie =
                    LeerTexto(
                        reader,
                        "SERIE"),

                NumDoc =
                    LeerDecimal(
                        reader,
                        "NUMDOC"),

                Fecha =
                    LeerFechaNullable(
                        reader,
                        "FECHA"),

                CodCli =
                    LeerTexto(
                        reader,
                        "CODCLI"),

                NomCli =
                    LeerTexto(
                        reader,
                        "NOMCLI"),

                Base =
                    LeerDecimal(
                        reader,
                        "BASE"),

                Iva =
                    LeerDecimal(
                        reader,
                        "TOTIVA"),

                Total =
                    LeerDecimal(
                        reader,
                        "TOTDOC"),

                Rectificativa =
                    LeerTexto(
                        reader,
                        "RECTIFICATIVA"),

                Borrador =
                    LeerDecimal(
                        reader,
                        "BORRADOR"),

                Traspassada =
                    Convert.ToInt32(
                        reader["TRASPASSADA"]) == 1
            };
        }

        private static string NormalizarFiltroTexto(
            string texto)
        {
            return texto == null
                ? string.Empty
                : texto.Trim();
        }

        private static string NormalizarFiltroTipo(
            string tipo)
        {
            string valor =
                (tipo ?? string.Empty)
                .Trim();

            if (string.Equals(
                    valor,
                    "Normal",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Normal";
            }

            if (string.Equals(
                    valor,
                    "Rectificativa",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Rectificativa";
            }

            return "Tots";
        }

        private static string NormalizarFiltroEstado(
            string estado)
        {
            string valor =
                (estado ?? string.Empty)
                .Trim();

            if (string.Equals(
                    valor,
                    "Pendents",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Pendents";
            }

            if (string.Equals(
                    valor,
                    "Traspassades",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "Traspassades";
            }

            return "Totes";
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

        private static DateTime? LeerFechaNullable(
            OleDbDataReader reader,
            string campo)
        {
            object valor =
                reader[campo];

            if (valor == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDateTime(
                valor);
        }
    }
}