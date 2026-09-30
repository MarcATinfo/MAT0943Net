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

        private readonly ConsultaFacturaDestinoService
            _consultaFacturaDestino =
                new ConsultaFacturaDestinoService();

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
                VincleTraspasPendienteDto pendiente =
                    _serveiVincle.ObtenerPendiente(
                        baseDatosOrigen,
                        facturaOrigen.IdFacv);

                if (pendiente != null)
                {
                    /*
                     * Per seguretat, un pendent només es recupera
                     * contra la mateixa empresa/base de dades.
                     */
                    if (!string.Equals(
                            pendiente.EmpresaDestino,
                            empresaDestino,
                            StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(
                            pendiente.BaseDatosDestino,
                            baseDatosDestino,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "La factura té un traspàs pendent cap a una altra empresa de destinació.");
                    }

                    if (pendiente.IdFacvDestino.HasValue &&
                        pendiente.IdFacvDestino.Value > 0)
                    {
                        FacturaDestinoRecuperacionDto facturaExistente =
                            _consultaFacturaDestino.Consultar(
                                pendiente.BaseDatosDestino,
                                pendiente.IdFacvDestino.Value);

                        if (facturaExistente != null)
                        {
                            /*
                             * CAS IMPORTANT:
                             * la factura ja existeix físicament.
                             * NO en creem una altra.
                             */
                            TraspasFacturasLogger.Advertencia(
                                "S'ha detectat un traspàs pendent amb la factura destí ja creada. " +
                                "No es crearà una nova factura i es completarà el vincle existent.",
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
                                        pendiente.EmpresaDestino
                                    },
                                    {
                                        "BaseDadesDesti",
                                        pendiente.BaseDatosDestino
                                    },
                                    {
                                        "IDFACVDesti",
                                        facturaExistente.IdFacv
                                    }
                                });

                            _serveiVincle.MarcarCreada(
                                baseDatosOrigen,
                                facturaOrigen.IdFacv,
                                pendiente.EmpresaDestino,
                                pendiente.BaseDatosDestino,
                                facturaExistente.IdFacv,
                                facturaExistente.Serie,
                                facturaExistente.NumDoc);

                            TraspasFacturasLogger.Informacio(
                                "S'ha recuperat el traspàs pendent i el vincle s'ha marcat com CREADA.",
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
                                        pendiente.EmpresaDestino
                                    },
                                    {
                                        "BaseDadesDesti",
                                        pendiente.BaseDatosDestino
                                    },
                                    {
                                        "IDFACVDesti",
                                        facturaExistente.IdFacv
                                    },
                                    {
                                        "SerieDesti",
                                        facturaExistente.Serie
                                    },
                                    {
                                        "NumDocDesti",
                                        facturaExistente.NumDoc
                                    }
                                });

                            resultado.Correcto =
                                true;

                            resultado.Recuperada =
                                true;

                            resultado.IdDocumento =
                                facturaExistente.IdFacv;

                            resultado.Serie =
                                facturaExistente.Serie;

                            resultado.NumDoc =
                                facturaExistente.NumDoc.ToString();

                            return resultado;
                        }

                        /*
                         * Teníem un ID destí guardat però aquella
                         * factura ja no existeix.
                         *
                         * En aquest cas és segur tornar-la a generar.
                         */
                        TraspasFacturasLogger.Advertencia(
                            "El traspàs pendent tenia una factura destí informada, " +
                            "però ja no existeix. Es tornarà a intentar el traspàs.",
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
                                    "IDFACVDestiAnterior",
                                    pendiente.IdFacvDestino.Value
                                }
                            });
                    }
                    else
                    {
                        TraspasFacturasLogger.Advertencia(
                            "S'ha detectat un traspàs pendent sense ID de factura destí. " +
                            "Es tornarà a intentar el traspàs.",
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
                }
                            });
                    }

                    _serveiVincle.ReiniciarPendienteParaReintento(
                        baseDatosOrigen,
                        facturaOrigen.IdFacv,
                        empresaDestino,
                        baseDatosDestino);

                    vinclePreparat =
                        true;

                    TraspasFacturasLogger.Debug(
                        "S'ha preparat el traspàs pendent per a un nou intent.",
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
                }
                else
                {
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
                }

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
                 *
                 * Ho marquem ABANS de qualsevol altra
                 * operació perquè, si falla alguna cosa,
                 * el vincle no es converteixi en ERROR.
                 */
                facturaCreada =
                    true;

                resultado.IdDocumento =
                    idDocumento;

                /*
                 * Guardem immediatament l'IDFACV destí
                 * mentre el vincle continua PENDIENTE.
                 *
                 * Així, si el procés falla abans de
                 * MarcarCreada(), podrem identificar
                 * exactament la factura que ja existeix.
                 */
                _serveiVincle.GuardarIdFacvDestinoPendiente(
                    baseDatosOrigen,
                    facturaOrigen.IdFacv,
                    idDocumento);

                TraspasFacturasLogger.Debug(
                    "S'ha guardat l'ID de la factura destí al vincle pendent.",
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
                        }
                    });

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