using A3ErpGestorFormulesTarifes.MotorExpressions;
using System;
using System.Text;

namespace A3ErpGestorFormulesTarifes.Diagnostics
{
    /// <summary>
    /// Executa proves automàtiques bàsiques
    /// sobre el motor segur d'expressions.
    ///
    /// Aquesta classe no modifica SQL
    /// ni cap dada d'a3ERP.
    /// </summary>
    public static class AutoprovaMotorExpressions
    {
        /// <summary>
        /// Executa totes les proves i retorna
        /// un resum textual quan són correctes.
        ///
        /// Si una prova falla, genera
        /// una excepció amb el detall.
        /// </summary>
        public static string Executar()
        {
            var resum =
                new StringBuilder();

            var avaluador =
                new AvaluadorExpressioFormula();

            /*
             * Article i paràmetres de referència:
             *
             * PRCCOMPRA   = 4,50
             * PRCCOSTE    = 3,85
             * PRCSTANDARD = 0,25
             * P1          = 0,70
             * P2          = 10
             * P3          = 15
             * P4          = 2
             */
            var context =
                new ContextAvaluacioFormula(
                    preuCompra: 4.50M,
                    preuCost: 3.85M,
                    preuStandard: 0.25M,
                    p1: 0.70M,
                    p2: 10M,
                    p3: 15M,
                    p4: 2M);

            int provesCorrectes =
                0;

            /*
             * Proves de càlcul.
             */
            ComprovarResultat(
                avaluador,
                context,
                "PRCCOSTE / P1",
                5.50M,
                "Divisió amb variable",
                resum,
                ref provesCorrectes);

            ComprovarResultat(
                avaluador,
                context,
                "PRCCOSTE + PRCSTANDARD",
                4.10M,
                "Suma de camps d'article",
                resum,
                ref provesCorrectes);

            ComprovarResultat(
                avaluador,
                context,
                "2 + 3 * 4",
                14M,
                "Prioritat de multiplicació",
                resum,
                ref provesCorrectes);

            ComprovarResultat(
                avaluador,
                context,
                "(2 + 3) * 4",
                20M,
                "Prioritat de parèntesis",
                resum,
                ref provesCorrectes);

            ComprovarResultat(
                avaluador,
                context,
                "PRCCOSTE * (1 + P2 / 100)",
                4.235M,
                "Percentatge d'increment",
                resum,
                ref provesCorrectes);

            ComprovarResultat(
                avaluador,
                context,
                "PRCCOMPRA - (PRCCOMPRA * P3 / 100)",
                3.825M,
                "Percentatge de descompte",
                resum,
                ref provesCorrectes);

            ComprovarResultat(
                avaluador,
                context,
                "-P4 + 10",
                8M,
                "Signe negatiu",
                resum,
                ref provesCorrectes);

            ComprovarResultat(
                avaluador,
                context,
                "2,5 + 1",
                3.5M,
                "Separador decimal amb coma",
                resum,
                ref provesCorrectes);

            ComprovarResultat(
                avaluador,
                context,
                "prccoste + prcstandard",
                4.10M,
                "Variables en minúscules",
                resum,
                ref provesCorrectes);

            /*
             * Proves d'errors controlats.
             */
            ComprovarError(
                avaluador,
                context,
                "PRCCOSTE / 0",
                "dividir per zero",
                "Divisió per zero",
                resum,
                ref provesCorrectes);

            ComprovarError(
                avaluador,
                context,
                "PRCCOSTEE + P1",
                "no està permesa",
                "Variable desconeguda",
                resum,
                ref provesCorrectes);

            ComprovarError(
                avaluador,
                context,
                "PRCCOSTE + (P1 * 2",
                "Falta tancar el parèntesi",
                "Parèntesi incomplet",
                resum,
                ref provesCorrectes);

            ComprovarError(
                avaluador,
                context,
                "PRCCOSTE ++ P1",
                "S'esperava un número",
                "Dos operadors consecutius",
                resum,
                ref provesCorrectes);

            ComprovarError(
                avaluador,
                context,
                "PRCCOSTE @ P1",
                "no està permès",
                "Caràcter no autoritzat",
                resum,
                ref provesCorrectes);

            resum.AppendLine();
            resum.AppendLine(
                "Autoprova finalitzada correctament.");

            resum.AppendLine(
                "Proves superades: "
                + provesCorrectes
                + ".");

            return resum.ToString();
        }

        /// <summary>
        /// Comprova que una expressió retorni
        /// exactament el resultat esperat.
        /// </summary>
        private static void ComprovarResultat(
            AvaluadorExpressioFormula avaluador,
            ContextAvaluacioFormula context,
            string expressio,
            decimal resultatEsperat,
            string nomProva,
            StringBuilder resum,
            ref int provesCorrectes)
        {
            decimal resultatObtingut =
                avaluador.Avaluar(
                    expressio,
                    context);

            if (resultatObtingut
                != resultatEsperat)
            {
                throw new InvalidOperationException(
                    "Ha fallat la prova \""
                    + nomProva
                    + "\"."
                    + "\r\nExpressió: "
                    + expressio
                    + "\r\nEsperat: "
                    + resultatEsperat
                    + "\r\nObtingut: "
                    + resultatObtingut);
            }

            provesCorrectes++;

            resum.AppendLine(
                "OK - "
                + nomProva
                + ": "
                + resultatObtingut);
        }

        /// <summary>
        /// Comprova que una expressió incorrecta
        /// generi l'error controlat esperat.
        /// </summary>
        private static void ComprovarError(
            AvaluadorExpressioFormula avaluador,
            ContextAvaluacioFormula context,
            string expressio,
            string fragmentMissatgeEsperat,
            string nomProva,
            StringBuilder resum,
            ref int provesCorrectes)
        {
            try
            {
                decimal resultat =
                    avaluador.Avaluar(
                        expressio,
                        context);

                throw new InvalidOperationException(
                    "Ha fallat la prova \""
                    + nomProva
                    + "\"."
                    + "\r\nL'expressió havia de generar un error, "
                    + "però ha retornat: "
                    + resultat);
            }
            catch (ExcepcioExpressioFormula ex)
            {
                if (ex.Message.IndexOf(
                        fragmentMissatgeEsperat,
                        StringComparison.OrdinalIgnoreCase)
                    < 0)
                {
                    throw new InvalidOperationException(
                        "Ha fallat la prova \""
                        + nomProva
                        + "\"."
                        + "\r\nMissatge esperat: "
                        + fragmentMissatgeEsperat
                        + "\r\nMissatge obtingut: "
                        + ex.Message,
                        ex);
                }

                provesCorrectes++;

                resum.AppendLine(
                    "OK - "
                    + nomProva
                    + ": error detectat correctament.");
            }
        }
    }
}