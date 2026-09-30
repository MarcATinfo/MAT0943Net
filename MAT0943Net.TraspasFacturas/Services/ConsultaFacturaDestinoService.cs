using MAT0943Net.TraspasFacturas.Infrastructure.Connections;
using MAT0943Net.TraspasFacturas.Models;
using System;
using System.Data;
using System.Data.OleDb;

namespace MAT0943Net.TraspasFacturas.Services
{
    internal sealed class ConsultaFacturaDestinoService
    {
        public FacturaDestinoRecuperacionDto Consultar(
            string baseDatosDestino,
            decimal idFacvDestino)
        {
            if (string.IsNullOrWhiteSpace(baseDatosDestino))
            {
                throw new InvalidOperationException(
                    "No s'ha informat la base de dades destí.");
            }

            if (idFacvDestino <= 0)
            {
                throw new InvalidOperationException(
                    "L'ID de la factura destí no és vàlid.");
            }

            string conexion =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioBaseDades(
                        baseDatosDestino);

            const string sql = @"
SELECT
    IDFACV,
    LTRIM(RTRIM(COALESCE(SERIE, ''))) AS SERIE,
    NUMDOC
FROM dbo.CABEFACV
WHERE IDFACV = ?;";

            using (var connection =
                   new OleDbConnection(conexion))
            using (var command =
                   new OleDbCommand(sql, connection))
            {
                command.CommandType =
                    CommandType.Text;

                command.Parameters
                    .Add(
                        "IDFACV",
                        OleDbType.Decimal)
                    .Value =
                        idFacvDestino;

                connection.Open();

                using (OleDbDataReader reader =
                       command.ExecuteReader())
                {
                    if (reader == null ||
                        !reader.Read())
                    {
                        return null;
                    }

                    return new FacturaDestinoRecuperacionDto
                    {
                        IdFacv =
                            Convert.ToDecimal(
                                reader["IDFACV"]),

                        Serie =
                            reader["SERIE"] == DBNull.Value
                                ? string.Empty
                                : Convert.ToString(
                                    reader["SERIE"])?.Trim()
                                  ?? string.Empty,

                        NumDoc =
                            reader["NUMDOC"] == DBNull.Value
                                ? 0m
                                : Convert.ToDecimal(
                                    reader["NUMDOC"])
                    };
                }
            }
        }
    }
}