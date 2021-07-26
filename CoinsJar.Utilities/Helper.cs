using Microsoft.IdentityModel.Tokens;
using System;

namespace CoinJar.Utilities
{
    public class Helper
    {
        public static int DecryptString(string encrString)
        {
            string decrypted = "";
            try
            {
                decrypted = Base64UrlEncoder.Decode(encrString);
            }
            catch (Exception ex)
            {
                decrypted = "";
            }
            if (decrypted.Length <= 9)
                return 0;
            else
                return Convert.ToInt32(decrypted.Substring(0, decrypted.Length - 9));
        }

        public static string EnryptString(string strEncrypted)
        {
            return Base64UrlEncoder.Encode(strEncrypted + "gp-ys-kar");            
        }
    }
}
