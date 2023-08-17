using System;
using System.Text;

/// <summary>
/// Used to support basic base64 encyrption / decyption
/// </summary>
public static class Base64String
{
    /// <summary>
    /// Encrypts a string replacing the + and - symbols so file name stays correct
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string Encrypt(string text)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(text)).TrimEnd('=').Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// DEscryts a string replacing the - and / back to proper symbols
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string Decrypt(string text)
    {
        text = text.Replace('_', '/').Replace('-', '+');
        switch (text.Length % 4)
        {
            case 2:
                text += "==";
                break;
            case 3:
                text += "=";
                break;
        }
        return Encoding.UTF8.GetString(Convert.FromBase64String(text));
    }
}