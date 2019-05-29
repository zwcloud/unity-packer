using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using YamlDotNet.RepresentationModel;

namespace UnityPacker
{
    internal static class RandomUtils
    {
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

        static readonly Random r = new Random();

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