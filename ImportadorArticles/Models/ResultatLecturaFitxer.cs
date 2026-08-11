using System.Data;

namespace A3ErpImportadorArticles.Models
{
    /// <summary>
    /// Conté el resultat de la lectura d'un fitxer Excel o CSV.
    /// Permet retornar les dades i qualsevol incidència sense
    /// barrejar la lectura física amb la interfície gràfica.
    /// </summary>
    public sealed class ResultatLecturaFitxer
    {
        public bool Correcte { get; private set; }

        public string Missatge { get; private set; }

        public string RutaFitxer { get; private set; }

        public string NomFull { get; private set; }

        public DataTable Dades { get; private set; }

        public int TotalRegistres
        {
            get
            {
                return Dades == null
                    ? 0
                    : Dades.Rows.Count;
            }
        }

        private ResultatLecturaFitxer()
        {
            Missatge = string.Empty;
            RutaFitxer = string.Empty;
            NomFull = string.Empty;
        }

        /// <summary>
        /// Genera un resultat correcte amb les dades llegides.
        /// </summary>
        public static ResultatLecturaFitxer CrearCorrecte(
            string rutaFitxer,
            string nomFull,
            DataTable dades)
        {
            return new ResultatLecturaFitxer
            {
                Correcte = true,
                RutaFitxer = rutaFitxer,
                NomFull = nomFull,
                Dades = dades,
                Missatge = string.Empty
            };
        }

        /// <summary>
        /// Genera un resultat incorrecte sense llançar l'error
        /// directament cap al formulari.
        /// </summary>
        public static ResultatLecturaFitxer CrearError(
            string rutaFitxer,
            string missatge)
        {
            return new ResultatLecturaFitxer
            {
                Correcte = false,
                RutaFitxer = rutaFitxer,
                NomFull = string.Empty,
                Dades = null,
                Missatge = missatge
            };
        }
    }
}