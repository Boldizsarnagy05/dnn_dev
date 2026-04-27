using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnitTestExample.Abstractions;
using UnitTestExample.Entities;
using UnitTestExample.Services;

namespace UnitTestExample.Controllers
{
    public class AccountController
    {        
        public IAccountManager AccountManager { get; set; }

        public AccountController()
        {
            AccountManager = new AccountManager();
        }

        public Account Register(string email, string password)
        {
            if(!ValidateEmail(email))
                throw new ValidationException(
                    "A megadott e-mail cím nem megfelelő!");
            if(!ValidatePassword(password))
                throw new ValidationException(
                    "A megadott jelszó nem megfelelő!\n" +
                    "A jelszó legalább 8 karakter hosszú kell legyen, csak az angol ABC betűiből és számokból állhat, és tartalmaznia kell legalább egy kisbetűt, egy nagybetűt és egy számot."); // 3 db t betű és eredetileg kétszer van az if-ben a !ValidateEmail(email) ahelyett, hogy !ValidatePassword(password)

            var account = new Account()
            {
                Email = email,
                Password = password
            };

            var newAccount = AccountManager.CreateAccount(account);

            return newAccount;
        }

        public bool ValidateEmail(string email)
        {            
            return Regex.IsMatch(
                email, 
                @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
        }

        public bool ValidatePassword(string password)
        {
            if(!Regex.IsMatch(password, @"^[a-zA-Z0-9]{8,}$"))
            {
                return false;
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return false;
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return false;
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                return false;
            }

            return true;
        }
    }
}
