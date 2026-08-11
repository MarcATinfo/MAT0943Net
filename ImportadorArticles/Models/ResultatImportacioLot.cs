namespace A3ErpImportadorArticles.Models
{
    /// <summary>
    /// Resumeix el resultat d'una importació de diversos articles.
    /// </summary>
    public sealed class ResultatImportacioLot
    {
        public int TotalProcessats { get; private set; }

        public int TotalCreats { get; private set; }

        public int TotalActualitzats { get; private set; }

        public int TotalErrors { get; private set; }

        public bool TeErrors
        {
            get
            {
                return TotalErrors > 0;
            }
        }

        public bool TotCorrecte
        {
            get
            {
                return TotalProcessats > 0 &&
                       TotalErrors == 0;
            }
        }

        public ResultatImportacioLot()
        {
            TotalProcessats = 0;
            TotalCreats = 0;
            TotalActualitzats = 0;
            TotalErrors = 0;
        }

        public void RegistrarCreat()
        {
            TotalProcessats++;
            TotalCreats++;
        }

        public void RegistrarActualitzat()
        {
            TotalProcessats++;
            TotalActualitzats++;
        }

        public void RegistrarError()
        {
            TotalProcessats++;
            TotalErrors++;
        }
    }
}