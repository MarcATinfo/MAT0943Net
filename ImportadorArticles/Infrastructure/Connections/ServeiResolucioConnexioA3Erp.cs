using A3ErpImportadorArticles.Models;
using System;
using System.Data.OleDb;

namespace A3ErpImportadorArticles.Infrastructure.Connections
{
    /// <summary>
    /// Resol una connexió operativa amb la base de dades
    /// de l'empresa activa d'a3ERP.
    ///
    /// Primer prova la connexió rebuda d'a3ERP.
    /// Si aquesta falla, conserva el mateix servidor,
    /// la mateixa base de dades i el mateix proveïdor,
    /// però substitueix les credencials per les d'AtInfo.
    /// </summary>
    public sealed class ServeiResolucioConnexioA3Erp
    {
        private const string UsuariAlternatiu = "atinfo";

        /*
         * Posa aquí exactament la mateixa contrasenya
         * que ja utilitza el projecte antic a CargaDatosConex.
         *
         * No la duplico al text del xat, però continuarà
         * quedant incorporada al codi, tal com heu acordat.
         */
        private const string ContrasenyaAlternativa =
            "4t1nf02150";

        /// <summary>
        /// Prova la connexió rebuda i aplica el fallback AtInfo
        /// únicament quan la connexió original no funciona.
        /// </summary>
        public ResultatResolucioConnexio Resoldre(
            string empresaActiva,
            string cadenaConnexioRebuda)
        {
            if (string.IsNullOrWhiteSpace(cadenaConnexioRebuda))
            {
                return ResultatResolucioConnexio.CrearError(
                    "a3ERP no ha proporcionat cap cadena de connexió.",
                    string.Empty);
            }

            ResultatProvaConnexio provaOriginal =
                ProvarConnexio(cadenaConnexioRebuda);

            if (provaOriginal.Correcte)
            {
                ContextImportadorA3Erp contextOriginal =
                    new ContextImportadorA3Erp(
                        empresaActiva,
                        provaOriginal.BaseDades,
                        cadenaConnexioRebuda,
                        false);

                return ResultatResolucioConnexio.CrearCorrecte(
                    contextOriginal,
                    "S'utilitza la connexió rebuda directament d'a3ERP.",
                    string.Empty);
            }

            string cadenaConnexioAlternativa;

            try
            {
                cadenaConnexioAlternativa =
                    ConstruirConnexioAlternativa(
                        cadenaConnexioRebuda);
            }
            catch (Exception ex)
            {
                return ResultatResolucioConnexio.CrearError(
                    "La connexió rebuda d'a3ERP ha fallat i no s'ha pogut " +
                    "construir la connexió alternativa AtInfo.\n\n" +
                    ex.Message,
                    provaOriginal.Error);
            }

            ResultatProvaConnexio provaAlternativa =
                ProvarConnexio(cadenaConnexioAlternativa);

            if (!provaAlternativa.Correcte)
            {
                return ResultatResolucioConnexio.CrearError(
                    "No s'ha pogut connectar a la base de dades activa.\n\n" +
                    "Error de la connexió rebuda d'a3ERP:\n" +
                    provaOriginal.Error +
                    "\n\nError de la connexió alternativa AtInfo:\n" +
                    provaAlternativa.Error,
                    provaOriginal.Error);
            }

            ContextImportadorA3Erp contextAlternatiu =
                new ContextImportadorA3Erp(
                    empresaActiva,
                    provaAlternativa.BaseDades,
                    cadenaConnexioAlternativa,
                    true);

            return ResultatResolucioConnexio.CrearCorrecte(
                contextAlternatiu,
                "La connexió rebuda d'a3ERP no s'ha pogut reutilitzar. " +
                "S'utilitza la connexió alternativa AtInfo.",
                provaOriginal.Error);
        }

        /// <summary>
        /// Prova una cadena de connexió obrint-la realment
        /// i consulta el nom de la base de dades activa.
        /// </summary>
        private static ResultatProvaConnexio ProvarConnexio(
            string cadenaConnexio)
        {
            try
            {
                using (OleDbConnection connexio =
                    new OleDbConnection(cadenaConnexio))
                {
                    connexio.Open();

                    using (OleDbCommand comanda =
                        connexio.CreateCommand())
                    {
                        comanda.CommandText =
                            "SELECT DB_NAME()";

                        object resultat =
                            comanda.ExecuteScalar();

                        string baseDades =
                            Convert.ToString(resultat).Trim();

                        return ResultatProvaConnexio.CrearCorrecte(
                            baseDades);
                    }
                }
            }
            catch (Exception ex)
            {
                return ResultatProvaConnexio.CrearError(
                    ex.Message);
            }
        }

        /// <summary>
        /// Conserva totes les propietats útils de la cadena original,
        /// però elimina possibles mecanismes d'autenticació integrada
        /// i aplica les credencials alternatives d'AtInfo.
        /// </summary>
        private static string ConstruirConnexioAlternativa(
            string cadenaConnexioOriginal)
        {
            OleDbConnectionStringBuilder builder =
                new OleDbConnectionStringBuilder(
                    cadenaConnexioOriginal);

            ValidarServidorIBaseDades(builder);

            /*
             * Eliminem opcions d'autenticació integrada perquè
             * no entrin en conflicte amb l'usuari i la contrasenya.
             */
            EliminarClauSiExisteix(
                builder,
                "Integrated Security");

            EliminarClauSiExisteix(
                builder,
                "Trusted_Connection");

            builder["User ID"] =
                UsuariAlternatiu;

            builder["Password"] =
                ContrasenyaAlternativa;

            builder["Persist Security Info"] =
                true;

            return builder.ConnectionString;
        }

        /// <summary>
        /// Comprova que la cadena rebuda permet identificar
        /// el servidor i la base de dades que s'han de conservar.
        /// </summary>
        private static void ValidarServidorIBaseDades(
            OleDbConnectionStringBuilder builder)
        {
            string servidor =
                ObtenirValorCadena(
                    builder,
                    "Data Source",
                    "Server");

            string baseDades =
                ObtenirValorCadena(
                    builder,
                    "Initial Catalog",
                    "Database");

            if (string.IsNullOrWhiteSpace(servidor))
            {
                throw new InvalidOperationException(
                    "La cadena de connexió no conté el servidor.");
            }

            if (string.IsNullOrWhiteSpace(baseDades))
            {
                throw new InvalidOperationException(
                    "La cadena de connexió no conté la base de dades.");
            }
        }

        /// <summary>
        /// Retorna el primer valor informat entre diverses claus
        /// equivalents d'una cadena de connexió.
        /// </summary>
        private static string ObtenirValorCadena(
            OleDbConnectionStringBuilder builder,
            params string[] claus)
        {
            foreach (string clau in claus)
            {
                if (!builder.ContainsKey(clau))
                {
                    continue;
                }

                object valor = builder[clau];

                if (valor == null)
                {
                    continue;
                }

                string text =
                    Convert.ToString(valor).Trim();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Elimina una propietat de la cadena si està present.
        /// </summary>
        private static void EliminarClauSiExisteix(
            OleDbConnectionStringBuilder builder,
            string clau)
        {
            if (builder.ContainsKey(clau))
            {
                builder.Remove(clau);
            }
        }

        /// <summary>
        /// Resultat intern d'una prova física de connexió.
        /// No s'exposa fora del servei.
        /// </summary>
        private sealed class ResultatProvaConnexio
        {
            public bool Correcte { get; private set; }

            public string BaseDades { get; private set; }

            public string Error { get; private set; }

            private ResultatProvaConnexio()
            {
                BaseDades = string.Empty;
                Error = string.Empty;
            }

            public static ResultatProvaConnexio CrearCorrecte(
                string baseDades)
            {
                return new ResultatProvaConnexio
                {
                    Correcte = true,
                    BaseDades = baseDades ?? string.Empty,
                    Error = string.Empty
                };
            }

            public static ResultatProvaConnexio CrearError(
                string error)
            {
                return new ResultatProvaConnexio
                {
                    Correcte = false,
                    BaseDades = string.Empty,
                    Error = error ?? string.Empty
                };
            }
        }
    }
}