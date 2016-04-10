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

            //Console.WriteLine("WriteEncode");
            //writeEncode(new[] { "32", "80000000", "80000000" });
            //writeEncode(new[] { "32", "80000000", "40000000" });
            //writeEncode(new[] { "32", "40000000", "80000000" });
            //writeEncode(new[] { "32", "80000000", "c0000000" });
            //writeEncode(new[] { "32", "c0000000", "80000000" });


            //for (int i = 30; i >= 0; i--)
            //{
            //    writeDecode("00000000", uintToHex((uint)1 << i));
            //}
            //for (int i = 31; i >= 30; i--)
            //{
            //    writeDecode(uintToHex((uint)1 << i), "00000000");
            //}
            writeDecode("00000009", "00000000");

            Console.ReadLine();
        }

        static void writeDecode(params string[] args)
        {
            Console.WriteLine(string.Join(" ", args) + " => ");
            decode(args);
        }

        static void decode(params string[] args)
        {
            var u = hexToUInt(args);
            foreach (var decodedValue in centurianDecoder(u[0], u[1]))
            {
                var result = ulong2uints(decodedValue);
                Console.Write(string.Join(" ", uintToHex(result)) + " => ");
                encode(new[] { "32" }.Concat(uintToHex(result)).ToArray());
            }
        }

        static IEnumerable<ulong> centurianDecoder(uint lsb, uint msb)
        {
            var solutions = new List<uint[]>(new[] { new uint[] { 0, 0 } });

            //For each bit in msb
            //Bit 31 can never be set!
            for (int bit = 30; bit >= 0; bit--)
            {
                var mask = ((uint)1 << bit);
                var expectedBitValue = msb & mask;
                if (expectedBitValue != 0)
                {
                    var newSolutions = new List<uint[]>();
                    foreach (var solution in solutions)
                    {
                        //Find all ways to manipulate the solution to solve for this bit
                        newSolutions.AddRange(flipMsbBitIn(solution, bit));
                    }
                    solutions = newSolutions;
                }
            }


            //For each bit in lsb
            for (int bit = 31; bit >= 0; bit--)
            {
                var mask = ((uint)1 << bit);
                var expectedBitValue = lsb & mask;
                if (expectedBitValue != 0)
                {
                    var newSolutions = new List<uint[]>();
                    foreach (var solution in solutions)
                    {
                        //Find all ways to manipulate the solution to solve for this bit
                        for (int j = 0; j <= bit; j++)
                        {
                            var mask0 = (uint)1 << j;
                            var mask1 = (uint)1 << (bit - j);

                            //TODO: What side-effects would this incur? Find remedies for all before accepting this as an answer

                            var v0 = solution[0] | mask0;
                            var v1 = solution[1] | mask1;
                            newSolutions.Add(new[] { v0, v1 });
                        }
                    }
                    solutions = newSolutions;
                }
            }

            return solutions.Select(v => uints2ulong(v[0], v[1]));
        }

        static ulong uints2ulong(uint lsb, uint msb)
        {
            return (ulong)lsb | (((ulong)msb) << 32);
        }

        static uint[] ulong2uints(ulong value)
        {
            return new uint[] { (uint)(value & 0xffff), (uint)(value >> 32) };
        }


        private static IEnumerable<uint[]> flipMsbBitIn(uint[] solution, int msbBit)
        {
            for (int j = 0; j < (31 - msbBit); j++)
            {
                var mask0 = (uint)1 << (31 - j);
                var mask1 = (uint)1 << (msbBit + 1 + j);
                var v0 = solution[0] | mask0;
                var v1 = solution[1] | mask1;
                yield return new[] { v0, v1 };
            }
        }

        static void writeEncode(string[] args)
        {
            Console.Write(string.Join(" ", args) + " => ");
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
