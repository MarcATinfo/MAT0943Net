using MAT0943Net.TraspasFacturas.Infrastructure.Logging;
using MAT0943Net.TraspasFacturas.Models;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace MAT0943Net.TraspasFacturas.Services
{
    internal sealed class ServeiTraspasFacturesActiveX
    {
        private const string ClauUsuari =
            "TraspasFacturas_ActiveXUsuario";

        private const string ClauPassword =
            "TraspasFacturas_ActiveXPassword";

        private readonly ConsultaLineasFacturaOrigenService
            _consultaLineas =
                new ConsultaLineasFacturaOrigenService();

        private readonly ServeiVincleTraspasFactures
            _serveiVincle =
                new ServeiVincleTraspasFactures();

        public ResultadoTraspasFacturaDto CrearFacturaVenda(
            string baseDatosOrigen,
            string empresaDestino,
            string baseDatosDestino,
            FacturaOrigenDto facturaOrigen)
        {
            var resultado =
                new ResultadoTraspasFacturaDto();

            a3ERPActiveX.Enlace enlace =
                null;

            a3ERPActiveX.Factura factura =
                null;

            bool facturaIniciada =
                false;

            bool facturaEnEdicion =
                false;

            bool vinclePreparat =
                false;

            bool facturaCreada =
                false;

            decimal idDocumentoCreat =
                0m;

            try
            {
                if (facturaOrigen == null)
                {
                    throw new InvalidOperationException(
                        "No s'ha informat la factura origen.");
                }

                if (!facturaOrigen.Fecha.HasValue)
                {
                    throw new InvalidOperationException(
                        "La factura origen no té data.");
                }

                if (string.IsNullOrWhiteSpace(baseDatosDestino))
                {
                    throw new InvalidOperationException(
                        "No s'ha informat la base de dades de destinació.");
                }

                TraspasFacturasLogger.Informacio(
                    "S'inicia el traspàs d'una factura de venda.",
                    new Dictionary<string, object>
                    {
                        {
                            "FacturaOrigen",
                            facturaOrigen.Factura
                        },
                        {
                            "IDFACVOrigen",
                            facturaOrigen.IdFacv
                        },
                        {
                            "BaseDadesOrigen",
                            baseDatosOrigen
                        },
                        {
                            "EmpresaDesti",
                            empresaDestino
                        },
                        {
                            "BaseDadesDesti",
                            baseDatosDestino
                        }
                    });

                List<LineaFacturaOrigenDto> lineas =
                    _consultaLineas.Consultar(
                        baseDatosOrigen,
                        facturaOrigen.IdFacv);

                if (lineas.Count == 0)
                {
                    throw new InvalidOperationException(
                        "La factura origen no té línies.");
                }

                TraspasFacturasLogger.Debug(
                    "S'han carregat les línies de la factura origen.",
                    new Dictionary<string, object>
                    {
                        {
                            "FacturaOrigen",
                            facturaOrigen.Factura
                        },
                        {
                            "IDFACVOrigen",
                            facturaOrigen.IdFacv
                        },
                        {
                            "NumLinies",
                            lineas.Count
                        }
                    });

                /*
                 * Reservem el traspàs abans de tocar ActiveX.
                 * Si ja existeix com CREADA o PENDIENTE,
                 * ServeiVincleTraspasFactures el bloquejarà.
                 */
                _serveiVincle.PrepararTraspaso(
                    baseDatosOrigen,
                    facturaOrigen.IdFacv,
                    empresaDestino,
                    baseDatosDestino);

                vinclePreparat =
                    true;

                TraspasFacturasLogger.Debug(
                    "S'ha preparat el vincle de traspàs en estat PENDIENTE.",
                    new Dictionary<string, object>
                    {
                        {
                            "IDFACVOrigen",
                            facturaOrigen.IdFacv
                        },
                        {
                            "EmpresaDesti",
                            empresaDestino
                        }
                    });

                string usuario =
                    ConfigurationManager.AppSettings[
                        ClauUsuari];

                string password =
                    ConfigurationManager.AppSettings[
                        ClauPassword];

                if (string.IsNullOrWhiteSpace(usuario))
                {
                    throw new InvalidOperationException(
                        "No s'ha configurat l'usuari ActiveX.");
                }

                enlace =
                    new a3ERPActiveX.Enlace();

                enlace.RaiseOnException =
                    true;

                bool loginOk =
                    enlace.LoginUsuario(
                        usuario,
                        password ?? string.Empty);

                if (!loginOk)
                {
                    throw new InvalidOperationException(
                        "LoginUsuario ha retornat false.");
                }

                enlace.Iniciar(
                    empresaDestino,
                    string.Empty);

                TraspasFacturasLogger.Debug(
                    "S'ha iniciat la sessió ActiveX de l'empresa destí.",
                    new Dictionary<string, object>
                    {
                        {
                            "EmpresaDesti",
                            empresaDestino
                        }
                    });

                factura =
                    new a3ERPActiveX.Factura();

                factura.Iniciar();

                facturaIniciada =
                    true;

                factura.OmitirMensajes =
                    true;

                factura.ValidarPrecios =
                    false;

                factura.ValidarArtBloqueado =
                    false;

                factura.AvisarRiesgo =
                    false;

                /*
                 * La factura destí porta la data
                 * del dia en què es fa el traspàs.
                 */
                string fecha =
                    DateTime.Today
                        .ToString("dd/MM/yyyy");

                factura.Nuevo(
                    fecha,
                    facturaOrigen.CodCli.Trim(),
                    false,
                    false,
                    true,
                    true);

                facturaEnEdicion =
                    true;

                foreach (LineaFacturaOrigenDto linea in lineas)
                {
                    factura.NuevaLineaArt(
                        linea.CodArt,
                        Convert.ToDouble(
                            linea.Unidades));

                    factura.AsStringLin["DESCLIN"] =
                        linea.DescLin;

                    factura.AsFloatLin["PRCMONEDA"] =
                        Convert.ToDouble(
                            linea.PrcMoneda);

                    factura.AsFloatLin["DESC1"] =
                        Convert.ToDouble(
                            linea.Desc1);

                    factura.AsFloatLin["DESC2"] =
                        Convert.ToDouble(
                            linea.Desc2);

                    factura.AsFloatLin["DESC3"] =
                        Convert.ToDouble(
                            linea.Desc3);

                    factura.AsFloatLin["DESC4"] =
                        Convert.ToDouble(
                            linea.Desc4);

                    factura.AsStringLin["TIPIVA"] =
                        linea.TipIva;

                    factura.AnadirLinea();
                }

                factura.CalcularImpuestosyTotales();

                decimal idDocumento =
                    Convert.ToDecimal(
                        factura.Anade());

                idDocumentoCreat =
                    idDocumento;

                facturaEnEdicion =
                    false;

                /*
                 * A partir d'aquí la factura ja existeix
                 * físicament a l'empresa destí.
                 */
                facturaCreada =
                    true;

                resultado.IdDocumento =
                    idDocumento;

                resultado.Serie =
                    factura.AsStringCab["SERIE"]
                    ?? string.Empty;

                resultado.NumDoc =
                    factura.AsStringCab["NUMDOC"]
                    ?? string.Empty;

                decimal numDocDestino;

                if (!decimal.TryParse(
                        resultado.NumDoc,
                        out numDocDestino))
                {
                    throw new InvalidOperationException(
                        "La factura s'ha creat, però no s'ha pogut " +
                        "interpretar el número de document retornat: '" +
                        resultado.NumDoc +
                        "'.");
                }

                _serveiVincle.MarcarCreada(
                    baseDatosOrigen,
                    facturaOrigen.IdFacv,
                    empresaDestino,
                    baseDatosDestino,
                    idDocumento,
                    resultado.Serie,
                    numDocDestino);

                TraspasFacturasLogger.Informacio(
                    "La factura s'ha traspassat correctament.",
                    new Dictionary<string, object>
                    {
                        {
                            "FacturaOrigen",
                            facturaOrigen.Factura
                        },
                        {
                            "IDFACVOrigen",
                            facturaOrigen.IdFacv
                        },
                        {
                            "EmpresaDesti",
                            empresaDestino
                        },
                        {
                            "BaseDadesDesti",
                            baseDatosDestino
                        },
                        {
                            "IDFACVDesti",
                            idDocumento
                        },
                        {
                            "SerieDesti",
                            resultado.Serie
                        },
                        {
                            "NumDocDesti",
                            resultado.NumDoc
                        },
                        {
                            "NumLinies",
                            lineas.Count
                        }
                    });

                resultado.Correcto =
                    true;

                return resultado;
            }
            catch (Exception ex)
            {
                resultado.Correcto =
                    false;

                resultado.Error =
                    ex.Message;

                if (factura != null &&
                    facturaEnEdicion)
                {
                    try
                    {
                        factura.Cancela();
                    }
                    catch
                    {
                    }
                }

                /*
                 * Només marquem ERROR si encara NO
                 * s'ha arribat a crear la factura destí.
                 *
                 * Si Anade() ja ha funcionat i falla alguna
                 * operació posterior, mantenim PENDIENTE
                 * per evitar un duplicat en un reintent.
                 */
                if (vinclePreparat &&
                    !facturaCreada)
                {
                    try
                    {
                        _serveiVincle.MarcarError(
                            baseDatosOrigen,
                            facturaOrigen.IdFacv,
                            empresaDestino);
                    }
                    catch (Exception exMarcarError)
                    {
                        TraspasFacturasLogger.Error(
                            "No s'ha pogut marcar el vincle del traspàs com a ERROR.",
                            exMarcarError,
                            new Dictionary<string, object>
                            {
                                {
                                    "IDFACVOrigen",
                                    facturaOrigen?.IdFacv ?? 0m
                                },
                                {
                                    "EmpresaDesti",
                                    empresaDestino ?? string.Empty
                                }
                            });
                    }
                }

                TraspasFacturasLogger.Error(
                    "Error durant el traspàs de la factura.",
                    ex,
                    new Dictionary<string, object>
                    {
                        {
                            "FacturaOrigen",
                            facturaOrigen?.Factura
                            ?? string.Empty
                        },
                        {
                            "IDFACVOrigen",
                            facturaOrigen?.IdFacv
                            ?? 0m
                        },
                        {
                            "BaseDadesOrigen",
                            baseDatosOrigen
                            ?? string.Empty
                        },
                        {
                            "EmpresaDesti",
                            empresaDestino
                            ?? string.Empty
                        },
                        {
                            "BaseDadesDesti",
                            baseDatosDestino
                            ?? string.Empty
                        },
                        {
                            "VinclePreparat",
                            vinclePreparat
                        },
                        {
                            "FacturaCreada",
                            facturaCreada
                        },
                        {
                            "IDFACVDesti",
                            idDocumentoCreat
                        }
                    });

                return resultado;
            }
            finally
            {
                if (factura != null &&
                    facturaIniciada)
                {
                    try
                    {
                        factura.Acabar();
                    }
                    catch
                    {
                    }
                }

                if (enlace != null)
                {
                    try
                    {
                        if (enlace.Estado ==
                            a3ERPActiveX.EstadoEnlace.estACTIVO)
                        {
                            enlace.Acabar();
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}