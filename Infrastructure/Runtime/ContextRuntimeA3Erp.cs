using System;
using System.Data.Common;

namespace MAT0943Net.Infrastructure.Runtime
{
    /// <summary>
    /// Manté les connexions que a3ERP entrega a la DLL
    /// quan l'usuari entra en una empresa.
    ///
    /// No es confia únicament en l'ordre dels paràmetres:
    /// les connexions de sistema i empresa s'identifiquen
    /// pel nom de la base de dades.
    /// </summary>
    internal sealed class ContextRuntimeA3Erp
    {
        private const string BaseDadesSistemaA3Erp =
            "A3ERP$SISTEMA";

        /// <summary>
        /// Cadena de connexió de la base de dades
        /// de l'empresa activa.
        /// </summary>
        public string ConnexioEmpresa { get; private set; }

        /// <summary>
        /// Cadena de connexió de la base de dades
        /// de sistema d'a3ERP.
        /// </summary>
        public string ConnexioSistema { get; private set; }

        /// <summary>
        /// Nom de la base de dades de l'empresa activa.
        /// </summary>
        public string BaseDadesEmpresa { get; private set; }

        /// <summary>
        /// Nom de la base de dades de sistema.
        /// </summary>
        public string BaseDadesSistema { get; private set; }

        /// <summary>
        /// Indica si les connexions han arribat en l'ordre contrari
        /// al que declara la signatura del mètode Iniciar.
        /// </summary>
        public bool ConnexionsInvertidesDetectades { get; private set; }

        /// <summary>
        /// Indica si ja disposem d'una empresa activa preparada.
        /// </summary>
        public bool TeEmpresaInicialitzada
        {
            get
            {
                return
                    !string.IsNullOrWhiteSpace(ConnexioEmpresa) &&
                    !string.IsNullOrWhiteSpace(BaseDadesEmpresa);
            }
        }

        public ContextRuntimeA3Erp()
        {
            Netejar();
        }

        /// <summary>
        /// Rep les dues connexions entregades per a3ERP i determina
        /// quina correspon al sistema i quina a l'empresa activa.
        /// </summary>
        public void InicialitzarDesA3Erp(
            string connexioPrimera,
            string connexioSegona)
        {
            Netejar();

            if (string.IsNullOrWhiteSpace(connexioPrimera))
            {
                throw new ArgumentException(
                    "La primera connexió rebuda d'a3ERP és buida.",
                    nameof(connexioPrimera));
            }

            if (string.IsNullOrWhiteSpace(connexioSegona))
            {
                throw new ArgumentException(
                    "La segona connexió rebuda d'a3ERP és buida.",
                    nameof(connexioSegona));
            }

            string baseDadesPrimera =
                ObtenirBaseDades(connexioPrimera);

            string baseDadesSegona =
                ObtenirBaseDades(connexioSegona);

            bool primeraEsSistema =
                EsBaseDadesSistema(baseDadesPrimera);

            bool segonaEsSistema =
                EsBaseDadesSistema(baseDadesSegona);

            if (primeraEsSistema && !segonaEsSistema)
            {
                // Ordre declarat habitual:
                // primera connexió = sistema
                // segona connexió = empresa
                ConnexioSistema = connexioPrimera;
                BaseDadesSistema = baseDadesPrimera;

                ConnexioEmpresa = connexioSegona;
                BaseDadesEmpresa = baseDadesSegona;

                ConnexionsInvertidesDetectades = false;
            }
            else if (!primeraEsSistema && segonaEsSistema)
            {
                // Les connexions han arribat invertides.
                ConnexioSistema = connexioSegona;
                BaseDadesSistema = baseDadesSegona;

                ConnexioEmpresa = connexioPrimera;
                BaseDadesEmpresa = baseDadesPrimera;

                ConnexionsInvertidesDetectades = true;
            }
            else
            {
                throw new InvalidOperationException(
                    "No s'han pogut distingir les connexions de sistema " +
                    "i empresa rebudes des d'a3ERP. " +
                    "Base de dades primera: '" +
                    baseDadesPrimera +
                    "'. Base de dades segona: '" +
                    baseDadesSegona +
                    "'.");
            }

            if (string.IsNullOrWhiteSpace(BaseDadesEmpresa))
            {
                throw new InvalidOperationException(
                    "No s'ha pogut identificar la base de dades " +
                    "de l'empresa activa.");
            }
        }

        /// <summary>
        /// Elimina totes les dades associades a l'empresa anterior.
        /// Es cridarà quan a3ERP executi Finalizar.
        /// </summary>
        public void Netejar()
        {
            ConnexioEmpresa = string.Empty;
            ConnexioSistema = string.Empty;
            BaseDadesEmpresa = string.Empty;
            BaseDadesSistema = string.Empty;
            ConnexionsInvertidesDetectades = false;
        }

        /// <summary>
        /// Extreu el nom de la base de dades d'una cadena de connexió.
        /// Admet les claus Initial Catalog i Database.
        /// </summary>
        private static string ObtenirBaseDades(
            string cadenaConnexio)
        {
            DbConnectionStringBuilder builder =
                new DbConnectionStringBuilder
                {
                    ConnectionString = cadenaConnexio
                };

            string baseDades =
                ObtenirValor(
                    builder,
                    "Initial Catalog",
                    "Database");

            return baseDades.Trim();
        }

        /// <summary>
        /// Indica si el nom rebut correspon a la base
        /// de dades de sistema d'a3ERP.
        /// </summary>
        private static bool EsBaseDadesSistema(
            string baseDades)
        {
            if (string.IsNullOrWhiteSpace(baseDades))
            {
                return false;
            }

            string valorNormalitzat =
                baseDades.Trim();

            return
                string.Equals(
                    valorNormalitzat,
                    BaseDadesSistemaA3Erp,
                    StringComparison.OrdinalIgnoreCase)
                ||
                valorNormalitzat.EndsWith(
                    "$SISTEMA",
                    StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Retorna el primer valor informat entre diverses
        /// claus equivalents.
        /// </summary>
        private static string ObtenirValor(
            DbConnectionStringBuilder builder,
            params string[] claus)
        {
            foreach (string clau in claus)
            {
                if (!builder.ContainsKey(clau))
                {
                    continue;
                }

                object valor = builder[clau];

                if (valor == null)
                {
                    continue;
                }

                string text =
                    Convert.ToString(valor);

                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text.Trim();
                }
            }

            return string.Empty;
        }
    }
}
