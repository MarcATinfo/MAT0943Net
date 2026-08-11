using A3ErpGestorFormulesTarifes.Models;
using System;

namespace A3ErpGestorFormulesTarifes.MotorExpressions
{
    /// <summary>
    /// Avalua les sis expressions
    /// d'una fórmula configurable.
    ///
    /// Aquesta classe no consulta SQL
    /// ni modifica dades d'a3ERP.
    /// </summary>
    public sealed class MotorFormulaTarifesConfigurable
    {
        /// <summary>
        /// Avalua totes les tarifes d'una fórmula
        /// utilitzant els valors del context indicat.
        /// </summary>
        public ResultatAvaluacioFormulaTarifes Avaluar(
            FormulaTarifa formula,
            ContextAvaluacioFormula context)
        {
            if (formula == null)
            {
                throw new ArgumentNullException(
                    nameof(formula));
            }

            if (context == null)
            {
                throw new ArgumentNullException(
                    nameof(context));
            }

            if (formula.Eliminada)
            {
                throw new InvalidOperationException(
                    "No es pot avaluar una fórmula eliminada.");
            }

            /*
             * Creem un avaluador per operació.
             * D'aquesta manera el càlcul queda
             * aïllat i no comparteix estat intern.
             */
            var avaluador =
                new AvaluadorExpressioFormula();

            decimal tarifa1 =
                AvaluarTarifa(
                    avaluador,
                    context,
                    formula,
                    1,
                    formula.ExpressioTarifa1);

            decimal tarifa2 =
                AvaluarTarifa(
                    avaluador,
                    context,
                    formula,
                    2,
                    formula.ExpressioTarifa2);

            decimal tarifa3 =
                AvaluarTarifa(
                    avaluador,
                    context,
                    formula,
                    3,
                    formula.ExpressioTarifa3);

            decimal tarifa4 =
                AvaluarTarifa(
                    avaluador,
                    context,
                    formula,
                    4,
                    formula.ExpressioTarifa4);

            decimal tarifa5 =
                AvaluarTarifa(
                    avaluador,
                    context,
                    formula,
                    5,
                    formula.ExpressioTarifa5);

            decimal tarifa6 =
                AvaluarTarifa(
                    avaluador,
                    context,
                    formula,
                    6,
                    formula.ExpressioTarifa6);

            return new ResultatAvaluacioFormulaTarifes(
                formula.Id,
                formula.Codi,
                formula.Nom,
                tarifa1,
                tarifa2,
                tarifa3,
                tarifa4,
                tarifa5,
                tarifa6,
                formula.GeneraDescomptes,
                formula.DescompteInicialGrup1,
                formula.DescompteInicialGrup2,
                formula.DescompteInicialGrup3,
                formula.DescompteInicialGrup4);
        }

        /// <summary>
        /// Avalua una tarifa individual
        /// i afegeix informació funcional
        /// quan l'expressió conté un error.
        /// </summary>
        private static decimal AvaluarTarifa(
            AvaluadorExpressioFormula avaluador,
            ContextAvaluacioFormula context,
            FormulaTarifa formula,
            int numeroTarifa,
            string expressio)
        {
            try
            {
                return avaluador.Avaluar(
                    expressio,
                    context);
            }
            catch (ExcepcioExpressioFormula ex)
            {
                throw new ExcepcioExpressioFormula(
                    "Error a la tarifa "
                    + numeroTarifa
                    + " de la fórmula \""
                    + ObtenirIdentificacioFormula(
                        formula)
                    + "\"."
                    + "\r\nExpressió: "
                    + (
                        expressio
                        ?? string.Empty
                    )
                    + "\r\nDetall: "
                    + ex.Message,
                    ex);
            }
        }

        /// <summary>
        /// Retorna una identificació llegible
        /// de la fórmula per als missatges d'error.
        /// </summary>
        private static string ObtenirIdentificacioFormula(
            FormulaTarifa formula)
        {
            if (!string.IsNullOrWhiteSpace(
                formula.Codi))
            {
                return formula.Codi.Trim();
            }

            if (!string.IsNullOrWhiteSpace(
                formula.Nom))
            {
                return formula.Nom.Trim();
            }

            return "ID "
                + formula.Id;
        }
    }
}