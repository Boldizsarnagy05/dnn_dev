using NaturaCo.RecipeEditor;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Receptek.Test
{
    public class ReceptSzamoloTest
    {
        private ReceptSzamolo _receptSzamolo; // azért, hogy ez osztály szinten létezzen a tesztekhez

        [SetUp]
        public void Setup()
        {
            _receptSzamolo = new ReceptSzamolo(); // minden teszt előtt le fog futni, így ebben pár példányosítva van, lényegében ez lesz az arrange része a test-nek
        }

        [Test,
            TestCase(15, 20, 25),
            TestCase(0, 45, 45),
            TestCase(-5, 10, 0)]
        public void teljesIdoTest(int keszit, int sut, int vartErtek)
        {
            var tenylegesErtek = _receptSzamolo.teljesIdo(keszit, sut);

            Assert.AreEqual(tenylegesErtek, vartErtek);
        }

        [Test,
            TestCase(1000.0, 4, 250.0),
            TestCase(800.0, 2, 300.0),
            TestCase(500.0, -1, 0.0)]
        public void kaloriaSzamitasTest(double osszKal, int adag, double vartErtek)
        {
            var tenylegesErtek = _receptSzamolo.kaloriaSzamitasa(osszKal, adag);

            Assert.AreEqual(vartErtek, tenylegesErtek);
        }

        [Test,
            TestCase(4, true),
            TestCase(0, false),
            TestCase(150, true)]
        public void ervenyesAdag(int adag, bool vartErtek)
        {
            var tenylegesErtek = _receptSzamolo.ervenyesAdag(adag);

            Assert.AreEqual(vartErtek, tenylegesErtek);
        }

        [Test,
            TestCase("Ez egy nagyon finom saláta recept.", true),
            TestCase("Fincsi", false),
            TestCase("", false)]
        public void ervenyesLeirasTest(string leiras, bool vartErtek)
        {
            var tenylegesErtek = _receptSzamolo.ervenyesLeiras(leiras);

            Assert.AreEqual(vartErtek, tenylegesErtek);
        }

        [Test,
            TestCase(15, true),
            TestCase(0, true),
            TestCase(-5, false)]
        public void ervenyesIdoTest(int perc, bool vartErtek)
        {
            var tenylegesErtek = _receptSzamolo.ervenyesIdo(perc);

            Assert.AreEqual(vartErtek, tenylegesErtek);
        }

        [Test,
            TestCase(5, true),
            TestCase(1, false),
            TestCase(0, false)]
        public void elegendoHozzavaloTest(int hozzavaloSzam, bool vartErtek)
        {
            var tenylegesErtek = _receptSzamolo.elegendoHozzavalo(hozzavaloSzam);

            Assert.AreEqual(vartErtek, tenylegesErtek);
        }
    }
}
