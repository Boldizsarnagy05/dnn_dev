using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaturaCo.RecipeEditor
{
    public class ReceptSzamolo
    {
        public double KaloriaKiszamitasa(double osszesKaloria, int adagokSzama)
        {
            if (adagokSzama <= 0) return 0;
            else return osszesKaloria / adagokSzama;
        }
    }
}
