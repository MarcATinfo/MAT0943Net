using A3ErpCalculadorTarifes.Models;
using A3ErpGestorFormulesTarifes.Models;
using A3ErpGestorFormulesTarifes.MotorExpressions;
using System;

namespace A3ErpCalculadorTarifes.Services
{
    /// <summary>
    /// Adapta el motor d'expressions configurable
    /// al model de resultats que ja utilitza
    /// el Calculador de tarifes.
    ///
    /// Això permet conservar sense canvis:
    /// - la previsualització;
    /// - la simulació transaccional;
    /// - l'aplicació definitiva;
    /// - la persistència de tarifes i descomptes.
    /// </summary>
    public sealed class MotorCalculTarifesConfigurable
    {
        private readonly MotorFormulaTarifesConfigurable
            _motorExpressions;

        /// <summary>
        /// Inicialitza el motor configurable.
        /// </summary>
        public MotorCalculTarifesConfigurable()
        {
            _motorExpressions =
                new MotorFormulaTarifesConfigurable();
        }

        /// <summary>
        /// Calcula les sis tarifes d'un article
        /// utilitzant la fórmula seleccionada
        /// i els valors actuals de la pantalla.
        /// </summary>
        public ResultatCalculTarifes Calcular(
            ArticleCalculTarifes article,
            FormulaTarifa formula,
            ParametresCalculTarifes parametres)
        {
            if (article == null)
            {
                throw new ArgumentNullException(
                    nameof(article));
            }

            if (formula == null)
            {
                throw new ArgumentNullException(
                    nameof(formula));
            }

            if (parametres == null)
            {
                throw new ArgumentNullException(
                    nameof(parametres));
            }

            try
            {
                /*
                 * La propietat antiga PreuTransport
                 * conté realment ARTICULO.PRCSTANDARD.
                 */
                var context =
                    new ContextAvaluacioFormula(
                        preuCompra:
                            article.PreuCompra,

                        preuCost:
                            article.PreuCost,

                        preuStandard:
                            article.PreuTransport,

                        p1:
                            parametres.ValorTarifa1,

                        p2:
                            parametres.ValorTarifa2,

                        p3:
                            parametres.ValorTarifa3,

                        p4:
                            parametres.ValorTarifa4);

                ResultatAvaluacioFormulaTarifes
                    resultatConfigurable =
                        _motorExpressions.Avaluar(
                            formula,
                            context);

                /*
                 * Convertim el resultat del motor nou
                 * al model que ja consumeixen
                 * la graella i la persistència actuals.
                 */
                return new ResultatCalculTarifes
                {
                    CodiArticle =
                        article.CodiArticle,

                    /*
                     * Conservem el CODART literal
                     * sense aplicar Trim ni reconstruir-lo.
                     */
                    CodiArticleBaseDades =
                        article.CodiArticleBaseDades,

                    Descripcio =
                        article.Descripcio,

                    Tarifa1 =
                        ArrodonirImport(
                            resultatConfigurable.Tarifa1),

                    Tarifa2 =
                        ArrodonirImport(
                            resultatConfigurable.Tarifa2),

                    Tarifa3 =
                        ArrodonirImport(
                            resultatConfigurable.Tarifa3),

                    Tarifa4 =
                        ArrodonirImport(
                            resultatConfigurable.Tarifa4),

                    Tarifa5 =
                        ArrodonirImport(
                            resultatConfigurable.Tarifa5),

                    Tarifa6 =
                        ArrodonirImport(
                            resultatConfigurable.Tarifa6),

                    GeneraDescomptes =
                        formula.GeneraDescomptes,

                    DescompteGrup1 =
                        formula.GeneraDescomptes
                            ? parametres.DescompteGrup1
                            : 0M,

                    DescompteGrup2 =
                        formula.GeneraDescomptes
                            ? parametres.DescompteGrup2
                            : 0M,

                    DescompteGrup3 =
                        formula.GeneraDescomptes
                            ? parametres.DescompteGrup3
                            : 0M,

                    DescompteGrup4 =
                        formula.GeneraDescomptes
                            ? parametres.DescompteGrup4
                            : 0M,

                    Correcte =
                        true,

                    Missatge =
                        "Càlcul realitzat correctament."
                };
            }
            catch (Exception ex)
            {
                /*
                 * Conservem el mateix comportament
                 * funcional del motor antic:
                 * l'error queda associat a l'article
                 * i apareix a la previsualització.
                 */
                return ResultatCalculTarifes.CrearError(
                    article,
                    ex.Message);
            }
        }

        /// <summary>
        /// Conserva quatre decimals internament,
        /// igual que el motor anterior.
        /// </summary>
        private static decimal ArrodonirImport(
            decimal valor)
        {
            return Math.Round(
                valor,
                4,
                MidpointRounding.AwayFromZero);
        }
    }
}