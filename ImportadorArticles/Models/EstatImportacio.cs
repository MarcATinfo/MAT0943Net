using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace A3ErpImportadorArticles.Models
{
    public enum EstatImportacio
    {
        Pendent = 0,
        Nou = 1,
        Actualitzacio = 2,
        SenseCanvis = 3,
        Error = 4,
        Importat = 5
    }
}
