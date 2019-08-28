using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

public static class Utility
{
    public const string EMAIL_PATTERN = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";
    public const string USERNAME_AND_DISCRIMINATOR_PATTERN = @"^[a-zA-Z0-9]{4,20}#[0-9]{4}$";
    public const string USERNAME_PATTERN = @"^[a-zA-Z0-9]{4,20}$";
    public const string RANDOM_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static bool IsEmail(string Email)
    {        
        if (Email != null)
        {
            return Regex.IsMatch(Email, EMAIL_PATTERN);
        }
        else
        {
            return false;
        }
    }

    public static bool IsUsername(string Username)
    {
        if (Username != null)
        {
            return Regex.IsMatch(Username, USERNAME_PATTERN);
        }
        else
        {
            return false;
        }
    }

    public static bool IsUsernameAndDiscriminator(string Username)
    {
        if (Username != null)
        {
            return Regex.IsMatch(Username, USERNAME_AND_DISCRIMINATOR_PATTERN);
        }
        else
        {
            return false;
        }
    }

    public static string GenerateRandomToken(int length)
    {
        Random r = new Random();
        return new string(Enumerable.Repeat(RANDOM_CHARS, length).Select(s => s[r.Next(s.Length)]).ToArray());
    }

    public static string Sha256FromString(string toHash)
    {
        var message = System.Text.Encoding.UTF8.GetBytes(toHash);
        SHA256Managed hashString = new SHA256Managed();

        string hex = "";
        var hashValue = hashString.ComputeHash(message);
        foreach(byte x in hashValue)
        {
            hex += string.Format("{0:x2}", x);
        }

        return hex;
    }

}
