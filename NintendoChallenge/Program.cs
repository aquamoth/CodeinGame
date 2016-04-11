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


            Console.Error.WriteLine("Testing low bits");
            for (int i = 0; i < 32; i++)
            {
                testDecoder(0U, 1U << i);
            }
            Console.Error.WriteLine("Testing high bits");
            for (int i = 32; i < 64; i++)
            {
                testDecoder(1U << i, 0U);
            }

            //writeDecode("00000009", "00000000");
            //writeDecode("0000ae47", "00000000");

            //writeDecode("00000020", "00000049");
            Console.ReadLine();
        }

        private static void testDecoder(uint lsb, uint msb)
        {
            int passedCounter = 0, failedCounter = 0;
            foreach (var decodedValue in centurianDecoder(lsb, msb))
            {
                var result = centaurianOperation(32, ulong2uints(decodedValue));
                if (result[0] != lsb || result[1] != msb)
                {
                    Console.Error.WriteLine("Expected {0} {1} decoded to {2}, which reencoded to {3} {4}", hexOf(lsb), hexOf(msb), hexOf(decodedValue), hexOf(result[0]), hexOf(result[1]));
                    failedCounter++;
                }
                else
                    passedCounter++;
            }
            if(failedCounter>0)
            {
                Console.Error.WriteLine("{0} tests passed and {1} failed", passedCounter, failedCounter);
            }
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
            var solutions = new List<ulong>(new[] { 0UL });
            var encodedValue = uints2ulong(lsb, msb);

            solutions = centurianDecoder_lowbits(solutions, encodedValue);
            solutions = centurianDecoder_highbits(solutions, encodedValue);

            return solutions;
        }

        private static List<ulong> centurianDecoder_lowbits(List<ulong> solutions, ulong encodedValue)
        {
            //For each bit
            for (byte bit = 0; bit < 32; bit++)
            {
                var mask = ((ulong)1 << bit);
                var expectedBitValue = encodedValue & mask;
                if (expectedBitValue != 0)
                {
                    var newSolutions = new List<ulong>();
                    foreach (var solution in solutions)
                    {
                        var lowestSetBit = lowestSetBitOf(solution, bit);

                        //Find all ways to manipulate the solution to solve for this bit
                        for (int lowBitToSet = 0; lowBitToSet <= bit; lowBitToSet++)
                        {
                            var highBitToSet = bit - lowBitToSet;
                            if (lowBitToSet > highBitToSet)
                                break;

                            var lowBitValue = 1UL << lowBitToSet;
                            var highBitValue = 0x1UL << (32 + (highBitToSet % 32));

                            if ((lowestSetBit + lowBitToSet < bit) && (solution & lowBitValue) == 0)
                                continue;

                            newSolutions.Add(solution | highBitValue | lowBitValue);
                        }
                    }
                    solutions = newSolutions;
                }
            }

            return solutions;
        }


        private static List<ulong> centurianDecoder_highbits(List<ulong> solutions, ulong encodedValue)
        {
            //For each bit
            for (byte bit = 32; bit < 64; bit++)
            {
                var mask = ((ulong)1 << bit);
                var expectedBitValue = encodedValue & mask;
                if (expectedBitValue != 0)
                {
                    var newSolutions = new List<ulong>();
                    foreach (var solution in solutions)
                    {
                        var lowestSetBit = lowestSetBitOf(solution, bit);

                        //Find all ways to manipulate the solution to solve for this bit
                        for (int lowBitToSet = 0; lowBitToSet < 32; lowBitToSet++)
                        {
                            var highBitToSet = bit - lowBitToSet;
                            if (highBitToSet >= 32)
                                continue;

                            if (lowBitToSet > highBitToSet)
                                break;

                            var lowBitValue = 1UL << lowBitToSet;
                            var highBitValue = 0x1UL << (32 + highBitToSet);

                            if ((lowestSetBit + lowBitToSet < bit) && (solution & lowBitValue) == 0)
                                continue;


                            //if (newSolutions.Count==15)
                            //{
                            //    var lowHex = hexOf(lowBitValue);
                            //    var highHex = hexOf(highBitValue);
                            //    var x = 0;
                            //}

                            newSolutions.Add(solution | highBitValue | lowBitValue);
                        }
                    }
                    solutions = newSolutions;
                }
            }

            return solutions;
        }

        private static byte lowestSetBitOf(ulong solution, byte highestBitToTest)
        {
            var bitValue = 1UL;
            for(byte i = 0; i < highestBitToTest; i++)
            {
                if ((solution & bitValue) != 0)
                    return i;
                bitValue = bitValue << 1;
            }
            return highestBitToTest;
        }

        static ulong uints2ulong(uint lsb, uint msb)
        {
            return (ulong)lsb | (((ulong)msb) << 32);
        }

        static uint[] ulong2uints(ulong value)
        {
            return new uint[] { (uint)(value & 0xffffffff), (uint)(value >> 32) };
        }

        static string hexOf(ulong value) { return string.Join(" ", uintToHex(ulong2uints(value))); }

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
        private static string hexOf(uint b)
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
