using A3ErpCalculadorTarifes.Models;
using A3ErpCalculadorTarifes.Services;
using System;

namespace A3ErpCalculadorTarifes.Diagnostics
{
    /// <summary>
    /// Executa comprovacions automàtiques sobre les set
    /// plantilles del motor de càlcul.
    ///
    /// Aquesta classe s'utilitza únicament en desenvolupament
    /// per detectar canvis accidentals en les fórmules.
    /// </summary>
    internal static class AutoprovaMotorCalculTarifes
    {
        /// <summary>
        /// Executa totes les comprovacions.
        ///
        /// Si alguna fórmula no retorna el valor esperat,
        /// genera una excepció descriptiva i no obre el formulari.
        /// </summary>
        public static void Executar()
        {
            ArticleCalculTarifes article =
                CrearArticleProva();

            MotorCalculTarifes motor =
                new MotorCalculTarifes();

            CatalegPlantillesTarifes cataleg =
                new CatalegPlantillesTarifes();

            ComprovarPlantilla1(
                motor,
                cataleg,
                article);

            ComprovarPlantilla2(
                motor,
                cataleg,
                article);

            ComprovarPlantilla3(
                motor,
                cataleg,
                article);

            ComprovarPlantilla4(
                motor,
                cataleg,
                article);

            ComprovarPlantilla5(
                motor,
                cataleg,
                article);

            ComprovarPlantilla6(
                motor,
                cataleg,
                article);

            ComprovarPlantilla7(
                motor,
                cataleg,
                article);

            ComprovarDivisioEntreZero(
                motor,
                article);
        }

        /// <summary>
        /// Utilitza valors semblants als de l'article 16
        /// creat durant les proves de l'importador.
        /// </summary>
        private static ArticleCalculTarifes
            CrearArticleProva()
        {
            return new ArticleCalculTarifes
            {
                Seleccionat =
                    true,

                CodiArticle =
                    "16",

                Descripcio =
                    "Article de prova",

                Familia =
                    "1",

                PreuCompra =
                    4.50m,

                PreuCost =
                    3.85m,

                PreuTransport =
                    0.25m
            };
        }

        private static void ComprovarPlantilla1(
            MotorCalculTarifes motor,
            CatalegPlantillesTarifes cataleg,
            ArticleCalculTarifes article)
        {
            ResultatCalculTarifes resultat =
                Calcular(
                    motor,
                    cataleg,
                    article,
                    TipusPlantillaTarifes
                        .CostDividitCoeficients);

            ComprovarResultatBase(
                resultat,
                "Plantilla 1");

            ComprovarDecimal(
                "Plantilla 1 - Tarifa 1",
                5.9231m,
                resultat.Tarifa1);

            ComprovarDecimal(
                "Plantilla 1 - Tarifa 2",
                5.5000m,
                resultat.Tarifa2);

            ComprovarDecimal(
                "Plantilla 1 - Tarifa 3",
                5.1333m,
                resultat.Tarifa3);

            ComprovarDecimal(
                "Plantilla 1 - Tarifa 4",
                4.8125m,
                resultat.Tarifa4);

            ComprovarTarifesFixes(
                resultat,
                "Plantilla 1");

            ComprovarBoolea(
                "Plantilla 1 - Genera descomptes",
                false,
                resultat.GeneraDescomptes);
        }

        private static void ComprovarPlantilla2(
            MotorCalculTarifes motor,
            CatalegPlantillesTarifes cataleg,
            ArticleCalculTarifes article)
        {
            ResultatCalculTarifes resultat =
                Calcular(
                    motor,
                    cataleg,
                    article,
                    TipusPlantillaTarifes
                        .CostMesPercentatges);

            ComprovarResultatBase(
                resultat,
                "Plantilla 2");

            ComprovarDecimal(
                "Plantilla 2 - Tarifa 1",
                5.1975m,
                resultat.Tarifa1);

            ComprovarDecimal(
                "Plantilla 2 - Tarifa 2",
                5.0050m,
                resultat.Tarifa2);

            ComprovarDecimal(
                "Plantilla 2 - Tarifa 3",
                4.8125m,
                resultat.Tarifa3);

            ComprovarDecimal(
                "Plantilla 2 - Tarifa 4",
                4.6200m,
                resultat.Tarifa4);

            ComprovarTarifesFixes(
                resultat,
                "Plantilla 2");
        }

        private static void ComprovarPlantilla3(
            MotorCalculTarifes motor,
            CatalegPlantillesTarifes cataleg,
            ArticleCalculTarifes article)
        {
            ResultatCalculTarifes resultat =
                Calcular(
                    motor,
                    cataleg,
                    article,
                    TipusPlantillaTarifes
                        .PreuCompraAmbDescomptes);

            ComprovarResultatBase(
                resultat,
                "Plantilla 3");

            ComprovarQuatreTarifesIguals(
                resultat,
                4.5000m,
                "Plantilla 3");

            ComprovarDescomptes(
                resultat,
                "Plantilla 3");

            ComprovarTarifesFixes(
                resultat,
                "Plantilla 3");
        }

        private static void ComprovarPlantilla4(
            MotorCalculTarifes motor,
            CatalegPlantillesTarifes cataleg,
            ArticleCalculTarifes article)
        {
            ResultatCalculTarifes resultat =
                Calcular(
                    motor,
                    cataleg,
                    article,
                    TipusPlantillaTarifes
                        .PreuCompraDobleAmbDescomptes);

            ComprovarResultatBase(
                resultat,
                "Plantilla 4");

            ComprovarQuatreTarifesIguals(
                resultat,
                9.0000m,
                "Plantilla 4");

            ComprovarDescomptes(
                resultat,
                "Plantilla 4");

            ComprovarTarifesFixes(
                resultat,
                "Plantilla 4");
        }

        private static void ComprovarPlantilla5(
            MotorCalculTarifes motor,
            CatalegPlantillesTarifes cataleg,
            ArticleCalculTarifes article)
        {
            ResultatCalculTarifes resultat =
                Calcular(
                    motor,
                    cataleg,
                    article,
                    TipusPlantillaTarifes
                        .CostDividitCoeficientsAlternatius);

            ComprovarResultatBase(
                resultat,
                "Plantilla 5");

            ComprovarDecimal(
                "Plantilla 5 - Tarifa 1",
                7.0000m,
                resultat.Tarifa1);

            ComprovarDecimal(
                "Plantilla 5 - Tarifa 2",
                6.4167m,
                resultat.Tarifa2);

            ComprovarDecimal(
                "Plantilla 5 - Tarifa 3",
                5.9231m,
                resultat.Tarifa3);

            ComprovarDecimal(
                "Plantilla 5 - Tarifa 4",
                5.5000m,
                resultat.Tarifa4);

            ComprovarTarifesFixes(
                resultat,
                "Plantilla 5");
        }

        private static void ComprovarPlantilla6(
            MotorCalculTarifes motor,
            CatalegPlantillesTarifes cataleg,
            ArticleCalculTarifes article)
        {
            ResultatCalculTarifes resultat =
                Calcular(
                    motor,
                    cataleg,
                    article,
                    TipusPlantillaTarifes
                        .CostMesImportsFixos);

            ComprovarResultatBase(
                resultat,
                "Plantilla 6");

            ComprovarDecimal(
                "Plantilla 6 - Tarifa 1",
                13.8500m,
                resultat.Tarifa1);

            ComprovarDecimal(
                "Plantilla 6 - Tarifa 2",
                12.8500m,
                resultat.Tarifa2);

            ComprovarDecimal(
                "Plantilla 6 - Tarifa 3",
                10.8500m,
                resultat.Tarifa3);

            ComprovarDecimal(
                "Plantilla 6 - Tarifa 4",
                9.8500m,
                resultat.Tarifa4);

            ComprovarTarifesFixes(
                resultat,
                "Plantilla 6");
        }

        private static void ComprovarPlantilla7(
            MotorCalculTarifes motor,
            CatalegPlantillesTarifes cataleg,
            ArticleCalculTarifes article)
        {
            ResultatCalculTarifes resultat =
                Calcular(
                    motor,
                    cataleg,
                    article,
                    TipusPlantillaTarifes
                        .CostMesPercentatgesAlternatius);

            ComprovarResultatBase(
                resultat,
                "Plantilla 7");

            ComprovarDecimal(
                "Plantilla 7 - Tarifa 1",
                5.3900m,
                resultat.Tarifa1);

            ComprovarDecimal(
                "Plantilla 7 - Tarifa 2",
                5.1975m,
                resultat.Tarifa2);

            ComprovarDecimal(
                "Plantilla 7 - Tarifa 3",
                5.0050m,
                resultat.Tarifa3);

            ComprovarDecimal(
                "Plantilla 7 - Tarifa 4",
                4.8125m,
                resultat.Tarifa4);

            ComprovarTarifesFixes(
                resultat,
                "Plantilla 7");
        }

        /// <summary>
        /// Confirma que el motor rebutja correctament
        /// un divisor amb valor zero.
        /// </summary>
        private static void ComprovarDivisioEntreZero(
            MotorCalculTarifes motor,
            ArticleCalculTarifes article)
        {
            ParametresCalculTarifes parametres =
                new ParametresCalculTarifes
                {
                    ValorTarifa1 =
                        0,

                    ValorTarifa2 =
                        0.70m,

                    ValorTarifa3 =
                        0.75m,

                    ValorTarifa4 =
                        0.80m
                };

            ResultatCalculTarifes resultat =
                motor.Calcular(
                    article,
                    TipusPlantillaTarifes
                        .CostDividitCoeficients,
                    parametres);

            if (resultat.Correcte)
            {
                throw new InvalidOperationException(
                    "La prova de divisió entre zero ha fallat: "
                    + "el motor ha retornat un resultat correcte.");
            }

            if (string.IsNullOrWhiteSpace(
                resultat.Missatge))
            {
                throw new InvalidOperationException(
                    "La prova de divisió entre zero ha fallat: "
                    + "no s'ha retornat cap missatge.");
            }
        }

        private static ResultatCalculTarifes Calcular(
            MotorCalculTarifes motor,
            CatalegPlantillesTarifes cataleg,
            ArticleCalculTarifes article,
            TipusPlantillaTarifes plantilla)
        {
            ParametresCalculTarifes parametres =
                cataleg.CrearParametresPredeterminats(
                    plantilla);

            return motor.Calcular(
                article,
                plantilla,
                parametres);
        }

        private static void ComprovarResultatBase(
            ResultatCalculTarifes resultat,
            string nomProva)
        {
            if (resultat == null)
            {
                throw new InvalidOperationException(
                    nomProva
                    + ": el motor ha retornat null.");
            }

            if (!resultat.Correcte)
            {
                throw new InvalidOperationException(
                    nomProva
                    + ": el càlcul ha fallat. "
                    + resultat.Missatge);
            }

            if (!string.Equals(
                resultat.CodiArticle,
                "16",
                StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    nomProva
                    + ": el codi d'article retornat no és correcte.");
            }
        }

        private static void ComprovarTarifesFixes(
            ResultatCalculTarifes resultat,
            string nomProva)
        {
            ComprovarDecimal(
                nomProva + " - Tarifa 5",
                3.8500m,
                resultat.Tarifa5);

            ComprovarDecimal(
                nomProva + " - Tarifa 6",
                4.1000m,
                resultat.Tarifa6);
        }

        private static void ComprovarQuatreTarifesIguals(
            ResultatCalculTarifes resultat,
            decimal valorEsperat,
            string nomProva)
        {
            ComprovarDecimal(
                nomProva + " - Tarifa 1",
                valorEsperat,
                resultat.Tarifa1);

            ComprovarDecimal(
                nomProva + " - Tarifa 2",
                valorEsperat,
                resultat.Tarifa2);

            ComprovarDecimal(
                nomProva + " - Tarifa 3",
                valorEsperat,
                resultat.Tarifa3);

            ComprovarDecimal(
                nomProva + " - Tarifa 4",
                valorEsperat,
                resultat.Tarifa4);
        }

        private static void ComprovarDescomptes(
            ResultatCalculTarifes resultat,
            string nomProva)
        {
            ComprovarBoolea(
                nomProva + " - Genera descomptes",
                true,
                resultat.GeneraDescomptes);

            ComprovarDecimal(
                nomProva + " - Descompte 1",
                10m,
                resultat.DescompteGrup1);

            ComprovarDecimal(
                nomProva + " - Descompte 2",
                15m,
                resultat.DescompteGrup2);

            ComprovarDecimal(
                nomProva + " - Descompte 3",
                20m,
                resultat.DescompteGrup3);

            ComprovarDecimal(
                nomProva + " - Descompte 4",
                25m,
                resultat.DescompteGrup4);
        }

        private static void ComprovarDecimal(
            string nomProva,
            decimal valorEsperat,
            decimal valorReal)
        {
            if (valorEsperat == valorReal)
            {
                return;
            }

            throw new InvalidOperationException(
                nomProva
                + ": s'esperava "
                + valorEsperat
                + " però el motor ha retornat "
                + valorReal
                + ".");
        }

        private static void ComprovarBoolea(
            string nomProva,
            bool valorEsperat,
            bool valorReal)
        {
            if (valorEsperat == valorReal)
            {
                return;
            }

            throw new InvalidOperationException(
                nomProva
                + ": el valor booleà retornat no és correcte.");
        }
    }
}