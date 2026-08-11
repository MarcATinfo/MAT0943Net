namespace A3ErpCalculadorTarifes.Models
{
    /// <summary>
    /// Resultat d'una simulació d'aplicació
    /// de tarifes i descomptes.
    ///
    /// Totes les operacions es desfan
    /// mitjançant ROLLBACK.
    /// </summary>
    public sealed class ResultatSimulacioAplicacioTarifes
    {
        public bool Correcte { get; private set; }

        public int ArticlesProcessats { get; private set; }

        public int TarifesProcessades { get; private set; }

        public int DescomptesProcessats { get; private set; }

        public int DescomptesEliminats { get; private set; }

        public string Missatge { get; private set; }

        private ResultatSimulacioAplicacioTarifes()
        {
            Missatge =
                string.Empty;
        }

        public static ResultatSimulacioAplicacioTarifes
            CrearCorrecte(
                int articlesProcessats,
                int tarifesProcessades,
                int descomptesProcessats,
                int descomptesEliminats)
        {
            return new ResultatSimulacioAplicacioTarifes
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

        public static ResultatSimulacioAplicacioTarifes
            CrearError(
                string missatge)
        {
            return new ResultatSimulacioAplicacioTarifes
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
                        ? "S'ha produït un error durant la simulació."
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
