using System;
using System.Collections.Generic;
using System.Globalization;

namespace MAT0943Net.Infrastructure.Events
{
    /// <summary>
    /// Interpreta el payload que a3ERP entrega als events de maestro.
    ///
    /// La documentació de maestro descriu un registro amb comptador:
    /// - posició 0: nombre de camps;
    /// - posicions 1..N: parelles camp/valor.
    ///
    /// Els exemples .NET disponibles a GRA0150Net mostren també payloads
    /// on root[1] és directament una llista de parelles camp/valor.
    /// Aquest helper accepta els dos formats i conserva les parelles originals
    /// per poder assignar PRCCOSTE dins del mateix payload.
    /// </summary>
    internal sealed class A3ErpMaestroEventData
    {
        private readonly List<object[]> _parellesCamp;

        private A3ErpMaestroEventData(
            List<object[]> parellesCamp)
        {
            _parellesCamp =
                parellesCamp;
        }

        public static bool TryCreate(
            object datos,
            out A3ErpMaestroEventData eventData,
            out string motiu)
        {
            eventData =
                null;

            motiu =
                string.Empty;

            List<object[]> parellesCamp;

            if (TryObtenirParellesCamp(
                datos,
                out parellesCamp))
            {
                eventData =
                    new A3ErpMaestroEventData(
                        parellesCamp);

                return true;
            }

            motiu =
                "No s'ha reconegut l'estructura de dades d'a3ERP com a parelles camp/valor.";

            return false;
        }

        public bool ConteCamp(
            string camp)
        {
            return TrobarParellaCamp(
                camp) != null;
        }

        public bool TryObtenirDouble(
            string camp,
            out double valor,
            out string motiu)
        {
            valor =
                0d;

            motiu =
                string.Empty;

            object[] parellaCamp =
                TrobarParellaCamp(
                    camp);

            if (parellaCamp == null ||
                parellaCamp.Length < 2)
            {
                motiu =
                    "El camp no existeix al payload.";

                return false;
            }

            object valorBrut =
                parellaCamp[1];

            if (valorBrut == null ||
                valorBrut == DBNull.Value)
            {
                valor =
                    0d;

                return true;
            }

            string text =
                Convert.ToString(
                    valorBrut,
                    CultureInfo.CurrentCulture);

            if (string.IsNullOrWhiteSpace(
                text))
            {
                valor =
                    0d;

                return true;
            }

            try
            {
                valor =
                    Convert.ToDouble(
                        valorBrut,
                        CultureInfo.CurrentCulture);

                return true;
            }
            catch
            {
            }

            if (double.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.CurrentCulture,
                out valor))
            {
                return true;
            }

            if (double.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out valor))
            {
                return true;
            }

            motiu =
                "El valor del camp no es pot convertir a número.";

            return false;
        }

        public bool TryAssignarValor(
            string camp,
            object valor)
        {
            object[] parellaCamp =
                TrobarParellaCamp(
                    camp);

            if (parellaCamp == null ||
                parellaCamp.Length < 2)
            {
                return false;
            }

            parellaCamp[1] =
                valor;

            return true;
        }

        public IReadOnlyList<string> ObtenirCampsDisponibles()
        {
            List<string> camps =
                new List<string>();

            foreach (object[] parellaCamp in _parellesCamp)
            {
                string nomCamp =
                    ObtenirNomCamp(
                        parellaCamp);

                if (!string.IsNullOrWhiteSpace(
                    nomCamp))
                {
                    camps.Add(
                        nomCamp);
                }
            }

            return camps;
        }

        private object[] TrobarParellaCamp(
            string camp)
        {
            if (string.IsNullOrWhiteSpace(
                camp))
            {
                return null;
            }

            foreach (object[] parellaCamp in _parellesCamp)
            {
                string nomCamp =
                    ObtenirNomCamp(
                        parellaCamp);

                if (string.Equals(
                    nomCamp,
                    camp,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return parellaCamp;
                }
            }

            return null;
        }

        private static string ObtenirNomCamp(
            object[] parellaCamp)
        {
            if (parellaCamp == null ||
                parellaCamp.Length == 0)
            {
                return string.Empty;
            }

            string nomCamp =
                Convert.ToString(
                    parellaCamp[0],
                    CultureInfo.InvariantCulture);

            return string.IsNullOrWhiteSpace(nomCamp)
                ? string.Empty
                : nomCamp.Trim();
        }

        private static bool TryObtenirParellesCamp(
            object data,
            out List<object[]> parellesCamp)
        {
            parellesCamp =
                new List<object[]>();

            object[] array =
                data as object[];

            if (array == null)
            {
                return false;
            }

            if (TryAfegirParellesCampDirectes(
                array,
                parellesCamp))
            {
                return true;
            }

            /*
             * El patró .NET observat a GRA0150Net rep root[1] com el bloc
             * de camps. El recorrem abans de fer una cerca recursiva genèrica
             * per prioritzar l'estructura principal del payload.
             */
            if (array.Length > 1 &&
                TryObtenirParellesCamp(
                    array[1],
                    out parellesCamp))
            {
                return true;
            }

            foreach (object item in array)
            {
                if (TryObtenirParellesCamp(
                    item,
                    out parellesCamp))
                {
                    return true;
                }
            }

            parellesCamp =
                new List<object[]>();

            return false;
        }

        private static bool TryAfegirParellesCampDirectes(
            object[] array,
            List<object[]> parellesCamp)
        {
            int inici;
            int fiExclusiu;

            if (!TryObtenirRangParelles(
                array,
                out inici,
                out fiExclusiu))
            {
                return false;
            }

            for (int i = inici; i < fiExclusiu; i++)
            {
                object[] parellaCamp =
                    array[i] as object[];

                if (!EsParellaCamp(
                    parellaCamp))
                {
                    return false;
                }

                parellesCamp.Add(
                    parellaCamp);
            }

            return parellesCamp.Count > 0;
        }

        private static bool TryObtenirRangParelles(
            object[] array,
            out int inici,
            out int fiExclusiu)
        {
            inici =
                0;

            fiExclusiu =
                0;

            if (array == null ||
                array.Length == 0)
            {
                return false;
            }

            int nombreEntrades;

            if (TryConvertirAEnter(
                array[0],
                out nombreEntrades)
                &&
                nombreEntrades > 0
                &&
                array.Length > 1
                &&
                EsParellaCamp(
                    array[1] as object[]))
            {
                inici =
                    1;

                fiExclusiu =
                    Math.Min(
                        array.Length,
                        nombreEntrades + 1);

                return true;
            }

            if (EsParellaCamp(
                array[0] as object[]))
            {
                inici =
                    0;

                fiExclusiu =
                    array.Length;

                return true;
            }

            return false;
        }

        private static bool EsParellaCamp(
            object[] valor)
        {
            return valor != null
                &&
                valor.Length >= 2
                &&
                valor[0] is string;
        }

        private static bool TryConvertirAEnter(
            object valor,
            out int resultat)
        {
            resultat =
                0;

            if (valor == null ||
                valor == DBNull.Value)
            {
                return false;
            }

            try
            {
                resultat =
                    Convert.ToInt32(
                        valor,
                        CultureInfo.InvariantCulture);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
