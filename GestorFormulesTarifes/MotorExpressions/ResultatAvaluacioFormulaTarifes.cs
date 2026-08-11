namespace A3ErpGestorFormulesTarifes.MotorExpressions
{
    /// <summary>
    /// Conté el resultat d'avaluar
    /// les sis tarifes d'una fórmula.
    ///
    /// També conserva la configuració
    /// de descomptes associada a la fórmula.
    /// </summary>
    public sealed class ResultatAvaluacioFormulaTarifes
    {
        public int IdFormula
        {
            get;
        }

        public string CodiFormula
        {
            get;
        }

        public string NomFormula
        {
            get;
        }

        public decimal Tarifa1
        {
            get;
        }

        public decimal Tarifa2
        {
            get;
        }

        public decimal Tarifa3
        {
            get;
        }

        public decimal Tarifa4
        {
            get;
        }

        public decimal Tarifa5
        {
            get;
        }

        public decimal Tarifa6
        {
            get;
        }

        public bool GeneraDescomptes
        {
            get;
        }

        public decimal DescompteGrup1
        {
            get;
        }

        public decimal DescompteGrup2
        {
            get;
        }

        public decimal DescompteGrup3
        {
            get;
        }

        public decimal DescompteGrup4
        {
            get;
        }

        /// <summary>
        /// Inicialitza el resultat complet
        /// d'una fórmula de tarifes.
        /// </summary>
        public ResultatAvaluacioFormulaTarifes(
            int idFormula,
            string codiFormula,
            string nomFormula,
            decimal tarifa1,
            decimal tarifa2,
            decimal tarifa3,
            decimal tarifa4,
            decimal tarifa5,
            decimal tarifa6,
            bool generaDescomptes,
            decimal descompteGrup1,
            decimal descompteGrup2,
            decimal descompteGrup3,
            decimal descompteGrup4)
        {
            IdFormula =
                idFormula;

            CodiFormula =
                codiFormula ?? string.Empty;

            NomFormula =
                nomFormula ?? string.Empty;

            Tarifa1 =
                tarifa1;

            Tarifa2 =
                tarifa2;

            Tarifa3 =
                tarifa3;

            Tarifa4 =
                tarifa4;

            Tarifa5 =
                tarifa5;

            Tarifa6 =
                tarifa6;

            GeneraDescomptes =
                generaDescomptes;

            DescompteGrup1 =
                descompteGrup1;

            DescompteGrup2 =
                descompteGrup2;

            DescompteGrup3 =
                descompteGrup3;

            DescompteGrup4 =
                descompteGrup4;
        }

        /// <summary>
        /// Retorna el resultat d'una tarifa
        /// segons el seu número d'1 a 6.
        /// </summary>
        public decimal ObtenirTarifa(
            int numeroTarifa)
        {
            switch (numeroTarifa)
            {
                case 1:
                    return Tarifa1;

                case 2:
                    return Tarifa2;

                case 3:
                    return Tarifa3;

                case 4:
                    return Tarifa4;

                case 5:
                    return Tarifa5;

                case 6:
                    return Tarifa6;

                default:
                    throw new System.ArgumentOutOfRangeException(
                        nameof(numeroTarifa),
                        numeroTarifa,
                        "El número de tarifa ha d'estar entre 1 i 6.");
            }
        }
    }
}