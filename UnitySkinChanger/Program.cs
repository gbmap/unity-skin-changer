using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitySkinChanger
{
    class Program
    {
        enum Theme
        {
            WHITE,
            DARK
        }

        static string targetExecutable = @"C:\Program Files\Unity\Editor\Unity.exe";
        static Theme theme = Theme.DARK;

        static Dictionary<Theme, byte> byteDict = new Dictionary<Theme, byte>()
        {
            { Theme.DARK, 0x74  },
            { Theme.WHITE, 0x75 }
        };

        

        static int[] GetSkinSignaturePosition(FileStream stream)
        {
            byte[] hexSignature = { 0x75, 0x08, 0x33, 0xC0, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3, 0x8B, 0x03, 0x48, 0x83, 0xC4, 0x20, 0x5B, 0xC3 };
            return ByteArrayRocks.Locate(ReadFully(stream), hexSignature);            
        }

        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static IEnumerable<long> PatternAt(byte[] source, byte[] pattern)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    yield return i;
                }
            }
        }

        static FileStream GetUnityExecutable()
        {
            if (!File.Exists(targetExecutable))
            {
                Console.WriteLine("Unity not found.");
                return null;
            }

            try
            {
                return File.Open(targetExecutable, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            }

            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("Unauthorized. Run as admin.");
                return null;
            }
        }

        static void ChangeSkin()
        {
            Console.WriteLine("Finding unity executable...");
            using (FileStream unityExecutable = GetUnityExecutable())
            {
                if (unityExecutable == null)
                    return;

                Console.WriteLine("Finding byte signature... this may take a while...");
                int[] position = GetSkinSignaturePosition(unityExecutable);

                if (position.Length == 0)
                {
                    Console.WriteLine("Couldn't find byte signature.");
                    return;
                }

                Console.WriteLine("Skin flag found at: " + position[0]);
                unityExecutable.Position = position[0];

                Console.WriteLine("Changing skin flag...");
                unityExecutable.WriteByte(byteDict[theme]); // Change skin flag.
                unityExecutable.Flush();

                Console.WriteLine("All set. Press any key to exit.");
                Console.ReadKey();
            }
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
                targetExecutable = args[0];
            if (args.Length > 1)
                theme = Convert.ToInt32(args[1]) == 1 ? Theme.DARK : Theme.WHITE;

            ChangeSkin();
        }



    }
}
