using A3ErpGestorFormulesTarifes.Models;
using A3ErpGestorFormulesTarifes.MotorExpressions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace A3ErpGestorFormulesTarifes.Diagnostics
{
    /// <summary>
    /// Avalua totes les fórmules actives
    /// recuperades de la base de dades.
    ///
    /// Aquesta prova:
    /// - no modifica SQL;
    /// - no escriu tarifes a a3ERP;
    /// - utilitza un article de referència;
    /// - utilitza els valors inicials P1-P4
    ///   configurats a cada fórmula.
    /// </summary>
    public static class AutoprovaFormulesConfigurables
    {
        /// <summary>
        /// Executa les sis expressions de totes
        /// les fórmules actives indicades.
        ///
        /// Si alguna expressió és incorrecta,
        /// genera una excepció amb la fórmula,
        /// la tarifa i el detall de l'error.
        /// </summary>
        public static string Executar(
            IEnumerable<FormulaTarifa> formules)
        {
            if (formules == null)
            {
                throw new ArgumentNullException(
                    nameof(formules));
            }

            var resum =
                new StringBuilder();

            var motor =
                new MotorFormulaTarifesConfigurable();

            int formulesAvaluades =
                0;

            int tarifesAvaluades =
                0;

            /*
             * Article de referència utilitzat
             * també a les autoproves del motor antic.
             */
            const decimal preuCompra =
                4.50M;

            const decimal preuCost =
                3.85M;

            const decimal preuStandard =
                0.25M;

            resum.AppendLine(
                "Article de referència:");

            resum.AppendLine(
                "PRCCOMPRA = "
                + FormatarDecimal(
                    preuCompra));

            resum.AppendLine(
                "PRCCOSTE = "
                + FormatarDecimal(
                    preuCost));

            resum.AppendLine(
                "PRCSTANDARD = "
                + FormatarDecimal(
                    preuStandard));

            resum.AppendLine();

            foreach (
                FormulaTarifa formula
                in formules)
            {
                if (formula == null)
                {
                    continue;
                }

                /*
                 * Aquesta autoprova només avalua
                 * fórmules vigents i actives.
                 */
                if (!formula.Activa
                    || formula.Eliminada)
                {
                    continue;
                }

                var context =
                    new ContextAvaluacioFormula(
                        preuCompra,
                        preuCost,
                        preuStandard,
                        formula.ValorInicialP1,
                        formula.ValorInicialP2,
                        formula.ValorInicialP3,
                        formula.ValorInicialP4);

                ResultatAvaluacioFormulaTarifes resultat;

                try
                {
                    resultat =
                        motor.Avaluar(
                            formula,
                            context);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        "Ha fallat l'avaluació de la fórmula \""
                        + ObtenirIdentificacioFormula(
                            formula)
                        + "\"."
                        + "\r\n\r\n"
                        + ex.Message,
                        ex);
                }

                formulesAvaluades++;

                resum.AppendLine(
                    "OK - "
                    + formula.Ordre
                    + " · "
                    + ObtenirIdentificacioFormula(
                        formula));

                resum.AppendLine(
                    "  P1="
                    + FormatarDecimal(
                        formula.ValorInicialP1)
                    + " | P2="
                    + FormatarDecimal(
                        formula.ValorInicialP2)
                    + " | P3="
                    + FormatarDecimal(
                        formula.ValorInicialP3)
                    + " | P4="
                    + FormatarDecimal(
                        formula.ValorInicialP4));

                for (int numeroTarifa = 1;
                     numeroTarifa <= 6;
                     numeroTarifa++)
                {
                    decimal valor =
                        resultat.ObtenirTarifa(
                            numeroTarifa);

                    resum.AppendLine(
                        "  Tarifa "
                        + numeroTarifa
                        + ": "
                        + FormatarDecimal(
                            valor));

                    tarifesAvaluades++;
                }

                if (resultat.GeneraDescomptes)
                {
                    resum.AppendLine(
                        "  Descomptes: "
                        + FormatarDecimal(
                            resultat.DescompteGrup1)
                        + " | "
                        + FormatarDecimal(
                            resultat.DescompteGrup2)
                        + " | "
                        + FormatarDecimal(
                            resultat.DescompteGrup3)
                        + " | "
                        + FormatarDecimal(
                            resultat.DescompteGrup4));
                }
                else
                {
                    resum.AppendLine(
                        "  Descomptes: no genera.");
                }

                resum.AppendLine();
            }

            if (formulesAvaluades == 0)
            {
                throw new InvalidOperationException(
                    "No s'ha trobat cap fórmula activa "
                    + "per executar l'autoprova.");
            }

            resum.AppendLine(
                "Autoprova de fórmules configurables "
                + "finalitzada correctament.");

            resum.AppendLine(
                "Fórmules avaluades: "
                + formulesAvaluades
                + ".");

            resum.AppendLine(
                "Tarifes avaluades: "
                + tarifesAvaluades
                + ".");

            return resum.ToString();
        }

        /// <summary>
        /// Retorna una identificació llegible
        /// de la fórmula.
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

        /// <summary>
        /// Formata els decimals amb la cultura
        /// actual i sense zeros innecessaris.
        /// </summary>
        private static string FormatarDecimal(
            decimal valor)
        {
            return valor.ToString(
                "0.######",
                CultureInfo.CurrentCulture);
        }
    }
}