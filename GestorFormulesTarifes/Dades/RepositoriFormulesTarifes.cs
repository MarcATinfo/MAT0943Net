using A3ErpGestorFormulesTarifes.Models;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;
using System.Data;

namespace A3ErpGestorFormulesTarifes.Dades
{
    /// <summary>
    /// Gestiona les operacions de lectura de les fórmules
    /// configurables emmagatzemades a
    /// dbo.AT_ARTICULO_FORMULAS.
    ///
    /// En aquesta primera fase, el repositori només llegeix.
    /// Les operacions d'alta, modificació i eliminació
    /// s'afegiran posteriorment i de manera controlada.
    /// </summary>
    public sealed class RepositoriFormulesTarifes
    {
        /// <summary>
        /// Recupera les fórmules no eliminades que s'han
        /// de mostrar al gestor.
        ///
        /// Inclou tant les fórmules actives com les inactives.
        /// </summary>
        public List<FormulaTarifa> ObtenirPerGestor(
            string cadenaConnexio)
        {
            return Obtenir(
                cadenaConnexio,
                nomesActives: false);
        }

        /// <summary>
        /// Recupera únicament les fórmules actives
        /// i no eliminades.
        ///
        /// Aquest mètode és utilitzat pel Calculador
        /// per obtenir només les fórmules disponibles
        /// per al càlcul.
        /// </summary>
        public List<FormulaTarifa> ObtenirActives(
            string cadenaConnexio)
        {
            return Obtenir(
                cadenaConnexio,
                nomesActives: true);
        }

        /// <summary>
        /// Executa la consulta comuna de fórmules.
        /// </summary>
        private static List<FormulaTarifa> Obtenir(
            string cadenaConnexio,
            bool nomesActives)
        {
            if (string.IsNullOrWhiteSpace(
                cadenaConnexio))
            {
                throw new ArgumentException(
                    "La connexió amb a3ERP és buida.",
                    nameof(cadenaConnexio));
            }

            string sql = @"
                SELECT
                    IDFORMULA,
                    CODIGO,
                    NOMBRE,
                    DESCRIPCION,

                    ORDEN,
                    ACTIVO,
                    ELIMINADO,

                    TIPO_VALORES,
                    UTILIZA_VALORES_TARIFAS,

                    VALOR_INICIAL_P1,
                    VALOR_INICIAL_P2,
                    VALOR_INICIAL_P3,
                    VALOR_INICIAL_P4,

                    EXPRESION_TARIFA1,
                    EXPRESION_TARIFA2,
                    EXPRESION_TARIFA3,
                    EXPRESION_TARIFA4,
                    EXPRESION_TARIFA5,
                    EXPRESION_TARIFA6,

                    GENERA_DESCUENTOS,

                    DESCUENTO_INICIAL_GRUPO1,
                    DESCUENTO_INICIAL_GRUPO2,
                    DESCUENTO_INICIAL_GRUPO3,
                    DESCUENTO_INICIAL_GRUPO4,

                    FECHA_ALTA,
                    FECHA_ACTUALIZACION,
                    FECHA_ELIMINACION
                FROM dbo.AT_ARTICULO_FORMULAS
                WHERE ELIMINADO = 0";

            if (nomesActives)
            {
                sql += @"
                    AND ACTIVO = 1";
            }

            sql += @"
                ORDER BY
                    ORDEN,
                    IDFORMULA;";

            var formules =
                new List<FormulaTarifa>();

            using (var connexio =
                new OleDbConnection(
                    cadenaConnexio))
            using (var comanda =
                new OleDbCommand(
                    sql,
                    connexio))
            {
                connexio.Open();

                using (OleDbDataReader lector =
                    comanda.ExecuteReader())
                {
                    while (lector != null
                           && lector.Read())
                    {
                        formules.Add(
                            CrearFormula(
                                lector));
                    }
                }
            }

            return formules;
        }

        /// <summary>
        /// Construeix una fórmula a partir
        /// de la fila llegida de SQL Server.
        /// </summary>
        private static FormulaTarifa CrearFormula(
            OleDbDataReader lector)
        {
            return new FormulaTarifa
            {
                Id =
                    LlegirEnter(
                        lector,
                        "IDFORMULA"),

                Codi =
                    LlegirText(
                        lector,
                        "CODIGO"),

                Nom =
                    LlegirText(
                        lector,
                        "NOMBRE"),

                Descripcio =
                    LlegirText(
                        lector,
                        "DESCRIPCION"),

                Ordre =
                    LlegirEnter(
                        lector,
                        "ORDEN"),

                Activa =
                    LlegirBoolea(
                        lector,
                        "ACTIVO"),

                Eliminada =
                    LlegirBoolea(
                        lector,
                        "ELIMINADO"),

                TipusValors =
                    LlegirText(
                        lector,
                        "TIPO_VALORES"),

                UtilitzaValorsTarifes =
                    LlegirBoolea(
                        lector,
                        "UTILIZA_VALORES_TARIFAS"),

                ValorInicialP1 =
                    LlegirDecimal(
                        lector,
                        "VALOR_INICIAL_P1"),

                ValorInicialP2 =
                    LlegirDecimal(
                        lector,
                        "VALOR_INICIAL_P2"),

                ValorInicialP3 =
                    LlegirDecimal(
                        lector,
                        "VALOR_INICIAL_P3"),

                ValorInicialP4 =
                    LlegirDecimal(
                        lector,
                        "VALOR_INICIAL_P4"),

                ExpressioTarifa1 =
                    LlegirText(
                        lector,
                        "EXPRESION_TARIFA1"),

                ExpressioTarifa2 =
                    LlegirText(
                        lector,
                        "EXPRESION_TARIFA2"),

                ExpressioTarifa3 =
                    LlegirText(
                        lector,
                        "EXPRESION_TARIFA3"),

                ExpressioTarifa4 =
                    LlegirText(
                        lector,
                        "EXPRESION_TARIFA4"),

                ExpressioTarifa5 =
                    LlegirText(
                        lector,
                        "EXPRESION_TARIFA5"),

                ExpressioTarifa6 =
                    LlegirText(
                        lector,
                        "EXPRESION_TARIFA6"),

                GeneraDescomptes =
                    LlegirBoolea(
                        lector,
                        "GENERA_DESCUENTOS"),

                DescompteInicialGrup1 =
                    LlegirDecimal(
                        lector,
                        "DESCUENTO_INICIAL_GRUPO1"),

                DescompteInicialGrup2 =
                    LlegirDecimal(
                        lector,
                        "DESCUENTO_INICIAL_GRUPO2"),

                DescompteInicialGrup3 =
                    LlegirDecimal(
                        lector,
                        "DESCUENTO_INICIAL_GRUPO3"),

                DescompteInicialGrup4 =
                    LlegirDecimal(
                        lector,
                        "DESCUENTO_INICIAL_GRUPO4"),

                DataAlta =
                    LlegirDataNullable(
                        lector,
                        "FECHA_ALTA"),

                DataActualitzacio =
                    LlegirDataNullable(
                        lector,
                        "FECHA_ACTUALIZACION"),

                DataEliminacio =
                    LlegirDataNullable(
                        lector,
                        "FECHA_ELIMINACION")
            };
        }

        /// <summary>
        /// Llegeix un text i converteix els valors
        /// nuls de SQL en una cadena buida.
        /// </summary>
        private static string LlegirText(
            OleDbDataReader lector,
            string columna)
        {
            int ordinal =
                lector.GetOrdinal(
                    columna);

            if (lector.IsDBNull(
                ordinal))
            {
                return string.Empty;
            }

            return Convert.ToString(
                       lector.GetValue(
                           ordinal),
                       CultureInfo.InvariantCulture)
                   ?? string.Empty;
        }

        /// <summary>
        /// Llegeix un valor enter.
        /// </summary>
        private static int LlegirEnter(
            OleDbDataReader lector,
            string columna)
        {
            int ordinal =
                lector.GetOrdinal(
                    columna);

            return Convert.ToInt32(
                lector.GetValue(
                    ordinal),
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Llegeix un valor decimal.
        /// </summary>
        private static decimal LlegirDecimal(
            OleDbDataReader lector,
            string columna)
        {
            int ordinal =
                lector.GetOrdinal(
                    columna);

            return Convert.ToDecimal(
                lector.GetValue(
                    ordinal),
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Llegeix un valor booleà emmagatzemat
        /// en una columna bit de SQL Server.
        /// </summary>
        private static bool LlegirBoolea(
            OleDbDataReader lector,
            string columna)
        {
            int ordinal =
                lector.GetOrdinal(
                    columna);

            return Convert.ToBoolean(
                lector.GetValue(
                    ordinal),
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Llegeix una data que pot contenir NULL.
        /// </summary>
        private static DateTime? LlegirDataNullable(
            OleDbDataReader lector,
            string columna)
        {
            int ordinal =
                lector.GetOrdinal(
                    columna);

            if (lector.IsDBNull(
                ordinal))
            {
                return null;
            }

            return Convert.ToDateTime(
                lector.GetValue(
                    ordinal),
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Activa o desactiva una fórmula existent.
        ///
        /// L'operació:
        /// - només afecta fórmules no eliminades;
        /// - actualitza FECHA_ACTUALIZACION;
        /// - impedeix desactivar l'última fórmula activa;
        /// - s'executa dins d'una transacció.
        /// </summary>
        public void CanviarEstatActiu(
            string cadenaConnexio,
            int idFormula,
            bool activar)
        {
            if (string.IsNullOrWhiteSpace(
                cadenaConnexio))
            {
                throw new ArgumentException(
                    "La connexió amb a3ERP és buida.",
                    nameof(cadenaConnexio));
            }

            if (idFormula <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idFormula),
                    idFormula,
                    "L'identificador de la fórmula no és vàlid.");
            }

            using (var connexio =
                new OleDbConnection(
                    cadenaConnexio))
            {
                connexio.Open();

                /*
                 * Serializable evita que dues operacions simultànies
                 * puguin desactivar les últimes fórmules actives
                 * al mateix temps.
                 */
                using (OleDbTransaction transaccio =
                    connexio.BeginTransaction(
                        IsolationLevel.Serializable))
                {
                    try
                    {
                        bool estatActual =
                            ObtenirEstatActiuActual(
                                connexio,
                                transaccio,
                                idFormula);

                        /*
                         * Si la fórmula ja té l'estat sol·licitat,
                         * no cal executar cap actualització.
                         */
                        if (estatActual == activar)
                        {
                            transaccio.Commit();

                            return;
                        }

                        if (!activar)
                        {
                            ComprovarQueNoSiguiUltimaActiva(
                                connexio,
                                transaccio,
                                idFormula);
                        }

                        const string sql = @"
                    UPDATE dbo.AT_ARTICULO_FORMULAS
                    SET
                        ACTIVO = ?,
                        FECHA_ACTUALIZACION = GETDATE()
                    WHERE IDFORMULA = ?
                      AND ELIMINADO = 0;";

                        using (var comanda =
                            new OleDbCommand(
                                sql,
                                connexio,
                                transaccio))
                        {
                            /*
                             * OleDb associa els paràmetres
                             * segons l'ordre dels símbols ?.
                             */
                            comanda.Parameters.Add(
                                "?",
                                OleDbType.Boolean)
                                .Value =
                                    activar;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.Integer)
                                .Value =
                                    idFormula;

                            int filesAfectades =
                                comanda.ExecuteNonQuery();

                            if (filesAfectades != 1)
                            {
                                throw new InvalidOperationException(
                                    "No s'ha pogut actualitzar "
                                    + "la fórmula seleccionada.");
                            }
                        }

                        transaccio.Commit();
                    }
                    catch
                    {
                        transaccio.Rollback();

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Recupera l'estat actual d'una fórmula
        /// no eliminada.
        ///
        /// Si no existeix, genera una excepció.
        /// </summary>
        private static bool ObtenirEstatActiuActual(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            int idFormula)
        {
            const string sql = @"
        SELECT ACTIVO
        FROM dbo.AT_ARTICULO_FORMULAS
        WHERE IDFORMULA = ?
          AND ELIMINADO = 0;";

            using (var comanda =
                new OleDbCommand(
                    sql,
                    connexio,
                    transaccio))
            {
                comanda.Parameters.Add(
                    "?",
                    OleDbType.Integer)
                    .Value =
                        idFormula;

                object valor =
                    comanda.ExecuteScalar();

                if (valor == null
                    || valor == DBNull.Value)
                {
                    throw new InvalidOperationException(
                        "La fórmula seleccionada no existeix "
                        + "o està eliminada.");
                }

                return Convert.ToBoolean(
                    valor,
                    CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Impedeix desactivar l'última fórmula
        /// disponible al Calculador.
        /// </summary>
        private static void ComprovarQueNoSiguiUltimaActiva(
            OleDbConnection connexio,
            OleDbTransaction transaccio,
            int idFormula)
        {
            const string sql = @"
        SELECT COUNT(*)
        FROM dbo.AT_ARTICULO_FORMULAS
        WHERE ELIMINADO = 0
          AND ACTIVO = 1
          AND IDFORMULA <> ?;";

            using (var comanda =
                new OleDbCommand(
                    sql,
                    connexio,
                    transaccio))
            {
                comanda.Parameters.Add(
                    "?",
                    OleDbType.Integer)
                    .Value =
                        idFormula;

                int altresFormulesActives =
                    Convert.ToInt32(
                        comanda.ExecuteScalar(),
                        CultureInfo.InvariantCulture);

                if (altresFormulesActives == 0)
                {
                    throw new InvalidOperationException(
                        "No es pot desactivar l'última fórmula activa. "
                        + "El Calculador ha de disposar com a mínim "
                        + "d'una fórmula disponible.");
                }
            }
        }

        /// <summary>
        /// Insereix una fórmula nova a la base de dades
        /// i retorna l'identificador generat per SQL.
        ///
        /// L'operació s'executa dins d'una transacció.
        /// La fórmula es crea sempre com a no eliminada.
        /// </summary>
        public int InserirFormula(
            string cadenaConnexio,
            FormulaTarifa formula)
        {
            if (string.IsNullOrWhiteSpace(
                cadenaConnexio))
            {
                throw new ArgumentException(
                    "La connexió amb a3ERP és buida.",
                    nameof(cadenaConnexio));
            }

            if (formula == null)
            {
                throw new ArgumentNullException(
                    nameof(formula));
            }

            if (formula.Id > 0)
            {
                throw new InvalidOperationException(
                    "No es pot inserir una fórmula "
                    + "que ja disposa d'identificador.");
            }

            const string sql = @"
        INSERT INTO dbo.AT_ARTICULO_FORMULAS
        (
            CODIGO,
            NOMBRE,
            DESCRIPCION,
            ORDEN,
            ACTIVO,
            ELIMINADO,
            TIPO_VALORES,
            UTILIZA_VALORES_TARIFAS,
            VALOR_INICIAL_P1,
            VALOR_INICIAL_P2,
            VALOR_INICIAL_P3,
            VALOR_INICIAL_P4,
            EXPRESION_TARIFA1,
            EXPRESION_TARIFA2,
            EXPRESION_TARIFA3,
            EXPRESION_TARIFA4,
            EXPRESION_TARIFA5,
            EXPRESION_TARIFA6,
            GENERA_DESCUENTOS,
            DESCUENTO_INICIAL_GRUPO1,
            DESCUENTO_INICIAL_GRUPO2,
            DESCUENTO_INICIAL_GRUPO3,
            DESCUENTO_INICIAL_GRUPO4,
            FECHA_ALTA,
            FECHA_ACTUALIZACION,
            FECHA_ELIMINACION
        )
        VALUES
        (
            ?,
            ?,
            ?,
            ?,
            ?,
            0,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            ?,
            GETDATE(),
            NULL,
            NULL
        );";

            using (var connexio =
                new OleDbConnection(
                    cadenaConnexio))
            {
                connexio.Open();

                using (OleDbTransaction transaccio =
                    connexio.BeginTransaction())
                {
                    try
                    {
                        using (var comanda =
                            new OleDbCommand(
                                sql,
                                connexio,
                                transaccio))
                        {
                            /*
                             * OleDb vincula els paràmetres
                             * segons la seva posició.
                             */
                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarChar,
                                50)
                                .Value =
                                    formula.Codi
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                150)
                                .Value =
                                    formula.Nom
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                1000)
                                .Value =
                                    formula.Descripcio
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.Integer)
                                .Value =
                                    formula.Ordre;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.Boolean)
                                .Value =
                                    formula.Activa;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                100)
                                .Value =
                                    formula.TipusValors
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.Boolean)
                                .Value =
                                    formula.UtilitzaValorsTarifes;

                            AfegirParametreDecimal(
                                comanda,
                                formula.ValorInicialP1,
                                18,
                                4);

                            AfegirParametreDecimal(
                                comanda,
                                formula.ValorInicialP2,
                                18,
                                4);

                            AfegirParametreDecimal(
                                comanda,
                                formula.ValorInicialP3,
                                18,
                                4);

                            AfegirParametreDecimal(
                                comanda,
                                formula.ValorInicialP4,
                                18,
                                4);

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                500)
                                .Value =
                                    formula.ExpressioTarifa1
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                500)
                                .Value =
                                    formula.ExpressioTarifa2
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                500)
                                .Value =
                                    formula.ExpressioTarifa3
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                500)
                                .Value =
                                    formula.ExpressioTarifa4
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                500)
                                .Value =
                                    formula.ExpressioTarifa5
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                500)
                                .Value =
                                    formula.ExpressioTarifa6
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.Boolean)
                                .Value =
                                    formula.GeneraDescomptes;

                            AfegirParametreDecimal(
                                comanda,
                                formula.DescompteInicialGrup1,
                                18,
                                4);

                            AfegirParametreDecimal(
                                comanda,
                                formula.DescompteInicialGrup2,
                                18,
                                4);

                            AfegirParametreDecimal(
                                comanda,
                                formula.DescompteInicialGrup3,
                                18,
                                4);

                            AfegirParametreDecimal(
                                comanda,
                                formula.DescompteInicialGrup4,
                                18,
                                4);

                            int filesAfectades =
                                comanda.ExecuteNonQuery();

                            if (filesAfectades != 1)
                            {
                                throw new InvalidOperationException(
                                    "No s'ha pogut inserir "
                                    + "la nova fórmula.");
                            }
                        }

                        int idGenerat;

                        /*
                         * @@IDENTITY retorna l'IDENTITY
                         * generat en aquesta mateixa connexió.
                         */
                        using (var comandaIdentitat =
                            new OleDbCommand(
                                "SELECT @@IDENTITY;",
                                connexio,
                                transaccio))
                        {
                            object valorIdentitat =
                                comandaIdentitat.ExecuteScalar();

                            if (valorIdentitat == null
                                || valorIdentitat == DBNull.Value)
                            {
                                throw new InvalidOperationException(
                                    "La fórmula s'ha inserit, però "
                                    + "no s'ha pogut recuperar "
                                    + "l'identificador generat.");
                            }

                            idGenerat =
                                Convert.ToInt32(
                                    valorIdentitat);
                        }

                        transaccio.Commit();

                        return idGenerat;
                    }
                    catch
                    {
                        transaccio.Rollback();

                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Afegeix un paràmetre decimal a una comanda
        /// OleDb indicant explícitament precisió i escala.
        ///
        /// Això evita que el proveïdor infereixi
        /// incorrectament el tipus segons el valor rebut.
        /// </summary>
        private static void AfegirParametreDecimal(
            OleDbCommand comanda,
            decimal valor,
            byte precisio,
            byte escala)
        {
            if (comanda == null)
            {
                throw new ArgumentNullException(
                    nameof(comanda));
            }

            OleDbParameter parametre =
                comanda.Parameters.Add(
                    "?",
                    OleDbType.Decimal);

            parametre.Precision =
                precisio;

            parametre.Scale =
                escala;

            parametre.Value =
                valor;
        }

        /// <summary>
        /// Actualitza totes les dades configurables
        /// d'una fórmula existent.
        ///
        /// L'operació:
        /// - només afecta fórmules no eliminades;
        /// - conserva FECHA_ALTA;
        /// - actualitza FECHA_ACTUALIZACION;
        /// - s'executa dins d'una transacció.
        /// </summary>
        public void ActualitzarFormula(
            string cadenaConnexio,
            FormulaTarifa formula)
        {
            if (string.IsNullOrWhiteSpace(
                cadenaConnexio))
            {
                throw new ArgumentException(
                    "La connexió amb a3ERP és buida.",
                    nameof(cadenaConnexio));
            }

            if (formula == null)
            {
                throw new ArgumentNullException(
                    nameof(formula));
            }

            if (formula.Id <= 0)
            {
                throw new InvalidOperationException(
                    "No es pot actualitzar una fórmula "
                    + "sense un identificador vàlid.");
            }

            if (formula.Eliminada)
            {
                throw new InvalidOperationException(
                    "No es pot actualitzar una fórmula eliminada.");
            }

            const string sql = @"
        UPDATE dbo.AT_ARTICULO_FORMULAS
        SET
            CODIGO = ?,
            NOMBRE = ?,
            DESCRIPCION = ?,
            ORDEN = ?,
            ACTIVO = ?,
            TIPO_VALORES = ?,
            UTILIZA_VALORES_TARIFAS = ?,
            VALOR_INICIAL_P1 = ?,
            VALOR_INICIAL_P2 = ?,
            VALOR_INICIAL_P3 = ?,
            VALOR_INICIAL_P4 = ?,
            EXPRESION_TARIFA1 = ?,
            EXPRESION_TARIFA2 = ?,
            EXPRESION_TARIFA3 = ?,
            EXPRESION_TARIFA4 = ?,
            EXPRESION_TARIFA5 = ?,
            EXPRESION_TARIFA6 = ?,
            GENERA_DESCUENTOS = ?,
            DESCUENTO_INICIAL_GRUPO1 = ?,
            DESCUENTO_INICIAL_GRUPO2 = ?,
            DESCUENTO_INICIAL_GRUPO3 = ?,
            DESCUENTO_INICIAL_GRUPO4 = ?,
            FECHA_ACTUALIZACION = GETDATE()
        WHERE IDFORMULA = ?
          AND ELIMINADO = 0;";

            using (var connexio =
                new OleDbConnection(
                    cadenaConnexio))
            {
                connexio.Open();

                using (OleDbTransaction transaccio =
                    connexio.BeginTransaction())
                {
                    try
                    {
                        /*
                         * Si l'edició desactiva una fórmula que ara
                         * està activa, comprovem que no sigui
                         * l'última fórmula activa disponible.
                         */
                        bool estatActiuActual =
                            ObtenirEstatActiuActual(
                                connexio,
                                transaccio,
                                formula.Id);

                        if (estatActiuActual
                            && !formula.Activa)
                        {
                            ComprovarQueNoSiguiUltimaActiva(
                                connexio,
                                transaccio,
                                formula.Id);
                        }

                        using (var comanda =
                            new OleDbCommand(
                                sql,
                                connexio,
                                transaccio))
                        {
                            /*
                             * OleDb vincula els paràmetres
                             * segons l'ordre dels símbols ?.
                             */
                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarChar,
                                50)
                                .Value =
                                    formula.Codi
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                150)
                                .Value =
                                    formula.Nom
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                1000)
                                .Value =
                                    formula.Descripcio
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.Integer)
                                .Value =
                                    formula.Ordre;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.Boolean)
                                .Value =
                                    formula.Activa;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                100)
                                .Value =
                                    formula.TipusValors
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.Boolean)
                                .Value =
                                    formula.UtilitzaValorsTarifes;

                            AfegirParametreDecimal(
                                comanda,
                                formula.ValorInicialP1,
                                18,
                                4);

                            AfegirParametreDecimal(
                                comanda,
                                formula.ValorInicialP2,
                                18,
                                4);

                            AfegirParametreDecimal(
                                comanda,
                                formula.ValorInicialP3,
                                18,
                                4);

                            AfegirParametreDecimal(
                                comanda,
                                formula.ValorInicialP4,
                                18,
                                4);

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                500)
                                .Value =
                                    formula.ExpressioTarifa1
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                500)
                                .Value =
                                    formula.ExpressioTarifa2
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                500)
                                .Value =
                                    formula.ExpressioTarifa3
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                500)
                                .Value =
                                    formula.ExpressioTarifa4
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                500)
                                .Value =
                                    formula.ExpressioTarifa5
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.VarWChar,
                                500)
                                .Value =
                                    formula.ExpressioTarifa6
                                    ?? string.Empty;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.Boolean)
                                .Value =
                                    formula.GeneraDescomptes;

                            AfegirParametreDecimal(
                                comanda,
                                formula.DescompteInicialGrup1,
                                18,
                                4);

                            AfegirParametreDecimal(
                                comanda,
                                formula.DescompteInicialGrup2,
                                18,
                                4);

                            AfegirParametreDecimal(
                                comanda,
                                formula.DescompteInicialGrup3,
                                18,
                                4);

                            AfegirParametreDecimal(
                                comanda,
                                formula.DescompteInicialGrup4,
                                18,
                                4);

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.Integer)
                                .Value =
                                    formula.Id;

                            int filesAfectades =
                                comanda.ExecuteNonQuery();

                            if (filesAfectades != 1)
                            {
                                throw new InvalidOperationException(
                                    "No s'ha pogut actualitzar la fórmula. "
                                    + "Potser ja no existeix o està eliminada.");
                            }
                        }

                        transaccio.Commit();
                    }
                    catch
                    {
                        transaccio.Rollback();

                        throw;
                    }
                }
            }
        }
        /// <summary>
        /// Retorna quants articles tenen assignada
        /// la fórmula indicada.
        /// </summary>
        public int ComptarArticlesAssignats(
            string cadenaConnexio,
            int idFormula)
        {
            if (string.IsNullOrWhiteSpace(
                cadenaConnexio))
            {
                throw new ArgumentException(
                    "La connexió amb a3ERP és buida.",
                    nameof(cadenaConnexio));
            }

            if (idFormula <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idFormula),
                    idFormula,
                    "L'identificador de la fórmula no és vàlid.");
            }

            const string sql = @"
                SELECT COUNT(*)
                FROM dbo.ARTICULO
                WHERE AT_FORMULA_TARIFA_ID = ?;";

            using (var connexio =
                new OleDbConnection(
                    cadenaConnexio))
            using (var comanda =
                new OleDbCommand(
                    sql,
                    connexio))
            {
                comanda.Parameters.Add(
                    "?",
                    OleDbType.Integer)
                    .Value =
                        idFormula;

                connexio.Open();

                object valor =
                    comanda.ExecuteScalar();

                return Convert.ToInt32(
                    valor,
                    CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Elimina lògicament una fórmula existent.
        ///
        /// L'operació:
        /// - conserva el registre a SQL;
        /// - estableix ELIMINADO = 1;
        /// - estableix ACTIVO = 0;
        /// - informa FECHA_ELIMINACION;
        /// - impedeix eliminar l'última fórmula activa;
        /// - s'executa dins d'una transacció.
        /// </summary>
        public void EliminarFormula(
            string cadenaConnexio,
            int idFormula)
        {
            if (string.IsNullOrWhiteSpace(
                cadenaConnexio))
            {
                throw new ArgumentException(
                    "La connexió amb a3ERP és buida.",
                    nameof(cadenaConnexio));
            }

            if (idFormula <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(idFormula),
                    idFormula,
                    "L'identificador de la fórmula no és vàlid.");
            }

            using (var connexio =
                new OleDbConnection(
                    cadenaConnexio))
            {
                connexio.Open();

                /*
                 * Serializable protegeix la comprovació
                 * de l'última fórmula activa davant
                 * d'operacions simultànies.
                 */
                using (OleDbTransaction transaccio =
                    connexio.BeginTransaction(
                        IsolationLevel.Serializable))
                {
                    try
                    {
                        bool formulaActiva =
                            ObtenirEstatActiuActual(
                                connexio,
                                transaccio,
                                idFormula);

                        /*
                         * Si la fórmula està activa,
                         * comprovem que en quedi alguna altra.
                         */
                        if (formulaActiva)
                        {
                            ComprovarQueNoSiguiUltimaActiva(
                                connexio,
                                transaccio,
                                idFormula);
                        }

                        const string sql = @"
                            UPDATE dbo.AT_ARTICULO_FORMULAS
                            SET
                                ACTIVO = 0,
                                ELIMINADO = 1,
                                FECHA_ACTUALIZACION = GETDATE(),
                                FECHA_ELIMINACION = GETDATE()
                            WHERE IDFORMULA = ?
                              AND ELIMINADO = 0
                              AND NOT EXISTS
                              (
                                  SELECT 1
                                  FROM dbo.ARTICULO
                                  WHERE AT_FORMULA_TARIFA_ID = ?
                              );";

                        using (var comanda =
                            new OleDbCommand(
                                sql,
                                connexio,
                                transaccio))
                        {
                            comanda.Parameters.Add(
                                "?",
                                OleDbType.Integer)
                                .Value =
                                    idFormula;

                            comanda.Parameters.Add(
                                "?",
                                OleDbType.Integer)
                                .Value =
                                    idFormula;

                            int filesAfectades =
                                comanda.ExecuteNonQuery();

                            if (filesAfectades != 1)
                            {
                                throw new InvalidOperationException(
                                    "No s'ha pogut eliminar la fórmula. "
                                    + "Potser ja no existeix, ja estava eliminada "
                                    + "o té articles assignats.");
                            }
                        }

                        transaccio.Commit();
                    }
                    catch
                    {
                        transaccio.Rollback();

                        throw;
                    }
                }
            }
        }
    }
}
