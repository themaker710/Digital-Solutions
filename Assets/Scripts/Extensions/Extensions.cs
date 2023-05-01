using System.Collections.Generic;
using System.Data;
using UnityEngine;
using System.Security.Cryptography;
using System;
using UnityEngine.Networking;
using System.IO;
using System.Text;

//Internal allows namespace script access
internal static class Extensions
{
    /// <summary>
    /// Generic SafeGet retrieves variables from the database, however inserts the default value in case of a null field.
    /// </summary>
    /// <typeparam name="T">Type of variable retrieving.</typeparam>
    /// <param name="index">Position of variable in query</param>
    /// <returns>Generic return variable</returns>
    internal static T SafeGet<T>(this IDataReader reader, int index)
    {
        //Shorthand version
        //if (!reader.IsDBNull(index))
        //    return (T)reader.GetValue(index);
        //return default;

        //This version handles potential known errors and increases usability.
        //According to some profiling reports, this way to check for null can cause major lag spikes and latency in other connections/queries. Optimization recommended.
        if (!reader.IsDBNull(index))
        {
            object value = reader.GetValue(index);
            System.Type type = value.GetType();
            
            //NUMBER datatype stored as 'long' datatype in SQLite (i.e. Int64)
            if (type == typeof(long))
            {
                //Unpack as int and update type. Implicit cast possible as int < long data wise.
                value = (int)(long)value;
                type = typeof(int);
            }

            if (type != typeof(T))
            {
                Debug.Log($"Expected data type '{typeof(T)}' was not returned, got '{type}' instead.");
                throw new System.FormatException("Type does not match retrieved data. Check specified type and try again");
            }
            else
                return (T)value;
        }
        Debug.Log($"Specified row of field '{reader.GetName(index)}' is null, returning default value for {typeof(T)}");
        return default;
    }
    internal static string HashFile(string uri)
    {
        FileStream file = new FileStream(uri, FileMode.Open);

        HashAlgorithm sha = new SHA1CryptoServiceProvider();

        //Compute the hash value from the filestream
        byte[] retVal = sha.ComputeHash(file);

        //Close the file
        file.Close();

        //Create a new Stringbuilder to collect the bytes and create a string
        StringBuilder sb = new StringBuilder();

        //Loop through each byte of the hashed data and format each one as a hexadecimal string
        for (int i = 0; i < retVal.Length; i++)
            sb.Append(retVal[i].ToString("x2"));

        Debug.Log($"File {uri} hash: {sb}");

        return sb.ToString();
    }


    //This algorithm is sourced from the Microsoft.AspNet.Identity NuGet package and is used to hash passwords for storage in the database.
    //As per the documentation on Rfc2898DeriveBytes, the method was refined to use more up to date hashing algorithms and is considered secure for the extent of the task.
    /// <summary>
    /// Hashes a password using PBKDF2 and a random salt.
    /// </summary>
    /// <param name="password"></param>
    /// <returns type="string" name="key">Salted key with salt prepended</returns>
    /// <exception cref="ArgumentNullException"></exception>
    internal static string HashPassword(string password)
    {
        //This method came with hex literals in place of integers, which have been converted to signed ints for readability as it is no longer package level code.
        //Rfc2898DeriveBytes uses PBKDF2 to hash, and a psuedo-random number generator from the SHA256 specification inside .NET to determine a random salt for every string.
        byte[] salt;
        byte[] key;
        //Null Check
        if (password == null)
            throw new ArgumentNullException("password");

        //initialising the hashing package, with a plain string, a salt size (16), and a number of iterations (10000)
        using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 16, 10000, HashAlgorithmName.SHA256))
        {
            //Retrieve the salt
            salt = bytes.Salt;
            //Retrieve the salted hexadecimal key with length 32
            key = bytes.GetBytes(32);
        }
        //create destination byte array of length 48
        byte[] dst = new byte[48];
        //copy the salt into the initial 16 bytes
        Buffer.BlockCopy(salt, 0, dst, 0, 16);
        //copy the key into the remaining 32 bytes.
        Buffer.BlockCopy(key, 0, dst, 16, 32);
        return Convert.ToBase64String(dst);
    }

    /// <summary>
    /// Compare two passwords to determine if they are the same without revealing the original password.
    /// </summary>
    /// <param name="hashedPassword"></param>
    /// <param name="password"></param>
    /// <returns type="bool"></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool VerifyHashedPassword(string hashedPassword, string password)
    {
        //Null checks
        if (hashedPassword == null) return false;
        if (password == null) throw new ArgumentNullException("password");

        byte[] src = Convert.FromBase64String(hashedPassword);

        //If the length of the hashed password is not 48, return false.
        if (src.Length != 48) return false;

        //Determine original salt
        byte[] salt = new byte[16];
        Buffer.BlockCopy(src, 0, salt, 0, 16);

        //Determine original key
        byte[] key1 = new byte[32];
        Buffer.BlockCopy(src, 16, key1, 0, 32);

        byte[] key2;
        //Recompute hash with the same salt
        using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256)) key2 = bytes.GetBytes(32);

        //Determine if resulting keys are the same
        return key1.ByteEquals(key2);
    }

    internal static bool ByteEquals(this byte[] a, byte[] b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null || a.Length != b.Length) return false;
        
        var areSame = true;
        for (var i = 0; i < a.Length; i++)
            areSame &= (a[i] == b[i]);
        return areSame;
    }
    
    //Iterate through a dictionaries values and return the key
    internal static TKey GetKey<TKey, TValue>(this Dictionary<TKey, TValue> dict, TValue value)
    {
        foreach (KeyValuePair<TKey, TValue> pair in dict)
        {
            if (pair.Value.Equals(value))
                return pair.Key;
        }
        return default;
    }
    internal static bool IsNullOrWhiteSpace(this string str)
    {
        return string.IsNullOrWhiteSpace(str);
    }
    internal static int IndexOf(this System.Array arr, object query)
    {
        return System.Array.IndexOf(arr, query);
    }
    internal static bool IsLetter(this char c)
    {
        return char.IsLetter(c);
    }
    internal static string Initials(this string s)
    {
        string[] words = s.Split(' ');

        //Get first character from every word and combine into a result string
        string result = "";
        foreach (string w in words)
            result += w[0].ToUpper();

        return result;
    }
    //internal static string Join(this System.Array arr, string seperator)
    //{
    //    return string.Join(seperator, arr);
    //}
    internal static char ToUpper(this char c)
    {
        return char.ToUpper(c);
    }
}

internal class BypassCertificate : CertificateHandler 
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        //Replace with specific key as determined by school signed certificate
        return true;

    }
}