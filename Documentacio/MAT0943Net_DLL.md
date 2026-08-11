# MAT0943Net.dll — Documentació tècnica i operativa

**Empresa / base de dades:** `CPORRAS`  
**Entorn:** a3ERP · SQL Server · Windows  
**Plataforma:** .NET Framework 4.7.2 · WinForms · x86  
**DLL:** `MAT0943Net.dll`  
**Versió funcional documentada:** 2.0 · 10/08/2026  
**Fitxer Markdown generat:** 11/08/2026  
**Ús:** intern / manteniment / suport / desplegament

> `MAT0943Net.dll` integra l'Importador d'articles, el Calculador de tarifes i el Gestor de fórmules. Coexisteix amb la DLL Delphi `MAT0943.dll`.

## Índex ràpid

- [1. Visió general i principis compartits](#1-visió-general-i-principis-compartits)
- [2. Importador d’articles](#2-importador-darticles)
- [3. Calculador de tarifes](#3-calculador-de-tarifes)
- [4. Gestor de fórmules](#4-gestor-de-fórmules)
- [5. Desplegament conjunt i integració amb a3ERP](#5-desplegament-conjunt-i-integració-amb-a3erp)
- [6. Configuració SQL de referència](#6-configuració-sql-de-referència)
- [7. Llistes de comprovació operativa](#7-llistes-de-comprovació-operativa)
- [8. Guia operativa d’usuari](#8-guia-operativa-dusuari)
- [9. Referències principals del projecte](#9-referències-principals-del-projecte)

---

## 1. Visió general i principis compartits

MAT0943Net.dll amplia a3ERP amb tres mòduls coordinats: Importador d’articles, Calculador de tarifes i Gestor de fórmules. Comparteixen el context de l’empresa activa, la integració COM x86 i una política de diagnòstic basada en logs independents i segurs.

### 1.1. Components principals

| **Component**             | **Tipus**                          | **Responsabilitat**                                                                       |
|---------------------------|------------------------------------|-------------------------------------------------------------------------------------------|
| **MAT0943Net.dll**        | DLL COM x86 · .NET Framework 4.7.2 | Punt d’entrada únic des d’a3ERP per als tres mòduls. Coexisteix amb MAT0943.dll (Delphi). |
| **ImportadorArticles**    | Mòdul WinForms                     | Llegeix Excel/CSV, valida dades i crea o actualitza articles mitjançant ActiveX.          |
| **CalculadorTarifes**     | Mòdul WinForms                     | Carrega articles per IDFORMULA, previsualitza i aplica tarifes/descomptes.                |
| **GestorFormulesTarifes** | Mòdul WinForms                     | Manté les fórmules configurables, estat, ordre, expressions i descomptes.                 |
| **SQL Server / BD CPORRAS** | Persistència i configuració      | Proporciona dades mestres, AT_ARTICULO_FORMULAS, TARIFAVE, DESCUENT i configuració.       |
| **a3ERP ActiveX**         | API d’integració                   | Canal oficial utilitzat per crear o actualitzar el mestre d’articles.                     |

### 1.2. Principis tècnics

- Compilació x86 obligatòria per compatibilitat amb a3ERP i els components COM/ActiveX.

- La connexió i l’empresa activa es reben des del punt d’entrada; no s’han d’exposar credencials als logs.

- La lectura SQL s’utilitza per validar, consultar existències i verificar resultats; les escriptures segueixen el canal oficial de cada mòdul.

- Cada mòdul disposa d’un fitxer de log propi i d’un mecanisme local de reserva independent.

- Les proves funcionals descrites en aquesta versió s’han realitzat a CPORRAS en entorn de proves abans de considerar estable cada canvi.

- No s’ha de canviar el ProgId ni el GUID de la DLL MAT0943Net durant actualitzacions ordinàries.

### 1.3. Patró de connexió

> **1.** Obtenir la connexió de l’empresa activa mitjançant el context rebut des d’a3ERP.
>
> **2.** Provar que la connexió és vàlida i apunta a la base de dades esperada.
>
> **3.** Aplicar l’alternativa prevista al mateix servidor/base de dades quan el context ho requereixi.
>
> **4.** Passar la connexió resolta al formulari i als serveis; el formulari no ha d’iniciar una segona instància d’Enlace.

## 2. Importador d’articles

L’Importador d’articles permet carregar informació des d’un fitxer Excel o CSV, analitzar-la abans de modificar l’ERP i crear o actualitzar els registres del mestre ARTICULO. La validació prèvia redueix errors d’ActiveX i evita processar files que ja se sap que són incorrectes.

### 2.1. Objectiu i arquitectura

| **Fase**          | **Component principal**                  | **Resultat**                                                                          |
|-------------------|------------------------------------------|---------------------------------------------------------------------------------------|
| **1. Obertura**   | Principal.cs → PuntEntradaImportador     | Rep empresa, base de dades i connexió; inicialitza el log i obre el formulari.        |
| **2. Lectura**    | FrmImportadorArticles + lector Excel/CSV | Llegeix el full o fitxer i converteix cada fila al model intern.                      |
| **3. Validació**  | ServeiValidacioArticlesA3Erp             | Classifica files com a altes, actualitzacions, sense canvis o errors.                 |
| **4. Consulta**   | RepositoriArticlesA3Erp                  | Consulta ARTICULO i mestres auxiliars; carrega els codis CAR1 vàlids una sola vegada. |
| **5. Importació** | ProcessadorImportacioArticles            | Processa únicament els articles seleccionats i importables.                           |
| **6. Escriptura** | a3ERP ActiveX · IMaestro                 | Crea o actualitza l’article mitjançant el mecanisme oficial de l’ERP.                 |
| **7. Diagnòstic** | ImportadorArticlesLogger                 | Registra inici, validació, resultats, incidències i resum del lot.                    |

### 2.2. Obertura des d’a3ERP

MAT0943Net.dll rep la selecció de menú a través del punt d’entrada COM i delega l’obertura al mòdul Importador. La petició queda registrada abans de crear el formulari i la connexió correspon sempre a l’empresa activa.

> **1.** La DLL rep l’origen de la crida, la base de dades activa i la connexió.
>
> **2.** El logger local de reserva s’inicialitza abans de llegir cap configuració o crear el formulari.
>
> **3.** PuntEntradaImportador resol la configuració pròpia del log i, si és possible, canvia a la ruta operativa.
>
> **4.** El formulari es crea i es mostra dins del flux iniciat per a3ERP.
>
> **5.** En tancar, queda registrada la fi de la sessió amb empresa i base de dades.

| La línia “Petició d’obertura de l’importador rebuda” s’escriu una sola vegada a Principal.cs. El punt d’entrada intern registra “S’inicia PuntEntradaImportador.Obrir()” per evitar duplicacions. |
|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

### 2.3. Configuració a AT_MAT0943NET_CONFIG

| **Clau**                         | **Valor recomanat** | **Funció**                              |
|----------------------------------|---------------------|-----------------------------------------|
| **ImportadorArticles_LogActivo** | True                | Activa el log operatiu de l’importador. |
| **ImportadorArticles_LogRuta**   | C:\Logs\A3ErpLogs\MAT0943Net   | Carpeta del fitxer de log operatiu.     |

Les claus de configuració han d’estar actives (`ACTIVO=1`). Si les claus no existeixen, la connexió falla o la ruta no és accessible, l’importador continua amb el log local de reserva sempre que la resta de l’operativa sigui possible.

```sql
SELECT ID, CLAVE, VALOR, ACTIVO, FECHA_ALTA, FECHA_ACTUALIZACION
FROM dbo.AT_MAT0943NET_CONFIG
WHERE CLAVE IN ('ImportadorArticles_LogActivo', 'ImportadorArticles_LogRuta')
ORDER BY CLAVE;
```

### 2.4. Formats Excel i CSV

El fitxer d’entrada ha d’incloure una fila de capçaleres amb els noms de camp admesos. En Excel, el full operatiu utilitzat a les proves és “Articles”. En CSV, s’han de conservar exactament les capçaleres i l’estructura de registres que espera el lector.

- CODART és obligatori i s’ha de tractar com a text.

- Les columnes opcionals poden quedar buides; una cel·la buida significa “no modificar aquest camp”.

- Els zeros inicials i els espais significatius de CODART no s’han de normalitzar des d’Excel.

- Cal evitar que Excel converteixi codis com 00098 a valor numèric 98.

- El nom complet del fitxer es registra als nivells normals; la ruta completa només apareix en DBG per diagnòstic.

| Recomanació: definir la columna CODART com a Text abans d’enganxar o importar dades a Excel. |
|----------------------------------------------------------------------------------------------|

### 2.5. Camps admesos

| **Camp**        | **Obligatori** | **Tractament**                                                                                                                               |
|-----------------|----------------|----------------------------------------------------------------------------------------------------------------------------------------------|
| **CODART**      | Sí             | Codi de l’article. Literal, màxim segons ARTICULO.CODART; no convertir a número.                                                             |
| **DESCART**     | No             | Descripció de l’article.                                                                                                                     |
| **CAR1**        | No             | Codi de característica principal. Es valida contra CARACTERISTICAS.CODCAR.                                                                   |
| **CAR2**        | No             | Segona característica. En aquesta versió no s’ha afegit la nova validació específica de CAR1.                                                |
| **PARAM1**      | No             | Valor del camp PARAM1 del mestre d’article.                                                                                                  |
| **CODPRO**      | No             | Codi de proveïdor associat.                                                                                                                  |
| **ARTPRO**      | No             | Referència o codi de l’article del proveïdor.                                                                                                |
| **PRCCOMPRA**   | No             | Preu de compra.                                                                                                                              |
| **DESC1**       | No             | Primer descompte del mestre d’article.                                                                                                       |
| **DESC2**       | No             | Segon descompte del mestre d’article.                                                                                                        |
| **DESC3**       | No             | Tercer descompte del mestre d’article.                                                                                                       |
| **PRCCOSTE**    | No             | Preu de cost.                                                                                                                                |
| **PRCSTANDARD** | No             | Preu estàndard utilitzat conjuntament amb PRCCOSTE a la Tarifa 6.                                                                            |
| **IDFORMULA**   | No             | ID de la fórmula de tarifa. Es valida contra AT_ARTICULO_FORMULAS.IDFORMULA i s’escriu a ARTICULO.AT_FORMULA_TARIFA_ID. Buit = no modificar. |

| La columna IDFORMULA és opcional i compatible amb fitxers antics. Una cel·la buida no elimina ni modifica l’assignació actual de fórmula. |
|-------------------------------------------------------------------------------------------------------------------------------------------|

### 2.6. Validacions

| **Validació**               | **Comportament**                                                                                                                                         |
|-----------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Estructura del fitxer**   | Comprova que el fitxer es pugui llegir i convertir al model intern.                                                                                      |
| **CODART obligatori**       | Una fila sense codi no és importable.                                                                                                                    |
| **Tipus i formats**         | Els valors han de poder convertir-se al tipus esperat pel model.                                                                                         |
| **Existència de l’article** | Determina si l’operació prevista és ALTA, ACTUALITZACIÓ o SENSE CANVIS.                                                                                  |
| **CAR1**                    | Si és informat, ha d’existir a dbo.CARACTERISTICAS.CODCAR.                                                                                               |
| **Cel·les buides**          | No provoquen la substitució del valor existent.                                                                                                          |
| **Selecció final**          | Només les files vàlides i amb canvis queden seleccionades com a importables.                                                                             |
| **IDFORMULA**               | Si és informat, ha de ser un enter existent a dbo.AT_ARTICULO_FORMULAS.IDFORMULA. Un valor invàlid o inexistent deixa la fila en Error i fora d’ActiveX. |

Per evitar una consulta per fila, els codis vàlids de CAR1 es carreguen una sola vegada durant la validació del lot. Un CAR1 incorrecte genera una incidència funcional en nivell WRN i no arriba al servei ActiveX.

```text
[WRN] L'article presenta una incidència de validació. |
IdAnalisi=... | Fila=4 | CODART=18 | CAR1=CARR |
Missatge=La característica indicada a CAR1 no existeix a a3ERP: CARR.
```

Els IDFORMULA vàlids es carreguen una sola vegada per lot. El valor original es conserva per mostrar valors no numèrics (per exemple, ABC) a la graella i al diagnòstic.

### 2.7. Actualització i creació d’articles

El processador treballa només amb les files seleccionades després de l’anàlisi. Per a cada article registra l’operació prevista, executa l’alta o actualització mitjançant IMaestro i informa del resultat real. Quan IDFORMULA està informat, ActiveX assigna el valor a ARTICULO.AT_FORMULA_TARIFA_ID; quan està buit, aquest camp no es toca.

| **Situació**                                  | **Operació**    | **Regla**                                            |
|-----------------------------------------------|-----------------|------------------------------------------------------|
| **El CODART no existeix**                     | ALTA            | Es crea un nou registre amb els camps informats.     |
| **El CODART existeix i hi ha diferències**    | ACTUALITZACIÓ   | Només s’apliquen els camps informats i modificables. |
| **El CODART existeix i no hi ha diferències** | SENSE CANVIS    | No es selecciona per processar.                      |
| **La fila té errors de validació**            | ERROR FUNCIONAL | No es selecciona i no arriba a ActiveX.              |

| Les escriptures no es fan amb INSERT/UPDATE directes sobre ARTICULO. L’alta i l’actualització utilitzen a3ERP ActiveX perquè l’ERP apliqui les seves regles i restriccions. |
|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

### 2.8. Tractament literal de CODART

ARTICULO.CODART és un camp de text. Alguns codis numèrics poden estar emmagatzemats amb espais a l’esquerra, mentre que altres codis són alfanumèrics o contenen zeros inicials. El codi s’ha de conservar com a literal durant tot el flux.

- No convertir CODART a int, decimal o qualsevol altre tipus numèric.

- No eliminar zeros inicials: 00098 ha de continuar sent 00098.

- No confondre el valor visible amb el literal real guardat a la base de dades.

- LTRIM/RTRIM només es pot utilitzar per localitzar un registre quan sigui necessari; l’escriptura ha de conservar el literal correcte.

- El model pot mantenir una propietat específica com CodiArticleBaseDades per reutilitzar el codi literal recuperat de SQL.

| **Exemple**        | **Tractament correcte**                                                                   |
|--------------------|-------------------------------------------------------------------------------------------|
| **2**              | Localitzar l’article encara que a SQL estigui justificat; conservar el literal recuperat. |
| **00098**          | Conservar exactament els cinc caràcters i els zeros inicials.                             |
| **TL / TLF / TNS** | Tractar com a codis alfanumèrics sense conversió.                                         |

### 2.9. Logs i retenció

| **Mode**          | **Ruta / fitxer**                                                                            | **Comportament**                                                                              |
|-------------------|----------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------------------------|
| **Reserva local** | %LOCALAPPDATA%\AT_Infoserveis\A3ErpImportadorArticles\Logs\ImportadorArticles_yyyy-MM-dd.log | S’inicialitza abans de disposar del context i absorbeix errors d’arrencada o de configuració. |
| **Operatiu**      | C:\Logs\A3ErpLogs\MAT0943Net\ImportadorArticles_yyyy-MM-dd.log                                          | S’utilitza quan la configuració és activa i la ruta és accessible.                            |

- Retenció de 7 dies tant a la ruta local com a la configurada.

- Només s’eliminen fitxers ImportadorArticles_*.log.

- No es copien ni es mouen línies antigues en canviar de ruta.

- Després del canvi, les línies noves només s’escriuen al log operatiu.

- Els errors interns del logger no bloquegen l’importador ni es mostren a l’usuari.

- No es registren cadenes de connexió, usuaris SQL, contrasenyes ni credencials.

| **Nivell** | **Ús**                                                                            |
|------------|-----------------------------------------------------------------------------------|
| **DBG**    | Detall tècnic, inclosa la ruta completa del fitxer quan és útil.                  |
| **INF**    | Flux normal, anàlisi correcta, articles creats/actualitzats i lot sense errors.   |
| **WRN**    | Incidències funcionals, validació parcial o lot amb resultats correctes i errors. |
| **ERR**    | Lot completament fallit, error tècnic o excepció que impedeix l’operació.         |

### 2.10. Desplegament i integració amb el menú

> **1. Compilar MAT0943Net en Release x86.**
>
> **2. Copiar MAT0943Net.dll i les dependències necessàries a la carpeta de desplegament conjunta.**
>
> **3. Substituir MAT0943Net.dll mantenint el ProgId i el GUID existents.**
>
> **4. En una instal·lació inicial, registrar la DLL amb RegAsm de 32 bits. En actualitzacions ordinàries, si el registre COM no canvia, n’hi ha prou amb substituir el binari amb a3ERP tancat.**
>
> **5. Verificar que l’XML de menú continua enviant l’opció correcta al punt d’entrada de MAT0943Net.**
>
> **6. Verificar les claus ImportadorArticles_LogActivo i ImportadorArticles_LogRuta a CPORRAS.**
>
> **7. Obrir l’opció des d’a3ERP i comprovar el log local, el canvi de ruta i la càrrega del formulari.**

| L’Importador, el Calculador i el Gestor formen part de MAT0943Net.dll. Qualsevol desplegament ha de validar l’obertura dels tres mòduls. |
|------------------------------------------------------------------------------------------------------------------------------------------|

### 2.11. Proves funcionals realitzades

| **Prova**                                | **Resultat esperat**                                               | **Resultat validat**                     |
|------------------------------------------|--------------------------------------------------------------------|------------------------------------------|
| **Execució directa / arrencada**         | Crear log local i advertir quan no hi ha context d’a3ERP.          | Correcte.                                |
| **Obertura des d’a3ERP**                 | Canviar del log local a C:\Logs\A3ErpLogs\MAT0943Net sense duplicar línies.   | Correcte.                                |
| **CAR1 invàlid**                         | Marcar incidència abans d’ActiveX.                                 | Correcte.                                |
| **IDFORMULA existent**                   | Classificar com Sense canvis o Actualitzar segons el valor actual. | Correcte.                                |
| **IDFORMULA buit**                       | No modificar AT_FORMULA_TARIFA_ID en una actualització real.       | Correcte; TVABISM835 conserva fórmula 2. |
| **IDFORMULA inexistent / no enter**      | Marcar Error i no seleccionar la fila.                             | Correcte; 99 i ABC bloquejats.           |
| **Actualització de fórmula via ActiveX** | Canviar l’assignació de fórmula.                                   | Correcte; FRABREIN250 passa a fórmula 2. |
| **Alta amb fórmula**                     | Crear article nou amb AT_FORMULA_TARIFA_ID informat.               | Correcte; ATFTEST01 creat amb fórmula 3. |
| **Integració Importador → Calculador**   | L’article importat ha d’aparèixer a la fórmula assignada.          | Correcte; ATFTEST01 apareix a Fórmula 3. |
| **CODART amb zeros inicials**            | Conservar el literal.                                              | Correcte; 00098 manté els zeros.         |

### 2.12. Resolució d’incidències

| **Símptoma**                            | **Comprovacions i actuació**                                                                                                     |
|-----------------------------------------|----------------------------------------------------------------------------------------------------------------------------------|
| **No s’obre el formulari des del menú** | Compilar x86; revisar registre COM, opció XML, ProgId/GUID i dependències de la DLL compartida.                                  |
| **No apareix el log operatiu**          | Revisar les dues claus, ACTIVO=1, permisos de C:\Logs\A3ErpLogs\MAT0943Net i el log local de reserva.                                       |
| **El fitxer no es pot llegir**          | Revisar extensió, capçaleres, full “Articles”, bloqueig del fitxer i valors no convertibles.                                     |
| **ActiveX falla a Guarda()**            | Consultar l’excepció completa; comprovar restriccions de l’ERP i afegir validació prèvia només quan la regla estigui confirmada. |
| **CAR1 no existeix**                    | Corregir el codi al fitxer o crear la característica a a3ERP; la fila no s’ha d’importar.                                        |
| **CODART perd zeros**                   | Formatar la columna com a text i evitar conversions numèriques en Excel o en codi.                                               |
| **Lot parcial**                         | El resum ha de quedar en WRN. Revisar cada incidència per Fila i CODART.                                                         |
| **IDFORMULA invàlid o inexistent**      | Corregir el valor al fitxer. Ha de ser un enter existent a AT_ARTICULO_FORMULAS.IDFORMULA. Buit significa no modificar.          |

## 3. Calculador de tarifes

El Calculador de tarifes genera tarifes de venda i, quan correspon, descomptes per família de client a partir dels preus de compra, cost i estàndard dels articles. El procés inclou previsualització, simulació transaccional i aplicació definitiva amb verificació.

### 3.1. Arquitectura funcional

| **Fase**                | **Component / servei**                          | **Responsabilitat**                                                                                  |
|-------------------------|-------------------------------------------------|------------------------------------------------------------------------------------------------------|
| **1. Obertura**         | Principal.cs → PuntEntradaCalculador            | Rep el context d’a3ERP, inicialitza el logger i crea FrmCalculadorTarifes.                           |
| **2. Càrrega**          | RepositoriArticlesCalculTarifes                 | Carrega només els articles amb ARTICULO.AT_FORMULA_TARIFA_ID = IDFORMULA de la fórmula seleccionada. |
| **3. Plantilla**        | RepositoriFormulesTarifes / model FormulaTarifa | Carrega les fórmules actives configurades a AT_ARTICULO_FORMULAS.                                    |
| **4. Càlcul**           | MotorCalculTarifes                              | Calcula T1-T6, descomptes i errors per article.                                                      |
| **5. Previsualització** | FrmCalculadorTarifes                            | Mostra resultats sense modificar SQL.                                                                |
| **6. Simulació**        | Servei d’aplicació                              | Executa procediments oficials dins una transacció, verifica i fa ROLLBACK.                           |
| **7. Aplicació**        | Servei d’aplicació                              | Repeteix l’operació, verifica i fa COMMIT.                                                           |
| **8. Diagnòstic**       | CalculadorTarifesLogger                         | Registra selecció, càlcul, simulació, confirmació, commit i errors.                                  |

### 3.2. Plantilles i fórmules

| **Núm.** | **Plantilla**                            | **Tarifes 1-4**                                                | **Valors inicials**                            | **Descomptes** |
|----------|------------------------------------------|----------------------------------------------------------------|------------------------------------------------|----------------|
| **1**    | Cost dividit per coeficients             | T1=PRCCOSTE/c1; T2=PRCCOSTE/c2; T3=PRCCOSTE/c3; T4=PRCCOSTE/c4 | 0,65 · 0,70 · 0,75 · 0,80                      | No             |
| **2**    | Cost més percentatges                    | Tn=PRCCOSTE × (1 + percentatge n)                              | 35% · 30% · 25% · 20%                          | No             |
| **3**    | Preu compra amb descomptes               | T1-T4=PRCCOMPRA                                                | Descomptes famílies 1-4: 10% · 15% · 20% · 25% | Sí             |
| **4**    | Preu compra × 2 amb descomptes           | T1-T4=PRCCOMPRA × 2                                            | Descomptes famílies 1-4: 10% · 15% · 20% · 25% | Sí             |
| **5**    | Cost dividit per coeficients alternatius | T1=PRCCOSTE/c1; ...; T4=PRCCOSTE/c4                            | 0,55 · 0,60 · 0,65 · 0,70                      | No             |
| **6**    | Cost més imports fixos                   | Tn=PRCCOSTE + import n                                         | 10 · 9 · 7 · 6                                 | No             |
| **7**    | Cost més percentatges alternatius        | Tn=PRCCOSTE × (1 + percentatge n)                              | 40% · 35% · 30% · 25%                          | No             |

| En totes les plantilles: Tarifa 5 = PRCCOSTE i Tarifa 6 = PRCCOSTE + PRCSTANDARD. |
|-----------------------------------------------------------------------------------|

### 3.3. Paràmetres editables

Els valors inicials de cada plantilla són una proposta. L’usuari pot editar els quatre coeficients, percentatges o imports abans de previsualitzar. El motor valida els paràmetres per article i retorna un error funcional quan la fórmula no és aplicable.

- Els coeficients utilitzats com a divisor no poden ser zero.

- Els percentatges i imports es poden modificar sense canviar el codi de la plantilla.

- La plantilla determina si es generen descomptes; l’usuari no converteix una plantilla sense descomptes en una plantilla amb descomptes només canviant valors.

- La previsualització s’ha de repetir després de modificar qualsevol paràmetre.

| \[WRN\] Resultat de previsualització incorrecte. \| Article=16 \| Motiu=El coeficient de Tarifa 1 no pot ser zero. |
|--------------------------------------------------------------------------------------------------------------------|

### 3.4. Previsualització

> **1.** Seleccionar la plantilla de càlcul.
>
> **2.** Revisar o editar els quatre paràmetres.
>
> **3.** Seleccionar un o més articles.
>
> **4.** Executar la previsualització.
>
> **5.** Revisar les tarifes 1-6, els descomptes i els missatges d’error.
>
> **6.** Aplicar només quan tots els resultats necessaris siguin correctes.

La previsualització calcula íntegrament en memòria i no modifica TARIFAVE ni DESCUENT. El log indica la plantilla, el nombre d’articles, si genera descomptes, els resultats correctes i els errors. Quan l’usuari canvia de fórmula, la previsualització anterior s’invalida i la graella d’articles es recarrega exclusivament amb els articles de la nova fórmula. Una fórmula amb 0 articles és un cas vàlid.

| La capçalera de selecció permet marcar o desmarcar tots els articles, inclosos els codis que puguin tenir valor visible 0. |
|----------------------------------------------------------------------------------------------------------------------------|

### 3.5. Escriptura a TARIFAVE

Les sis tarifes es desen mitjançant el procediment oficial dbo.ActualizarTarifa. No es fan INSERT ni UPDATE directes i no es calcula l’identificador amb MAX+1.

| **Camp / regla**  | **Valor o comportament**                                                    |
|-------------------|-----------------------------------------------------------------------------|
| **Procediment**   | dbo.ActualizarTarifa                                                        |
| **Identificador** | El procediment utilitza NextValue 'IDTARIFAV'.                              |
| **TARIFA**        | Codis 1, 2, 3, 4, 5 i 6, conservant el literal que espera la base de dades. |
| **CODART**        | Literal de base de dades; no es reconstrueix a partir d’un número.          |
| **CODMON**        | EURO                                                                        |
| **FECMIN**        | 1900-01-01                                                                  |
| **FECMAX**        | 9999-12-31                                                                  |
| **UNIDADES**      | 0                                                                           |
| **Preus**         | Arrodonits segons dbo.DATOSCONFIG.NUMDECPRC abans de persistir.             |

| No substituir el procediment oficial per SQL directe. A més de la numeració, el procediment concentra les regles esperades per a3ERP. |
|---------------------------------------------------------------------------------------------------------------------------------------|

### 3.6. Escriptura a DESCUENT

Les plantilles 3 i 4 generen quatre registres de descompte per article, un per cada família de client. L’escriptura utilitza el procediment oficial dbo.ActualizarDescArtFam dins de la mateixa transacció que TARIFAVE.

| **Camp**                   | **Valor**                                           |
|----------------------------|-----------------------------------------------------|
| **Procediment**            | dbo.ActualizarDescArtFam                            |
| **TIPREG**                 | AF                                                  |
| **FAMCLI**                 | 1, 2, 3 i 4                                         |
| **DESC1**                  | Descompte calculat o configurat per a cada família. |
| **DESC2, DESC3, DESC4**    | 0                                                   |
| **CODCLI, CODPRO, FAMART** | NULL                                                |
| **UNIDADES**               | 0                                                   |
| **FECMIN / FECMAX**        | 1900-01-01 / 9999-12-31                             |
| **ID**                     | Identity gestionat per SQL Server.                  |

La relació funcional depèn de CLIENTES.FAMCLIDESC, que ha de correspondre amb DESCUENT.FAMCLI. La tarifa del client es determina amb CLIENTES.TARIFA i selecciona la fila corresponent de TARIFAVE.

| Els valors Identity consumits durant una simulació amb ROLLBACK poden no reutilitzar-se. Els salts d’ID a DESCUENT són normals i no indiquen que la simulació hagi fet COMMIT. |
|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

### 3.7. Procediments oficials utilitzats

| **Destinació** | **Procediment**          | **Motiu**                                                                        |
|----------------|--------------------------|----------------------------------------------------------------------------------|
| **TARIFAVE**   | dbo.ActualizarTarifa     | Alta o actualització de tarifes amb numeració oficial IDTARIFAV.                 |
| **DESCUENT**   | dbo.ActualizarDescArtFam | Alta o actualització del descompte article-família respectant la clau funcional. |

- La simulació i l’aplicació real utilitzen exactament els mateixos procediments.

- Després de cada escriptura es verifica que els valors persistits coincideixin amb el resultat calculat.

- Qualsevol error de procediment o verificació provoca l’anul·lació de la transacció.

### 3.8. Simulació, ROLLBACK i COMMIT

| **Etapa**            | **Transacció**                                            | **Resultat**                                                  |
|----------------------|-----------------------------------------------------------|---------------------------------------------------------------|
| **Previsualització** | Sense transacció SQL                                      | Només càlcul en memòria.                                      |
| **Simulació**        | BEGIN TRANSACTION → procediments → verificació → ROLLBACK | Demostra que l’operació és executable sense conservar canvis. |
| **Aplicació real**   | BEGIN TRANSACTION → procediments → verificació → COMMIT   | Conserva tarifes i descomptes només si tot és correcte.       |
| **Error**            | ROLLBACK                                                  | No deixa una aplicació parcial del lot.                       |

> **1.** L’usuari confirma una sola vegada l’aplicació.
>
> **2.** El sistema executa la simulació i comprova els resultats.
>
> **3.** Quan la simulació és correcta, continua amb l’aplicació real sense demanar una segona confirmació.
>
> **4.** El missatge final informa que les tarifes i, quan correspon, els descomptes s’han aplicat correctament.

| Una cancel·lació de l’usuari no executa ni la simulació ni el COMMIT. Aquesta situació queda registrada al log. |
|-----------------------------------------------------------------------------------------------------------------|

### 3.9. Arrodoniment segons NUMDECPRC

El nombre de decimals dels preus no està codificat de manera fixa. Abans d’escriure, el sistema consulta dbo.DATOSCONFIG.NUMDECPRC i arrodoneix tots els imports segons aquesta configuració.

```sql
SELECT NUMDECPRC
FROM dbo.DATOSCONFIG;
```

A l’empresa CPORRAS, el valor validat és 2. Per tant, els preus es persisteixen amb dos decimals. En una altra empresa, el comportament s’adapta al seu NUMDECPRC.

### 3.10. Regla sobre plantilles sense descomptes

| Quan s’aplica una fórmula configurada sense descomptes, MAT0943Net elimina els quatre descomptes gestionats per al mateix article (famílies 1–4) dins del mateix flux transaccional. |
|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

L’eliminació es limita als registres gestionats per MAT0943Net: TIPREG = AF, FAMCLI 1–4, UNIDADES = 0 i les dates de vigència gestionades. No s’elimina cap altre tipus de descompte. Les fórmules amb descomptes creen o actualitzen els quatre registres corresponents.

Aquesta regla evita conservar descomptes antics quan un article passa d’una fórmula amb descomptes a una fórmula que no en genera.

### 3.11. Log normal i log local de reserva

| **Mode**          | **Ruta / fitxer**                                                                          | **Comportament**                                                                         |
|-------------------|--------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------|
| **Reserva local** | %LOCALAPPDATA%\AT_Infoserveis\A3ErpCalculadorTarifes\Logs\CalculadorTarifes_yyyy-MM-dd.log | Registra l’arrencada abans de disposar de configuració i els errors previs al formulari. |
| **Operatiu**      | C:\Logs\A3ErpLogs\MAT0943Net\CalculadorTarifes_yyyy-MM-dd.log                                         | Registra la sessió normal, previsualització, simulació, aplicació i tancament.           |

| **Clau**                        | **Valor recomanat** |
|---------------------------------|---------------------|
| **CalculadorTarifes_LogActivo** | True                |
| **CalculadorTarifes_LogRuta**   | C:\Logs\A3ErpLogs\MAT0943Net   |

- Retenció de 7 dies aplicada exclusivament a CalculadorTarifes_*.log.

- Un únic logger per sessió, amb fase local i fase operativa.

- La línia local “El log operatiu continua a la ruta configurada” marca el canvi.

- La línia operativa “El logger s’ha inicialitzat des del log local de reserva” marca la recepció.

- No es dupliquen els registres posteriors entre les dues rutes.

- No es registren credencials ni la cadena de connexió.

### 3.12. Integració amb a3ERP

El Calculador forma part de MAT0943Net.dll i s’obre des del mateix punt d’entrada COM que l’Importador i el Gestor. El context rebut d’a3ERP determina empresa, base de dades i connexió.

| PuntEntradaCalculador.Obrir(string baseDadesEmpresa, string connexioEmpresa) |
|------------------------------------------------------------------------------|

- El formulari s’obre dins del context de l’empresa activa.

- La connexió rebuda es valida abans de carregar articles.

- El log local s’inicialitza al primer punt d’entrada, abans de crear el formulari.

- Els canvis al punt d’entrada COM s’han de revisar perquè comparteix l’obertura dels tres mòduls.

- La compilació i el desplegament de MAT0943Net.dll han de ser x86.

### 3.13. Proves funcionals realitzades

| **Prova**                      | **Resultat validat**                                                                          |
|--------------------------------|-----------------------------------------------------------------------------------------------|
| **Set fórmules**               | Les 7 fórmules actives es carreguen correctament des de AT_ARTICULO_FORMULAS.                 |
| **Càrrega per fórmula**        | Fórmula 1 → 3 articles; Fórmula 2 → 1; fórmula sense assignacions → 0, sense fallback massiu. |
| **Canvi de fórmula**           | Neteja la previsualització anterior i recarrega només els articles de la nova fórmula.        |
| **Coeficient zero**            | La previsualització queda en WRN i mostra l’error funcional abans de SQL.                     |
| **Simulació**                  | Executa procediments, verifica i fa ROLLBACK.                                                 |
| **Aplicació**                  | Repeteix el procés i fa COMMIT només després de validar.                                      |
| **Fórmula 2 · ABSOLAVRSTA0/3** | PRCCOSTE=31,60; T1=42,66; T2=41,08; T3=39,50; T4=37,92; T5=31,60; T6=38,60.                   |
| **PV sense descompte**         | Client amb Tarifa 3 rep preu 39,50 i descompte 0%.                                            |
| **Fórmula 3 · ABSOLAVRSTA0/3** | PRCCOMPRA=24,60; T1–T4=24,60; T5=31,60; T6=38,60; D1–D4=10/15/20/25%.                         |
| **DESCUENT**                   | Es creen FAMCLI 1–4 amb DESC1 10%, 15%, 20% i 25%.                                            |
| **PV amb descompte**           | Client Tarifa 3 + Família 3: preu 24,60; descompte 20%; base 19,68.                           |
| **Log d’arrencada**            | Una sola càrrega inicial per fórmula; canvi local → operatiu sense duplicacions.              |

### 3.14. Diagnòstic d’errors

| **Símptoma**                                                        | **Diagnòstic**                                                                                                               |
|---------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------|
| **No es carreguen articles**                                        | Revisar IDFORMULA seleccionat i que ARTICULO.AT_FORMULA_TARIFA_ID tingui assignacions. 0 articles és un cas funcional vàlid. |
| **Error de fórmula**                                                | Revisar paràmetres; els divisors no poden ser zero.                                                                          |
| **La simulació falla**                                              | Consultar procediment, paràmetres enviats i verificació posterior; no s’ha de continuar al COMMIT.                           |
| **Les tarifes no coincideixen**                                     | Comprovar NUMDECPRC, fórmula, PRCCOSTE/PRCCOMPRA/PRCSTANDARD i literal de CODART/TARIFA.                                     |
| **No s’aplica el descompte a la venda**                             | Revisar CLIENTES.TARIFA, CLIENTES.FAMCLIDESC, FAMCLI, TIPREG=AF i dates de vigència.                                         |
| **Hi ha salts d’ID a DESCUENT**                                     | Comportament normal d’Identity després de simulacions amb ROLLBACK.                                                          |
| **Queden descomptes antics després d’una fórmula sense descomptes** | Revisar el DELETE gestionat sobre DESCUENT (TIPREG=AF, FAMCLI 1–4, unitats i dates gestionades) i el log de la transacció.   |
| **No apareix el log operatiu**                                      | Revisar claus, ACTIVO, permisos de carpeta i log local de reserva.                                                           |

## 4. Gestor de fórmules

El Gestor de fórmules manté la configuració de càlcul que consumeix el Calculador. Les fórmules es desen a dbo.AT_ARTICULO_FORMULAS i s’identifiquen físicament per IDFORMULA.

### 4.1. Model de dades i relació amb ARTICULO

| **Element**              | **Definició**                                                                                |
|--------------------------|----------------------------------------------------------------------------------------------|
| **Taula**                | dbo.AT_ARTICULO_FORMULAS                                                                     |
| **Clau principal**       | IDFORMULA int IDENTITY NOT NULL                                                              |
| **Vinculació d’article** | ARTICULO.AT_FORMULA_TARIFA_ID → AT_ARTICULO_FORMULAS.IDFORMULA                               |
| **Camp visible a a3ERP** | La lupa del camp extern mostra NOMBRE, mentre que internament es guarda IDFORMULA.           |
| **Estat**                | ACTIVO controla disponibilitat; ELIMINADO i FECHA_ELIMINACION implementen eliminació lògica. |

| La PK es va renombrar de ID a IDFORMULA per eliminar ambigüitats amb altres camps ID generats pel Diccionari d’a3ERP. El Gestor i el Calculador han estat validats després del canvi. |
|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

### 4.2. Camps principals de la fórmula

| **Camp**                                   | **Funció**                                                |
|--------------------------------------------|-----------------------------------------------------------|
| **CODIGO / NOMBRE / DESCRIPCION**          | Identificació funcional i descripció de la fórmula.       |
| **ORDEN**                                  | Ordre de presentació al selector.                         |
| **ACTIVO**                                 | Determina si la fórmula es pot seleccionar al Calculador. |
| **TIPO_VALORES / UTILIZA_VALORES_TARIFAS** | Metadades de comportament dels valors editables.          |
| **VALOR_INICIAL_P1..P4**                   | Valors inicials dels quatre paràmetres.                   |
| **EXPRESION_TARIFA1..6**                   | Expressions configurables del motor de càlcul.            |
| **GENERA_DESCUENTOS**                      | Activa la generació dels quatre grups de descompte.       |
| **DESCUENTO_INICIAL_GRUPO1..4**            | Percentatges inicials D1–D4.                              |

### 4.3. Operacions del Gestor

| **Operació**             | **Comportament**                                        | **Observació**                                                            |
|--------------------------|---------------------------------------------------------|---------------------------------------------------------------------------|
| **Crear**                | Insereix una fórmula nova i recupera la seva Identity.  | IDFORMULA no s’assigna manualment.                                        |
| **Editar**               | Actualitza dades, paràmetres, expressions i descomptes. | Els canvis afecten futurs càlculs; no recalculen articles automàticament. |
| **Activar / desactivar** | Canvia ACTIVO.                                          | Una fórmula inactiva deixa d’aparèixer al selector del Calculador.        |
| **Eliminar**             | Eliminació lògica.                                      | Només es permet quan no hi ha articles assignats.                         |

### 4.4. Protecció del botó Eliminar

Abans de mostrar la confirmació, el Gestor executa un recompte parametritzat sobre ARTICULO. Si hi ha articles amb AT_FORMULA_TARIFA_ID = IDFORMULA, l’eliminació queda bloquejada i es mostra el nombre d’articles afectats.

| No es pot eliminar una fórmula que tingui articles assignats. Primer cal reassignar aquests articles a una altra fórmula. |
|---------------------------------------------------------------------------------------------------------------------------|

La protecció existeix a dos nivells: comprovació prèvia a la interfície i condició NOT EXISTS al mateix UPDATE d’eliminació lògica. Si no hi ha articles, s’actualitzen ELIMINADO = 1 i FECHA_ELIMINACION = GETDATE(); no es fa DELETE físic.

### 4.5. Integració amb el Calculador

El Calculador carrega les fórmules actives i utilitza FormulaTarifa.Id com a representació C# de la PK física IDFORMULA. En seleccionar una fórmula, la consulta d’articles filtra directament a SQL per ARTICULO.AT_FORMULA_TARIFA_ID = IDFORMULA.

Després de tancar el Gestor, el selector del Calculador es recarrega. Si la fórmula seleccionada canvia, també es neteja qualsevol previsualització anterior.

### 4.6. Logs i diagnòstic

El Gestor registra l’obertura, la càrrega de fórmules, altes/modificacions, activació/desactivació, eliminacions i errors. Quan una eliminació es bloqueja, s’emet un WRN amb IdFormula, CodiFormula, NomFormula i ArticlesAssignats.

| **Clau**                              | **Valor recomanat**              | **Funció**                                   |
|---------------------------------------|----------------------------------|----------------------------------------------|
| **GestorFormulesTarifes_LogActivo**   | True                             | Activa el log operatiu del gestor.           |
| **GestorFormulesTarifes_LogRuta**     | C:\Logs\A3ErpLogs\MAT0943Net     | Carpeta del fitxer de log operatiu.          |

### 4.7. Proves funcionals realitzades

| **Prova**                           | **Resultat validat**                                                       |
|-------------------------------------|----------------------------------------------------------------------------|
| **Migració ID → IDFORMULA**         | Gestor carrega les 7 fórmules i totes les operacions continuen funcionant. |
| **Activar / desactivar**            | Canvi d’estat correcte i valors recuperats correctament.                   |
| **Lupa a la fitxa d’article**       | Mostra el nom de la fórmula i desa el seu IDFORMULA sense ambigüitat.      |
| **Eliminar fórmula amb articles**   | Fórmula 4 amb 3 articles: eliminació bloquejada i WRN registrat.           |
| **Eliminar fórmula sense articles** | Fórmula temporal: confirmació i eliminació lògica correctes.               |

### 4.8. Diagnòstic d’incidències

| **Símptoma**                            | **Diagnòstic**                                                                          |
|-----------------------------------------|-----------------------------------------------------------------------------------------|
| **La fórmula no apareix al Calculador** | Revisar ACTIVO=1 i ELIMINADO=0.                                                         |
| **No es pot eliminar**                  | Comprovar quants articles tenen AT_FORMULA_TARIFA_ID igual a l’IDFORMULA.               |
| **Error de columna ID ambigua**         | Verificar que la PK física sigui IDFORMULA i que la FK d’ARTICULO apunti a aquest camp. |
| **Els canvis no es reflecteixen**       | Tancar el Gestor i verificar la recàrrega del selector del Calculador.                  |

## 5. Desplegament conjunt i integració amb a3ERP

### 5.1. Ordre recomanat

> **1. Fer còpia de seguretat del binari desplegat i de la configuració actual.**
>
> **2. Tancar a3ERP i compilar MAT0943Net en Release x86.**
>
> **3. Copiar MAT0943Net.dll a C:\Program Files (x86)\A3\a3erp\extensiones\AT\MAT0943\Binarios\\**
>
> **4. En primera instal·lació, registrar MAT0943Net.dll amb RegAsm x86. En actualitzacions amb ProgId/GUID invariants, substituir el binari amb a3ERP tancat.**
>
> **5. Verificar les opcions XML de menú de l’Importador i del Calculador; el Gestor s’obre des del Calculador.**
>
> **6. Verificar les claus de log de l’Importador i del Calculador a CPORRAS.**
>
> **7. Comprovar permisos de C:\Logs\A3ErpLogs\MAT0943Net per a l’usuari que executa a3ERP.**
>
> **8. Obrir Importador, Calculador i Gestor des d’a3ERP i validar els logs i la connexió a CPORRAS.**
>
> **9. Executar les proves ràpides descrites a les llistes de comprovació.**

### 5.2. Binaris i compatibilitat

| **Element**       | **Requisit**                                                                                     |
|-------------------|--------------------------------------------------------------------------------------------------|
| **Arquitectura**  | x86 en tots els projectes que interactuen amb a3ERP/COM.                                         |
| **Framework**     | .NET Framework 4.7.2.                                                                            |
| **DLL principal** | MAT0943Net.dll; coexisteix amb MAT0943.dll (Delphi).                                             |
| **ProgId / GUID** | Mantenir els valors actuals. No cal tornar a registrar la DLL en cada substitució si no canvien. |
| **Configuració**  | Per empresa/base de dades, mitjançant dbo.AT_MAT0943NET_CONFIG.                                |
| **Logs**          | Fitxers separats tot i compartir C:\Logs\A3ErpLogs\MAT0943Net.                                              |

No es desplega Interop.a3ERPActiveX.dll com a fitxer separat. Els recursos visuals necessaris, com la icona, formen part del binari segons la configuració actual del projecte.

### 5.3. Seguretat operativa

- No desplegar primer en producció; repetir les proves en una empresa de test equivalent.

- No registrar ni mostrar cadenes de connexió o credencials.

- No concedir permisos d’escriptura més amplis dels necessaris a la carpeta de logs.

- No fer escriptures directes a ARTICULO, TARIFAVE o DESCUENT fora dels mecanismes documentats.

- No barrejar fitxers de log: cada procés elimina exclusivament el seu patró de nom.

- Conservar els logs de la prova de desplegament fins que l’operativa estigui validada.

## 6. Configuració SQL de referència

### 6.1. Alta o actualització de les claus de log

El script següent crea o actualitza les sis claus sense duplicar-les. Cal executar-lo a cada base de dades d’empresa on s’utilitzin els mòduls.

```sql
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @Config TABLE
(
CLAVE varchar(100) NOT NULL,
VALOR varchar(500) NOT NULL,
DESCRIPCION nvarchar(500) NOT NULL
);
INSERT INTO @Config (CLAVE, VALOR, DESCRIPCION)
VALUES
('ImportadorArticles_LogActivo', 'True', N'Activa o desactiva el log de l''importador d''articles.'),
('ImportadorArticles_LogRuta', 'C:\Logs\A3ErpLogs\MAT0943Net', N'Ruta on s''escriu el log de l''importador d''articles.'),
('CalculadorTarifes_LogActivo', 'True', N'Activa o desactiva el log del calculador de tarifes.'),
('CalculadorTarifes_LogRuta', 'C:\Logs\A3ErpLogs\MAT0943Net', N'Ruta on s''escriu el log del calculador de tarifes.'),
('GestorFormulesTarifes_LogActivo', 'True', N'Activa o desactiva el log del gestor de fórmules de tarifes.'),
('GestorFormulesTarifes_LogRuta', 'C:\Logs\A3ErpLogs\MAT0943Net', N'Ruta on s''escriu el log del gestor de fórmules de tarifes.');
UPDATE C
SET C.VALOR = X.VALOR,
C.DESCRIPCION = X.DESCRIPCION,
C.ACTIVO = 1,
C.FECHA_ACTUALIZACION = GETDATE()
FROM dbo.AT_MAT0943NET_CONFIG C
INNER JOIN @Config X ON X.CLAVE = C.CLAVE;
INSERT INTO dbo.AT_MAT0943NET_CONFIG
(
CLAVE, VALOR, DESCRIPCION, ACTIVO,
FECHA_ALTA, FECHA_ACTUALIZACION
)
SELECT X.CLAVE, X.VALOR, X.DESCRIPCION, 1, GETDATE(), NULL
FROM @Config X
WHERE NOT EXISTS
(
SELECT 1
FROM dbo.AT_MAT0943NET_CONFIG C
WHERE C.CLAVE = X.CLAVE
);
COMMIT TRANSACTION;
```

### 6.2. Verificació de configuració

```sql
SELECT ID, CLAVE, VALOR, DESCRIPCION, ACTIVO,
FECHA_ALTA, FECHA_ACTUALIZACION
FROM dbo.AT_MAT0943NET_CONFIG
WHERE CLAVE IN
(
'ImportadorArticles_LogActivo',
'ImportadorArticles_LogRuta',
'CalculadorTarifes_LogActivo',
'CalculadorTarifes_LogRuta',
'GestorFormulesTarifes_LogActivo',
'GestorFormulesTarifes_LogRuta'
)
ORDER BY CLAVE;
SELECT NUMDECPRC
FROM dbo.DATOSCONFIG;
SELECT
OBJECT_ID('dbo.ActualizarTarifa') AS ProcActualizarTarifa,
OBJECT_ID('dbo.ActualizarDescArtFam') AS ProcActualizarDescArtFam;
```

### 6.3. Verificacions funcionals de dades

```sql
-- Característiques vàlides per a CAR1
SELECT CODCAR
FROM dbo.CARACTERISTICAS
ORDER BY CODCAR;
-- Tarifes d'un article de prova
SELECT CODART, TARIFA, PRECIO, CODMON, FECMIN, FECMAX, UNIDADES
FROM dbo.TARIFAVE
WHERE LTRIM(RTRIM(CODART)) = 'ABSOLAVRSTA0/3'
ORDER BY TARIFA;
-- Descomptes per família de client
SELECT ID, CODART, FAMCLI, TIPREG, DESC1, DESC2, DESC3, DESC4,
UNIDADES, FECMIN, FECMAX
FROM dbo.DESCUENT
WHERE LTRIM(RTRIM(CODART)) = 'ABSOLAVRSTA0/3'
ORDER BY FAMCLI;
```

| El nom del camp de preu de TARIFAVE pot variar segons la versió o l’esquema concret. Ajustar la consulta de verificació al nom real de la columna sense alterar el procediment oficial d’escriptura. |
|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

### 6.4. Verificació de fórmules i assignacions

```sql
SELECT IDFORMULA, CODIGO, NOMBRE, ACTIVO, ORDEN
FROM dbo.AT_ARTICULO_FORMULAS
WHERE ELIMINADO = 0
ORDER BY ORDEN, IDFORMULA;
SELECT A.CODART, A.AT_FORMULA_TARIFA_ID, F.NOMBRE
FROM dbo.ARTICULO AS A
LEFT JOIN dbo.AT_ARTICULO_FORMULAS AS F
ON F.IDFORMULA = A.AT_FORMULA_TARIFA_ID
WHERE A.AT_FORMULA_TARIFA_ID IS NOT NULL
ORDER BY A.AT_FORMULA_TARIFA_ID, A.CODART;
```

La relació física esperada és ARTICULO.AT_FORMULA_TARIFA_ID → AT_ARTICULO_FORMULAS.IDFORMULA. Els articles amb NULL no apareixen al Calculador.

## 7. Llistes de comprovació operativa

### 7.1. Importador d’articles

| **✓** | **Comprovació**                                                                    |
|-------|------------------------------------------------------------------------------------|
| **☐** | La solució està compilada en Release x86.                                          |
| **☐** | MAT0943Net.dll està registrada i l’opció de menú obre l’Importador.                |
| **☐** | El log local es crea a %LOCALAPPDATA%\AT_Infoserveis\A3ErpImportadorArticles\Logs. |
| **☐** | El log canvia a C:\Logs\A3ErpLogs\MAT0943Net quan la configuració és correcta.                |
| **☐** | El fitxer conté les capçaleres admeses i CODART és text.                           |
| **☐** | Una cel·la opcional buida no modifica el valor existent.                           |
| **☐** | CAR1 incorrecte queda en WRN i no arriba a ActiveX.                                |
| **☐** | Un codi amb zeros inicials es conserva literalment.                                |
| **☐** | El resum del lot utilitza INF, WRN o ERR segons el resultat.                       |
| **☐** | IDFORMULA informat es valida contra AT_ARTICULO_FORMULAS.IDFORMULA.                |
| **☐** | IDFORMULA buit no modifica la fórmula existent.                                    |
| **☐** | Una alta amb IDFORMULA apareix a la fórmula corresponent del Calculador.           |

### 7.2. Calculador de tarifes

| **✓** | **Comprovació**                                                                                                        |
|-------|------------------------------------------------------------------------------------------------------------------------|
| **☐** | L’opció MuestraFrmCalculadorTarifas obre el formulari a l’empresa correcta.                                            |
| **☐** | Els articles es carreguen exclusivament per la fórmula seleccionada; una fórmula sense articles retorna 0 sense error. |
| **☐** | Les set plantilles mostren els valors inicials documentats.                                                            |
| **☐** | Un coeficient zero genera error abans de SQL.                                                                          |
| **☐** | La previsualització no modifica TARIFAVE ni DESCUENT.                                                                  |
| **☐** | La simulació escriu, verifica i fa ROLLBACK.                                                                           |
| **☐** | L’aplicació real escriu, verifica i fa COMMIT.                                                                         |
| **☐** | NUMDECPRC s’aplica a l’arrodoniment.                                                                                   |
| **☐** | Les fórmules sense descomptes eliminen només els quatre DESCUENT gestionats per MAT0943Net.                            |
| **☐** | El log local i l’operatiu no dupliquen línies.                                                                         |

### 7.3. Gestor de fórmules

| **✓** | **Comprovació**                                                                         |
|-------|-----------------------------------------------------------------------------------------|
| **☐** | Les 7 fórmules actives es carreguen correctament.                                       |
| **☐** | IDFORMULA és la PK física i la lupa d’ARTICULO mostra el nom de la fórmula.             |
| **☐** | Activar/desactivar actualitza el selector del Calculador.                               |
| **☐** | No es pot eliminar una fórmula amb articles assignats.                                  |
| **☐** | Una fórmula sense articles s’elimina lògicament després de confirmació.                 |
| **☐** | Els canvis del Gestor es reflecteixen després de recarregar el selector del Calculador. |

### 7.4. Criteri de tancament d’una incidència

- L’error és reproduïble o queda identificat amb IdAnalisi, Fila, CODART o article afectat.

- S’ha confirmat si és una dada funcional incorrecta, un problema de configuració, una restricció d’a3ERP o una excepció tècnica.

- La correcció s’ha provat en execució directa quan correspon i des d’a3ERP.

- S’ha verificat que MAT0943Net.dll continua obrint Importador, Calculador i Gestor.

- S’ha revisat el resultat funcional a a3ERP, no només el missatge de la pantalla.

- Els canvis s’han compilat, provat, documentat i pujat amb el repositori net.

### 7.5. Evolució i manteniment

| AT_ARTICULO_FORMULAS i la vinculació ARTICULO.AT_FORMULA_TARIFA_ID formen part de l’arquitectura actual i s’han de mantenir sincronitzades amb el Gestor, l’Importador i el Calculador. |
|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|

Qualsevol nova validació de camps, plantilla de càlcul, regla de descompte o mecanisme de persistència s’ha d’afegir a aquesta documentació després de superar proves equivalents a les descrites.

---

## 8. Guia operativa d’usuari

Aquesta secció resumeix l’ús diari dels tres mòduls. La part tècnica, SQL, logs i desplegament es detalla als apartats anteriors.

### 8.1. Flux recomanat

1. Assignar o actualitzar `IDFORMULA` a l’article, des de l’Importador o puntualment des de la fitxa d’article d’a3ERP.
2. Obrir el Calculador i seleccionar la fórmula corresponent.
3. Revisar els articles carregats per aquella fórmula.
4. Previsualitzar el càlcul.
5. Aplicar només si T1–T6 i, quan correspongui, D1–D4 són correctes.
6. Verificar el resultat a a3ERP quan sigui necessari, especialment després de canvis de fórmula.

### 8.2. Importador d’articles

L’Importador permet crear i actualitzar articles des d’un fitxer Excel o CSV. Abans d’escriure res a a3ERP, analitza el fitxer i classifica cada fila.

#### Fitxer d’entrada

- `CODART` és obligatori.
- La resta de camps són opcionals.
- En articles existents, una cel·la buida significa **no modificar aquell camp**.
- El codi de l’article (`CODART`) s’ha d’introduir exactament tal com existeix a a3ERP. Si conté zeros inicials, s’han de conservar, per exemple `00098`. No s’ha de convertir ni reformatejar com a número.
- `IDFORMULA` és opcional:
  - buit → no modifica la fórmula existent;
  - enter existent → assigna o canvia la fórmula;
  - enter inexistent → Error;
  - valor no enter, per exemple `ABC` → Error i es mostra el valor original.

#### Estats de l’anàlisi

| Estat | Significat | Acció habitual |
|---|---|---|
| Nou | `CODART` no existeix a a3ERP. | Crear si la fila és vàlida. |
| Actualitzar | L’article existeix i hi ha algun camp informat diferent. | Revisar i importar si el canvi és correcte. |
| Sense canvis | L’article existeix i els camps informats coincideixen. | No cal importar. |
| Error | Hi ha una incidència de conversió o validació. | Corregir el fitxer i tornar a analitzar. |

#### Passos d’ús

1. Obrir l’Importador des del menú d’a3ERP.
2. Seleccionar el fitxer Excel o CSV.
3. Analitzar el fitxer.
4. Revisar la graella i els missatges.
5. Comprovar les files seleccionades.
6. Importar només els articles seleccionats.
7. Revisar el resum final.

#### Assignació manual de fórmula

Per a canvis puntuals, la fórmula també es pot assignar des de la fitxa d’article d’a3ERP, a **Campos externos**. La lupa mostra el nom de la fórmula i internament es guarda `IDFORMULA` a `ARTICULO.AT_FORMULA_TARIFA_ID`.

### 8.3. Calculador de tarifes

El Calculador treballa per fórmula. Quan se selecciona una fórmula, només carrega els articles que tenen assignat aquell `IDFORMULA`.

> Quan canvieu de fórmula, el Calculador elimina la previsualització anterior i carrega només els articles assignats a la nova fórmula. Si la fórmula seleccionada no té cap article assignat, la graella quedarà buida; aquest comportament és correcte i no indica cap error.

#### Passos d’ús

1. Seleccionar la fórmula al desplegable **Plantilla**.
2. Revisar els valors o paràmetres; es poden modificar abans de calcular.
3. Seleccionar els articles. El cercador i “seleccionar tots” afecten només els articles de la fórmula actual.
4. Prémer **Previsualitzar càlcul**.
5. Revisar T1–T6 i, si escau, D1–D4.
6. Prémer **Aplicar tarifes** només quan el resultat sigui correcte.

#### Regles comunes

- T1–T4 depenen de la fórmula seleccionada.
- T5 = `PRCCOSTE`.
- T6 = `PRCCOSTE + PRCSTANDARD`.
- L’arrodoniment s’adapta a `DATOSCONFIG.NUMDECPRC`.
- Si una fórmula genera descomptes, es gestionen D1–D4 per família de client.
- Quan s’aplica una fórmula **sense descomptes**, MAT0943Net elimina només els quatre `DESCUENT` gestionats per MAT0943Net per a l’article, segons els criteris documentats a l’apartat 3.10.

#### Venda / Pedido de Venta

- La tarifa configurada al client determina quin preu T1–T6 utilitza a3ERP.
- La família de descomptes del client determina quin D1–D4 s’aplica quan la fórmula genera descomptes.
- Després d’aplicar les tarifes, en crear un Pedido de Venta, el preu de l’article correspon a la tarifa assignada al client.
- Exemple validat: preu `24,60`, família de descompte 3 (`20%`) → base `19,68`.

### 8.4. Gestor de fórmules

El Gestor permet mantenir les fórmules consumides pel Calculador: codi, nom, descripció, ordre, estat, P1–P4, expressions T1–T6 i descomptes D1–D4.

#### Criteri d’identificació

Per assignar una fórmula als articles, utilitzeu sempre el seu `IDFORMULA`. L’ordre en què apareixen les fórmules al selector només serveix per ordenar-les visualment.

#### Modificar una fórmula

1. Obrir el Gestor des del Calculador.
2. Seleccionar una fórmula o crear-ne una de nova.
3. Revisar nom, descripció, ordre, paràmetres, expressions i descomptes.
4. Desar.
5. Tancar el Gestor i tornar al Calculador; el selector es recarrega.
6. Fer una previsualització amb pocs articles abans d’una aplicació general.

#### Activar o desactivar

Desactivar una fórmula la retira del selector actiu del Calculador. Els articles poden continuar tenint el seu `IDFORMULA` guardat, però no podran treballar amb aquella fórmula fins que es torni a activar o es reassignin.

#### Eliminar una fórmula

- Només es pot eliminar si **no té articles assignats**.
- Si hi ha articles vinculats, l’eliminació queda bloquejada i el Gestor indica quants articles hi ha.
- Cal reassignar els articles abans de tornar-ho a intentar.
- L’eliminació és **lògica**, no física: s’informa `ELIMINADO` i `FECHA_ELIMINACION`.
- El repositori també protegeix l’operació amb `NOT EXISTS` sobre `ARTICULO`.

### 8.5. Fluxos habituals

#### Crear un article i deixar-lo preparat per calcular

1. Afegir una fila nova amb `CODART` i els camps necessaris.
2. Informar `IDFORMULA`.
3. Analitzar i importar.
4. Obrir el Calculador i seleccionar aquella fórmula.
5. Comprovar que l’article apareix.
6. Previsualitzar i aplicar quan correspongui.

#### Canviar la fórmula d’un article existent

1. Preparar una fila amb el mateix `CODART` i el nou `IDFORMULA`.
2. Deixar buits els camps que no es vulguin modificar.
3. L’anàlisi ha de mostrar **Actualitzar**.
4. Importar i verificar que l’article desapareix de la fórmula anterior i apareix a la nova.

#### Actualitzar un camp sense tocar la fórmula

1. Informar `CODART` i el camp a modificar.
2. Deixar `IDFORMULA` buit.
3. L’Importador actualitza el camp informat i conserva l’assignació de fórmula existent.

### 8.6. Incidències habituals

| Símptoma | Causa habitual | Què revisar |
|---|---|---|
| L’article no apareix al Calculador | No té `IDFORMULA` o està assignat a una altra fórmula. | Fitxa d’article / Excel i fórmula seleccionada. |
| La fórmula mostra 0 articles | No hi ha articles assignats. | És un cas vàlid; revisar assignacions. |
| `IDFORMULA` = Error | L’ID no existeix o no és un enter. | Corregir el fitxer. |
| `CAR1` = Error | La característica no existeix a a3ERP. | Corregir `CAR1`. |
| Un article surt “Sense canvis” | Els camps informats coincideixen amb a3ERP. | No cal importar. |
| La previsualització desapareix | S’ha canviat de fórmula o recarregat articles. | És correcte; generar una nova previsualització. |
| El preu del PV no és l’esperat | El client té una altra tarifa o família de descompte. | Revisar Tarifa i Família de descomptes del client. |
| No es pot eliminar una fórmula | Té articles assignats. | Reassignar-los abans d’eliminar. |
| Incidència tècnica | Error d’a3ERP, ActiveX, SQL o dades. | Anotar hora, `CODART`, `IDFORMULA`, operació i consultar el log. |

### 8.7. Checklist de producció

- [ ] L’empresa activa és CPORRAS.
- [ ] `MAT0943Net.dll` està desplegada en Release x86.
- [ ] Importador, Calculador i Gestor s’obren correctament des d’a3ERP.
- [ ] Les fórmules necessàries estan actives.
- [ ] L’article de prova té `IDFORMULA` assignat.
- [ ] L’article apareix només a la fórmula corresponent.
- [ ] La previsualització mostra valors coherents.
- [ ] T5 correspon a `PRCCOSTE`.
- [ ] T6 correspon a `PRCCOSTE + PRCSTANDARD`.
- [ ] Si hi ha descomptes, D1–D4 són els esperats.
- [ ] Després d’aplicar, el preu del Pedido de Venta correspon a la tarifa del client.
- [ ] Quan hi ha descomptes, el PV aplica el grup corresponent a la família de descompte del client.
- [ ] Els logs local i operatiu funcionen sense duplicacions.
- [ ] Una fórmula amb articles assignats no es pot eliminar.

### 8.8. Criteri de seguretat operativa

**Previsualitzar sempre abans d’aplicar.** Si el resultat no és l’esperat, no confirmeu l’aplicació: reviseu fórmula, article, tarifes, descomptes i paràmetres abans de continuar.

---

## 9. Referències principals del projecte

- DLL de producció: `MAT0943Net.dll`
- DLL Delphi coexistint: `MAT0943.dll`
- Base de dades / empresa: `CPORRAS`
- Taula de fórmules: `dbo.AT_ARTICULO_FORMULAS`
- PK de fórmules: `IDFORMULA`
- Vinculació article-fórmula: `ARTICULO.AT_FORMULA_TARIFA_ID`
- Tarifes: `dbo.TARIFAVE`
- Descomptes: `dbo.DESCUENT`
- Procediment de tarifes: `dbo.ActualizarTarifa`
- Procediment de descomptes: `dbo.ActualizarDescArtFam`
- Configuració: `dbo.AT_MAT0943NET_CONFIG`
- Ruta de desplegament: `C:\Program Files (x86)\A3\a3erp\extensiones\AT\MAT0943\Binarios\`
- Ruta de logs operatius: `C:\Logs\A3ErpLogs\MAT0943Net`

---

**AT Infoserveis · MAT0943Net.dll · CPORRAS**
