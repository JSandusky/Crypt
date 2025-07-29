using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Crypt
{
    public class SimplerAES
    {
        private static string abc = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";
        private static byte[] key = { 123, 217, 19, 11, 24, 26, 85, 45, 114, 184, 27, 162, 37, 112, 222, 209, 241, 24, 175, 144, 173, 53, 196, 29, 24, 26, 17, 218, 131, 236, 53, 209 };

        // Unused since the SHA1 is being used
        private static byte[] vector = { 146, 64, 191, 111, 23, 3, 113, 119, 231, 121, 221, 112, 79, 32, 114, 156 };
        private ICryptoTransform encryptor, decryptor;
        private ASCIIEncoding encoder;

        public static int Sha1Length = 20;

        //public SimplerAES()
        //{
        //    RijndaelManaged rm = new RijndaelManaged();
        //    encryptor = rm.CreateEncryptor(key, vector);
        //    decryptor = rm.CreateDecryptor(key, vector);
        //    encoder = new UTF8Encoding();
        //}

        public SimplerAES(byte[] sha1)
        {
            RijndaelManaged rm = new RijndaelManaged();
            encryptor = rm.CreateEncryptor(key, sha1.Take(vector.Length).ToArray());
            decryptor = rm.CreateDecryptor(key, sha1.Take(vector.Length).ToArray());
            encoder = new ASCIIEncoding();
        }

        static int WrapZero(int x, int min, int max)
        {
            return ((x - min) % (max - min)) + min;
        }

        static char GarbleChar(char value, int index)
        {
            if (value == ' ')
                return value;
            int c = abc.IndexOf(value);
            c += key[WrapZero(index, 0, key.Length - 1)];
            return abc[WrapZero(c, 0, abc.Length - 1)];
        }

        static char UngarbleChar(char value, int index)
        {
            if (value == ' ')
                return value;
            int c = abc.IndexOf(value);
            c -= key[WrapZero(index, 0, key.Length - 1)];
            while (c < 0)
                c += abc.Length - 1;
            return abc[WrapZero(c, 0, abc.Length - 1)];
        }

        public static string GarbleName(string inName)
        {
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < inName.Length; ++i)
                ret.Append(GarbleChar(inName[i], i));
            return ret.ToString();
        }

        public static string UngarbleName(string inName)
        {
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < inName.Length; ++i)
                ret.Append(UngarbleChar(inName[i], i));
            return ret.ToString();
        }

        /// <summary>
        /// Quickly encrypt a file at the given path
        /// </summary>
        /// <param name="path">file to encrypt</param>
        public static void EncryptFile(string path, bool garble)
        {
            // Compute SHA1 for using as IV
            byte[] data = System.IO.File.ReadAllBytes(path);
            byte[] sha1 = SimplerAES.SHA1(data);
            SimplerAES aes = new SimplerAES(sha1);

            // Write SHA1 followed by encrypted file data
            List<byte> ret = new List<byte>(sha1);
            ret.AddRange(aes.Encrypt(data));

            if (!garble)
                System.IO.File.WriteAllBytes(path, ret.ToArray());
            else
            {
                System.IO.File.WriteAllBytes(
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(path), GarbleName(System.IO.Path.GetFileNameWithoutExtension(path))) + System.IO.Path.GetExtension(path), ret.ToArray());
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// Quickly decrypt a file at the given path
        /// </summary>
        /// <param name="path">path to file to decrypt</param>
        public static void DecryptFile(string path, bool garble)
        {
            byte[] data = System.IO.File.ReadAllBytes(path);
            // Extract the prepended SHA1 for IV
            byte[] sha1 = data.Take(Sha1Length).ToArray();
            SimplerAES aes = new SimplerAES(sha1);

            // Decrypt the file data
            if (!garble)
                System.IO.File.WriteAllBytes(path, aes.Decrypt(data.Skip(Sha1Length).ToArray()));
            else
            {
                System.IO.File.WriteAllBytes(
                    System.IO.Path.Combine(
                        System.IO.Path.GetDirectoryName(path), UngarbleName(System.IO.Path.GetFileNameWithoutExtension(path))) + System.IO.Path.GetExtension(path), data.Skip(Sha1Length).ToArray());
                System.IO.File.Delete(path);
            }
        }

        public static byte[] SHA1(byte[] buffer)
        {
            using (SHA1CryptoServiceProvider prov = new SHA1CryptoServiceProvider())
                return prov.ComputeHash(buffer);
        }

        public string Encrypt(string unencrypted)
        {
            return Convert.ToBase64String(Encrypt(encoder.GetBytes(unencrypted)));
        }

        public string Decrypt(string encrypted)
        {
            return encoder.GetString(Decrypt(Convert.FromBase64String(encrypted)));
        }

        public byte[] Encrypt(byte[] buffer)
        {
            return Transform(buffer, encryptor);
        }

        public byte[] Decrypt(byte[] buffer)
        {
            return Transform(buffer, decryptor);
        }

        protected byte[] Transform(byte[] buffer, ICryptoTransform transform)
        {
            MemoryStream stream = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                cs.Write(buffer, 0, buffer.Length);
            }
            return stream.ToArray();
        }
    }
}
