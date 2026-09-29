using System;
using System.Configuration;
using System.Data.OleDb;

namespace MAT0943Net.TraspasFacturas.Infrastructure.Connections
{
    internal static class TraspasFacturasConnectionStringHelper
    {
        public const string ClauConnexioPlantilla =
            "TraspasFacturas_ConexionSqlPlantillaOrigen";

        public static string CrearConnexioOrigen(
            string baseDatosOrigen)
        {
            return CrearConnexioBaseDades(
                baseDatosOrigen);
        }

        public static string CrearConnexioBaseDades(
            string baseDatos)
        {
            if (string.IsNullOrWhiteSpace(baseDatos))
            {
                throw new InvalidOperationException(
                    "No s'ha informat la base de dades.");
            }

            string plantilla =
                ConfigurationManager.AppSettings[
                    ClauConnexioPlantilla];

            if (string.IsNullOrWhiteSpace(plantilla))
            {
                throw new InvalidOperationException(
                    "No s'ha configurat la connexió SQL plantilla. " +
                    "Cal informar la clau '" +
                    ClauConnexioPlantilla +
                    "' a l'App.config de MAT0943Net.TraspasFacturas.");
            }

            OleDbConnectionStringBuilder builder =
                new OleDbConnectionStringBuilder(
                    plantilla);

            builder["Initial Catalog"] =
                baseDatos.Trim();

            return builder.ConnectionString;
        }
    }
}