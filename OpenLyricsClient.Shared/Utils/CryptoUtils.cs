using System.Security.Cryptography;
using System.Text;

namespace OpenLyricsClient.Shared.Utils;

public class CryptoUtils
{
    public static string ToMD5(string data)
    {
        MD5CryptoServiceProvider md5CryptoServiceProvider = new MD5CryptoServiceProvider();
        byte[] compute = md5CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(data));
        
        StringBuilder strBuilder = new StringBuilder();
        for (int i = 0; i < compute.Length; i++)
        {
            strBuilder.Append(compute[i].ToString("x2"));
        }
        
        return strBuilder.ToString();
    }
}