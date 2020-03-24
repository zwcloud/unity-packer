using System;
using System.Security.Cryptography;
using System.Text;

namespace UnityPacker {

    internal static class RandomUtils
    {
        private static readonly Random r = new Random();

        static string CreateMd5(string input)
        {
            var md5 = MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            var sb = new StringBuilder();
            foreach (byte t in hashBytes)
            {
                sb.Append(t.ToString("X2"));
            }
            return sb.ToString();
        }

        public static string RandomString(int len = 32)
        {
            var c = "";
            for (int i = 0; i < len; i++)
            {
                c += r.Next(0, 128);
            }
            return c;
        }

        public static string RandomHash()
        {
            return CreateMd5(RandomString()).ToLower();
        }
    }
}