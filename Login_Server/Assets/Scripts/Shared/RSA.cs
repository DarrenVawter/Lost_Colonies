using System;
using System.Security.Cryptography;
using UnityEngine;

public static class RSA
{

    public static string[] getKeys()
    {
        //lets take a new CSP with a new 2048 bit rsa key pair
        RSACryptoServiceProvider csp = new RSACryptoServiceProvider(1112);
        //get pub key
        var pubKey = csp.ExportParameters(false);
        //get priv key
        var privKey = csp.ExportParameters(true);
        
        //converting  public key into a string representation
        string pubKeyString;
        {
            //we need some buffer
            var sw = new System.IO.StringWriter();
            //we need a serializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //serialize the key into the stream
            xs.Serialize(sw, pubKey);
            //get the string from the stream
            pubKeyString = sw.ToString();
        }

        //converting  private key into a string representation
        string privKeyString;
        {
            //we need some buffer
            var sw = new System.IO.StringWriter();
            //we need a serializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //serialize the key into the stream
            xs.Serialize(sw, privKey);
            //get the string from the stream
            privKeyString = sw.ToString();
        }

        //return key strings
        return new string[] { pubKeyString, privKeyString };

    }

    public static string encrypt(string textToEncrypt, string pubKeyString)
    {
        //***************client-side**************

        //retrieve public key from key string
        var sr = new System.IO.StringReader(pubKeyString);
        var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
        var pubKey = (RSAParameters)xs.Deserialize(sr);

        //load key into csp
        RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
        csp.ImportParameters(pubKey);

        //for encryption, always handle bytes...
        var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(textToEncrypt);

        //encrypt data 
        var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);

        //return encrypted string as string
        return Convert.ToBase64String(bytesCypherText);
        
    }

    public static string decrypt(string encryptedText, string privKeyString)
    {
        //***************server-side**************
        byte[] encryptedStringBytes = Convert.FromBase64String(encryptedText);

        //retrieve private key
        var sr = new System.IO.StringReader(privKeyString);
        var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
        var privKey = (RSAParameters)xs.Deserialize(sr);

        //load private key into csp for decryption
        RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
        csp.ImportParameters(privKey);

        //decrypt and strip pkcs#1.5 padding
        var bytesPlainTextData = csp.Decrypt(encryptedStringBytes, false);

        //get decrypted message as string and return
        return System.Text.Encoding.Unicode.GetString(bytesPlainTextData);

    }
    
}