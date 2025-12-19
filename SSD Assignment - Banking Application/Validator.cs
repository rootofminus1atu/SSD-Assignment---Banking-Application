using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking_Application
{
    public static class Validator
    {
        public static bool IsValidName(string name) => !string.IsNullOrWhiteSpace(name) && name.Length <= 100;

        public static bool IsValidAmount(double amount) => amount > 0;

        public static bool IsValidAccountNo(string accNo) => Guid.TryParse(accNo, out _);
    }

}
