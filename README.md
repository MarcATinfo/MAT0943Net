# MAT0943Net

DLL .NET x86 per integrar funcionalitats operatives amb a3ERP a l'empresa/base de dades `CPORRAS`.

El projecte agrupa tres moduls principals:

- Importador d'articles des de fitxers Excel o CSV.
- Calculador de tarifes i descomptes per formula.
- Gestor de formules configurables per al calculador.

## Entorn

- .NET Framework 4.7.2
- WinForms
- Compilacio x86
- SQL Server
- a3ERP ActiveX / COM

## Estructura

- `ImportadorArticles/`: lectura, validacio i importacio d'articles.
- `CalculadorTarifes/`: calcul, previsualitzacio i aplicacio de tarifes.
- `GestorFormulesTarifes/`: manteniment de formules, expressions i descomptes.
- `Infrastructure/`: utilitats compartides de runtime i integracio.
- `ScriptsSQL/`: scripts SQL generals del projecte.
- `Documentacio/`: documentacio tecnica, manual d'usuari i plantilles.

## Configuracio

La configuracio operativa es desa a:

```sql
dbo.AT_MAT0943NET_CONFIG
```

Les claus principals de log son:

- `ImportadorArticles_LogActivo`
- `ImportadorArticles_LogRuta`
- `CalculadorTarifes_LogActivo`
- `CalculadorTarifes_LogRuta`
- `GestorFormulesTarifes_LogActivo`
- `GestorFormulesTarifes_LogRuta`

Ruta recomanada de logs:

```text
C:\Logs\A3ErpLogs\MAT0943Net
```

## Desplegament

La DLL s'ha de compilar en `Release x86` i desplegar-se a l'entorn d'a3ERP corresponent. No s'ha de canviar el ProgId ni el GUID de `MAT0943Net.dll` en actualitzacions ordinaries.

## Documentacio

La documentacio principal del projecte es troba a:

```text
Documentacio/MAT0943Net_DLL.md
```
