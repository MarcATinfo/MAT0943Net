using System;

namespace A3ErpCalculadorTarifes.Models
{
    /// <summary>
    /// Resultat de validar una connexió amb
    /// la base de dades activa d'a3ERP.
    /// </summary>
    public sealed class ResultatResolucioConnexio
    {
        public bool Correcte { get; private set; }

        public ContextCalculadorA3Erp Context
        {
            get;
            private set;
        }

        public string Missatge { get; private set; }

        public string ErrorConnexioOriginal
        {
            get;
            private set;
        }

        private ResultatResolucioConnexio()
        {
            Missatge = string.Empty;
            ErrorConnexioOriginal = string.Empty;
        }

        public static ResultatResolucioConnexio
            CrearCorrecte(
                ContextCalculadorA3Erp context,
                string missatge,
                string errorConnexioOriginal)
        {
            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            return new ResultatResolucioConnexio
            {
                Correcte = true,
                Context = context,
                Missatge = missatge ?? string.Empty,
                ErrorConnexioOriginal =
                    errorConnexioOriginal ?? string.Empty
            };
        }

        public static ResultatResolucioConnexio
            CrearError(
                string missatge,
                string errorConnexioOriginal)
        {
            return new ResultatResolucioConnexio
            {
                Correcte = false,
                Context = null,
                Missatge = missatge ?? string.Empty,
                ErrorConnexioOriginal =
                    errorConnexioOriginal ?? string.Empty
            };
        }
    }
}
