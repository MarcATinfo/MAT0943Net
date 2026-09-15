using A3ErpCalculadorTarifes.Models;
using ContextImportadorA3Erp =
    A3ErpImportadorArticles.Models.ContextImportadorA3Erp;
using ResultatResolucioConnexioImportador =
    A3ErpImportadorArticles.Models.ResultatResolucioConnexio;
using ServeiResolucioConnexioImportador =
    A3ErpImportadorArticles.Infrastructure.Connections.ServeiResolucioConnexioA3Erp;
using System;

namespace A3ErpCalculadorTarifes.Infrastructure.Connections
{
    /// <summary>
    /// Resol la connexió rebuda d'a3ERP utilitzant
    /// el mateix mecanisme operatiu que l'importador.
    ///
    /// Primer prova la connexió original i, si falla,
    /// aplica el fallback AtInfo definit al projecte.
    /// El resultat es transforma al context propi
    /// del calculador.
    /// </summary>
    public sealed class ServeiResolucioConnexioA3Erp
    {
        public ResultatResolucioConnexio Resoldre(
            string empresaActiva,
            string cadenaConnexioRebuda)
        {
            if (string.IsNullOrWhiteSpace(
                cadenaConnexioRebuda))
            {
                return ResultatResolucioConnexio.CrearError(
                    "a3ERP no ha proporcionat cap connexió.",
                    string.Empty);
            }

            ServeiResolucioConnexioImportador resolutorImportador =
                new ServeiResolucioConnexioImportador();

            ResultatResolucioConnexioImportador resultatImportador =
                resolutorImportador.Resoldre(
                    empresaActiva,
                    cadenaConnexioRebuda);

            if (!resultatImportador.Correcte)
            {
                return ResultatResolucioConnexio.CrearError(
                    resultatImportador.Missatge,
                    resultatImportador.ErrorConnexioOriginal);
            }

            ContextImportadorA3Erp contextImportador =
                resultatImportador.Context;

            ContextCalculadorA3Erp contextCalculador =
                new ContextCalculadorA3Erp(
                    contextImportador.EmpresaActiva,
                    contextImportador.BaseDadesEmpresa,
                    contextImportador.CadenaConnexio,
                    contextImportador.UtilitzaConnexioAlternativa);

            return ResultatResolucioConnexio.CrearCorrecte(
                contextCalculador,
                resultatImportador.Missatge,
                resultatImportador.ErrorConnexioOriginal);
        }
    }
}
