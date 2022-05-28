using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PetBookAPI.Extras
{
    public static class PasswordManager
    {
        private static int cost = 16;
        public static async Task<string> Hash(string password)
        {

            string s = await Task.Run(() => {
                string salt = BCrypt.Net.BCrypt.GenerateSalt(cost);
                return BCrypt.Net.BCrypt.HashPassword(password, salt);
            });
            return s;
        }
        public static bool isMatched(string input, string dbPassword)
        {
            return BCrypt.Net.BCrypt.Verify(input, dbPassword);
        }
    }
}