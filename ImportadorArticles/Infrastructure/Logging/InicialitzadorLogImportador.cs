using A3ErpImportadorArticles.Models;
using A3ErpImportadorArticles.Services;
using System;
using System.Collections.Generic;

namespace A3ErpImportadorArticles.Infrastructure.Logging
{
    /// <summary>
    /// Inicialitza el sistema de log de l'importador reutilitzant
    /// la configuració de AT_MAT0943NET_CONFIG.
    /// </summary>
    public static class InicialitzadorLogImportador
    {
        private const int DiesRetencioLog =
            7;

        /// <summary>
        /// Llegeix la configuració, configura el logger
        /// i conserva el log local com a reserva si alguna cosa falla.
        /// </summary>
        public static void Inicialitzar(
            ContextImportadorA3Erp context)
        {
            if (context == null)
            {
                return;
            }

            ConfiguracioLogImportador configuracio =
                new ConfiguracioLogImportador();

            try
            {
                ServeiConfiguracioLogImportador serveiConfiguracio =
                    new ServeiConfiguracioLogImportador();

                configuracio =
                    serveiConfiguracio.Carregar(
                        context.CadenaConnexio);
            }
            catch (Exception ex)
            {
                ImportadorArticlesLogger.Error(
                    "No s'ha pogut llegir la configuració del log de l'importador. Es continua amb el log local de reserva.",
                    ex,
                    CrearCampsContext(
                        context));
            }

            ImportadorArticlesLogger.AplicarConfiguracioLog(
                configuracio);

            ImportadorArticlesLogger.Informacio(
                "S'ha inicialitzat el sistema de log.",
                new Dictionary<string, object>
                {
                    {
                        "Empresa",
                        context.EmpresaActiva
                    },
                    {
                        "BaseDades",
                        context.BaseDadesEmpresa
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

        /// <summary>
        /// Construeix els camps comuns del context.
        /// No inclou cap contrasenya ni cadena de connexió.
        /// </summary>
        private static Dictionary<string, object> CrearCampsContext(
            ContextImportadorA3Erp context)
        {
            return new Dictionary<string, object>
            {
                {
                    "Empresa",
                    context?.EmpresaActiva
                    ?? string.Empty
                },
                {
                    "BaseDades",
                    context?.BaseDadesEmpresa
                    ?? string.Empty
                }
            };
        }
    }
}
