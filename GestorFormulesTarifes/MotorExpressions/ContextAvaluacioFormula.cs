using System;

namespace A3ErpGestorFormulesTarifes.MotorExpressions
{
    /// <summary>
    /// Conté els valors disponibles
    /// durant l'avaluació d'una fórmula.
    ///
    /// Només admet les variables oficials:
    /// PRCCOMPRA, PRCCOSTE, PRCSTANDARD
    /// i els paràmetres P1, P2, P3 i P4.
    /// </summary>
    public sealed class ContextAvaluacioFormula
    {
        public decimal PreuCompra
        {
            get;
        }

        public decimal PreuCost
        {
            get;
        }

        public decimal PreuStandard
        {
            get;
        }

        public decimal P1
        {
            get;
        }

        public decimal P2
        {
            get;
        }

        public decimal P3
        {
            get;
        }

        public decimal P4
        {
            get;
        }

        /// <summary>
        /// Inicialitza tots els valors que poden
        /// participar en una expressió de tarifes.
        /// </summary>
        public ContextAvaluacioFormula(
            decimal preuCompra,
            decimal preuCost,
            decimal preuStandard,
            decimal p1,
            decimal p2,
            decimal p3,
            decimal p4)
        {
            PreuCompra =
                preuCompra;

            PreuCost =
                preuCost;

            PreuStandard =
                preuStandard;

            P1 =
                p1;

            P2 =
                p2;

            P3 =
                p3;

            P4 =
                p4;
        }

        /// <summary>
        /// Retorna el valor corresponent
        /// al nom d'una variable.
        ///
        /// Els noms no distingeixen
        /// entre majúscules i minúscules.
        /// </summary>
        public decimal ObtenirValor(
            string nomVariable)
        {
            if (string.IsNullOrWhiteSpace(
                nomVariable))
            {
                throw new ExcepcioExpressioFormula(
                    "El nom de la variable és buit.");
            }

            string nomNormalitzat =
                nomVariable
                    .Trim()
                    .ToUpperInvariant();

            switch (nomNormalitzat)
            {
                case "PRCCOMPRA":
                    return PreuCompra;

                case "PRCCOSTE":
                    return PreuCost;

                case "PRCSTANDARD":
                    return PreuStandard;

                case "P1":
                    return P1;

                case "P2":
                    return P2;

                case "P3":
                    return P3;

                case "P4":
                    return P4;

                default:
                    throw new ExcepcioExpressioFormula(
                        "La variable \""
                        + nomVariable
                        + "\" no està permesa.");
            }
        }

        /// <summary>
        /// Indica si un nom correspon
        /// a una variable autoritzada.
        /// </summary>
        public static bool EsVariablePermesa(
            string nomVariable)
        {
            if (string.IsNullOrWhiteSpace(
                nomVariable))
            {
                return false;
            }

            switch (
                nomVariable
                    .Trim()
                    .ToUpperInvariant())
            {
                case "PRCCOMPRA":
                case "PRCCOSTE":
                case "PRCSTANDARD":
                case "P1":
                case "P2":
                case "P3":
                case "P4":
                    return true;

                default:
                    return false;
            }
        }
    }
}