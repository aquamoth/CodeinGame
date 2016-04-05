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
        log("Started");

        string morseMessage = Console.ReadLine();
        //log(morseMessage);

        var dictionaryRoot = new TreeNode<string>(0);
        int N = int.Parse(Console.ReadLine());
        //log(N.ToString());
        for (int i = 0; i < N; i++)
        {
            string W = Console.ReadLine();
            //log(W);
            dictionaryRoot.Add(W, W);
        }
        log("Dictionary of {0} words", N);


        var stack = new Stack<ParseState>();
        stack.Push(new ParseState(0, dictionaryRoot, Morse));

        var counter = 0;
        var lastTimer = 0L;
        var successfullMessagesCounter = 0;
        while (stack.Any())
        {
            var state = stack.Pop();
            counter++;

            if (logTimer.ElapsedMilliseconds > lastTimer + 100)
            {
                lastTimer = logTimer.ElapsedMilliseconds;
                log("#{1}: Stack = {0}", stack.Count, counter);
            }

            if (state.Index == morseMessage.Length)
            {
                //log("At end of morse message");

                //We reached the end of the morse message. 
                // If we don't look at a partial word at this case, we have successfully deciphered the message
                if (state.Dictionary.IsRoot && state.Morse.IsRoot)
                {
                    //log("Found successful deciper");
                    successfullMessagesCounter++;
                }
            }
            else
            {
                var nextMorseToken = morseMessage[state.Index];
                if (state.Morse.Children.ContainsKey(nextMorseToken))
                {
                    //Test composit morse token as a morse letter
                    foreach (var newState in testFor(nextMorseToken, state, dictionaryRoot))
                    {
                        //log("Saving state: {0}", newState);
                        stack.Push(newState);
                    }
                }
            }
        }

        Console.WriteLine(successfullMessagesCounter);
        Console.ReadLine();
    }
    
    private static IEnumerable<ParseState> testFor(char nextMorseCode, ParseState state, TreeNode<string> dictionaryRoot)
    {
        var nextMorse = state.Morse.Children[nextMorseCode];

        //Test even longer instances of the composit morse token
        yield return new ParseState(state.Index + 1, state.Dictionary, nextMorse);

        var letter = nextMorse.Value;
        //log("Testing morse letter: {0} = {1}", morseLetter, letter);

        //Does any words in the dictionary start with the part we are testing?
        //var word = state.PartialWord + letter;
        if (state.Dictionary.Children.ContainsKey(letter))
        {
            var subTree = state.Dictionary.Children[letter];

            //Keep decoding with the found word
            yield return new ParseState(state.Index + 1, subTree, Morse);

            //If we found an exact word, we also try decoding the rest as a new word
            if (subTree.Value != null)
            {
                //log("{0}", subTree.Value);
                yield return new ParseState(state.Index + 1, dictionaryRoot, Morse);
            }
        }
    }

    private static char LetterAt(int letterIndex)
    {
        return ASCIIEncoding.Default.GetString(new[] { (byte)(65 + letterIndex) })[0];
    }

    class TreeNode<T>
    {
        public T Value { get; set; }
        public int Level { get; private set; }
        public IDictionary<char, TreeNode<T>> Children { get; set; }
        public bool IsRoot { get { return Level == 0; } }

        public void Add(string word, T value)
        {
            if (word.Length == 0)
            {
                this.Value = value;
            }
            else
            {
                if (!Children.ContainsKey(word[0]))
                    Children.Add(word[0], new TreeNode<T>(this.Level + 1));
                Children[word[0]].Add(word.Substring(1), value);
            }
        }

        public TreeNode(int level)
        {
            Level = level;
            Children = new Dictionary<char, TreeNode<T>>();
        }
    }

    class ParseState
    {
        public int Index { get; private set; }
        public TreeNode<string> Dictionary { get; private set; }
        public TreeNode<char> Morse { get; private set; }

        public override string ToString()
        {
            return string.Format("Index={0}, Word={1}, Morse={2}", Index, Dictionary.Level, Morse.Level);
        }

        public ParseState(int index, TreeNode<string> dictionary, TreeNode<char> morse)
        {
            Index = index;
            Dictionary = dictionary;
            Morse = morse;
        }
    }

    static void log(string format, params object[] args)
    {
        if(logTimer == null)
        {
            logTimer = new System.Diagnostics.Stopwatch();
            logTimer.Start();
        }
        Console.Error.WriteLine(logTimer.ElapsedMilliseconds + " ms: " + format, args);
    }
    static System.Diagnostics.Stopwatch logTimer = null;

    static TreeNode<char> Morse
    {
        get
        {
            if (_morse == null)
            {
                log("Calculating Morse Tree");
                var morseCodes = new[] { ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--.." };
                _morse = new TreeNode<char>(0);
                for (var i = 0; i < morseCodes.Length; i++)
                {
                    _morse.Add(morseCodes[i], LetterAt(i));
                }
            }
            return _morse;
        }
    }
    static TreeNode<char> _morse = null;
}
