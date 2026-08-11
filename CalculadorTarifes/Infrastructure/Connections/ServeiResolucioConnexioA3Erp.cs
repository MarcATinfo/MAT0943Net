using A3ErpCalculadorTarifes.Models;
using System;
using System.Data.OleDb;

namespace A3ErpCalculadorTarifes.Infrastructure.Connections
{
    /// <summary>
    /// Valida la connexió rebuda d'a3ERP i prepara
    /// el context que utilitzarà el calculador.
    /// </summary>
    public sealed class ServeiResolucioConnexioA3Erp
    {
        public ResultatResolucioConnexio Resoldre(
            string empresaActiva,
            string cadenaConnexioRebuda)
        {
            return Resoldre(
                empresaActiva,
                cadenaConnexioRebuda,
                string.Empty);
        }

        /// <summary>
        /// Permet provar una connexió alternativa proporcionada
        /// externament sense desar credencials dins del projecte.
        /// </summary>
        public ResultatResolucioConnexio Resoldre(
            string empresaActiva,
            string cadenaConnexioRebuda,
            string cadenaConnexioAlternativa)
        {
            if (string.IsNullOrWhiteSpace(
                cadenaConnexioRebuda))
            {
                return ResultatResolucioConnexio.CrearError(
                    "a3ERP no ha proporcionat cap connexió.",
                    string.Empty);
            }

            ResultatProvaConnexio provaOriginal =
                ProvarConnexio(
                    cadenaConnexioRebuda);

            if (provaOriginal.Correcte)
            {
                return CrearResultatCorrecte(
                    empresaActiva,
                    cadenaConnexioRebuda,
                    provaOriginal.BaseDades,
                    false,
                    string.Empty);
            }

            if (string.IsNullOrWhiteSpace(
                cadenaConnexioAlternativa))
            {
                return ResultatResolucioConnexio.CrearError(
                    "No s'ha pogut obrir la connexió "
                    + "de l'empresa activa.",
                    provaOriginal.Error);
            }

            ResultatProvaConnexio provaAlternativa =
                ProvarConnexio(
                    cadenaConnexioAlternativa);

            if (!provaAlternativa.Correcte)
            {
                return ResultatResolucioConnexio.CrearError(
                    "No s'ha pogut obrir cap connexió "
                    + "amb l'empresa activa.",
                    provaOriginal.Error);
            }

            return CrearResultatCorrecte(
                empresaActiva,
                cadenaConnexioAlternativa,
                provaAlternativa.BaseDades,
                true,
                provaOriginal.Error);
        }

        private static ResultatResolucioConnexio
            CrearResultatCorrecte(
                string empresaActiva,
                string cadenaConnexio,
                string baseDades,
                bool utilitzaConnexioAlternativa,
                string errorConnexioOriginal)
        {
            var context =
                new ContextCalculadorA3Erp(
                    empresaActiva,
                    baseDades,
                    cadenaConnexio,
                    utilitzaConnexioAlternativa);

            return ResultatResolucioConnexio.CrearCorrecte(
                context,
                utilitzaConnexioAlternativa
                    ? "S'utilitza una connexió alternativa."
                    : "S'utilitza la connexió rebuda d'a3ERP.",
                errorConnexioOriginal);
        }

        private static ResultatProvaConnexio
            ProvarConnexio(
                string cadenaConnexio)
        {
            try
            {
                using (var connexio =
                    new OleDbConnection(
                        cadenaConnexio))
                {
                    connexio.Open();

                    using (OleDbCommand comanda =
                        connexio.CreateCommand())
                    {
                        comanda.CommandText =
                            "SELECT DB_NAME()";

                        string baseDades =
                            Convert.ToString(
                                comanda.ExecuteScalar())
                            ?.Trim()
                            ?? string.Empty;

                        return ResultatProvaConnexio
                            .CrearCorrecte(
                                baseDades);
                    }
                }
            }
            catch (Exception ex)
            {
                return ResultatProvaConnexio
                    .CrearError(
                        ex.Message);
            }
        }

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

            public static ResultatProvaConnexio
                CrearCorrecte(
                    string baseDades)
            {
                return new ResultatProvaConnexio
                {
                    Correcte = true,
                    BaseDades =
                        baseDades ?? string.Empty
                };
            }

            public static ResultatProvaConnexio
                CrearError(
                    string error)
            {
                return new ResultatProvaConnexio
                {
                    Correcte = false,
                    Error = error ?? string.Empty
                };
            }
        }
    }
}
