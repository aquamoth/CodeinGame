using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Solution
{

    static void Main(string[] args)
    {
        string morseMessage = Console.ReadLine();
        Console.Error.WriteLine(morseMessage);

        var dictionary = new HashSet<string>();
        int N = int.Parse(Console.ReadLine());
        for (int i = 0; i < N; i++)
        {
            string W = Console.ReadLine();
            dictionary.Add(W);
            //Console.Error.WriteLine(W);
        }

        //Console.Error.WriteLine("--------------------------------");

        var stack = new Stack<ParseState>();
        stack.Push(new ParseState(0, "", ""));

        var successfullMessagesCounter = 0;
        while (stack.Any())
        {
            var state = stack.Pop();
            //Console.Error.WriteLine("");
            //Console.Error.WriteLine(state);

            if (state.Index == morseMessage.Length)
            {
                Console.Error.WriteLine("At end of morse message");

                //We reached the end of the morse message. 
                // If we don't look at a partial word at this case, we have successfully deciphered the message
                if (state.PartialWord == "" && state.PartialMorse == "")
                {
                    Console.Error.WriteLine("Found successful deciper");
                    successfullMessagesCounter++;
                }
            }
            else
            {
                var nextMorseToken = morseMessage[state.Index];

                //if (state.PartialMorse != "")
                //{
                    var morseLetter = state.PartialMorse + nextMorseToken;
                    if (MorseAlphabeth.Contains(morseLetter))
                    {
                        //Test composit morse token as a morse letter
                        foreach (var newState in testFor(morseLetter, state, dictionary))
                        {
                            //Console.Error.WriteLine("Saving state: {0}", newState);
                            stack.Push(newState);
                        }
                    }
                //}

                ////Test with new token as start of a new morse letter
                //Console.Error.WriteLine("testFor() Loop 2");
                //foreach (var newState in testFor(nextMorseToken.ToString(), state, dictionary))
                //{
                //    Console.Error.WriteLine("Saving state: {0}", newState);
                //    stack.Push(newState);
                //}
            }
        }

        Console.WriteLine(successfullMessagesCounter);
        Console.ReadLine();
    }

    private static IEnumerable<ParseState> testFor(string morseLetter, ParseState state, HashSet<string> dictionary)
    {
        var letterIndex = MorseAlphabeth.IndexOf(morseLetter);
        //if (letterIndex == -1)
        //    throw new ApplicationException();

        //Test even longer instances of the composit morse token
        yield return new ParseState(state.Index + 1, state.PartialWord, morseLetter);

        var letter = ASCIIEncoding.Default.GetString(new[] { (byte)(65 + letterIndex) });
        //Console.Error.WriteLine("Testing morse letter: {0} = {1}", morseLetter, letter);

        //Does any words in the dictionary start with the part we are testing?
        var word = state.PartialWord + letter;
        if (dictionary.Any(x => x.StartsWith(word)))
        {
            //Keep decoding with the found word
            yield return new ParseState(state.Index + 1, word, "");

            //If we found an exact word, we also try decoding the rest as a new word
            if (dictionary.Contains(word))
            {
                Console.Error.WriteLine("Word: {0}", word);
                yield return new ParseState(state.Index + 1, "", "");
            }
        }
    }





    class ParseState
    {
        public int Index { get; private set; }
        public string PartialWord { get; private set; }
        public string PartialMorse { get; private set; }

        public override string ToString()
        {
            return string.Format("Index={0}, Word={1}, Morse={2}", Index, PartialWord, PartialMorse);
        }

        public ParseState(int index, string word, string morse)
        {
            Index = index;
            PartialMorse = morse;
            PartialWord = word;
        }
    }



    static List<string> MorseAlphabeth = new List<string>(new[] { ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--.." });
}
