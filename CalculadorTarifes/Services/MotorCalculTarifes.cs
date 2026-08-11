using A3ErpCalculadorTarifes.Models;
using System;

namespace A3ErpCalculadorTarifes.Services
{
    /// <summary>
    /// Aplica les set plantilles de càlcul
    /// definides pel client.
    ///
    /// Aquest servei no consulta ni modifica
    /// la base de dades.
    /// </summary>
    public sealed class MotorCalculTarifes
    {
        /// <summary>
        /// Calcula les tarifes d'un article.
        ///
        /// El resultat conserva tant el codi visible
        /// com el codi literal de la base de dades.
        /// </summary>
        public ResultatCalculTarifes Calcular(
            ArticleCalculTarifes article,
            TipusPlantillaTarifes plantilla,
            ParametresCalculTarifes parametres)
        {
            if (article == null)
            {
                throw new ArgumentNullException(
                    nameof(article));
            }

            if (parametres == null)
            {
                throw new ArgumentNullException(
                    nameof(parametres));
            }

            ResultatCalculTarifes resultat =
                new ResultatCalculTarifes
                {
                    /*
                     * Codi que es mostra a l'usuari.
                     */
                    CodiArticle =
                        article.CodiArticle,

                    /*
                     * Codi literal que utilitzarem
                     * posteriorment per persistir.
                     *
                     * No s'aplica Trim ni es reconstrueix.
                     */
                    CodiArticleBaseDades =
                        article.CodiArticleBaseDades,

                    Descripcio =
                        article.Descripcio,

                    /*
                     * Les tarifes 5 i 6 sempre es calculen
                     * de la mateixa manera.
                     */
                    Tarifa5 =
                        article.PreuCost,

                    Tarifa6 =
                        article.PreuCost
                        + article.PreuTransport,

                    GeneraDescomptes =
                        parametres.GeneraDescomptes,

                    DescompteGrup1 =
                        parametres.GeneraDescomptes
                            ? parametres.DescompteGrup1
                            : 0,

                    DescompteGrup2 =
                        parametres.GeneraDescomptes
                            ? parametres.DescompteGrup2
                            : 0,

                    DescompteGrup3 =
                        parametres.GeneraDescomptes
                            ? parametres.DescompteGrup3
                            : 0,

                    DescompteGrup4 =
                        parametres.GeneraDescomptes
                            ? parametres.DescompteGrup4
                            : 0
                };

            try
            {
                switch (plantilla)
                {
                    case TipusPlantillaTarifes
                        .CostDividitCoeficients:

                    case TipusPlantillaTarifes
                        .CostDividitCoeficientsAlternatius:

                        CalcularCostDividit(
                            article,
                            parametres,
                            resultat);

                        break;

                    case TipusPlantillaTarifes
                        .CostMesPercentatges:

                    case TipusPlantillaTarifes
                        .CostMesPercentatgesAlternatius:

                        CalcularCostMesPercentatges(
                            article,
                            parametres,
                            resultat);

                        break;

                    case TipusPlantillaTarifes
                        .PreuCompraAmbDescomptes:

                        CalcularPreuCompra(
                            article,
                            multiplicador: 1,
                            resultat);

                        break;

                    case TipusPlantillaTarifes
                        .PreuCompraDobleAmbDescomptes:

                        CalcularPreuCompra(
                            article,
                            multiplicador: 2,
                            resultat);

                        break;

                    case TipusPlantillaTarifes
                        .CostMesImportsFixos:

                        CalcularCostMesImportsFixos(
                            article,
                            parametres,
                            resultat);

                        break;

                    default:

                        return ResultatCalculTarifes.CrearError(
                            article,
                            "La plantilla seleccionada no és vàlida.");
                }

                /*
                 * Conservem quatre decimals internament.
                 * La pantalla en mostra dos.
                 */
                ArrodonirResultat(
                    resultat);

                resultat.Correcte =
                    true;

                resultat.Missatge =
                    "Càlcul realitzat correctament.";

                return resultat;
            }
            catch (Exception ex)
            {
                return ResultatCalculTarifes.CrearError(
                    article,
                    ex.Message);
            }
        }

        /// <summary>
        /// Fórmula:
        /// PRCCOSTE / coeficient.
        /// </summary>
        private static void CalcularCostDividit(
            ArticleCalculTarifes article,
            ParametresCalculTarifes parametres,
            ResultatCalculTarifes resultat)
        {
            ValidarDivisor(
                parametres.ValorTarifa1,
                "Tarifa 1");

            ValidarDivisor(
                parametres.ValorTarifa2,
                "Tarifa 2");

            ValidarDivisor(
                parametres.ValorTarifa3,
                "Tarifa 3");

            ValidarDivisor(
                parametres.ValorTarifa4,
                "Tarifa 4");

            resultat.Tarifa1 =
                article.PreuCost
                / parametres.ValorTarifa1;

            resultat.Tarifa2 =
                article.PreuCost
                / parametres.ValorTarifa2;

            resultat.Tarifa3 =
                article.PreuCost
                / parametres.ValorTarifa3;

            resultat.Tarifa4 =
                article.PreuCost
                / parametres.ValorTarifa4;
        }

        /// <summary>
        /// Fórmula:
        /// PRCCOSTE + percentatge.
        ///
        /// Exemple:
        /// cost 100 i valor 35
        /// dona un resultat de 135.
        /// </summary>
        private static void CalcularCostMesPercentatges(
            ArticleCalculTarifes article,
            ParametresCalculTarifes parametres,
            ResultatCalculTarifes resultat)
        {
            resultat.Tarifa1 =
                AplicarPercentatge(
                    article.PreuCost,
                    parametres.ValorTarifa1);

            resultat.Tarifa2 =
                AplicarPercentatge(
                    article.PreuCost,
                    parametres.ValorTarifa2);

            resultat.Tarifa3 =
                AplicarPercentatge(
                    article.PreuCost,
                    parametres.ValorTarifa3);

            resultat.Tarifa4 =
                AplicarPercentatge(
                    article.PreuCost,
                    parametres.ValorTarifa4);
        }

        /// <summary>
        /// Utilitza PRCCOMPRA com a preu
        /// de les tarifes 1–4.
        ///
        /// La plantilla 4 aplica abans
        /// un multiplicador de 2.
        /// </summary>
        private static void CalcularPreuCompra(
            ArticleCalculTarifes article,
            decimal multiplicador,
            ResultatCalculTarifes resultat)
        {
            decimal preu =
                article.PreuCompra
                * multiplicador;

            resultat.Tarifa1 =
                preu;

            resultat.Tarifa2 =
                preu;

            resultat.Tarifa3 =
                preu;

            resultat.Tarifa4 =
                preu;
        }

        /// <summary>
        /// Fórmula:
        /// PRCCOSTE + import fix.
        /// </summary>
        private static void CalcularCostMesImportsFixos(
            ArticleCalculTarifes article,
            ParametresCalculTarifes parametres,
            ResultatCalculTarifes resultat)
        {
            resultat.Tarifa1 =
                article.PreuCost
                + parametres.ValorTarifa1;

            resultat.Tarifa2 =
                article.PreuCost
                + parametres.ValorTarifa2;

            resultat.Tarifa3 =
                article.PreuCost
                + parametres.ValorTarifa3;

            resultat.Tarifa4 =
                article.PreuCost
                + parametres.ValorTarifa4;
        }

        /// <summary>
        /// Aplica un increment percentual
        /// sobre un import base.
        /// </summary>
        private static decimal AplicarPercentatge(
            decimal importBase,
            decimal percentatge)
        {
            return importBase
                + (
                    importBase
                    * percentatge
                    / 100
                );
        }

        /// <summary>
        /// Evita divisions entre zero.
        /// </summary>
        private static void ValidarDivisor(
            decimal divisor,
            string nomTarifa)
        {
            if (divisor == 0)
            {
                throw new InvalidOperationException(
                    "El coeficient de "
                    + nomTarifa
                    + " no pot ser zero.");
            }
        }

        /// <summary>
        /// Arrodoneix tots els imports calculats
        /// a quatre decimals.
        /// </summary>
        private static void ArrodonirResultat(
            ResultatCalculTarifes resultat)
        {
            resultat.Tarifa1 =
                ArrodonirImport(
                    resultat.Tarifa1);

            resultat.Tarifa2 =
                ArrodonirImport(
                    resultat.Tarifa2);

            resultat.Tarifa3 =
                ArrodonirImport(
                    resultat.Tarifa3);

            resultat.Tarifa4 =
                ArrodonirImport(
                    resultat.Tarifa4);

            resultat.Tarifa5 =
                ArrodonirImport(
                    resultat.Tarifa5);

            resultat.Tarifa6 =
                ArrodonirImport(
                    resultat.Tarifa6);
        }

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