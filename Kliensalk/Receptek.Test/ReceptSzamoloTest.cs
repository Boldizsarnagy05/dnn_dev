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
        [Test,
            TestCase(1000, 4, 250)
            ]
        public void KaloriaKiszamitasaTest(double kaloria, int adagok, double vartErtek)
        {
            // Arrange
            var receptSzamolo = new ReceptSzamolo();

            // Act
            var tenylegesErtek = receptSzamolo.KaloriaKiszamitasa(kaloria, adagok);

            // Assert
            Assert.That(tenylegesErtek, Is.EqualTo(vartErtek));
        }
    }
}
