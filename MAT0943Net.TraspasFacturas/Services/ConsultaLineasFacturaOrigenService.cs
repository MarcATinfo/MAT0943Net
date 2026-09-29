using MAT0943Net.TraspasFacturas.Infrastructure.Connections;
using MAT0943Net.TraspasFacturas.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;

namespace MAT0943Net.TraspasFacturas.Services
{
    internal sealed class ConsultaLineasFacturaOrigenService
    {
        public List<LineaFacturaOrigenDto> Consultar(
            string baseDatosOrigen,
            decimal idFacv)
        {
            string conexion =
                TraspasFacturasConnectionStringHelper
                    .CrearConnexioBaseDades(
                        baseDatosOrigen);

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

            var resultado =
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

            return resultado;
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
    }
}