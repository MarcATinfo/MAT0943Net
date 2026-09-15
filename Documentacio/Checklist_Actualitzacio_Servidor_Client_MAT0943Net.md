# Checklist d'actualitzacio al servidor del client

Projecte: `MAT0943Net.dll`  
Empresa / base de dades: `CPORRAS`  
Entorn: a3ERP, SQL Server, Windows, .NET Framework 4.7.2, x86  
Ruta de desplegament: `C:\Program Files (x86)\A3\a3erp\extensiones\AT\MAT0943\Binarios\`  
Ruta recomanada de logs: `C:\Logs\A3ErpLogs\MAT0943Net`

> Objectiu: actualitzar el nou desenvolupament al servidor del client amb un procediment controlat, verificable i reversible.

---

## 1. Preparacio previa

- [ ] Confirmar data i finestra d'intervencio amb el client.
- [ ] Confirmar que cap usuari estara treballant amb a3ERP durant l'actualitzacio.
- [ ] Confirmar servidor, empresa i base de dades: `CPORRAS`.
- [ ] Confirmar ruta real d'instal.lacio d'a3ERP i carpeta de binaris AT.
- [ ] Confirmar usuari Windows amb permisos per copiar fitxers a la carpeta de desplegament.
- [ ] Confirmar usuari SQL o connexio disponible per executar scripts de verificacio.
- [ ] Confirmar que es disposa de la DLL nova compilada en `Release x86`.
- [ ] Confirmar que no s'han modificat ProgId ni GUID de `MAT0943Net.dll`.
- [ ] Tenir a ma la documentacio tecnica: `Documentacio\MAT0943Net_DLL.md`.
- [ ] Tenir a ma el script SQL de configuracio: `ScriptsSQL\001_Crear_AT_MAT0943NET_CONFIG.sql`.

## 2. Copies de seguretat

- [ ] Fer copia de seguretat de la base de dades `CPORRAS`.
- [ ] Copiar la DLL actual del servidor abans de substituir-la.
- [ ] Copiar les dependencies actuals si existeixen a la carpeta de desplegament.
- [ ] Guardar la copia en una carpeta amb data i hora, per exemple:

```text
C:\Backup_AT\MAT0943Net\2026-09-03_AbansActualitzacio\
```

- [ ] Anotar versio, data i mida de la DLL antiga.
- [ ] Conservar els logs existents abans de la prova si poden ajudar en cas d'incidencia.

## 3. Aturada controlada

- [ ] Avisar els usuaris abans de tallar l'operativa.
- [ ] Tancar a3ERP en tots els equips o sessions afectades.
- [ ] Revisar que no quedin processos d'a3ERP oberts al servidor o en sessions remotes.
- [ ] Confirmar que `MAT0943Net.dll` no queda bloquejada pel sistema.

## 4. Compilacio i paquet de desplegament

- [ ] Compilar la solucio en `Release x86`.
- [ ] Verificar que la DLL generada es troba a:

```text
bin\x86\Release\MAT0943Net.dll
```

- [ ] Incloure dependencies necessaries, especialment:

```text
ExcelDataReader.dll
ExcelDataReader.DataSet.dll
System.ValueTuple.dll
```

- [ ] No incloure `bin`, `obj`, `.vs`, paquets NuGet complets ni fitxers d'usuari del desenvolupador.
- [ ] Si es prepara un ZIP, verificar-ne el contingut abans de portar-lo al servidor.

## 5. Configuracio SQL

- [ ] Executar o revisar el script:

```text
ScriptsSQL\001_Crear_AT_MAT0943NET_CONFIG.sql
```

- [ ] Confirmar que existeix la taula:

```sql
SELECT OBJECT_ID('dbo.AT_MAT0943NET_CONFIG') AS TaulaConfig;
```

- [ ] Confirmar les sis claus de log:

```sql
SELECT CLAVE, VALOR, ACTIVO
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
```

- [ ] Confirmar que les claus necessaries tenen `ACTIVO = 1`.
- [ ] Confirmar que les rutes de log apunten a:

```text
C:\Logs\A3ErpLogs\MAT0943Net
```

- [ ] Confirmar que existeixen les formules:

```sql
SELECT IDFORMULA, CODIGO, NOMBRE, ACTIVO, ELIMINADO, ORDEN
FROM dbo.AT_ARTICULO_FORMULAS
ORDER BY ORDEN, IDFORMULA;
```

- [ ] Confirmar que els procediments requerits existeixen:

```sql
SELECT
OBJECT_ID('dbo.ActualizarTarifa') AS ProcActualizarTarifa,
OBJECT_ID('dbo.ActualizarDescArtFam') AS ProcActualizarDescArtFam;
```

## 6. Permisos i logs

- [ ] Crear la carpeta de logs si no existeix:

```text
C:\Logs\A3ErpLogs\MAT0943Net
```

- [ ] Donar permisos d'escriptura a l'usuari que executa a3ERP.
- [ ] Evitar permisos mes amplis dels necessaris.
- [ ] Confirmar que cada modul escriu el seu propi fitxer:

```text
ImportadorArticles_yyyy-MM-dd.log
CalculadorTarifes_yyyy-MM-dd.log
GestorFormulesTarifes_yyyy-MM-dd.log
```

- [ ] Confirmar que no es registren credencials ni cadenes de connexio completes.

## 7. Substitucio de binaris

- [ ] Copiar `MAT0943Net.dll` nova a la carpeta de desplegament.
- [ ] Copiar les dependencies necessaries a la mateixa carpeta.
- [ ] Mantenir la DLL Delphi existent `MAT0943.dll` si continua formant part de la integracio.
- [ ] No eliminar fitxers antics que no formin part directa de l'actualitzacio sense haver-los identificat.
- [ ] Si es primera instal.lacio, registrar `MAT0943Net.dll` amb RegAsm de 32 bits.
- [ ] Si es una actualitzacio ordinaria i no han canviat ProgId/GUID, no cal tornar a registrar la DLL.

## 8. Verificacio tecnica inicial

- [ ] Obrir a3ERP amb l'empresa `CPORRAS`.
- [ ] Confirmar que el menu AT mostra les opcions esperades.
- [ ] Obrir l'Importador d'articles.
- [ ] Obrir el Calculador de tarifes.
- [ ] Obrir el Gestor de formules.
- [ ] Confirmar que els tres moduls obren sense error.
- [ ] Confirmar que els logs es creen a la ruta operativa configurada.
- [ ] Confirmar que no es dupliquen linies entre log local i log operatiu.

## 9. Proves funcionals minimes

### Importador d'articles

- [ ] Obrir l'Importador des d'a3ERP.
- [ ] Carregar una plantilla Excel o CSV controlada.
- [ ] Analitzar el fitxer sense importar.
- [ ] Confirmar que `CODART` es tracta com a text.
- [ ] Confirmar que una cel.la opcional buida no modifica el valor existent.
- [ ] Confirmar que un `CAR1` invalid queda com a error funcional i no arriba a ActiveX.
- [ ] Confirmar que un `IDFORMULA` invalid queda com a error funcional.
- [ ] Fer una importacio petita i controlada.
- [ ] Verificar el resultat a la fitxa d'article d'a3ERP.

### Calculador de tarifes

- [ ] Obrir el Calculador des d'a3ERP.
- [ ] Seleccionar una formula activa.
- [ ] Confirmar que nomes es carreguen articles d'aquella formula.
- [ ] Fer una previsualitzacio sense aplicar.
- [ ] Revisar T1-T6.
- [ ] Revisar D1-D4 si la formula genera descomptes.
- [ ] Confirmar que la previsualitzacio no modifica `TARIFAVE` ni `DESCUENT`.
- [ ] Aplicar sobre pocs articles de prova.
- [ ] Verificar el resultat a a3ERP.

### Gestor de formules

- [ ] Obrir el Gestor des del Calculador.
- [ ] Confirmar que es carreguen les formules actives.
- [ ] Revisar una formula sense desar canvis.
- [ ] Provar activacio/desactivacio nomes si esta previst en la intervencio.
- [ ] Confirmar que no es pot eliminar una formula amb articles assignats.
- [ ] Tancar el Gestor i confirmar que el selector del Calculador es recarrega.

## 10. Validacio amb a3ERP

- [ ] Obrir una fitxa d'article amb `IDFORMULA` assignat.
- [ ] Confirmar que la lupa o camp extern mostra la formula correcta.
- [ ] Crear o simular un Pedido de Venta amb un article actualitzat.
- [ ] Confirmar que el preu aplicat correspon a la tarifa del client.
- [ ] Confirmar que el descompte correspon a la familia de descompte del client.
- [ ] Revisar qualsevol diferencia amb captures o dades concretes abans de donar-ho per validat.

## 11. Criteri d'acceptacio

- [ ] a3ERP obre correctament.
- [ ] `MAT0943Net.dll` carrega sense errors.
- [ ] Importador, Calculador i Gestor obren correctament.
- [ ] La configuracio SQL apunta a `dbo.AT_MAT0943NET_CONFIG`.
- [ ] Els logs es generen a `C:\Logs\A3ErpLogs\MAT0943Net`.
- [ ] Les proves funcionals minimes han passat.
- [ ] El client valida el resultat operatiu.
- [ ] Les copies de seguretat es conserven fins tancament definitiu.

## 12. Pla de rollback

- [ ] Tancar a3ERP.
- [ ] Restaurar la DLL anterior des de la copia de seguretat.
- [ ] Restaurar dependencies anteriors si s'havien substituit.
- [ ] Si s'han executat scripts SQL amb canvis no compatibles, valorar restaurar copia de BD.
- [ ] Obrir a3ERP i validar que l'operativa anterior torna a funcionar.
- [ ] Conservar logs i evidencies de l'error.
- [ ] Documentar hora, usuari, operacio, article o formula afectada i missatge d'error.

## 13. Notes de la intervencio

Data:

Responsable:

Servidor:

Usuari Windows:

Base de dades:

Versio DLL anterior:

Versio DLL nova:

Resultat:

Incidencies:

Accions pendents:
