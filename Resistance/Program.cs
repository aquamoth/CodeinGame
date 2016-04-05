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
        log(morseMessage.Length.ToString());

        var dictionaryRoot = new TreeNode(0);
        //var dictionary = new List<string>();
        int N = int.Parse(Console.ReadLine());
        for (int i = 0; i < N; i++)
        {
            string W = Console.ReadLine();
            //dictionary.Add(W);
            dictionaryRoot.Add(W);
            //log(W);
        }
        log("Dictionary of {0} words", N);
        //var dictionaryRoot = treeOf(dictionary);


        var stack = new Stack<ParseState>();
        stack.Push(new ParseState(0, dictionaryRoot, ""));

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
                if (!state.DictionaryPosition.IsRoot && state.PartialMorse == "")
                {
                    //log("Found successful deciper");
                    successfullMessagesCounter++;
                }
            }
            else
            {
                var nextMorseToken = morseMessage[state.Index];
                var morseLetter = state.PartialMorse + nextMorseToken;
                if (MorseAlphabeth.Contains(morseLetter))
                {
                    //Test composit morse token as a morse letter
                    foreach (var newState in testFor(morseLetter, state, dictionaryRoot))
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

    //private static TreeNode treeOf(IEnumerable<string> dictionary)
    //{
    //    var tree = new TreeNode(0);
    //    foreach(var word in dictionary)
    //    {
    //        var walker = tree;
    //        for (var i = 0; i < word.Length; i++)
    //        {
    //            var letter = word[i];

    //            if (!walker.Children.ContainsKey(letter))
    //                walker.Children.Add(letter, new TreeNode(walker.Level) { Letter = letter });
    //            walker = walker.Children[letter];

    //            if (i == word.Length - 1)
    //                walker.IsWord = true;
    //        }
    //    }
    //    return tree;
    //}


    private static IEnumerable<ParseState> testFor(string morseLetter, ParseState state, TreeNode dictionaryRoot)
    {
        var letterIndex = MorseAlphabeth.IndexOf(morseLetter);
        //if (letterIndex == -1)
        //    throw new ApplicationException();

        //Test even longer instances of the composit morse token
        yield return new ParseState(state.Index + 1, state.DictionaryPosition, morseLetter);

        var letter = ASCIIEncoding.Default.GetString(new[] { (byte)(65 + letterIndex) })[0];
        //log("Testing morse letter: {0} = {1}", morseLetter, letter);

        //Does any words in the dictionary start with the part we are testing?
        //var word = state.PartialWord + letter;
        if (state.DictionaryPosition.Children.ContainsKey(letter))
        {
            var subTree = state.DictionaryPosition.Children[letter];

            //Keep decoding with the found word
            yield return new ParseState(state.Index + 1, subTree, "");

            //If we found an exact word, we also try decoding the rest as a new word
            if (subTree.IsWord)
            {
                //log("Word!");
                yield return new ParseState(state.Index + 1, dictionaryRoot, "");
            }
        }
    }




    class TreeNode
    {
        public char? Letter { get; set; }
        public bool IsWord { get; set; }
        public int Level { get; private set; }
        public IDictionary<char, TreeNode> Children { get; set; }
        public bool IsRoot { get { return !Letter.HasValue; } }

        public void Add(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                this.IsWord = true;
            }
            else
            {
                if (!Children.ContainsKey(word[0]))
                    Children.Add(word[0], new TreeNode(this.Level + 1) { Letter = word[0] });
                Children[word[0]].Add(word.Substring(1));
            }
        }

        public TreeNode(int level)
        {
            Level = level;
            Children = new Dictionary<char, TreeNode>();
        }
    }

    class ParseState
    {
        public int Index { get; private set; }
        public TreeNode DictionaryPosition { get; private set; }
        public string PartialMorse { get; private set; }

        public override string ToString()
        {
            return string.Format("Index={0}, Word={1}, Morse={2}", Index, DictionaryPosition.Letter, PartialMorse);
        }

        public ParseState(int index, TreeNode tree, string morse)
        {
            Index = index;
            PartialMorse = morse;
            DictionaryPosition = tree;
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

    static TreeNode Morse
    {
        get
        {
            if (_morse == null)
            {
                log("Calculating Morse Tree");
                _morse = new TreeNode(0);

                foreach (var letter in MorseAlphabeth)
                {
                    _morse.Add(letter);
                }
            }
            return _morse;
        }
    }
    static TreeNode _morse = null;

    static List<string> MorseAlphabeth = new List<string>(new[] { ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--.." });
}
