using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using UnitTestExample.Controllers;

namespace UnitTestExample.Test
{
    public class AccountControllerTestFixture
    {
        [Test,
        TestCase("abcd1234", false),
        TestCase("irf@uni-corvinus", false),
        TestCase("irf.uni-corvinus.hu", false),
        TestCase("irf@uni-corvinus.hu", true),
            ]
        public void TestValidateEmail(string email, bool expectedResult)
        {
            // Arrange
            var accountController = new AccountController();

            // Act
            var actualResult = accountController.ValidateEmail(email);

            // Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test,
            TestCase("abcdeASDSA", false),
            TestCase("ABCDEFHG123", false),
            TestCase("abcedsfdsd123", false),
            TestCase("abc123", false),
            TestCase("Abcd1234", true),
            ]
        public void TestValidatePassword(string password, bool expectedResult)
        {
            // Arrange
            var accountController = new AccountController();
            // Act
            var actualResult = accountController.ValidatePassword(password);
            // Assert
            Assert.AreEqual(actualResult, expectedResult);
        }

        [Test,
        TestCase("irf@uni-corvinus.hu", "Abcd1234"),
        TestCase("irf@uni-corvinus.hu", "Abcd1234567"),
        ]
        public void TestRegisterHappyPath(string email, string password)
        {
            // Arrange
            var accountController = new AccountController();

            // Act
            var actualResult = accountController.Register(email, password);

            // Assert
            Assert.AreEqual(email, actualResult.Email);
            Assert.AreEqual(password, actualResult.Password);
            Assert.AreNotEqual(Guid.Empty, actualResult.ID);
        }

        [
        Test,
        TestCase("irf@uni-corvinus", "Abcd1234"),
        TestCase("irf.uni-corvinus.hu", "Abcd1234"),
        TestCase("irf@uni-corvinus.hu", "abcd1234"),
        TestCase("irf@uni-corvinus.hu", "ABCD1234"),
        TestCase("irf@uni-corvinus.hu", "abcdABCD"),
        TestCase("irf@uni-corvinus.hu", "Ab1234"),
        ]
        public void TestRegisterValidateException(string email, string password)
        {
            // Arrange
            var accountController = new AccountController();

            // Act

            try
            {
                var actualResult = accountController.Register(email, password);
                Assert.Fail(); // be kellett írni az Account.cs file-ba a Guid-hoz, hogy = Guid.NewGuid(), mert amúgy sosem kapott értéket, és mindig üres volt
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<ValidationException>(ex);
            }
        }
    }
}