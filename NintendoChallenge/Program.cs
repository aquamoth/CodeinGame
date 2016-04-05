using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NintendoChallenge
{
    class Program
    {
        static void Main(string[] args)
        {
            testEncoder();
            encode(args);


            Console.ReadLine();
        }

        static void encode(string[] args)
        {
            var size = int.Parse(args[0]);
            var a = hexToUInt(args.Skip(1).Take(size / 16).ToArray());

            var b = centaurianOperation(size, a);

            Console.WriteLine(string.Join(" ", uintToHex(b)));
        }

        internal static uint[] centaurianOperation(int size, uint[] a)
        {
            var b = new uint[a.Length];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    b[(i + j) / 32] ^= (uint)(((a[i / 32] >> (i % 32)) & (a[j / 32 + size / 32] >> (j % 32)) & 1) << ((i + j) % 32));
                }
            }

            return b;
        }


        #region Converters

        private static string[] uintToHex(uint[] b)
        {
            return b.Select(x => x.ToString("x8")).ToArray();
        }

        private static UInt32[] hexToUInt(string[] v)
        {
            return v.Select(value => Convert.ToUInt32(value, 16)).ToArray();
        }

        #endregion Converters

        #region TDD

        private static void testEncoder()
        {
            var expected = hexToUInt(new[] { "46508fb7", "6677e201" });

            var actual = centaurianOperation(32, hexToUInt(new[] { "ebf2831f", "b0c152f9" }));

            if (!Enumerable.SequenceEqual(actual, expected))
                throw new ApplicationException("Encoder broken");
        }

        #endregion TDD

    }
}
