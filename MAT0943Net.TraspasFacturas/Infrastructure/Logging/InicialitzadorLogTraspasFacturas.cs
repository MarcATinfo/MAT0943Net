using MAT0943Net.TraspasFacturas.Infrastructure.Connections;
using MAT0943Net.TraspasFacturas.Models;
using MAT0943Net.TraspasFacturas.Services;
using System;
using System.Collections.Generic;

namespace MAT0943Net.TraspasFacturas.Infrastructure.Logging
{
    internal static class InicialitzadorLogTraspasFacturas
    {
        private const int DiesRetencioLog =
            7;

        public static void Inicialitzar(
            string baseDatosOrigen)
        {
            TraspasFacturasLogger.InicialitzarLogReserva();

            ConfiguracioLogTraspasFacturas configuracio =
                new ConfiguracioLogTraspasFacturas();

            try
            {
                string cadenaConnexio =
                    TraspasFacturasConnectionStringHelper
                        .CrearConnexioBaseDades(
                            baseDatosOrigen);

                ServeiConfiguracioLogTraspasFacturas serveiConfiguracio =
                    new ServeiConfiguracioLogTraspasFacturas();

                configuracio =
                    serveiConfiguracio.Carregar(
                        cadenaConnexio);
            }
            catch (Exception ex)
            {
                TraspasFacturasLogger.Error(
                    "No s'ha pogut llegir la configuració del log. " +
                    "Es continua amb el log local de reserva.",
                    ex,
                    new Dictionary<string, object>
                    {
                        {
                            "BaseDadesOrigen",
                            baseDatosOrigen ?? string.Empty
                        }
                    });
            }

            TraspasFacturasLogger.AplicarConfiguracioLog(
                configuracio);

            TraspasFacturasLogger.Informacio(
                "S'ha inicialitzat el sistema de log de traspàs de factures.",
                new Dictionary<string, object>
                {
                    {
                        "BaseDadesOrigen",
                        baseDatosOrigen ?? string.Empty
                    },
                    {
                        "Ruta",
                        configuracio.RutaLog
                    },
                    {
                        "RetencioDies",
                        DiesRetencioLog
                    }
                });
        }
    }
}