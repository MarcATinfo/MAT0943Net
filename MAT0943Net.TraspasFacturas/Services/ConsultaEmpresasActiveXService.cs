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
                    enlace,
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

        public void ValidarEmpresaDisponible(
            string empresa)
        {
            if (string.IsNullOrWhiteSpace(empresa))
            {
                throw new InvalidOperationException(
                    "No s'ha seleccionat cap empresa de destinació.");
            }

            string usuari =
                ConfigurationManager.AppSettings[ClauUsuari];

            string password =
                ConfigurationManager.AppSettings[ClauPassword];

            if (string.IsNullOrWhiteSpace(usuari))
            {
                throw new InvalidOperationException(
                    "No s'ha configurat l'usuari ActiveX.");
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
                        password ?? string.Empty);

                if (!loginCorrecte)
                {
                    throw new InvalidOperationException(
                        "No s'ha pogut iniciar sessió a a3ERP ActiveX.");
                }

                ObtenirBaseDades(
                    enlace,
                    empresa.Trim());
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
                        // No bloquegem el tancament d'una consulta de només lectura.
                    }
                }
            }
        }

        private static IReadOnlyList<string> ConvertirEmpresas(
            a3ERPActiveX.Enlace enlace,
            object resultat,
            string empresaOrigen)
        {
            if (!(resultat is Array array))
            {
                throw new InvalidOperationException(
                    "Empresas() ha retornat un format no reconegut.");
            }

            int minim =
                array.GetLowerBound(0);

            int maxim =
                array.GetUpperBound(0);

            if (minim > 0 || maxim < 0)
            {
                throw new InvalidOperationException(
                    "Empresas() no conté el total d'empreses a la posició 0.");
            }

            int total;

            if (!int.TryParse(
                    Convert.ToString(
                        array.GetValue(0),
                        CultureInfo.InvariantCulture),
                    out total) ||
                total < 0)
            {
                throw new InvalidOperationException(
                    "Empresas() no ha retornat un total d'empreses vàlid.");
            }

            List<string> empreses =
                new List<string>();

            for (int index = 1;
                 index <= total && index <= maxim;
                 index++)
            {
                string empresa =
                    Convert.ToString(
                        array.GetValue(index),
                        CultureInfo.InvariantCulture)
                    ?.Trim();

                if (string.IsNullOrWhiteSpace(empresa))
                {
                    continue;
                }

                try
                {
                    string baseDades =
                        ObtenirBaseDades(
                            enlace,
                            empresa);

                    if (string.Equals(
                            baseDades,
                            empresaOrigen?.Trim(),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    empreses.Add(empresa);
                }
                catch
                {
                    // Una empresa amb configuracio invalida no ha de bloquejar la llista.
                }
            }

            return empreses
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    x => x,
                    StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        private static string ObtenirBaseDades(
            a3ERPActiveX.Enlace enlace,
            string empresa)
        {
            object resultat =
                enlace.ParamConexion(empresa);

            Array valors =
                resultat as Array;

            if (valors == null)
            {
                throw new InvalidOperationException(
                    "ParamConexion no ha retornat el format esperat per a l'empresa '" +
                    empresa +
                    "'.");
            }

            int indexBaseDades =
                valors.GetLowerBound(0) + 5;

            if (indexBaseDades > valors.GetUpperBound(0))
            {
                throw new InvalidOperationException(
                    "ParamConexion no conté la base de dades de l'empresa '" +
                    empresa +
                    "'.");
            }

            string baseDades =
                Convert.ToString(
                    valors.GetValue(indexBaseDades),
                    CultureInfo.InvariantCulture)
                ?.Trim();

            if (string.IsNullOrWhiteSpace(baseDades))
            {
                throw new InvalidOperationException(
                    "La base de dades retornada per ParamConexion és buida per a l'empresa '" +
                    empresa +
                    "'.");
            }

            return baseDades;
        }
    }
}
