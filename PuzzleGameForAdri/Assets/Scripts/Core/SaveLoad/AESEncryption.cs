using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Used to encrypt / decrypt save data using AES.
/// Code inspried from: https://stackoverflow.com/questions/8041451/good-aes-initialization-vector-practice
/// </summary>
public static class AESEncryption
{
    private const int AesKeySize = 16;                  // default key size
    private const string key = "j#lD1&_mAl4!*d$p";      // key

    #region Encryption
    private static string EncryptData(string data, byte[] keyValue)
    {
        return Convert.ToBase64String(EncryptData(Encoding.ASCII.GetBytes(data), keyValue));
    }

    public static byte[] EncryptData(byte[] data)
    {
        return EncryptData(data, Encoding.ASCII.GetBytes(key));
    }

    public static string EncryptDataString(string data)
    {
        return EncryptData(data, Encoding.ASCII.GetBytes(key));
    }

    public static byte[] EncryptDataS(string data)
    {
        return EncryptData(Encoding.ASCII.GetBytes(data), Encoding.ASCII.GetBytes(key));
    }

    private static byte[] EncryptData(byte[] data, byte[] keyValue)
    {
        if (data == null || keyValue.Length <= 0)
            throw new ArgumentNullException($"{nameof(data)} cannot be empty");

        if (keyValue == null || keyValue.Length != AesKeySize)
            throw new ArgumentException($"{nameof(keyValue)} must be length of {AesKeySize}");

        using (var aes = new AesCryptoServiceProvider
        {
            Key = keyValue,
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7
        })
        {
            aes.GenerateIV();
            var iv = aes.IV;
            using (var encrypter = aes.CreateEncryptor(aes.Key, iv))
            using (var cipherStream = new MemoryStream())
            {
                using (var tCryptoStream = new CryptoStream(cipherStream, encrypter, CryptoStreamMode.Write))
                using (var tBinaryWriter = new BinaryWriter(tCryptoStream))
                {
                    // prepend IV to data
                    cipherStream.Write(iv, 0, iv.Length);
                    tBinaryWriter.Write(data);
                    tCryptoStream.FlushFinalBlock();
                }
                var cipherBytes = cipherStream.ToArray();

                return cipherBytes;
            }
        }
    }
    #endregion

    #region Decryption
    public static string DecryptData(string data)
    {
        return DecryptData(data, Encoding.UTF8.GetBytes(key));
    }

    public static string DecryptData(byte[] data)
    {
        return Encoding.UTF8.GetString(DecryptData(data, Encoding.UTF8.GetBytes(key)));
    }

    private static string DecryptData(string data, byte[] keyValue)
    {
        return Encoding.UTF8.GetString(DecryptData(Convert.FromBase64String(data), keyValue));
    }

    private static byte[] DecryptData(byte[] data, byte[] keyValue)
    {
        if (data == null || data.Length <= 0)
        {
            throw new ArgumentNullException($"{nameof(data)} cannot be empty");
        }

        if (keyValue == null || keyValue.Length != AesKeySize)
        {
            throw new ArgumentException($"{nameof(keyValue)} must be length of {AesKeySize}");
        }

        using (var aes = new AesCryptoServiceProvider
        {
            Key = keyValue,
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7
        })
        {
            // get first KeySize bytes of IV and use it to decrypt
            var iv = new byte[AesKeySize];
            Array.Copy(data, 0, iv, 0, iv.Length);

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, aes.CreateDecryptor(aes.Key, iv), CryptoStreamMode.Write))
                using (var binaryWriter = new BinaryWriter(cs))
                {
                    // decrypt cipher text from data, starting just past the IV
                    binaryWriter.Write(
                        data,
                        iv.Length,
                        data.Length - iv.Length
                    );
                }

                var dataBytes = ms.ToArray();

                return dataBytes;
            }
        }
    }
    #endregion
}