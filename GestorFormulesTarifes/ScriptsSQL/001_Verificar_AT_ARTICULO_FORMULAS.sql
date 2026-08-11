/*
===============================================================================
 Projecte: A3ErpGestorFormulesTarifes
 Script:   001_Verificar_AT_ARTICULO_FORMULAS.sql

 Objectiu:
 Verificar l'estructura de la taula creada mitjançant
 el Diccionari d'a3ERP.

 Aquest script només consulta informació.
 No crea ni modifica estructures o dades.
===============================================================================
*/

SET NOCOUNT ON;

/*
 * Columnes, tipus, longituds, decimals,
 * nul·labilitat i propietat IDENTITY.
 */
SELECT
    C.column_id AS ORDRE_COLUMNA,
    C.name AS COLUMNA,
    T.name AS TIPUS_DADA,

    CASE
        WHEN T.name IN ('nvarchar', 'nchar')
            THEN C.max_length / 2
        WHEN T.name IN ('varchar', 'char')
            THEN C.max_length
        ELSE NULL
    END AS LONGITUD,

    C.precision AS PRECISIO,
    C.scale AS ESCALA,
    C.is_nullable AS ADM_NULL,
    C.is_identity AS ES_IDENTITY,
    DC.definition AS VALOR_PER_DEFECTE
FROM sys.columns C
INNER JOIN sys.types T
    ON T.user_type_id = C.user_type_id
LEFT JOIN sys.default_constraints DC
    ON DC.object_id = C.default_object_id
WHERE C.object_id =
    OBJECT_ID('dbo.AT_ARTICULO_FORMULAS')
ORDER BY C.column_id;

/*
 * Claus i índexs.
 */
SELECT
    I.name AS INDEX_NAME,
    I.is_primary_key,
    I.is_unique,
    IC.key_ordinal,
    C.name AS COLUMNA
FROM sys.indexes I
INNER JOIN sys.index_columns IC
    ON IC.object_id = I.object_id
   AND IC.index_id = I.index_id
INNER JOIN sys.columns C
    ON C.object_id = IC.object_id
   AND C.column_id = IC.column_id
WHERE I.object_id =
    OBJECT_ID('dbo.AT_ARTICULO_FORMULAS')
  AND IC.is_included_column = 0
ORDER BY
    I.name,
    IC.key_ordinal;

/*
 * Dades actuals.
 */
SELECT
    *
FROM dbo.AT_ARTICULO_FORMULAS
ORDER BY
    ELIMINADO,
    ACTIVO DESC,
    ORDEN,
    IDFORMULA;
