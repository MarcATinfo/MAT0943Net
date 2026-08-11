namespace A3ErpCalculadorTarifes.Models
{
    /// <summary>
    /// Resultat d'una aplicació real
    /// de tarifes i descomptes.
    ///
    /// Quan el resultat és correcte,
    /// tots els canvis han estat confirmats
    /// mitjançant COMMIT.
    /// </summary>
    public sealed class ResultatAplicacioTarifes
    {
        public bool Correcte { get; private set; }

        public int ArticlesProcessats { get; private set; }

        public int TarifesProcessades { get; private set; }

        public int DescomptesProcessats { get; private set; }

        public int DescomptesEliminats { get; private set; }

        public string Missatge { get; private set; }

        private ResultatAplicacioTarifes()
        {
            Missatge =
                string.Empty;
        }

        public static ResultatAplicacioTarifes CrearCorrecte(
            int articlesProcessats,
            int tarifesProcessades,
            int descomptesProcessats,
            int descomptesEliminats)
        {
            return new ResultatAplicacioTarifes
            {
                Correcte =
                    true,

                ArticlesProcessats =
                    articlesProcessats,

                TarifesProcessades =
                    tarifesProcessades,

                DescomptesProcessats =
                    descomptesProcessats,

                DescomptesEliminats =
                    descomptesEliminats,

                Missatge =
                    CrearMissatgeCorrecte(
                        descomptesProcessats,
                        descomptesEliminats)
            };
        }

        public static ResultatAplicacioTarifes CrearError(
            string missatge)
        {
            return new ResultatAplicacioTarifes
            {
                Correcte =
                    false,

                ArticlesProcessats =
                    0,

                TarifesProcessades =
                    0,

                DescomptesProcessats =
                    0,

                DescomptesEliminats =
                    0,

                Missatge =
                    string.IsNullOrWhiteSpace(
                        missatge)
                        ? "S'ha produït un error aplicant "
                          + "les tarifes i els descomptes."
                        : missatge
            };
        }

        private static string CrearMissatgeCorrecte(
            int descomptesProcessats,
            int descomptesEliminats)
        {
            if (descomptesProcessats > 0)
            {
                return "Les tarifes i els descomptes "
                       + "s'han aplicat correctament.";
            }

            if (descomptesEliminats > 0)
            {
                return "Les tarifes s'han aplicat i "
                       + "els descomptes gestionats "
                       + "s'han eliminat correctament.";
            }

            return "Les tarifes s'han aplicat correctament.";
        }
    }
}
