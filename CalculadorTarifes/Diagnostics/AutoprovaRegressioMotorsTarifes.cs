using A3ErpCalculadorTarifes.Models;
using A3ErpCalculadorTarifes.Services;
using A3ErpGestorFormulesTarifes.Dades;
using A3ErpGestorFormulesTarifes.Models;
using A3ErpGestorFormulesTarifes.MotorExpressions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace A3ErpCalculadorTarifes.Diagnostics
{
    /// <summary>
    /// Compara els resultats del motor antic
    /// amb els del motor configurable.
    ///
    /// La prova utilitza:
    /// - les set plantilles codificades antigues;
    /// - les set fórmules actives de SQL;
    /// - el mateix article i els mateixos paràmetres.
    ///
    /// No modifica cap dada d'a3ERP.
    /// </summary>
    internal static class AutoprovaRegressioMotorsTarifes
    {
        /// <summary>
        /// Executa la regressió completa
        /// i retorna un resum textual.
        /// </summary>
        public static string Executar(
            string cadenaConnexio)
        {
            if (string.IsNullOrWhiteSpace(
                cadenaConnexio))
            {
                throw new ArgumentException(
                    "La connexió amb a3ERP és buida.",
                    nameof(cadenaConnexio));
            }

            var repositori =
                new RepositoriFormulesTarifes();

            List<FormulaTarifa> formulesActives =
                repositori.ObtenirActives(
                    cadenaConnexio);

            IReadOnlyList<CorrespondenciaFormula>
                correspondencies =
                    CrearCorrespondencies();

            /*
             * En aquesta fase de regressió han d'existir
             * únicament les set fórmules originals actives.
             */
            if (formulesActives.Count
                != correspondencies.Count)
            {
                string codisCarregats =
                    string.Join(
                        ", ",
                        formulesActives
                            .Select(
                                formula =>
                                    formula.Codi));

                throw new InvalidOperationException(
                    "La regressió necessita exactament "
                    + correspondencies.Count
                    + " fórmules actives."
                    + "\r\nFórmules trobades: "
                    + formulesActives.Count
                    + "."
                    + "\r\nCodis carregats: "
                    + codisCarregats);
            }

            var motorAntic =
                new MotorCalculTarifes();

            var catalegAntic =
                new CatalegPlantillesTarifes();

            var motorConfigurable =
                new MotorFormulaTarifesConfigurable();

            ArticleCalculTarifes article =
                CrearArticleReferencia();

            var resum =
                new StringBuilder();

            int formulesComparades =
                0;

            int tarifesComparades =
                0;

            int parametresComparats =
                0;

            int descomptesComparats =
                0;

            resum.AppendLine(
                "Regressió motor antic vs. motor configurable");

            resum.AppendLine(
                "============================================");

            resum.AppendLine();

            resum.AppendLine(
                "Article de referència:");

            resum.AppendLine(
                "PRCCOMPRA = "
                + FormatarDecimal(
                    article.PreuCompra));

            resum.AppendLine(
                "PRCCOSTE = "
                + FormatarDecimal(
                    article.PreuCost));

            resum.AppendLine(
                "PRCSTANDARD = "
                + FormatarDecimal(
                    article.PreuTransport));

            resum.AppendLine();

            foreach (
                CorrespondenciaFormula correspondencia
                in correspondencies)
            {
                FormulaTarifa formula =
                    ObtenirFormulaPerCodi(
                        formulesActives,
                        correspondencia.Codi);

                DefinicioPlantillaTarifes definicioAntiga =
                    ObtenirDefinicioAntiga(
                        catalegAntic,
                        correspondencia.TipusPlantilla);

                ParametresCalculTarifes parametresAntics =
                    catalegAntic
                        .CrearParametresPredeterminats(
                            correspondencia.TipusPlantilla);

                /*
                 * Primer confirmem que la configuració
                 * inicial de SQL és equivalent al catàleg antic.
                 */
                ComprovarEnter(
                    correspondencia.Codi
                    + " - Ordre",
                    correspondencia.Ordre,
                    formula.Ordre);

                ComprovarText(
                    correspondencia.Codi
                    + " - Tipus de valors",
                    definicioAntiga.TipusValors,
                    formula.TipusValors);

                ComprovarBoolea(
                    correspondencia.Codi
                    + " - Utilitza valors",
                    definicioAntiga
                        .UtilitzaValorsTarifes,
                    formula
                        .UtilitzaValorsTarifes);

                ComprovarBoolea(
                    correspondencia.Codi
                    + " - Genera descomptes",
                    definicioAntiga
                        .GeneraDescomptes,
                    formula
                        .GeneraDescomptes);

                ComprovarDecimalExacte(
                    correspondencia.Codi
                    + " - P1",
                    parametresAntics.ValorTarifa1,
                    formula.ValorInicialP1);

                ComprovarDecimalExacte(
                    correspondencia.Codi
                    + " - P2",
                    parametresAntics.ValorTarifa2,
                    formula.ValorInicialP2);

                ComprovarDecimalExacte(
                    correspondencia.Codi
                    + " - P3",
                    parametresAntics.ValorTarifa3,
                    formula.ValorInicialP3);

                ComprovarDecimalExacte(
                    correspondencia.Codi
                    + " - P4",
                    parametresAntics.ValorTarifa4,
                    formula.ValorInicialP4);

                parametresComparats +=
                    4;

                ComprovarDecimalExacte(
                    correspondencia.Codi
                    + " - Descompte 1",
                    parametresAntics.DescompteGrup1,
                    formula.DescompteInicialGrup1);

                ComprovarDecimalExacte(
                    correspondencia.Codi
                    + " - Descompte 2",
                    parametresAntics.DescompteGrup2,
                    formula.DescompteInicialGrup2);

                ComprovarDecimalExacte(
                    correspondencia.Codi
                    + " - Descompte 3",
                    parametresAntics.DescompteGrup3,
                    formula.DescompteInicialGrup3);

                ComprovarDecimalExacte(
                    correspondencia.Codi
                    + " - Descompte 4",
                    parametresAntics.DescompteGrup4,
                    formula.DescompteInicialGrup4);

                descomptesComparats +=
                    4;

                /*
                 * Executem el motor antic.
                 */
                ResultatCalculTarifes resultatAntic =
                    motorAntic.Calcular(
                        article,
                        correspondencia.TipusPlantilla,
                        parametresAntics);

                if (resultatAntic == null)
                {
                    throw new InvalidOperationException(
                        correspondencia.Codi
                        + ": el motor antic ha retornat null.");
                }

                if (!resultatAntic.Correcte)
                {
                    throw new InvalidOperationException(
                        correspondencia.Codi
                        + ": el motor antic ha fallat."
                        + "\r\n"
                        + resultatAntic.Missatge);
                }

                /*
                 * Executem el motor nou amb els valors
                 * configurats realment a SQL.
                 */
                var contextNou =
                    new ContextAvaluacioFormula(
                        article.PreuCompra,
                        article.PreuCost,
                        article.PreuTransport,
                        formula.ValorInicialP1,
                        formula.ValorInicialP2,
                        formula.ValorInicialP3,
                        formula.ValorInicialP4);

                ResultatAvaluacioFormulaTarifes resultatNou =
                    motorConfigurable.Avaluar(
                        formula,
                        contextNou);

                for (int numeroTarifa = 1;
                     numeroTarifa <= 6;
                     numeroTarifa++)
                {
                    decimal valorAntic =
                        ObtenirTarifaAntiga(
                            resultatAntic,
                            numeroTarifa);

                    decimal valorNou =
                        resultatNou.ObtenirTarifa(
                            numeroTarifa);

                    /*
                     * El motor antic conserva quatre decimals.
                     * Apliquem el mateix criteri al resultat nou
                     * abans de comparar-los.
                     */
                    decimal valorNouArrodonit =
                        ArrodonirQuatreDecimals(
                            valorNou);

                    ComprovarDecimalExacte(
                        correspondencia.Codi
                        + " - Tarifa "
                        + numeroTarifa,
                        valorAntic,
                        valorNouArrodonit);

                    tarifesComparades++;
                }

                ComprovarBoolea(
                    correspondencia.Codi
                    + " - Resultat genera descomptes",
                    resultatAntic.GeneraDescomptes,
                    resultatNou.GeneraDescomptes);

                ComprovarDecimalExacte(
                    correspondencia.Codi
                    + " - Resultat descompte 1",
                    resultatAntic.DescompteGrup1,
                    resultatNou.DescompteGrup1);

                ComprovarDecimalExacte(
                    correspondencia.Codi
                    + " - Resultat descompte 2",
                    resultatAntic.DescompteGrup2,
                    resultatNou.DescompteGrup2);

                ComprovarDecimalExacte(
                    correspondencia.Codi
                    + " - Resultat descompte 3",
                    resultatAntic.DescompteGrup3,
                    resultatNou.DescompteGrup3);

                ComprovarDecimalExacte(
                    correspondencia.Codi
                    + " - Resultat descompte 4",
                    resultatAntic.DescompteGrup4,
                    resultatNou.DescompteGrup4);

                formulesComparades++;

                resum.AppendLine(
                    "OK - "
                    + correspondencia.Ordre
                    + " · "
                    + correspondencia.Codi);

                resum.AppendLine(
                    "  Tarifes 1–6 coincidents.");

                resum.AppendLine(
                    "  P1–P4 coincidents.");

                resum.AppendLine(
                    "  Descomptes coincidents.");

                resum.AppendLine();
            }

            resum.AppendLine(
                "Regressió finalitzada correctament.");

            resum.AppendLine(
                "Fórmules comparades: "
                + formulesComparades
                + ".");

            resum.AppendLine(
                "Tarifes comparades: "
                + tarifesComparades
                + ".");

            resum.AppendLine(
                "Paràmetres P1–P4 comparats: "
                + parametresComparats
                + ".");

            resum.AppendLine(
                "Descomptes inicials comparats: "
                + descomptesComparats
                + ".");

            return resum.ToString();
        }

        /// <summary>
        /// Defineix la relació entre els codis
        /// de SQL i les plantilles antigues.
        /// </summary>
        private static IReadOnlyList<CorrespondenciaFormula>
            CrearCorrespondencies()
        {
            return new List<CorrespondenciaFormula>
            {
                new CorrespondenciaFormula(
                    1,
                    "COST_DIVIDIT_COEFICIENTS",
                    TipusPlantillaTarifes
                        .CostDividitCoeficients),

                new CorrespondenciaFormula(
                    2,
                    "COST_MES_PERCENTATGES",
                    TipusPlantillaTarifes
                        .CostMesPercentatges),

                new CorrespondenciaFormula(
                    3,
                    "PREU_COMPRA_AMB_DESCOMPTES",
                    TipusPlantillaTarifes
                        .PreuCompraAmbDescomptes),

                new CorrespondenciaFormula(
                    4,
                    "PREU_COMPRA_DOBLE_AMB_DESCOMPTES",
                    TipusPlantillaTarifes
                        .PreuCompraDobleAmbDescomptes),

                new CorrespondenciaFormula(
                    5,
                    "COST_DIVIDIT_COEFICIENTS_ALTERNATIUS",
                    TipusPlantillaTarifes
                        .CostDividitCoeficientsAlternatius),

                new CorrespondenciaFormula(
                    6,
                    "COST_MES_IMPORTS_FIXOS",
                    TipusPlantillaTarifes
                        .CostMesImportsFixos),

                new CorrespondenciaFormula(
                    7,
                    "COST_MES_PERCENTATGES_ALTERNATIUS",
                    TipusPlantillaTarifes
                        .CostMesPercentatgesAlternatius)
            };
        }

        /// <summary>
        /// Crea l'article utilitzat també
        /// per l'autoprova del motor antic.
        /// </summary>
        private static ArticleCalculTarifes
            CrearArticleReferencia()
        {
            return new ArticleCalculTarifes
            {
                Seleccionat =
                    true,

                CodiArticle =
                    "16",

                CodiArticleBaseDades =
                    "16",

                Descripcio =
                    "Article de prova",

                Familia =
                    "1",

                PreuCompra =
                    4.50M,

                PreuCost =
                    3.85M,

                /*
                 * Aquesta propietat antiga conté
                 * realment el valor PRCSTANDARD.
                 */
                PreuTransport =
                    0.25M
            };
        }

        private static FormulaTarifa ObtenirFormulaPerCodi(
            IEnumerable<FormulaTarifa> formules,
            string codi)
        {
            List<FormulaTarifa> coincidencies =
                formules
                    .Where(
                        formula =>
                            string.Equals(
                                formula.Codi,
                                codi,
                                StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (coincidencies.Count != 1)
            {
                throw new InvalidOperationException(
                    "S'esperava una única fórmula activa "
                    + "amb el codi \""
                    + codi
                    + "\", però se n'han trobat "
                    + coincidencies.Count
                    + ".");
            }

            return coincidencies[0];
        }

        private static DefinicioPlantillaTarifes
            ObtenirDefinicioAntiga(
                CatalegPlantillesTarifes cataleg,
                TipusPlantillaTarifes tipus)
        {
            DefinicioPlantillaTarifes definicio =
                cataleg
                    .ObtenirPlantilles()
                    .FirstOrDefault(
                        element =>
                            element.Tipus
                            == tipus);

            if (definicio == null)
            {
                throw new InvalidOperationException(
                    "No s'ha trobat la definició antiga "
                    + "de la plantilla "
                    + tipus
                    + ".");
            }

            return definicio;
        }

        private static decimal ObtenirTarifaAntiga(
            ResultatCalculTarifes resultat,
            int numeroTarifa)
        {
            switch (numeroTarifa)
            {
                case 1:
                    return resultat.Tarifa1;

                case 2:
                    return resultat.Tarifa2;

                case 3:
                    return resultat.Tarifa3;

                case 4:
                    return resultat.Tarifa4;

                case 5:
                    return resultat.Tarifa5;

                case 6:
                    return resultat.Tarifa6;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(numeroTarifa),
                        numeroTarifa,
                        "La tarifa ha d'estar entre 1 i 6.");
            }
        }

        private static decimal ArrodonirQuatreDecimals(
            decimal valor)
        {
            return Math.Round(
                valor,
                4,
                MidpointRounding.AwayFromZero);
        }

        private static void ComprovarDecimalExacte(
            string nomProva,
            decimal esperat,
            decimal real)
        {
            if (esperat == real)
            {
                return;
            }

            throw new InvalidOperationException(
                nomProva
                + ": s'esperava "
                + FormatarDecimal(
                    esperat)
                + " però s'ha obtingut "
                + FormatarDecimal(
                    real)
                + ".");
        }

        private static void ComprovarBoolea(
            string nomProva,
            bool esperat,
            bool real)
        {
            if (esperat == real)
            {
                return;
            }

            throw new InvalidOperationException(
                nomProva
                + ": el valor booleà no coincideix.");
        }

        private static void ComprovarEnter(
            string nomProva,
            int esperat,
            int real)
        {
            if (esperat == real)
            {
                return;
            }

            throw new InvalidOperationException(
                nomProva
                + ": s'esperava "
                + esperat
                + " però s'ha obtingut "
                + real
                + ".");
        }

        private static void ComprovarText(
            string nomProva,
            string esperat,
            string real)
        {
            if (string.Equals(
                esperat ?? string.Empty,
                real ?? string.Empty,
                StringComparison.Ordinal))
            {
                return;
            }

            throw new InvalidOperationException(
                nomProva
                + ": s'esperava \""
                + (
                    esperat
                    ?? string.Empty
                )
                + "\" però s'ha obtingut \""
                + (
                    real
                    ?? string.Empty
                )
                + "\".");
        }

        private static string FormatarDecimal(
            decimal valor)
        {
            return valor.ToString(
                "0.####",
                CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Relaciona un codi de SQL
        /// amb una plantilla del motor antic.
        /// </summary>
        private sealed class CorrespondenciaFormula
        {
            public int Ordre
            {
                get;
            }

            public string Codi
            {
                get;
            }

            public TipusPlantillaTarifes TipusPlantilla
            {
                get;
            }

            public CorrespondenciaFormula(
                int ordre,
                string codi,
                TipusPlantillaTarifes tipusPlantilla)
            {
                Ordre =
                    ordre;

                Codi =
                    codi ?? string.Empty;

                TipusPlantilla =
                    tipusPlantilla;
            }
        }
    }
}