using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace MAT0943Net.TraspasFacturas.Services
{
    internal sealed class ConsultaEmpresasActiveXService
    {
        private const string ClauUsuari =
            "TraspasFacturas_ActiveXUsuario";

        private const string ClauPassword =
            "TraspasFacturas_ActiveXPassword";

        public IReadOnlyList<string> ObtenirEmpresasDisponibles(
            string empresaOrigen)
        {
            string usuari =
                ConfigurationManager.AppSettings[ClauUsuari];

            string password =
                ConfigurationManager.AppSettings[ClauPassword];

            if (string.IsNullOrWhiteSpace(usuari))
            {
                throw new InvalidOperationException(
                    "No s'ha configurat l'usuari ActiveX.");
            }

            if (password == null)
            {
                password = string.Empty;
            }

            a3ERPActiveX.Enlace enlace = null;

            try
            {
                enlace =
                    new a3ERPActiveX.Enlace();

                enlace.RaiseOnException = true;

                bool loginCorrecte =
                    enlace.LoginUsuario(
                        usuari,
                        password);

                if (!loginCorrecte)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut iniciar sessió a a3ERP ActiveX.");
                }

                object resultat =
                    enlace.Empresas();

                return ConvertirEmpresas(
                    resultat,
                    empresaOrigen);
            }
            finally
            {
                if (enlace != null)
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
                        // No bloquegem el tancament
                        // d'una consulta de només lectura.
                    }
                }
            }
        }

        private static IReadOnlyList<string> ConvertirEmpresas(
            object resultat,
            string empresaOrigen)
        {
            if (!(resultat is Array array))
            {
                throw new InvalidOperationException(
                    "Empresas() ha retornat un format no reconegut.");
            }

            List<string> empreses =
                new List<string>();

            foreach (object valor in array)
            {
                if (valor == null)
                {
                    continue;
                }

                string text =
                    Convert.ToString(
                        valor,
                        CultureInfo.InvariantCulture)
                    ?.Trim();

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                // La prova real ha confirmat que
                // el primer element és numèric:
                // [0] = nombre d'empreses.
                if (int.TryParse(text, out _))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(empresaOrigen) &&
                    string.Equals(
                        text,
                        empresaOrigen.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                empreses.Add(text);
            }

            return empreses
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    x => x,
                    StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }
    }
}