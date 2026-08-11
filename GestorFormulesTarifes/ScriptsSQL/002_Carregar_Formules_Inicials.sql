/*
===============================================================================
002_Carregar_Formules_Inicials.sql
===============================================================================

Projecte:
    A3ErpGestorFormulesTarifes

Objectiu:
    Carregar de manera idempotent les set fórmules originals del Calculador
    a dbo.AT_ARTICULO_FORMULAS.

Comportament:
    - Insereix una fórmula només si no existeix cap registre vigent
      (ELIMINADO = 0) amb el mateix CODIGO.
    - No modifica fórmules existents.
    - No reactiva fórmules desactivades.
    - No recupera fórmules eliminades.
    - Es pot executar més d'una vegada sense duplicar registres vigents.

Resultat esperat:
    Primera execució en una taula buida:
        FORMULES_INSERIDES = 7
        TOTAL_FORMULES_VIGENTS = 7

    Execucions posteriors:
        FORMULES_INSERIDES = 0
        TOTAL_FORMULES_VIGENTS = 7

Important:
    La recuperació excepcional d'una fórmula eliminada correspon al script 003,
    no a aquest fitxer.
===============================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF OBJECT_ID(N'dbo.AT_ARTICULO_FORMULAS', N'U') IS NULL
BEGIN
    RAISERROR(
        'No existeix la taula dbo.AT_ARTICULO_FORMULAS.',
        16,
        1
    );

    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @FormulesInicials TABLE
    (
        CODIGO varchar(50) NOT NULL,
        NOMBRE nvarchar(150) NOT NULL,
        DESCRIPCION nvarchar(1000) NOT NULL,
        ORDEN int NOT NULL,
        ACTIVO bit NOT NULL,
        ELIMINADO bit NOT NULL,
        TIPO_VALORES nvarchar(100) NOT NULL,
        UTILIZA_VALORES_TARIFAS bit NOT NULL,
        VALOR_INICIAL_P1 decimal(18, 4) NOT NULL,
        VALOR_INICIAL_P2 decimal(18, 4) NOT NULL,
        VALOR_INICIAL_P3 decimal(18, 4) NOT NULL,
        VALOR_INICIAL_P4 decimal(18, 4) NOT NULL,
        EXPRESION_TARIFA1 nvarchar(500) NOT NULL,
        EXPRESION_TARIFA2 nvarchar(500) NOT NULL,
        EXPRESION_TARIFA3 nvarchar(500) NOT NULL,
        EXPRESION_TARIFA4 nvarchar(500) NOT NULL,
        EXPRESION_TARIFA5 nvarchar(500) NOT NULL,
        EXPRESION_TARIFA6 nvarchar(500) NOT NULL,
        GENERA_DESCUENTOS bit NOT NULL,
        DESCUENTO_INICIAL_GRUPO1 decimal(18, 4) NOT NULL,
        DESCUENTO_INICIAL_GRUPO2 decimal(18, 4) NOT NULL,
        DESCUENTO_INICIAL_GRUPO3 decimal(18, 4) NOT NULL,
        DESCUENTO_INICIAL_GRUPO4 decimal(18, 4) NOT NULL
    );

    INSERT INTO @FormulesInicials
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
        DESCUENTO_INICIAL_GRUPO4
    )
    VALUES
    (
        'COST_DIVIDIT_COEFICIENTS',
        N'Cost dividit per coeficients',
        N'Calcula les tarifes 1–4 dividint PRCCOSTE pels coeficients indicats.',
        1,
        1,
        0,
        N'Coeficients divisors',
        1,
        0.6500,
        0.7000,
        0.7500,
        0.8000,
        N'PRCCOSTE / P1',
        N'PRCCOSTE / P2',
        N'PRCCOSTE / P3',
        N'PRCCOSTE / P4',
        N'PRCCOSTE',
        N'PRCCOSTE + PRCSTANDARD',
        0,
        0.0000,
        0.0000,
        0.0000,
        0.0000
    ),
    (
        'COST_MES_PERCENTATGES',
        N'Cost més percentatges',
        N'Calcula les tarifes 1–4 incrementant PRCCOSTE segons els percentatges indicats.',
        2,
        1,
        0,
        N'Percentatges d''increment',
        1,
        35.0000,
        30.0000,
        25.0000,
        20.0000,
        N'PRCCOSTE * (1 + P1 / 100)',
        N'PRCCOSTE * (1 + P2 / 100)',
        N'PRCCOSTE * (1 + P3 / 100)',
        N'PRCCOSTE * (1 + P4 / 100)',
        N'PRCCOSTE',
        N'PRCCOSTE + PRCSTANDARD',
        0,
        0.0000,
        0.0000,
        0.0000,
        0.0000
    ),
    (
        'PREU_COMPRA_AMB_DESCOMPTES',
        N'Preu compra amb descomptes',
        N'Utilitza PRCCOMPRA com a preu de les tarifes 1–4 i genera descomptes per als grups 1–4.',
        3,
        1,
        0,
        N'No aplica',
        0,
        0.0000,
        0.0000,
        0.0000,
        0.0000,
        N'PRCCOMPRA',
        N'PRCCOMPRA',
        N'PRCCOMPRA',
        N'PRCCOMPRA',
        N'PRCCOSTE',
        N'PRCCOSTE + PRCSTANDARD',
        1,
        10.0000,
        15.0000,
        20.0000,
        25.0000
    ),
    (
        'PREU_COMPRA_DOBLE_AMB_DESCOMPTES',
        N'Preu compra × 2 amb descomptes',
        N'Utilitza PRCCOMPRA multiplicat per 2 com a preu de les tarifes 1–4 i genera descomptes per als grups 1–4.',
        4,
        1,
        0,
        N'No aplica',
        0,
        0.0000,
        0.0000,
        0.0000,
        0.0000,
        N'PRCCOMPRA * 2',
        N'PRCCOMPRA * 2',
        N'PRCCOMPRA * 2',
        N'PRCCOMPRA * 2',
        N'PRCCOSTE',
        N'PRCCOSTE + PRCSTANDARD',
        1,
        10.0000,
        15.0000,
        20.0000,
        25.0000
    ),
    (
        'COST_DIVIDIT_COEFICIENTS_ALTERNATIUS',
        N'Cost dividit per coeficients alternatius',
        N'Calcula les tarifes 1–4 dividint PRCCOSTE pels coeficients alternatius.',
        5,
        1,
        0,
        N'Coeficients divisors',
        1,
        0.5500,
        0.6000,
        0.6500,
        0.7000,
        N'PRCCOSTE / P1',
        N'PRCCOSTE / P2',
        N'PRCCOSTE / P3',
        N'PRCCOSTE / P4',
        N'PRCCOSTE',
        N'PRCCOSTE + PRCSTANDARD',
        0,
        0.0000,
        0.0000,
        0.0000,
        0.0000
    ),
    (
        'COST_MES_IMPORTS_FIXOS',
        N'Cost més imports fixos',
        N'Calcula les tarifes 1–4 sumant un import fix diferent a PRCCOSTE.',
        6,
        1,
        0,
        N'Imports fixos',
        1,
        10.0000,
        9.0000,
        7.0000,
        6.0000,
        N'PRCCOSTE + P1',
        N'PRCCOSTE + P2',
        N'PRCCOSTE + P3',
        N'PRCCOSTE + P4',
        N'PRCCOSTE',
        N'PRCCOSTE + PRCSTANDARD',
        0,
        0.0000,
        0.0000,
        0.0000,
        0.0000
    ),
    (
        'COST_MES_PERCENTATGES_ALTERNATIUS',
        N'Cost més percentatges alternatius',
        N'Calcula les tarifes 1–4 incrementant PRCCOSTE amb els percentatges alternatius.',
        7,
        1,
        0,
        N'Percentatges d''increment',
        1,
        40.0000,
        35.0000,
        30.0000,
        25.0000,
        N'PRCCOSTE * (1 + P1 / 100)',
        N'PRCCOSTE * (1 + P2 / 100)',
        N'PRCCOSTE * (1 + P3 / 100)',
        N'PRCCOSTE * (1 + P4 / 100)',
        N'PRCCOSTE',
        N'PRCCOSTE + PRCSTANDARD',
        0,
        0.0000,
        0.0000,
        0.0000,
        0.0000
    );

    INSERT INTO dbo.AT_ARTICULO_FORMULAS
    (
        ACTIVO,
        CODIGO,
        DESCRIPCION,
        DESCUENTO_INICIAL_GRUPO1,
        DESCUENTO_INICIAL_GRUPO2,
        DESCUENTO_INICIAL_GRUPO3,
        DESCUENTO_INICIAL_GRUPO4,
        ELIMINADO,
        EXPRESION_TARIFA1,
        EXPRESION_TARIFA2,
        EXPRESION_TARIFA3,
        EXPRESION_TARIFA4,
        EXPRESION_TARIFA5,
        EXPRESION_TARIFA6,
        FECHA_ACTUALIZACION,
        FECHA_ALTA,
        FECHA_ELIMINACION,
        GENERA_DESCUENTOS,
        NOMBRE,
        ORDEN,
        TIPO_VALORES,
        UTILIZA_VALORES_TARIFAS,
        VALOR_INICIAL_P1,
        VALOR_INICIAL_P2,
        VALOR_INICIAL_P3,
        VALOR_INICIAL_P4
    )
    SELECT
        S.ACTIVO,
        S.CODIGO,
        S.DESCRIPCION,
        S.DESCUENTO_INICIAL_GRUPO1,
        S.DESCUENTO_INICIAL_GRUPO2,
        S.DESCUENTO_INICIAL_GRUPO3,
        S.DESCUENTO_INICIAL_GRUPO4,
        S.ELIMINADO,
        S.EXPRESION_TARIFA1,
        S.EXPRESION_TARIFA2,
        S.EXPRESION_TARIFA3,
        S.EXPRESION_TARIFA4,
        S.EXPRESION_TARIFA5,
        S.EXPRESION_TARIFA6,
        NULL,
        GETDATE(),
        NULL,
        S.GENERA_DESCUENTOS,
        S.NOMBRE,
        S.ORDEN,
        S.TIPO_VALORES,
        S.UTILIZA_VALORES_TARIFAS,
        S.VALOR_INICIAL_P1,
        S.VALOR_INICIAL_P2,
        S.VALOR_INICIAL_P3,
        S.VALOR_INICIAL_P4
    FROM @FormulesInicials AS S
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.AT_ARTICULO_FORMULAS AS F
        WHERE F.CODIGO = S.CODIGO
          AND F.ELIMINADO = 0
    );

    DECLARE @FormulesInserides int;

    SET @FormulesInserides = @@ROWCOUNT;

    COMMIT TRANSACTION;

    SELECT
        @FormulesInserides AS FORMULES_INSERIDES,
        COUNT(*) AS TOTAL_FORMULES_VIGENTS
    FROM dbo.AT_ARTICULO_FORMULAS AS F
    INNER JOIN @FormulesInicials AS S
        ON S.CODIGO = F.CODIGO
    WHERE F.ELIMINADO = 0;

    SELECT
        F.IDFORMULA,
        F.ORDEN,
        F.CODIGO,
        F.NOMBRE,
        F.ACTIVO,
        F.ELIMINADO,
        F.TIPO_VALORES,
        F.UTILIZA_VALORES_TARIFAS,
        F.GENERA_DESCUENTOS
    FROM dbo.AT_ARTICULO_FORMULAS AS F
    INNER JOIN @FormulesInicials AS S
        ON S.CODIGO = F.CODIGO
    WHERE F.ELIMINADO = 0
    ORDER BY
        F.ORDEN,
        F.IDFORMULA;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    DECLARE @ErrorMessage nvarchar(4000);
    DECLARE @ErrorSeverity int;
    DECLARE @ErrorState int;

    SELECT
        @ErrorMessage = ERROR_MESSAGE(),
        @ErrorSeverity = ERROR_SEVERITY(),
        @ErrorState = ERROR_STATE();

    RAISERROR(
        @ErrorMessage,
        @ErrorSeverity,
        @ErrorState
    );
END CATCH;
