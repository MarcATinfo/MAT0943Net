/*
===============================================================================
SCRIPT DE DIAGNÒSTIC — TARIFES I DESCOMPTES D'UN ARTICLE
===============================================================================

Projecte:
    A3ErpCalculadorTarifes

Objectiu:
    Consultar, en una sola execució, les dades d'origen de l'article,
    les sis tarifes gestionades pel Calculador i els quatre descomptes
    per família de client.

Seguretat:
    - Script exclusivament de lectura.
    - No executa INSERT, UPDATE, DELETE ni procediments d'escriptura.
    - No modifica TARIFAVE, DESCUENT ni ARTICULO.

Ús:
    1. Executar-lo sobre la base de dades de l'empresa que es vol revisar.
    2. Informar únicament @CODART_VISIBLE.
    3. Revisar els resultats i la llegenda final de camps.

Nota sobre CODART:
    La consulta localitza l'article amb LTRIM/RTRIM per permetre introduir
    el codi visible. Internament també mostra el literal real guardat a la
    base de dades, inclosos els possibles espais a l'esquerra.

Nota sobre UNIDADES:
    UNIDADES no és la quantitat d'estoc ni la quantitat venuda acumulada.
    És el llindar mínim d'unitats a partir del qual és aplicable aquell
    registre de tarifa o descompte.

    UNIDADES = 0 significa:
    - registre base;
    - aplicable des de zero unitats;
    - sense un mínim de quantitat específic;
    - si existeixen trams superiors, a3ERP pot escollir el tram més alt
      que no superi les unitats de la línia de venda.

===============================================================================
*/

SET NOCOUNT ON;

-- ============================================================================
-- ÚNIC PARÀMETRE D'ENTRADA
-- ============================================================================
DECLARE @CODART_VISIBLE varchar(15);

SET @CODART_VISIBLE = '16';


-- ============================================================================
-- RESOLUCIÓ DEL CODI LITERAL REAL DE LA BASE DE DADES
-- ============================================================================
DECLARE @CODART_BD varchar(15);

SELECT TOP 1
    @CODART_BD = A.CODART
FROM dbo.ARTICULO AS A
WHERE LTRIM(RTRIM(A.CODART)) =
      LTRIM(RTRIM(@CODART_VISIBLE))
ORDER BY A.CODART;

IF @CODART_BD IS NULL
BEGIN
    RAISERROR(
        'No s''ha trobat cap article amb el codi visible indicat.',
        16,
        1
    );

    RETURN;
END;


-- ============================================================================
-- 1. CONTEXT DE LA VERIFICACIÓ
-- ============================================================================
SELECT
    DB_NAME() AS BASE_DADES,
    LTRIM(RTRIM(@CODART_VISIBLE)) AS CODART_CERCAT,
    LTRIM(RTRIM(@CODART_BD)) AS CODART_VISIBLE,
    '[' + @CODART_BD + ']' AS CODART_LITERAL_BD,
    LEN(@CODART_BD) AS LONGITUD_SENSE_ESPAIS_FINALS,
    DATALENGTH(@CODART_BD) AS BYTES_LITERAL_BD,
    (
        SELECT TOP 1
            DC.NUMDECPRC
        FROM dbo.DATOSCONFIG AS DC
    ) AS NUM_DECIMALS_PREUS;

-- Interpretació:
-- CODART_LITERAL_BD:
--     Els claudàtors permeten veure si el codi conté espais a l'esquerra.
--
-- NUM_DECIMALS_PREUS:
--     Nombre de decimals amb què el Calculador arrodoneix els preus abans
--     de persistir-los a TARIFAVE.


-- ============================================================================
-- 2. DADES D'ORIGEN DE L'ARTICLE
-- ============================================================================
SELECT
    LTRIM(RTRIM(A.CODART)) AS CODART,
    '[' + A.CODART + ']' AS CODART_LITERAL_BD,
    A.DESCART AS DESCRIPCIO,
    A.PRCCOMPRA AS PRCCOMPRA,
    A.PRCCOSTE AS PRCCOSTE,
    A.PRCSTANDARD AS PRCSTANDARD_TRANSPORT
FROM dbo.ARTICULO AS A
WHERE A.CODART = @CODART_BD;

-- Descripció dels camps:
-- PRCCOMPRA:
--     Preu de compra de l'article.
--
-- PRCCOSTE:
--     Preu de cost utilitzat per les fórmules del Calculador.
--
-- PRCSTANDARD_TRANSPORT:
--     En aquest projecte, ARTICULO.PRCSTANDARD s'interpreta com l'import
--     de transport. És la variable PRCSTANDARD del motor configurable.


-- ============================================================================
-- 3. TARIFES ESTÀNDARD GESTIONADES PEL CALCULADOR
-- ============================================================================
SELECT
    LTRIM(RTRIM(TV.CODART)) AS CODART,
    LTRIM(RTRIM(TV.TARIFA)) AS TARIFA,
    TV.PRECIO AS PREU,
    LTRIM(RTRIM(TV.CODMON)) AS MONEDA,
    TV.FECMIN AS DATA_INICI_VIGENCIA,
    TV.FECMAX AS DATA_FINAL_VIGENCIA,
    TV.UNIDADES AS UNITATS_MINIMES,
    CASE
        WHEN TV.UNIDADES = 0
            THEN 'Registre base: aplicable des de 0 unitats.'
        ELSE
            'Tram aplicable a partir de '
            + CONVERT(varchar(30), TV.UNIDADES)
            + ' unitats.'
    END AS SIGNIFICAT_UNITATS,
    CASE
        WHEN GETDATE() >= TV.FECMIN
         AND GETDATE() <= TV.FECMAX
            THEN 'Sí'
        ELSE 'No'
    END AS VIGENT_AVUI,
    TV.IDTARIFAV AS IDENTIFICADOR_A3ERP
FROM dbo.TARIFAVE AS TV
WHERE TV.CODART = @CODART_BD
  AND LTRIM(RTRIM(TV.TARIFA)) IN
      ('1', '2', '3', '4', '5', '6')
  AND LTRIM(RTRIM(TV.CODMON)) = 'EURO'
  AND TV.FECMIN = CONVERT(datetime, '19000101', 112)
  AND TV.FECMAX = CONVERT(datetime, '99991231', 112)
  AND TV.UNIDADES = 0
ORDER BY
    CASE LTRIM(RTRIM(TV.TARIFA))
        WHEN '1' THEN 1
        WHEN '2' THEN 2
        WHEN '3' THEN 3
        WHEN '4' THEN 4
        WHEN '5' THEN 5
        WHEN '6' THEN 6
        ELSE 99
    END;

-- Clau funcional utilitzada pel Calculador:
-- TARIFA   = 1, 2, 3, 4, 5 o 6
-- CODART   = literal real de l'article
-- CODMON   = EURO
-- FECMIN   = 1900-01-01
-- FECMAX   = 9999-12-31
-- UNIDADES = 0
--
-- PREU:
--     Import persistit a TARIFAVE després d'arrodonir-lo segons
--     DATOSCONFIG.NUMDECPRC.
--
-- IDTARIFAV:
--     Identificador intern gestionat pel procediment oficial
--     dbo.ActualizarTarifa mitjançant NextValue.


-- ============================================================================
-- 4. DESCOMPTES ESTÀNDARD PER FAMÍLIA DE CLIENT
-- ============================================================================
SELECT
    D.ID AS IDENTIFICADOR,
    LTRIM(RTRIM(D.CODART)) AS CODART,
    D.TIPREG AS TIPUS_REGISTRE,
    LTRIM(RTRIM(D.FAMCLI)) AS FAMILIA_CLIENT,
    CASE LTRIM(RTRIM(D.FAMCLI))
        WHEN '1' THEN 'Família de descompte de client 1'
        WHEN '2' THEN 'Família de descompte de client 2'
        WHEN '3' THEN 'Família de descompte de client 3'
        WHEN '4' THEN 'Família de descompte de client 4'
        ELSE 'Família de descompte no estàndard'
    END AS DESCRIPCIO_FAMILIA_CLIENT,
    D.DESC1 AS DESCOMPTE_1_PERCENT,
    D.DESC2 AS DESCOMPTE_2_PERCENT,
    D.DESC3 AS DESCOMPTE_3_PERCENT,
    D.DESC4 AS DESCOMPTE_4_PERCENT,
    D.UNIDADES AS UNITATS_MINIMES,
    CASE
        WHEN D.UNIDADES = 0
            THEN 'Registre base: aplicable des de 0 unitats.'
        ELSE
            'Tram aplicable a partir de '
            + CONVERT(varchar(30), D.UNIDADES)
            + ' unitats.'
    END AS SIGNIFICAT_UNITATS,
    D.FECMIN AS DATA_INICI_VIGENCIA,
    D.FECMAX AS DATA_FINAL_VIGENCIA,
    CASE
        WHEN GETDATE() >= D.FECMIN
         AND GETDATE() <= D.FECMAX
            THEN 'Sí'
        ELSE 'No'
    END AS VIGENT_AVUI
FROM dbo.DESCUENT AS D
WHERE D.CODART = @CODART_BD
  AND D.TIPREG = 'AF'
  AND LTRIM(RTRIM(D.FAMCLI)) IN
      ('1', '2', '3', '4')
  AND D.FECMIN = CONVERT(datetime, '19000101', 112)
  AND D.FECMAX = CONVERT(datetime, '99991231', 112)
  AND D.UNIDADES = 0
ORDER BY
    CASE LTRIM(RTRIM(D.FAMCLI))
        WHEN '1' THEN 1
        WHEN '2' THEN 2
        WHEN '3' THEN 3
        WHEN '4' THEN 4
        ELSE 99
    END;

-- Descripció dels camps:
-- TIPREG = AF:
--     Descompte vinculat a un Article i una Família de client.
--
-- FAMCLI:
--     Família de descompte del client. Ha de correspondre amb
--     CLIENTES.FAMCLIDESC.
--
-- DESC1, DESC2, DESC3 i DESC4:
--     Camps de percentatge de descompte encadenat admesos per a3ERP.
--     El Calculador actual desa el percentatge configurat a DESC1
--     i deixa DESC2, DESC3 i DESC4 a zero.
--
-- UNIDADES = 0:
--     Descompte base aplicable des de zero unitats, sense un mínim
--     específic de quantitat.
--
-- FECMIN / FECMAX:
--     Interval de vigència. El patró 1900-01-01 / 9999-12-31 indica
--     una vigència general sense una data comercial limitada.


-- ============================================================================
-- 5. ALTRES TARIFES DE L'ARTICLE FORA DEL PATRÓ DEL CALCULADOR
-- ============================================================================
SELECT
    LTRIM(RTRIM(TV.CODART)) AS CODART,
    LTRIM(RTRIM(TV.TARIFA)) AS TARIFA,
    TV.PRECIO AS PREU,
    LTRIM(RTRIM(TV.CODMON)) AS MONEDA,
    TV.FECMIN AS DATA_INICI_VIGENCIA,
    TV.FECMAX AS DATA_FINAL_VIGENCIA,
    TV.UNIDADES AS UNITATS_MINIMES,
    TV.IDTARIFAV AS IDENTIFICADOR_A3ERP
FROM dbo.TARIFAVE AS TV
WHERE TV.CODART = @CODART_BD
  AND NOT
  (
      LTRIM(RTRIM(TV.TARIFA)) IN
          ('1', '2', '3', '4', '5', '6')
      AND LTRIM(RTRIM(TV.CODMON)) = 'EURO'
      AND TV.FECMIN = CONVERT(datetime, '19000101', 112)
      AND TV.FECMAX = CONVERT(datetime, '99991231', 112)
      AND TV.UNIDADES = 0
  )
ORDER BY
    LTRIM(RTRIM(TV.TARIFA)),
    TV.UNIDADES,
    TV.FECMIN;

-- Aquest resultat és només informatiu.
-- Pot mostrar trams per quantitat, altres monedes, altres dates o tarifes
-- que el Calculador no modifica.


-- ============================================================================
-- 6. ALTRES DESCOMPTES DE L'ARTICLE FORA DEL PATRÓ DEL CALCULADOR
-- ============================================================================
SELECT
    D.ID AS IDENTIFICADOR,
    LTRIM(RTRIM(D.CODART)) AS CODART,
    D.TIPREG AS TIPUS_REGISTRE,
    LTRIM(RTRIM(D.CODCLI)) AS CLIENT,
    LTRIM(RTRIM(D.CODPRO)) AS PROVEIDOR,
    LTRIM(RTRIM(D.FAMART)) AS FAMILIA_ARTICLE,
    LTRIM(RTRIM(D.FAMCLI)) AS FAMILIA_CLIENT,
    D.DESC1 AS DESCOMPTE_1_PERCENT,
    D.DESC2 AS DESCOMPTE_2_PERCENT,
    D.DESC3 AS DESCOMPTE_3_PERCENT,
    D.DESC4 AS DESCOMPTE_4_PERCENT,
    D.UNIDADES AS UNITATS_MINIMES,
    D.FECMIN AS DATA_INICI_VIGENCIA,
    D.FECMAX AS DATA_FINAL_VIGENCIA
FROM dbo.DESCUENT AS D
WHERE D.CODART = @CODART_BD
  AND NOT
  (
      D.TIPREG = 'AF'
      AND LTRIM(RTRIM(D.FAMCLI)) IN
          ('1', '2', '3', '4')
      AND D.FECMIN = CONVERT(datetime, '19000101', 112)
      AND D.FECMAX = CONVERT(datetime, '99991231', 112)
      AND D.UNIDADES = 0
  )
ORDER BY
    D.TIPREG,
    D.FAMCLI,
    D.UNIDADES,
    D.FECMIN;

-- Aquest resultat és només informatiu.
-- Pot contenir altres tipus de descompte legítims d'a3ERP.
-- El Calculador no els elimina ni els modifica.


-- ============================================================================
-- 7. LLEGENDA DE CAMPS
-- ============================================================================
SELECT
    X.TAULA,
    X.CAMP,
    X.DESCRIPCIO
FROM
(
    SELECT
        1 AS ORDRE,
        'ARTICULO' AS TAULA,
        'CODART' AS CAMP,
        'Codi literal de l''article. Pot contenir espais a l''esquerra; no s''ha de reconstruir com a número.' AS DESCRIPCIO

    UNION ALL SELECT
        2,
        'ARTICULO',
        'PRCCOMPRA',
        'Preu de compra utilitzat per les fórmules configurables.'

    UNION ALL SELECT
        3,
        'ARTICULO',
        'PRCCOSTE',
        'Preu de cost utilitzat pel motor i, habitualment, per a la tarifa 5.'

    UNION ALL SELECT
        4,
        'ARTICULO',
        'PRCSTANDARD',
        'En aquest projecte representa l''import de transport i participa habitualment en la tarifa 6.'

    UNION ALL SELECT
        5,
        'TARIFAVE',
        'TARIFA',
        'Codi de tarifa comercial. El Calculador gestiona els codis 1 a 6.'

    UNION ALL SELECT
        6,
        'TARIFAVE',
        'PRECIO',
        'Preu final persistit per a la tarifa, arrodonit segons DATOSCONFIG.NUMDECPRC.'

    UNION ALL SELECT
        7,
        'TARIFAVE',
        'CODMON',
        'Moneda de la tarifa. El Calculador utilitza EURO.'

    UNION ALL SELECT
        8,
        'TARIFAVE',
        'FECMIN / FECMAX',
        'Interval inclusiu de vigència del registre de tarifa.'

    UNION ALL SELECT
        9,
        'TARIFAVE',
        'UNIDADES',
        'Llindar mínim d''unitats. El valor 0 identifica la tarifa base aplicable des de zero unitats.'

    UNION ALL SELECT
        10,
        'TARIFAVE',
        'IDTARIFAV',
        'Identificador intern d''a3ERP, gestionat pel procediment oficial dbo.ActualizarTarifa.'

    UNION ALL SELECT
        11,
        'DESCUENT',
        'TIPREG',
        'Tipus de regla de descompte. AF significa Article + Família de client.'

    UNION ALL SELECT
        12,
        'DESCUENT',
        'FAMCLI',
        'Família de descompte del client; es relaciona funcionalment amb CLIENTES.FAMCLIDESC.'

    UNION ALL SELECT
        13,
        'DESCUENT',
        'DESC1',
        'Primer percentatge de descompte. És el camp que utilitza el Calculador actual.'

    UNION ALL SELECT
        14,
        'DESCUENT',
        'DESC2 / DESC3 / DESC4',
        'Percentatges addicionals de descompte encadenat. El Calculador actual els deixa a zero.'

    UNION ALL SELECT
        15,
        'DESCUENT',
        'UNIDADES',
        'Llindar mínim d''unitats del descompte. El valor 0 identifica el descompte base.'

    UNION ALL SELECT
        16,
        'DESCUENT',
        'FECMIN / FECMAX',
        'Interval inclusiu de vigència de la regla de descompte.'

    UNION ALL SELECT
        17,
        'CLIENTES',
        'TARIFA',
        'Tarifa comercial assignada al client; selecciona la fila corresponent de TARIFAVE.'

    UNION ALL SELECT
        18,
        'CLIENTES',
        'FAMCLIDESC',
        'Família de descompte del client; s''ha de correspondre amb DESCUENT.FAMCLI.'
) AS X
ORDER BY
    X.ORDRE;
