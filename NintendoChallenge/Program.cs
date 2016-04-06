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

            //writeEncode(args);

            //writeEncode(new[] { "32", "e5", "83" });

            //writeEncode(new[] { "32", "00000000", "00000000" });
            //writeEncode(new[] { "32", "00000000", "00000001" });
            //writeEncode(new[] { "32", "00000001", "00000001" });
            //writeEncode(new[] { "32", "00000001", "00000000" });
            //writeEncode(new[] { "32", "00000002", "00000001" });
            //writeEncode(new[] { "32", "00000001", "00000002" });

            //writeEncode(new[] { "32", "00000004", "00000001" });
            //writeEncode(new[] { "32", "00000002", "00000002" });
            //writeEncode(new[] { "32", "00000001", "00000004" });

            //writeEncode(new[] { "32", "00000008", "00000001" });
            //writeEncode(new[] { "32", "00000004", "00000002" });
            //writeEncode(new[] { "32", "00000002", "00000004" });
            //writeEncode(new[] { "32", "00000001", "00000008" });

            //writeEncode(new[] { "32", "00000010", "00000001" });
            //writeEncode(new[] { "32", "00000008", "00000002" });
            //writeEncode(new[] { "32", "00000004", "00000004" });
            //writeEncode(new[] { "32", "00000002", "00000008" });
            //writeEncode(new[] { "32", "00000001", "00000010" });

            //writeEncode(new[] { "32", "00000014", "00000001" });
            //writeEncode(new[] { "32", "0000000a", "00000002" });
            //writeEncode(new[] { "32", "00000005", "00000004" });
            //writeEncode(new[] { "32", "00000005", "10000004" });



            //writeEncode(new[] { "32", "00000011", "00000001" });
            //writeEncode(new[] { "32", "00000011", "00000010" });
            //writeEncode(new[] { "32", "00000011", "00000011" });
            //writeEncode(new[] { "32", "11000011", "00000010" });
            //writeEncode(new[] { "32", "11000011", "00000100" });
            ////writeEncode(new[] { "16", "00010001", "00000010" });
            ////writeEncode(new[] { "16", "00010001", "01010101" });
            ////writeEncode(new[] { "32", "ffffffff", "ffffffff" });

            Console.WriteLine("WriteEncode");
            writeEncode(new[] { "32", "80000000", "80000000" });

            Console.WriteLine("Trying to decode");
            decode("00000000", "40000000");

            Console.ReadLine();
        }

        static void decode(params string[] args)
        {
            var u = hexToUInt(args);
            foreach (var result in centurianDecoder(u[0], u[1]))
            {
                Console.WriteLine(string.Join(" ", uintToHex(result)));

                Console.WriteLine("Reencoding result:");
                encode(new[] { "32" }.Concat(uintToHex(result)).ToArray());
            }
        }

        static IEnumerable<uint[]> centurianDecoder(uint lsb, uint msb)
        {
            var solutions = new List<uint[]>(new[] { new uint[] { 0, 0 } });

            //For each bit in msb
            //Bit 31 can never be set!
            for (int bit = 30; bit >= 30; bit--)
            {
                var mask = ((uint)1 << bit);
                var expectedBitValue = msb & mask;
                if (expectedBitValue != 0)
                {
                    foreach (var solution in solutions)
                    {
                        //Find all ways to manipulate the solution to solve for this bit
                        for (int j = 0; j < 1; j++)
                        {
                            solution[0] |= ((uint)1 << 31);
                            solution[1] |= ((uint)1 << 31);
                        }
                    }
                }
            }

            return solutions;
        }

        static void writeEncode(string[] args)
        {
            Console.Write(string.Join(" ", args) + ": ");
            encode(args);
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
        private static string uintToHex(uint b)
        {
            return b.ToString("x8");
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
