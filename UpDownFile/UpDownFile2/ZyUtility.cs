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

        /// <summary>
        /// 生成加密的文件链接字符串
        /// </summary>
        /// <param name="absoluteUri">Uri地址，例：http://192.168.1.1:8080</param>
        /// <param name="localfileuri">本地文件相对地址，例：/upload/file1.docx</param>
        /// <param name="openmode">文件的打开模式，read:只读，edit：编辑</param>
        /// <param name="userid">当前用户ID</param>
        /// <param name="username">当前用户名</param>
        /// <returns>加密后的文件链接字符串，例:ycsy://sfdkj239asdfasdfajsdfjajsd</returns>
        public static string EncryptedFileUri(string absoluteUri, string localfileuri, string openmode, string userid, string username)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("url=").Append(absoluteUri).Append(localfileuri)
              .Append("|")
              .Append("openmode=").Append(openmode)
              .Append("|")
              .Append("userid=").Append(userid)
              .Append("|")
              .Append("username=").Append(username);

            return "ycsy://" + ZyUtility.Cryptography.Encrypt(sb.ToString());
        }
    }
}
