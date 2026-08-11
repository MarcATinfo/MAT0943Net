/*
===============================================================================
SCRIPT PRIVAT DE MANTENIMENT - VERSIÓ 2
Recuperació d'una fórmula eliminada lògicament mitjançant l'ID
===============================================================================

ÚS:
1. Executar sobre la base de dades correcta.
2. Informar únicament @IDFormula.

NO CAL INFORMAR CAP CODI.

El script:
- recupera el CODIGO actual de la fórmula eliminada;
- genera automàticament CODIGO + " (RECUPERADA)";
- genera automàticament NOMBRE + " (RECUPERADA)";
- deixa la fórmula inactiva;
- posa ELIMINADO = 0;
- posa FECHA_ELIMINACION = NULL;
- actualitza FECHA_ACTUALIZACION;
- conserva expressions, valors, descomptes i ordre.
===============================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @IDFormula INT = 0;  -- Exemple: 9

DECLARE @SufixCodi VARCHAR(20);
DECLARE @SufixNom NVARCHAR(20);
DECLARE @LongitudMaximaCodi INT;
DECLARE @LongitudMaximaNom INT;

DECLARE @CodiAnterior VARCHAR(50);
DECLARE @CodiRecuperat VARCHAR(50);

SET @SufixCodi = ' (RECUPERADA)';
SET @SufixNom = N' (RECUPERADA)';
SET @LongitudMaximaCodi = 50;
SET @LongitudMaximaNom = 150;

BEGIN TRY
    BEGIN TRANSACTION;

    IF @IDFormula <= 0
    BEGIN
        RAISERROR(
            'Cal informar un ID de fórmula vàlid.',
            16,
            1
        );
    END;

    /*
     * Recuperem el codi directament a partir de l'ID.
     */
    SELECT
        @CodiAnterior = CODIGO
    FROM dbo.AT_ARTICULO_FORMULAS WITH (UPDLOCK, HOLDLOCK)
    WHERE IDFORMULA = @IDFormula
      AND ELIMINADO = 1;

    IF @CodiAnterior IS NULL
    BEGIN
        RAISERROR(
            'La fórmula indicada no existeix o no està marcada com a eliminada.',
            16,
            1
        );
    END;

    SET @CodiAnterior = LTRIM(RTRIM(@CodiAnterior));

    /*
     * Generem automàticament el codi recuperat.
     * Si el sufix ja existeix, no el repetim.
     */
    IF RIGHT(@CodiAnterior, LEN(@SufixCodi)) = @SufixCodi
    BEGIN
        SET @CodiRecuperat =
            LEFT(
                @CodiAnterior,
                @LongitudMaximaCodi
            );
    END
    ELSE
    BEGIN
        SET @CodiRecuperat =
            LEFT(
                @CodiAnterior,
                @LongitudMaximaCodi - LEN(@SufixCodi)
            )
            + @SufixCodi;
    END;

    /*
     * El codi recuperat no pot estar ocupat
     * per una fórmula vigent.
     */
    IF EXISTS
    (
        SELECT 1
        FROM dbo.AT_ARTICULO_FORMULAS WITH (UPDLOCK, HOLDLOCK)
        WHERE CODIGO = @CodiRecuperat
          AND ELIMINADO = 0
          AND IDFORMULA <> @IDFormula
    )
    BEGIN
        RAISERROR(
            'Ja existeix una fórmula no eliminada amb el codi recuperat generat.',
            16,
            1
        );
    END;

    UPDATE dbo.AT_ARTICULO_FORMULAS
    SET
        CODIGO = @CodiRecuperat,
        NOMBRE =
            CASE
                WHEN RIGHT(RTRIM(NOMBRE), LEN(@SufixNom)) = @SufixNom
                    THEN RTRIM(NOMBRE)
                ELSE
                    LEFT(
                        RTRIM(NOMBRE),
                        @LongitudMaximaNom - LEN(@SufixNom)
                    )
                    + @SufixNom
            END,
        ACTIVO = 0,
        ELIMINADO = 0,
        FECHA_ACTUALIZACION = GETDATE(),
        FECHA_ELIMINACION = NULL
    WHERE IDFORMULA = @IDFormula
      AND ELIMINADO = 1;

    IF @@ROWCOUNT <> 1
    BEGIN
        RAISERROR(
            'No s''ha pogut recuperar la fórmula indicada.',
            16,
            1
        );
    END;

    COMMIT TRANSACTION;

    SELECT
        IDFORMULA,
        CODIGO,
        NOMBRE,
        ORDEN,
        ACTIVO,
        ELIMINADO,
        FECHA_ALTA,
        FECHA_ACTUALIZACION,
        FECHA_ELIMINACION
    FROM dbo.AT_ARTICULO_FORMULAS
    WHERE IDFORMULA = @IDFormula;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    DECLARE @MissatgeError NVARCHAR(4000);
    DECLARE @SeveritatError INT;
    DECLARE @EstatError INT;

    SELECT
        @MissatgeError = ERROR_MESSAGE(),
        @SeveritatError = ERROR_SEVERITY(),
        @EstatError = ERROR_STATE();

    RAISERROR(
        @MissatgeError,
        @SeveritatError,
        @EstatError
    );
END CATCH;
