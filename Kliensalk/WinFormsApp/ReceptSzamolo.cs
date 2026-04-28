using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaturaCo.RecipeEditor
{
    public class ReceptSzamolo
    {
        public int teljesIdo(int elkeszites, int sutes)
        {
            if (elkeszites < 0 || sutes < 0) return 0;
            return elkeszites + sutes;
        }

        public double kaloriaSzamitasa(double osszKaloria, int adag)
        {
            if (adag <= 0) return 0;
            return Math.Round(osszKaloria / adag, 2);
        }

        public bool ervenyesAdag(int adag)
        {
            return adag > 0 && adag <= 100;
        }

        public bool ervenyesLeiras(string leiras)
        {
            if (string.IsNullOrEmpty(leiras)) return true;
            return leiras.Trim().Length >= 10;
        }
        public bool ervenyesIdo(int perc)
        {
            return perc >= 0;
        }

        public bool elegendoHozzavalo(int hozzavalokSzama)
        {
            return hozzavalokSzama >= 1;
        }
    }
}