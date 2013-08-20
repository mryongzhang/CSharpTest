using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace ZyUtility
{
    /// <summary>
    /// 用于字符串的加密解密
    /// </summary>
    public static class Cryptography
    {
        public static string Encrypt(this string dataToEncrypt)
        {
            // Initialize
            AesManaged encryptor = new AesManaged();

            // Get the string salt, on this case I pass a hard coded value. Then, create the byte[]
            string salt = "zy_#fkerdos$%xxxhbt";
            byte[] saltBytes = new UTF8Encoding().GetBytes(salt);
            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(salt, saltBytes);

            encryptor.Key = rfc.GetBytes(16);
            encryptor.IV = rfc.GetBytes(16);
            encryptor.BlockSize = 128;

            // create a memory stream
            using (MemoryStream encryptionStream = new MemoryStream())
            {
                // Create the crypto stream
                using (CryptoStream encrypt = new CryptoStream(encryptionStream, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    // Encrypt
                    byte[] utfD1 = UTF8Encoding.UTF8.GetBytes(dataToEncrypt);
                    encrypt.Write(utfD1, 0, utfD1.Length);
                    encrypt.FlushFinalBlock();
                    encrypt.Close();

                    // Return the encrypted data
                    return Convert.ToBase64String(encryptionStream.ToArray());
                }
            }
        }

        public static string Decrypt(this string encryptedString)
        {
            // Initialize
            AesManaged decryptor = new AesManaged();
            byte[] encryptedData = Convert.FromBase64String(encryptedString);

            // Get the string salt, on this case I pass a hard coded value. Then, create the byte[]
            string salt = "zy_#fkerdos$%xxxhbt";
            byte[] saltBytes = new UTF8Encoding().GetBytes(salt);
            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(salt, saltBytes);

            decryptor.Key = rfc.GetBytes(16);
            decryptor.IV = rfc.GetBytes(16);
            decryptor.BlockSize = 128;

            // create a memory stream
            using (MemoryStream decryptionStream = new MemoryStream())
            {
                // Create the crypto stream
                using (CryptoStream decrypt = new CryptoStream(decryptionStream, decryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    try
                    {
                        // Encrypt
                        decrypt.Write(encryptedData, 0, encryptedData.Length);
                        decrypt.Flush();
                        decrypt.Close();
                    }
                    catch { }

                    // Return the unencrypted data
                    byte[] decryptedData = decryptionStream.ToArray();
                    return UTF8Encoding.UTF8.GetString(decryptedData, 0, decryptedData.Length);
                }
            }
        }
    }
}
