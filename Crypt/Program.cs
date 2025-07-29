using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Crypt
{
    class ConsoleColorPop : IDisposable
    {
        ConsoleColor col;
        ConsoleColor old;
        public ConsoleColorPop(ConsoleColor col)
        {
            this.col = col;
            old = Console.ForegroundColor;
            Console.ForegroundColor = col;
        }

        public void Dispose()
        {
            Console.ForegroundColor = old;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                using (new ConsoleColorPop(ConsoleColor.Green))
                    Console.WriteLine("Crypt - Copryright (C) Jonathan Sandusky 2016");
                Console.WriteLine(" ");
                Console.WriteLine("    Usage:");
                Console.WriteLine("crypt.exe <encrypt-mode> <path> <garble-names> -u [ungarble input path]");
                Console.WriteLine("   encrypt-mode:");
                Console.WriteLine("       /e = encrypt");
                Console.WriteLine("       /d = decrypt");
                Console.WriteLine("   path:");
                Console.WriteLine("       either a dir or file path");
                Console.WriteLine("       If path is a directory will recursively encrypt/decrypt files");
                Console.WriteLine("   garble-names:");
                Console.WriteLine("       default = off");
                Console.WriteLine("       garble = garble file and directory names");
                return;
            }
            string file = args[1];
            bool encrypt = true;
            string arg0 = args[0].ToLowerInvariant();
            if (arg0.Equals("encrypt") || arg0.Equals("e") || arg0.Equals("/e") || arg0.Equals("-e"))
                encrypt = true;
            else if (arg0.Equals("decrypt") || arg0.Equals("d") || args[0].ToLowerInvariant().Equals("/d") || arg0.Equals("-d"))
                encrypt = false;

            bool garble = false;
            if (args.Length >= 3)
            {
                if (args[2].ToLowerInvariant().Equals("garble"))
                    garble  = true;
            }

            if (!encrypt && args.Length >= 4 && args[3].ToLowerInvariant().Equals("-u"))
                file = SimplerAES.GarbleName(file);


            using (new ConsoleColorPop(ConsoleColor.Green))
            {
                if (encrypt)
                    Console.WriteLine("Encrypting...");
                else
                    Console.WriteLine("Decrypting...");
            }

            if (System.IO.Directory.Exists(file))
            {
                processDir(file, encrypt, garble);
                if (garble)
                    garbleDirs(file, encrypt);
            }
            else if (System.IO.File.Exists(file))
                processFile(file, encrypt, garble);
            else
            {
                using (new ConsoleColorPop(ConsoleColor.Red))
                    Console.WriteLine("Error: unable to determine file/path");
                return;
            }
            using (new ConsoleColorPop(ConsoleColor.Green))
                Console.WriteLine(string.Format("{0} Complete", encrypt ? "Encryption":"Decryption"));
        }

        static void processDir(string path, bool encrypt, bool garble)
        {
            using (new ConsoleColorPop(ConsoleColor.Yellow))
                Console.WriteLine(string.Format("Processing dir: {0}", path));
            foreach (string str in System.IO.Directory.EnumerateFiles(path))
                processFile(str, encrypt, garble);


            foreach (string dir in System.IO.Directory.EnumerateDirectories(path))
                processDir(dir, encrypt, garble);
        }

        static void garbleDirs(string path, bool encrypt)
        {
            foreach (string dir in System.IO.Directory.EnumerateDirectories(path))
                garbleDirs(dir, encrypt);
            System.IO.DirectoryInfo parent = System.IO.Directory.GetParent(path);
            System.IO.DirectoryInfo current = new System.IO.DirectoryInfo(path);
            if (encrypt)
                System.IO.Directory.Move(path, System.IO.Path.Combine(parent.FullName, SimplerAES.GarbleName(current.Name)));
            else
                System.IO.Directory.Move(path, System.IO.Path.Combine(parent.FullName, SimplerAES.UngarbleName(current.Name)));
        }

        static void processFile(string file, bool encrypt, bool garble)
        {
            using (new ConsoleColorPop(ConsoleColor.Cyan))
                Console.WriteLine(string.Format("{0} file '{1}'", encrypt ? "Encrypting" : "Decrypting", file));

            if (encrypt)
                SimplerAES.EncryptFile(file, garble);
            else
                SimplerAES.DecryptFile(file, garble);
        }
    }
}
