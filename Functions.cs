using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MarinaRegSystem
{
    public class Functions
    {
        private const string AesIV256 = @"!QAZ2WSX#EDC4REV";
        private const string AesKey256 = @"5TGB&YHZ7UJM(IK<5TGB&YHN7UJM(IS<";
        public static string Encrypt256(string text)
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.IV = Encoding.UTF8.GetBytes(AesIV256);
            aes.Key = Encoding.UTF8.GetBytes(AesKey256);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            byte[] src = Encoding.Unicode.GetBytes(text);
            using (ICryptoTransform encrypt = aes.CreateEncryptor())
            {
                byte[] dest = encrypt.TransformFinalBlock(src, 0, src.Length);

                return Convert.ToBase64String(dest);
            }
        }
        public static string Decrypt256(string text)
        {
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.IV = Encoding.UTF8.GetBytes(AesIV256);
            aes.Key = Encoding.UTF8.GetBytes(AesKey256);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] src = System.Convert.FromBase64String(text);
            using (ICryptoTransform decrypt = aes.CreateDecryptor())
            {
                byte[] dest = decrypt.TransformFinalBlock(src, 0, src.Length);
                return Encoding.Unicode.GetString(dest);
            }
        }

    }
}
