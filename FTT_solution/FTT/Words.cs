using System;

namespace FTT
{
    #region Chars class
    /// <summary>Represents predefined letters.</summary>
    public static class Chars
    {
        static char[] template = { 'A', 'C', 'G', 'T' };

        /// <summary>The total number of used characters.</summary>
        public static byte Count = (byte)template.Length;

        /// <summary>The undefined index.</summary>
        public static byte Undef = byte.MaxValue;

        /// <summary>The total number of used characters.</summary>
        /// <summary>The index of lastcharacter. This property is read-only.</summary>
        /// <remarks>Is equal Count-1 value and defined for efficiency within loops only.</remarks>
        public static byte LastIndex = (byte)(template.Length - 1);

        /// <summary>Gets the character by its index.</summary>
        /// <param name="index">The index of the character.</param>
        /// <returns></returns>
        public static char GetChar(byte index)
        {
            return template[index];
        }

        /// <summary>Gets the index by character.</summary>
        /// <param name="letter">The character.</param>
        /// <returns></returns>
        public static byte GetIndex(char letter)
        {
            for (byte k = 0; k < Count; k++)
                if (letter == template[k])
                    return k;
            return Undef;
        }

        #region Old Version
#if VERSION2
    /// <summary>
    /// Represents correspondence between inputArray characters and their codes used on inner data.
    /// </summary>
    static public class CharCode
    {
        /* 
         * Number are used instead characters for more efficiency.
         * Commented realisation is the simplest way. A code of character is the same as its index.
         * Unommented version used if we need to apply bitwise operation to character's codes.
         * A code and an index of the character are different in this way.
         * Codes: 'A'->1, 'C'->2, 'G'->4, 'T'->8
         */
        static char[] template = { 'A', 'C', 'G', 'T' };

        /// <summary>The total number of used characters.</summary>
        public static byte Count = (byte)template.Length;

        /// <summary>Gets the code (inner presentation)  of the character.</summary>
        /// <param name="index">The index of the character.</param>
        /// <returns></returns>
        public static byte GetCodeByIndex(byte index)
        {
            //return index;
            return (byte)(1 << index);
        }

        /// <summary>Gets the character by its index.</summary>
        /// <param name="index">The index of the character.</param>
        /// <returns></returns>
        public static char GetCharByIndex  (byte index)
        {
            return template[index];
        }

        /// <summary>Gets the index of character by its code.</summary>
        /// <param name="index">The code of the character.</param>
        /// <returns></returns>
        public static byte GetIndexByCode   (byte code)
        {
            //return code;
            byte index = 0;
            while ((code >> ++index) > 0);
            return --index;
        }

        /// <summary>Gets the character by its code.</summary>
        /// <param name="index">The code of the character.</param>
        /// <returns></returns>
        public static char GetCharByCode (byte code)
        {
            //return template[code];
            return template[GetIndexByCode(code)];
        }
    }
#endif
        #endregion

    }   // end of class Chars
    #endregion
    #region Similars class
    /// <summary>
    /// Represents a set of uniq similar words, which differ by some number of mismatches.
    /// </summary>
    public class Similars
    {
        #region Const and Static members
        /// <summary>Undefined word number.</summary>
        public const int UndefNumber = int.MaxValue;
        /// <summary>The factor wich multiplied by lengh of word defines Depth of Clustering.</summary>
        private const int DoC_FACTOR = 5;
        /// <summary>Depth of Clustering - the max difference between two word's indexes in the input sequense,
        /// inside wich these words belongs to one cluster.</summary>
        private static int _DepthOfClastering;
        /// <summary>The maximum possible numbers of similar words.</summary>
        private static long _Capacity;
        /// <summary>True if Turbo mode is defined.</summary>
        private static bool _IsTurbo;
        /// <summary>The number of mismatches.</summary>
        private static  byte _MismatchCount = 0;

        /// <summary>Static storage for keeping seed word during filling Similars.</summary>
        private static byte[] _Word;
        /// <summary>Static storage for keeping current Similars; refresh by the next filling.</summary>
        private static Similars _Similars;
        /// <summary>Static storage for keeping Similars for each word; used in Turbo mode only.</summary> 
        private static Similars[] _ArrSimilars;
        #endregion

        /// <summary>The array of numbers of similar words.</summary>
        private int[]   _numbers;

        /// <summary>Initialises a new, empty instance of the Similars class with given capacity.</summary>
        /// <param name="capacity">The total number of the Similars to create.</param>
        Similars(long capacity)
        {
            _numbers = new int[capacity];
        }

        /// <summary>Gets/sets the number of similar word by its index.</summary>
        /// <param name="index">The index of similar word.</param>
        /// <returns></returns>
        public int this[int index]
        {
            get { return _numbers[index]; }
            set { _numbers[index] = value; }
        }

        /// <summary>Gets the current number of words.</summary>
        public int Count
        {
            get { return _numbers.Length; }
        }

        /// <summary>True if Turbo mode is defined. This propery is read-only.</summary>
        public static bool IsTurbo
        {
            get { return _IsTurbo; }
        }
        
        /// <summary>Gets the maximum possible amount of memory required for proposed length of word and number of mismatches.</summary>
        /// <param name="wordLength">The proposed length of word.</param>
        /// <param name="mismatchCount">The proposed number of mismatches.</param>
        /// <returns>The number of bytes of memory required for all possible similars.</returns>
        public static int OneSize(byte wordLength, byte mismatchCount)
        {
            return GetCapacity(wordLength, mismatchCount) * sizeof(int);
        }

        /// <summary>Initializes static members.</summary>
        /// <param name="mismatchCount">The number of acceptable mismatches</param>
        /// <param name="isTurbo">True if Turbo mode is defined.</param>
        /// <param name="isDifferentWordLength">True if word's length is differ with previous session.</param>
        public static void Init (byte mismatchCount, bool isTurbo, bool isDifferentWordLength)
        {
            _Capacity = GetCapacity(Words.WordLength, mismatchCount);
            bool isDiffer = mismatchCount != _MismatchCount || isDifferentWordLength;
            if (_IsTurbo = isTurbo)
            {
                // save filling Similars in case of the same MismatchCount & WordLength
                _Similars = null;
                if (isDiffer || _ArrSimilars == null)
                    _ArrSimilars = new Similars[Words.Count];
            }
            else
            {
                _ArrSimilars = null;
                if (isDiffer || _Similars == null)
                    _Similars = new Similars(_Capacity);
            }
            _MismatchCount = mismatchCount;
            _DepthOfClastering = DoC_FACTOR * Words.WordLength;
        }

        /// <summary>Gets the estimated capacity for proposed length of seed and number of mismatches.</summary>
        /// <param name="wordLength">The proposed length of word.</param>
        /// <param name="mismatchCount">The proposed number of mismatches.</param>
        /// <returns>The total number of all possible similar words.</returns>
        static int GetCapacity(byte wordLength, byte mismatchCount)
        {
            if (mismatchCount == 0)                 return 0;
            if (mismatchCount == 1)                 return wordLength * Chars.LastIndex;
            if (mismatchCount == Words.WordLength)  return Words.Count - 1;

            int cpcCount = 0;
            GetCapacity_recurs(wordLength, 0, mismatchCount, ref cpcCount);
            return cpcCount;
    }

        /// <summary>Adds the total number of uniq similar words under given letter's position, starting with 0.</summary>
        /// <param name="wordLength">The length of word.</param>
        /// <param name="startPos">Started letter's position.</param>
        /// <param name="mismatchCnt">Number of mismathes.</param>
        /// <param name="cpcCount">Capacity counter.</param>
        static void GetCapacity_recurs(byte wordLength, byte startPos, byte mismatchCnt, ref int cpcCount)
        {
            // We can not use static 'Words.WordLength' instead of param 'wordLength' becaus of possibility to call GetCapacity() before Words.Init()
            byte j;

            for (byte i = startPos; i < wordLength; cpcCount += Chars.LastIndex, i++)
                for (j = 0; j < Chars.LastIndex; j++)
                    if (mismatchCnt > 1)
                        GetCapacity_recurs(wordLength, (byte)(i + 1), (byte)(mismatchCnt - 1), ref cpcCount);
        }

        /// <summary>Gets inizializeted similar words for given seed word.</summary>
        /// <param name="seedNumber">Serial number of seed word.</param>
        /// <returns>The total nubmer of filled similars.</returns>
        /// <remarks>In Turbo mode getting Similars are saved and returned from memory by requery.</remarks>
        public static Similars GetSimilars(int seedNumber)
        {
            if (_MismatchCount == 0)
                return null;
            _Word = Words.GetWord(seedNumber);
            int sum = 0;
            if (_IsTurbo)
            {
                Similars smls = _ArrSimilars[seedNumber];
                if (smls == null)
                {
                    // create and fill Similars 
                    _ArrSimilars[seedNumber] = smls = new Similars(_Capacity);
                    Fill_recurs(0, _MismatchCount, seedNumber, smls._numbers, ref sum, true);
                }
                return smls;
            }
            else
            {
                Fill_recurs(0, _MismatchCount, seedNumber, _Similars._numbers, ref sum, true);
                return _Similars;
            }
        }

        /// <summary>
        /// Gets sum total of the values of referenced array, indexed by the numbers of similar words for given seed word.
        /// </summary>
        /// <param name="seedNumber">Serial number of seed word.</param>
        /// <param name="summands">Array of integers should be sum, indexed by similar number.</param>
        /// <returns>The total sum.</returns>
        public static int SumUp(int seedNumber, int[] summands)
        {
            int sum = 0;

            if (_MismatchCount > 0)
            {
                _Word = Words.GetWord(seedNumber);
                Fill_recurs(0, _MismatchCount, seedNumber, summands, ref sum, false);
            }
            return sum;
        }

        /// <summary>Adds to referenced sum the values of referenced array, indexed by the numbers of similar words for given seed word,
        /// or adds to referenced array the numbers of similar words for given seed word</summary>
        /// <param name="startPos">Started letter's position in seed word at wich search begins</param>
        /// <param name="mismatchCnt">Number of mismathes.</param>
        /// <param name="number">Number of current similar word.</param>
        /// <param name="store">Array of similar words or of integers should be sum.</param>
        /// <param name="sum">The summator.</param>
        static void Fill_recurs(byte startPos, byte mismatchCnt, int number, int[] store, ref int sum, bool isFill)
        {
            byte i, j, originLetter;
            int currNumber, shiftBase;

            for (i = startPos; i < Words.WordLength; i++)
            {
                originLetter = _Word[i];                    // save original letter wich would be changed
                shiftBase = (Words.WordLastIndex - i) << 1; // save shift to avoid recalculation
                number -= originLetter << shiftBase;        // remove contribution of original letter wich would be changed
                for (j = 0; j < Chars.Count; j++)           // substitute other letters
                    if (j != originLetter)
                    {
                        currNumber = number + (j << shiftBase); // substitute other letter
                        if (isFill)                             // to fill Similars...
                            store[sum++] = currNumber;          // ...write words number to store
                        else                                    // or to sum...
                            sum += store[currNumber];           // ...add value from store
                        if (mismatchCnt > 1)                    // lock letter and pass down
                            Fill_recurs((byte)(i + 1), (byte)(mismatchCnt - 1), currNumber, store, ref sum, isFill);
                    }
                number += originLetter << shiftBase;        // restore contribution of original letter
            }
        }

        /// <summary>Gets a copy of this instance.</summary>
        /// <returns></returns>
        public Similars Copy()
        {
            Similars copySmls = new Similars(Count);
            Array.Copy(_numbers, copySmls._numbers, Count);
            return copySmls;
        }

        /// <summary>Removes all the elements with undefined number.</summary>
        /// <returns>The Similars without elements with undefined number</returns>
        public Similars Shrink()
        {
            Array.Sort(_numbers);
            for (int i = 0; i < Count; i++)
                if (this[i] == UndefNumber)
                {
                    Array.Resize(ref _numbers, i);
                    break;
                }
            return this;
        }

        ///// <summary>Gets a copy of sorted Similars without elements with undefined number.</summary>
        ///// <returns>The sorted Similars without elements with undefined number</returns>
        //public Similars GetShrink()
        //{
        //    int i;
        //    Array.Sort(_numbers);
        //    for (i = 0; i < Count; i++)
        //        if (this[i] == UndefNumber)
        //            break;
        //    Similars res = new Similars(i);
        //    Array.Copy(_numbers, res._numbers, i);
        //    return res;
        //}

        /// <summary>Returns the Coefficient of Variation.</summary>
        /// <param name="startIndex">The index in the input sequence at which scanning begins.</param>
        /// <param name="length">The length of scanning range.</param>
        /// <param name="seedNumber">The serial number of seed word.</param>
        /// <returns>Coefficient of Variation</returns>
        /// <remarks>This instance should be shrinked.</remarks>
        public float GetCV(int startIndex, int length, int seedNumber)
        {
            if (Count == 0) return 0;
            int i, j = 0, number, lastFound = -1;
            int stopInd = startIndex + length - Words.WordLength;
            // set the array of count of neighbouring word's (clusters) with max possible capacity
            int[] grps = new int[length];
            int k = 1;

            for (i = startIndex; i < stopInd; i++)
            {
                number = Words.GetNumber(Sequence.Original, i);
                // this collection is sorted by increasing!
                if (number != seedNumber
                && Array.BinarySearch(_numbers, number) < 0)    // word is not belong to similars
                    continue;
                if (lastFound >= 0)     // count began
                    if (i - lastFound <= _DepthOfClastering)    // founded word is in group
                        k++;            // increase group counter
                    else                // founded word is out of group
                    {
                        grps[j++] = k;  // close current group
                        k = 1;          // start new group
                    }
                lastFound = i;          // remember current position
            }

            // fill sum Counter
            Average.SumCounter counter = new Average.SumCounter();
            // check i<cnt needs when groups is full, in other words, when each "group" has just one member
            for (i = 0; i < Count && (k = grps[i]) > 0; i++)
                counter += k;

            return new Average(counter).CV;
        }

        /// <summary>Outputs all the similar words in a string with the 'end-of-line' separator.</summary>
        /// <returns></returns>
        public new string ToString()
        {
            System.Text.StringBuilder res = new System.Text.StringBuilder(Count * (Words.WordLength + 1));
            foreach (int n in _numbers)
                res.Append(Words.WordToString(n, true));
            //int i;
            //for (i=0; i<Count-1; i++)
            //    res.Append(Words.WordToString(this[i], true));
            //res.Append(Words.WordToString(this[i], false));   // do not add EOL
            return res.ToString();
        }

    #region Old Version
#if VERSION2
        /// <summary>
        /// Fills referenced array by the indexes of similar words of given seed word an returns number of indexes.
        /// </summary>
        /// <param name="words">Words sequence.</param>
        /// <param name="wrdIndex">The index of the seed word in the words sequence.</param>
        /// <param name="smlIndexes">Array of indexes of similar words.</param>
        /// <returns></returns>
        public static int Fill(byte[] words, int wrdIndex, ref Similars smlIndexes)
        {
            bool[] lockedLetters = new bool[Words.WordLength];    // array of signs indicated wich letter's position is locked
            int currSmlIndex = 0;

            if (_MismatchCount > 0)
                FillUnder(ref words, wrdIndex, ref lockedLetters, 0, _MismatchCount, ref smlIndexes, ref currSmlIndex);

            return smlIndexes._count = currSmlIndex;
        }

        /// <summary>Adds to referenced array the indexes of similar words of given seed word.</summary>
        /// <param name="words">Words sequence.</param>
        /// <param name="wrdIndex">The index of the seed word in the word's sequence.</param>
        /// <param name="lockedLetters">Array of signs indicated wich letter's position is locked on this step.</param>
        /// <param name="startPos">Started letter's position in seed word at wich search begins</param>
        /// <param name="mismatchCnt">Number of mismathes.</param>
        /// <param name="smlIndexes">Array of indexes of similar words.</param>
        /// <param name="currSmlIndex">Current writing position in array of indexes of similar words.</param>
        static void FillUnder(ref byte[] words, int wrdIndex, ref bool[] lockedLetters, int startPos, int mismatchCnt, ref Similars smlIndexes, ref int currSmlIndex)
        {
            byte j, originLetter, currLetter;
            int ind, i;

            mismatchCnt--;      // reduce count of mismatches because of processing current mismatch's level
            for (i = startPos; i < Words.WordLength; i++)
                if (!lockedLetters[i])   // pass locked letter
                {
                    ind = i + wrdIndex;
                    originLetter = words[ind];     // save original letter wich would be changed
                    // substitute other letters
                    for (j = 0; j < Chars.Count; j++)
                    {
                        //currLetter = Chars.GetCodeByIndex(j);
                        currLetter = j;
                        if (currLetter != originLetter)
                        {
                            words[ind] = currLetter;                                        // substitute other letter
                            smlIndexes[currSmlIndex++] = Words.GetNumber(words, wrdIndex);  // save words index
                            if (mismatchCnt > 0)                                            // lock letter and pass down
                            {
                                lockedLetters[i] = true;    // lock letter
                                FillUnder(ref words, wrdIndex, ref lockedLetters, i + 1, mismatchCnt, ref smlIndexes, ref currSmlIndex);
                                lockedLetters[i] = false;   // unlock letter
                            }
                        }
                    }
                    words[ind] = originLetter;     // restore original letter
                }
            mismatchCnt++;      // restore count of mismatches
        }

        /// <summary>
        /// Fills referenced array by the numbers of similar words for given seed word and returns number of similars.
        /// </summary>
        /// <param name="word">Seed word.</param>
        /// <param name="smlIndexes">External uninitialised array of numbers of similar words.</param>
        /// <returns>The total nubmer of filled similars.</returns>
        public static int Fill(byte[] word, ref Similars smlIndexes)
        {
            _word = word;
            _smlNumbers = smlIndexes;
            _currSmlIndex = 0;

            if (_MismatchCount > 0)
                FillUnder_recurs(0, _MismatchCount);

            return smlIndexes._count = _currSmlIndex;
        }

        /// <summary>Adds to referenced array the numbers of similar words for given seed word.</summary>
        /// <param name="startPos">Started letter's position in seed word at wich search begins</param>
        /// <param name="mismatchCnt">Number of mismathes.</param>
        static void FillUnder_recurs    (byte startPos, byte mismatchCnt)
        {
            byte j, originLetter;

            for (byte i = startPos; i < Words.WordLength; i++)
            {
                originLetter = _word[i];     // save original letter wich would be changed
                // substitute other letters
                for (j = 0; j < Chars.Count; j++)
                    if (j != originLetter)
                    {
                        _word[i] = j;                                           // substitute other letter
                        _smlNumbers[_currSmlIndex++] = Words.GetNumber(_word);    // save words index
                        if (mismatchCnt > 1)                                    // lock letter and pass down
                            FillUnder_recurs((byte)(i + 1), (byte)(mismatchCnt - 1));
                    }
                _word[i] = originLetter;     // restore original letter
            }
        }

        /// <summary>Removes all the elements with undefined number.</summary>
        /// <remarks>Sorts the collection by numbers.</remarks>
        public void Shrink()
        {
            Array.Sort(_numbers);
            for (int i = 0; i < _count; i++)
                if (this[i] == UndefNumber)
                {
                    Array.Resize(ref _numbers, _count = i);
                    break;
                }
        }

#endif
    #endregion
    }   // end of Similars class
    #endregion
    #region Words class
    /// <summary>Represents a virtual full predefined word table.</summary>
    class Words
    {
        static private byte _wordLength = 0;
        static private byte _wordLastIndex = 0;
        static private int _count = 0;

        /// <summary>Gets length of current word. This property is read-only.</summary>
        public static byte WordLength
        {
            get { return _wordLength; }
        }

        /// <summary>Gets last index of current word. This property is read-only.</summary>
        /// <remarks>This property returns WordLength-1 value and is defined only for efficiency within loops</remarks>
        public static byte WordLastIndex
        {
            get { return _wordLastIndex; }
        }

        /// <summary>Gets total number of words. This property is read-only.</summary>
        public static int Count
        {
            get { return _count; }
        }

        /// <summary>Gets the maximum total number of words for geven length of word.</summary>
        /// <param name="wordLength">The proposed length of word.</param>
        /// <returns></returns>
        public static int GetCount  (int wordLength)
        {
            return (int)Math.Pow(Chars.Count, wordLength);
        }

        /// <summary>Initialezas static member.</summary>
        /// <param name="wordLength">The length of word.</param>
        /// <param name="mismatchCount">The number of acceptable mismatches</param>
        /// <param name="isTurbo">True if Turbo mode is defined.</param>
        public static void Init(byte wordLength, byte mismatchCount, bool isTurbo)
        {
            bool isDifferentLength = _wordLength != wordLength;
            if (isDifferentLength)
            {
                _wordLength = wordLength;
                _wordLastIndex = (byte)(wordLength - 1);
                _count = GetCount(wordLength);
            }
            Similars.Init(mismatchCount, isTurbo, isDifferentLength);   // _wordLength & _count are used in that method, so it could be the last
        }

        /// <summary>Returns serial number of stated word started by index.</summary>
        /// <param name="extWords">The external words train.</param>
        /// <param name="wrdIndex">Index in the external words train.</param>
        /// <returns>Serial nummber of word.</returns>
        public static int GetNumber(byte[] extWords, int wrdIndex)
        {
            int res = 0;
            for (byte i = 0; i < _wordLength; i++)
                // that's a trick; instead calculate Chars.Count^X we use left shift, knowing that Chars.Count = 4:
                // 4^3 == 1<<6, 4^2 == 1<<4, 4^1 == 1<<2, 4^0 == 1<<0 );
                res += extWords[wrdIndex + i] << ((_wordLastIndex - i) << 1);
            //res += Chars.GetIndexByCode(words[nmsIndex + i]) * (int)Math.Pow(Chars.Count, length - i);
            return res;
        }

        /// <summary>Returns serial number of stated word.</summary>
        /// <param name="extWord">The external word.</param>
        /// <returns>Serial nummber of word.</returns>
        /// <remarks>Almost duplicates GetNumber(byte[], int) for faster runtime.</remarks>
        public static int GetNumber(byte[] extWord)
        {
            int res = 0;
            for (byte i = 0; i < _wordLength; i++)
                res += extWord[i] << ((_wordLastIndex - i) << 1);
            return res;
        }

        /// <summary>Returns word by number to stated destination.</summary>
        /// <param name="number">Serial nummber of word.</param>
        /// <param name="dest">Destination for keeping result.</param>
        public static void GetWord(int number, byte[] dest)
        {
            for (byte i = 0; i < _wordLength; i++)
                dest[i] = (byte)((number >> ((_wordLastIndex - i) << 1)) % Chars.Count);
            //dest[i] = Chars.GetCodeByIndex((byte)((number / (int)Math.Pow(4, length - i)) % Chars.Count));
        }

        /// <summary>Returns word by number.</summary>
        /// <param name="number">Serial nummber of word.</param>
        /// <returns>Word.</returns>
        public static byte[] GetWord(int number)
        {
            byte[] res = new byte[_wordLength];
            GetWord(number, res);
            return res;
        }

        /// <summary>Gets the string represented the word by its serial number.</summary>
        /// <param name="wordNumber">The number of word in the total words sequence.</param>
        /// <param name="isEOL">True if 'end-of-line' character is added to the end of string.</param>
        /// <returns>The string represented the word.</returns>
        public static string WordToString(int number, bool isEOL)
        {
            byte[] word = Words.GetWord(number);
            char[] res = new char[Words.WordLength + (isEOL ? 1 : 0)];
            for (byte i = 0; i < Words.WordLength; i++)
                res[i] = Chars.GetChar(word[i]);
            if (isEOL)
                res[Words.WordLength] = Abbr.EOL;
            return new string(res);
        }

#if DEBUG
        static void ShowTemplate(byte[] names)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < names.Length / Words.WordLength; i++)
                sb.Append(string.Format("{0}: {1}  ", i * Words.WordLength, Words.GetWord(i * Words.WordLength)));
            System.Windows.Forms.MessageBox.Show(
                String.Format("{0}", sb.ToString()), "",
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }
#endif

        #region Old Version
#if VERSION2
                /// <summary>Initialezas static member.</summary>
        /// <param name="wordLength">The length of word.</param>
        /// <param name="mismatchCount">The number of acceptable mismatches</param>
        public static void Init(byte wordLength, byte mismatchCount)
        {
            _wordLength = wordLength;
            _wordLastIndex = (byte)(wordLength - 1);
            _count = (int)Math.Pow(Chars.Count, wordLength);
            Similars.Init(mismatchCount);
            // inizialise words
            _wordsTrain = new byte[_count * wordLength];
            SetRank_recurs(0, 1);
        }

        /// <summary>Initialises the specified position in all the words
        /// which makes up the template array, by char codes.</summary>
        /// <param name="charRank">Current char's position in a word.</param>
        /// <param name="groupCount">The count of groups of words with the same char code in the specified position.</param>
        static void SetRank_recurs(byte charRank, int groupCount)
        {
            if (charRank == _wordLength)
                return;
            // set the length of groups of words with the same char in the specified position
            int c, l, groupLength = _count / (Chars.Count * groupCount);

            for (byte i = 0; i < Chars.Count; i++)
                for (c = 0; c < groupCount; c++)
                    for (l = 0; l < groupLength; l++)
                        _wordsTrain[charRank + (groupLength * (c * Chars.Count + i) + l) * _wordLength]
                            = Chars.GetCodeByIndex(i);
            SetRank_recurs((byte)(charRank + 1), groupCount * Chars.Count);
        }


#endif
        #endregion
    
    }   // end of Words class
    #endregion
}
