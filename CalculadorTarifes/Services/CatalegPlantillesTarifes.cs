using A3ErpCalculadorTarifes.Models;
using System;
using System.Collections.Generic;

namespace A3ErpCalculadorTarifes.Services
{
    /// <summary>
    /// Proporciona les set plantilles disponibles
    /// i els seus valors predeterminats.
    ///
    /// Els valors retornats són inicials:
    /// l'usuari els podrà modificar abans de calcular.
    /// </summary>
    public sealed class CatalegPlantillesTarifes
    {
        /// <summary>
        /// Retorna totes les plantilles
        /// en l'ordre funcional definit pel client.
        /// </summary>
        public IReadOnlyList<DefinicioPlantillaTarifes>
            ObtenirPlantilles()
        {
            return new List<DefinicioPlantillaTarifes>
            {
                new DefinicioPlantillaTarifes
                {
                    Tipus =
                        TipusPlantillaTarifes
                            .CostDividitCoeficients,

                    Nom =
                        "1 · Cost dividit per coeficients",

                    Descripcio =
                        "Calcula les tarifes 1–4 dividint "
                        + "PRCCOSTE pels coeficients indicats.",

                    TipusValors =
                        "Coeficients divisors",

                    UtilitzaValorsTarifes =
                        true,

                    GeneraDescomptes =
                        false
                },

                new DefinicioPlantillaTarifes
                {
                    Tipus =
                        TipusPlantillaTarifes
                            .CostMesPercentatges,

                    Nom =
                        "2 · Cost més percentatges",

                    Descripcio =
                        "Calcula les tarifes 1–4 incrementant "
                        + "PRCCOSTE segons els percentatges indicats.",

                    TipusValors =
                        "Percentatges d'increment",

                    UtilitzaValorsTarifes =
                        true,

                    GeneraDescomptes =
                        false
                },

                new DefinicioPlantillaTarifes
                {
                    Tipus =
                        TipusPlantillaTarifes
                            .PreuCompraAmbDescomptes,

                    Nom =
                        "3 · Preu compra amb descomptes",

                    Descripcio =
                        "Utilitza PRCCOMPRA com a preu de les "
                        + "tarifes 1–4 i genera descomptes "
                        + "per als grups 1–4.",

                    TipusValors =
                        "No aplica",

                    UtilitzaValorsTarifes =
                        false,

                    GeneraDescomptes =
                        true
                },

                new DefinicioPlantillaTarifes
                {
                    Tipus =
                        TipusPlantillaTarifes
                            .PreuCompraDobleAmbDescomptes,

                    Nom =
                        "4 · Preu compra × 2 amb descomptes",

                    Descripcio =
                        "Utilitza PRCCOMPRA multiplicat per 2 "
                        + "com a preu de les tarifes 1–4 i "
                        + "genera descomptes per als grups 1–4.",

                    TipusValors =
                        "No aplica",

                    UtilitzaValorsTarifes =
                        false,

                    GeneraDescomptes =
                        true
                },

                new DefinicioPlantillaTarifes
                {
                    Tipus =
                        TipusPlantillaTarifes
                            .CostDividitCoeficientsAlternatius,

                    Nom =
                        "5 · Cost dividit per coeficients alternatius",

                    Descripcio =
                        "Calcula les tarifes 1–4 dividint "
                        + "PRCCOSTE pels coeficients alternatius.",

                    TipusValors =
                        "Coeficients divisors",

                    UtilitzaValorsTarifes =
                        true,

                    GeneraDescomptes =
                        false
                },

                new DefinicioPlantillaTarifes
                {
                    Tipus =
                        TipusPlantillaTarifes
                            .CostMesImportsFixos,

                    Nom =
                        "6 · Cost més imports fixos",

                    Descripcio =
                        "Calcula les tarifes 1–4 sumant "
                        + "un import fix diferent a PRCCOSTE.",

                    TipusValors =
                        "Imports fixos",

                    UtilitzaValorsTarifes =
                        true,

                    GeneraDescomptes =
                        false
                },

                new DefinicioPlantillaTarifes
                {
                    Tipus =
                        TipusPlantillaTarifes
                            .CostMesPercentatgesAlternatius,

                    Nom =
                        "7 · Cost més percentatges alternatius",

                    Descripcio =
                        "Calcula les tarifes 1–4 incrementant "
                        + "PRCCOSTE amb els percentatges alternatius.",

                    TipusValors =
                        "Percentatges d'increment",

                    UtilitzaValorsTarifes =
                        true,

                    GeneraDescomptes =
                        false
                }
            };
        }

        /// <summary>
        /// Crea una nova configuració editable amb els
        /// valors predeterminats de la plantilla indicada.
        ///
        /// Sempre retorna una instància nova perquè modificar
        /// els valors al formulari no alteri el catàleg.
        /// </summary>
        public ParametresCalculTarifes
            CrearParametresPredeterminats(
                TipusPlantillaTarifes plantilla)
        {
            switch (plantilla)
            {
                /*
                 * PRCCOSTE / 0,65
                 * PRCCOSTE / 0,70
                 * PRCCOSTE / 0,75
                 * PRCCOSTE / 0,80
                 */
                case TipusPlantillaTarifes
                    .CostDividitCoeficients:

                    return CrearParametres(
                        valor1: 0.65m,
                        valor2: 0.70m,
                        valor3: 0.75m,
                        valor4: 0.80m,
                        generaDescomptes: false);

                /*
                 * PRCCOSTE + 35 %
                 * PRCCOSTE + 30 %
                 * PRCCOSTE + 25 %
                 * PRCCOSTE + 20 %
                 */
                case TipusPlantillaTarifes
                    .CostMesPercentatges:

                    return CrearParametres(
                        valor1: 35m,
                        valor2: 30m,
                        valor3: 25m,
                        valor4: 20m,
                        generaDescomptes: false);

                /*
                 * Tarifes 1–4 = PRCCOMPRA
                 * Descomptes = 10, 15, 20 i 25 %
                 */
                case TipusPlantillaTarifes
                    .PreuCompraAmbDescomptes:

                    return CrearParametres(
                        valor1: 0m,
                        valor2: 0m,
                        valor3: 0m,
                        valor4: 0m,
                        generaDescomptes: true,
                        descompte1: 10m,
                        descompte2: 15m,
                        descompte3: 20m,
                        descompte4: 25m);

                /*
                 * Tarifes 1–4 = PRCCOMPRA × 2
                 * Descomptes = 10, 15, 20 i 25 %
                 */
                case TipusPlantillaTarifes
                    .PreuCompraDobleAmbDescomptes:

                    return CrearParametres(
                        valor1: 0m,
                        valor2: 0m,
                        valor3: 0m,
                        valor4: 0m,
                        generaDescomptes: true,
                        descompte1: 10m,
                        descompte2: 15m,
                        descompte3: 20m,
                        descompte4: 25m);

                /*
                 * PRCCOSTE / 0,55
                 * PRCCOSTE / 0,60
                 * PRCCOSTE / 0,65
                 * PRCCOSTE / 0,70
                 */
                case TipusPlantillaTarifes
                    .CostDividitCoeficientsAlternatius:

                    return CrearParametres(
                        valor1: 0.55m,
                        valor2: 0.60m,
                        valor3: 0.65m,
                        valor4: 0.70m,
                        generaDescomptes: false);

                /*
                 * PRCCOSTE + 10
                 * PRCCOSTE + 9
                 * PRCCOSTE + 7
                 * PRCCOSTE + 6
                 */
                case TipusPlantillaTarifes
                    .CostMesImportsFixos:

                    return CrearParametres(
                        valor1: 10m,
                        valor2: 9m,
                        valor3: 7m,
                        valor4: 6m,
                        generaDescomptes: false);

                /*
                 * PRCCOSTE + 40 %
                 * PRCCOSTE + 35 %
                 * PRCCOSTE + 30 %
                 * PRCCOSTE + 25 %
                 */
                case TipusPlantillaTarifes
                    .CostMesPercentatgesAlternatius:

                    return CrearParametres(
                        valor1: 40m,
                        valor2: 35m,
                        valor3: 30m,
                        valor4: 25m,
                        generaDescomptes: false);

                default:

                    throw new ArgumentOutOfRangeException(
                        nameof(plantilla),
                        plantilla,
                        "La plantilla indicada no existeix.");
            }
        }

        /// <summary>
        /// Construeix els paràmetres inicials
        /// d'una plantilla.
        /// </summary>
        private static ParametresCalculTarifes
            CrearParametres(
                decimal valor1,
                decimal valor2,
                decimal valor3,
                decimal valor4,
                bool generaDescomptes,
                decimal descompte1 = 0,
                decimal descompte2 = 0,
                decimal descompte3 = 0,
                decimal descompte4 = 0)
        {
            return new ParametresCalculTarifes
            {
                ValorTarifa1 =
                    valor1,

                ValorTarifa2 =
                    valor2,

                ValorTarifa3 =
                    valor3,

                ValorTarifa4 =
                    valor4,

                GeneraDescomptes =
                    generaDescomptes,

                DescompteGrup1 =
                    descompte1,

                DescompteGrup2 =
                    descompte2,

                DescompteGrup3 =
                    descompte3,

                DescompteGrup4 =
                    descompte4
            };
        }
    }
}