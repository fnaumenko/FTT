using System;
using System.Collections;
using System.ComponentModel;

namespace FTT
{
    #region Pattern structure

//    /// <summary>Represents a pattern: seed word and his frequency.</summary>
//    public struct Pattern : IComparable
//    {
//        /// <summary>The serial number of pattern's seed word in the total words sequence.</summary>
//        public int WordNumber;
//        /// <summary>The frequency of the pattern with mismatches.</summary>
//        public int Freq;

//        /// <summary>Compares by default the current instance with another Pattern by Frequency;
//        /// descending order.</summary>
//        /// <param name="obj">The compared Pattern</param>
//        /// <returns>1 if obj Frequency is more; -1 if obj Frequency is less; 0 if both are equal.</returns>
//        int IComparable.CompareTo(object obj)
//        {
//            return Math.Sign(((Pattern)obj).Freq - Freq);
//        }

//        /// <summary>Outputs the seed word and Frequency of Pattern.</summary>
//        /// <returns></returns>
//        public override string ToString()
//        {
//            return Words.WordToString(WordNumber, false) + Freq.ToString("\t0");
//        }

//        #region Old Version
//#if VERSION2
 
//        /// <summary>Creates the new Pattern structure instance.</summary>
//        /// <param name="wordNumber">The number of pattern's word in the total words sequence.</param>
//        /// <param name="freq">The frequency of the pattern.</param>
//        /// <remarks>Full constructor is required for reading from file only.</remarks>
//        public Pattern(int wordNumber, int freq)
//        {
//            WordNumber = wordNumber;
//            Freq = freq;
//        }

//        /// <summary>Gets a value that indicates whether patterns are equal allow for mismatches.</summary>
//        /// <param name="names">Common comparison 'names' sequence.</param>
//        /// <param name="ind1">The index of the first comparison in the 'names' sequence.</param>
//        /// <param name="ind2">The index of the second comparison in the 'names' sequence.</param>
//        /// <param name="mismCnt">Count of mismatches.</param>
//        /// <returns>true if specified pattern </returns>
//        public static bool IsEqual(byte[] names, int ind1, int ind2, byte mismCnt)
//        {
//            ////working, but not faster. And more unclear.
//            ////in this variant parameter 'mismCnt' should be int!
//            //for (byte i = 0; i < Names.NameLength; i++)
//            //{
//            //    mismCnt -= 1 >> (names[ind1 + i] & names[ind2 + i]);
//            //    if (mismCnt < 0)
//            //        return false;
//            //}

//            for (byte i = 0; i < Names.NameLength; i++)           // compare each byte in pattern
//                if ((names[ind1 + i] & names[ind2 + i]) == 0    // a bit more faster then 'seq1[ind1+i] != seq2[ind2+i]'
//                && mismCnt-- == 0 )                             // byte is unsigned so it should be compared with '0' BEFORE descent since minus value is impossible
//                    return false;
//            return true;
//        }

//        /// <summary>Compares two patterns from different collections by names.</summary>
//        /// <param name="name1">First 'names' sequance.</param>
//        /// <param name="ind1">The array index of the first Pattern.</param>
//        /// <param name="name2">Second 'names' sequance.</param>
//        /// <param name="ind2">The array index of the second Pattern.</param>
//        /// <returns>-1 if first patern's name is less then second; 1 if first is more then second; 0 if they are equal.
//        /// This order provides sorting by increase.</returns>
//        public static int CompareByName(byte[] name1, int ind1, byte[] name2, int ind2)
//        {
//            int diffr;
//            for (byte i = 0; i < Names.NameLength; i++)           // compare each byte in name
//            {
//                diffr = name1[ind1 + i] - name2[ind2 + i];
//                if (diffr != 0) return Math.Sign(diffr);
//            }
//            return 0;
//        }
//#endif
//        #endregion

//    }   // end of Pattern structure
    #endregion
    #region Patterns class
    /// <summary>The Collection of Patterns.</summary>
    /// <remarks>
    /// Common description of patterns and they collection:
    /// In terms of algorithm PATTERN is a "word" of specified length.
    /// In terms of realization PATTERN is an index in the 'names' sequence. 
    /// This way allows to avoid duplication all "words" intersection and thereby minimises memory demands.
    /// Structure Pattern is a triad 'index'-'strict frequency' - 'extended frequency' .
    /// Collection of Patterns keeps initial sequence and the array of Patterns.
    /// </remarks>
	public class Patterns : StructArray<int>
	{
        #region Common descripton
        /* 
         * There are 2 types of collection:
         * - created as an 'empty' ('empty' type)
         * - created by template ('template' type)
         * 
         * Nominally the count of 'Empty' type coollection is the count of elements with non-zero strict frequency.
         * Really it's an array of empty elements with possible maximum length.
         * Method 'Add' set element to the first 'zero' place and increses the counter.
         * A 'names holder' is an input sequance.
         * That allows to avoid names duplicate and decrease required memory.
         * This type created and used by 'From input only' word patterns mode,
         * and always as a holder of just the names of patterns, Motives f.e.
         * 
         * 'Template' collection is a collection of patterns with the all possible names (for given Words.WordLength)
         * and with 'zero' strict frequency initially.
         * A 'names holder' is a 'names' template - array contained all possible 'words' for given length.
         * The count of elements is invariable.
         * This type created and used by 'All possible' word patterns mode.
         * Initially "Template' collection is created and inizilised as a static.
         * Then each new collection is just copied by static "Template' collection.
         * That is made for fastest runtime.
        */
        #endregion
        #region Static members & methods

        /// <summary>Curretn treatment state: scanning.</summary>
        public const int TreatSCANNING = -1;
        /// <summary>Curretn treatment state: sorting.</summary>
        public const int TreatSORTING = -2;
        ///// <summary>Curretn treatment state: drawing.</summary>
        //public const int TreatDRAWING = -3;

        /// <summary>True if all possible words are processed. Defined by user.</summary>
        /// <remarks>Used on the second step of filling algorithm.</remarks>
        static bool IsAllPossible;

        /// <summary>Gets the amount of memory required for one element of collection.</summary>
        /// <returns>The maximum number of bytes of memory required for one element of collection.</returns>
        public static int OneSize()
        {
            //return System.Runtime.InteropServices.Marshal.SizeOf(new Pattern());
            return 3*sizeof(int);
        }
        #endregion

        int _realCount = 0;
        System.Drawing.PointF[] _pts = null;

        #region Initializator

        /// <summary>Initialises static member and creates names template.</summary>
        /// <param name="isAllPossible">True if all possible words are processed.</param>
        public static void Init(bool isAllPossible)
        {
            IsAllPossible = isAllPossible;
        }

        #endregion
        #region Constructor and main algorithm

        /// <summary>Constructor of Patterns.</summary>
        /// <param name="sortMode">Mode of sorting. Reserved; not used in this realise.</param>
        Patterns() : base(Words.Count)
        {
            _realCount = Words.Count;
        }

        /// <summary>Initializes an instance of the Patterns.</summary>
        /// <param name="worker">The BackgroundWorker in wich Processing is run.</param>
        /// <param name="sequence">The 'names' sequence.</param>
        /// <param name="startIndex">The index in the inputArray at which scanning begins.</param>
        /// <param name="strictLength">The precise length of range of inputArray for scanning.</param>
        /// <param name="strictFreqs">The external array of strict frequencies; needed to avoid memory overallotment.</param>
        /// <param name="loopsPerStep">The amount of loops per one progress bar step; -1 if without progress bar steps.</param>
        /// <param name="maxFreqNumber">Return number of Pattern with max Frequence: if init value = -1, then no return.</param>
        /// <param name="isFirstCall">True if method is called first time in cycle; otherwise, false.</param>
        /// <param name="isDrawPlot">True if plot is drawing; otherwise, false.</param>
        /// <returns>Maximum frequency value in the instance; if init maxFreqNumber param = -1, then 0.</returns>
        int Init(
            BackgroundWorker worker, byte[] sequence,
            int startIndex, int strictLength, int[] strictFreqs, int loopsPerStep, ref int maxFreqNumber,
            bool isFirstCall, bool isDrawPlot)
        {
            #region Comments
            /* 
             * Patterns are generated from inputArray sequence on 2 steps.
             * 
             * Step 1: Initialization strict frequency
             * We create a new collection with the maximum possible length and an temporary array of strict frequencies.
             * Index of each element for both collections is equal to absolute word number,
             * so we scan through the input sequence and increase strict frequency in array directly by indexing.
             * 
             * Step 2: Initialization Frequency
             * From each pattern in collection we added the Frequency of all his Similars.
             * In ALL POSSIBLE mode we search all the patterns, because their Similars may have Frequency > 0;
             * in FROM INPUT mode we skip zero-frequency patterns.
             * 
             * In TURBO mode all the Similars are keeping in memory after first generation,
             * so we should go through the given Similars collection and adding thier Frequency directly by word number, equls to index in this instance.
             * In ORDINARY mode we receive the sum of Frequencies of all Similars through single call of Similars.SumUp().
             * 
             * So we receive the frequencies with mismatches for all the Patterns in this instance.
             * 
             * This is the maximum fastest method because of using only direct indexing.
             * We scan input sequence just once? and no searches through collection are required.
             */
            #endregion

            //System.Windows.Forms.MessageBox.Show(loopsPerStep.ToString());

            int i, maxFreq = 0;

            worker.ReportProgress(TreatSCANNING);
            OnPrgBarReseat();

            this.Clear();
            Array.Clear(strictFreqs, 0, strictFreqs.Length);

            //System.Windows.Forms.MessageBox.Show(startIndex.ToString("startIndex= 0  ") + strictLength.ToString("strictLength= 0  "));
            //*** Step_1: Look over all the patterns in a range of sequence
            for (i = 0; i <= strictLength; i++)
                strictFreqs[Words.GetNumber(sequence, startIndex + i)]++;

            //*** Step_2: expanding to mismatches
            int k;
            Similars smls;
            //Pattern ptn;
            // compare each Pattern to all the Pattern's in the same collection with a glance of mismathes;
            // collection is still unsorted
            for (i = 0; i < Count; i++)
            {
                if (worker.CancellationPending)     // Canceled by user.
                    throw new ApplicationException(string.Empty);

                if (IsAllPossible || (strictFreqs[i] > 0))
                {
                    //ptn = this[i];
                    if (Similars.IsTurbo)
                    {
                        smls = Similars.GetSimilars(i);
                        for (k = 0; k < smls.Count; k++)
                            //ptn.Freq += strictFreqs[smls[k]];
                            this[i] += strictFreqs[smls[k]];
                    }
                    else
                        //ptn.Freq += Similars.SumUp(i, strictFreqs);
                        this[i] += Similars.SumUp(i, strictFreqs);

                    //ptn.WordNumber = i;             // initialize number
                    //ptn.Freq += strictFreqs[i];     // frequency itself
                    this[i] += strictFreqs[i];     // frequency itself
                    //this[i] = ptn;
                    // keep index of maximum Freq without sorting: needs only in case of Max Average Choice or Global task.
                    //if (maxFreqNumber >= 0 && ptn.Freq > maxFreq)
                    if (maxFreqNumber >= 0 && this[i] > maxFreq)
                    {
                        //maxFreq = ptn.Freq;
                        maxFreq = this[i];
                        maxFreqNumber = i;
                    }
                }
                OnPrgBarIncreased(worker, loopsPerStep);
            }
            if (isDrawPlot)
            {
                // if it needs to keep sorting by name, we should clone collection for drawing,
                // because it will be sorted by Freq during drawing
                worker.ReportProgress(TreatSORTING);
                worker.ReportProgress(Convert.ToInt32(isFirstCall), Clone());
            }
            return maxFreq;
        }

        /// <summary>Creates and filled a shrinked or 'crude' instance of Patterns class from the range of the inputArray array 
        /// and calculate their averages and standart deviation values.
        /// </summary>
        /// <param name="worker">The BackgroundWorker in wich Processing is run.</param>
        /// <param name="startIndex">The index in the input sequence at which scanning begins.</param>
        /// <param name="length">The length of scanning range.</param>
        /// <param name="shakeCnt">The amount of shakes.</param>
        /// <param name="avrgs">Null in case of Individual mode; the array of single Average otherwise.</param>
        /// <param name="maxFreqNumber">The number of Pattern with max Frequence: used in 'Global' only.</param>
        /// <param name="stepCntOnShake">The number of progress bar steps generated by method within one shake; 0 if without progress bar steps.</param>
        /// <param name="isDrawPlot">True if a plot should be drawing; otherwise, false.</param>
        /// <returns>Sorted by Frequence and shrinked collection in case of isSortByFreq=true; otherwise sorted by Number and unshrinked.</returns>
        public static Patterns GetTreatedPatterns (
            BackgroundWorker worker, 
            int startIndex, int length, short shakeCnt,
            ref Average[] avrgs, ref int maxFreqNumber, float stepCntOnShake,
            bool isDrawPlot)
        {
            int strictLength = length - Words.WordLength;
            int[] strictFreqs = new int[Words.Count];
            int loopsPerStep = stepCntOnShake > 0 ? (int)(Words.Count / stepCntOnShake) : -1;

            Patterns mainPtns = new Patterns();
            mainPtns.Init(
                worker, Sequence.Original, startIndex, 
                strictLength, strictFreqs, loopsPerStep, ref maxFreqNumber, true, isDrawPlot
                );
            if (shakeCnt > 0)		// calculate mean & SD
            {
                int i;
                int maxFreqNumberTmp = 0;
                Patterns shakedPtns = new Patterns();
                // corretcing ProgressBar steps for for shake and scanning
                int shakeSteps = 0;
                //if (loopsPerStep > 3)       shakeSteps = 3;
                //else if (loopsPerStep > 2)  shakeSteps = 2;
                //else if (loopsPerStep > 1)  shakeSteps = 1;
                loopsPerStep -= shakeSteps;

                if (avrgs != null)  //*** Max Average Choice mode set
                {
                    Average.SumCounter sumCounter = new Average.SumCounter();
                    // search patterns with max Frequance for each shake and add this value to the sumCounter
                    for (i = 0; i < shakeCnt; i++)
                        // we need only max Frequance from the shaked Patterns
                        sumCounter += shakedPtns.Init(worker,
                            Sequence.Shake(startIndex, length, worker, shakeSteps),
                            0, strictLength, strictFreqs, loopsPerStep, ref maxFreqNumberTmp, false, isDrawPlot
                            );
                    avrgs[0] = new Average(sumCounter); // set total Average on the first place;
                }
                else                //*** Individual Average Choice mode set
                {
                    // this case is possible only in Local task, so mainPtns is always sorted and shrinked
                    // create and fill the array of sum counters of frequencies from shaked Patterns;
                    // it is 'sinchronised' with shakedPtns, or the same order as a shakedPtns
                    Average.SumCounter[] SCounters = new Average.SumCounter[Words.Count];
                    maxFreqNumberTmp = -1;      // disable max frequency return
                    for (int j = 0; j < shakeCnt; j++)
                    {
                        shakedPtns.Init( worker,
                            Sequence.Shake(startIndex, length, worker, shakeSteps),
                            0, strictLength, strictFreqs, loopsPerStep, ref maxFreqNumberTmp, false, isDrawPlot
                            );
                        for (i = 0; i < Words.Count; i++)
                            //if (shakedPtns[i].Freq > 0)
                            //    SCounters[i] += shakedPtns[i].Freq;
                            if (shakedPtns[i] > 0)
                                SCounters[i] += shakedPtns[i];
                    }
                    // set the average of frequency and SD for each Pattern in the initial collection
                    avrgs = new Average[mainPtns.Count];
                    for (i = 0; i < mainPtns.Count; i++)   // each initial pattern with unzero Frequency...
                        avrgs[i] = new Average(SCounters[i]);
                }
            }
            return mainPtns;
        }
        #endregion
        #region Plot methods

        /// <summary>
        /// Gets the array of floating-points represented collection of points on the painting surface.
        /// </summary>
        public System.Drawing.PointF[] Points
        {
            get
            {
                SetPoints();
                return _pts;
            }
        }

        /// <summary>Sets Points collection and _maxYValue.</summary>
        void SetPoints ()
        {
            if (_pts != null) return;

            Sort();
            int  i, amount, ind = 0, currVal, prevVal = this[0];
            // get precise size of point array
            for (i = amount = 0; i < Count - 1; i++)
                if (this[i] != this[i + 1])
                    amount++;
            _pts = new System.Drawing.PointF[amount+1];

            for (i = amount = 1; i < Count; amount++, i++)
            {
                currVal = this[i];
                if (currVal == 0)       // cut patterns with zero frequency
                    break;
                if (currVal != prevVal)
                {
                    SetPoint(ind++, prevVal, amount);
                    amount = 0;
                    prevVal = currVal;
                }
            }
            SetPoint(ind++, prevVal, amount);   // finally the last (first in plot) Point
        }

        /// <summary>Set Point and _maxYValue.</summary>
        /// <param name="ptIndex">Point index in _pts</param>
        /// <param name="X">X-value.</param>
        /// <param name="Y">Y-value.</param>
        void SetPoint(int ptIndex, int X, int Y)
        {
            _pts[ptIndex].X = X;
            _pts[ptIndex].Y = Y;
        }

        #endregion
        #region Management of collection

        /// <summary>Gets the number of inserted patterns.</summary>
        public new int Count
        {
            get { return _realCount; }
        }

        public new void Sort()
        {
            //ArraySorter<int>.SortDescending(_coll);
            base.SortReverse();
        }

        /// <summary>Creates a sorted by Frequency (default), 'shrinked' copy of the collection.</summary>
        /// <returns>A sorted copy of the collection</returns>
        Patterns Clone()
        {
            Patterns res = new Patterns();
            CloneTo(res, Count);
            res.Sort();
            // 'shrink' collection: set real count without resizing
            // start from the end because zero Frequencies are much more less then unzero
            for (int i = Count - 1; i >= 0; i--)
                if (res[i] > 0)
                {
                    //Array.Resize(ref _coll, _realCount = i + 1);
                    res._realCount = i + 1;
                    break;
                }
            return res;

        }

        ///// <summary>Removes all the elements with the zero Frequency from the instance.</summary>
        ///// <remarks>Sorts the collection by Frequency.</remarks>
        //void Shrink ()
        //{
        //    if (_isShrinked == true)
        //        return;
        //    Sort();
        //    //System.Console.Beep(400, 100);
        //    // start from the end because zero Frequencies are much more less then unzero
        //    for (int i = Count-1; i >= 0; i--)
        //        //if (this[i].Freq > 0)
        //        if (this[i] > 0)
        //        {
        //            Array.Resize(ref _coll, _insCount = i+1);
        //            break;
        //        }
        //    _isShrinked = true;
        //}

        #endregion
        #region Statistic methods

        /// <summary>Reports the shrinked (without zero Frequency) Similars of the specified unsorted pattern.</summary>
        /// <param name="seedNumber">The number of the seed-word in the collection.</param>
        /// <returns>If Similars have zero-frequency items, the shrinked (without zero Frequency) Similars; otherwise initial Similars.</returns>
        public Similars GetSimilars(int seedNumber)
        {
            Similars smls = Similars.GetSimilars(seedNumber);
            bool isNotCopied = true;

            // We have to remove from the total Similars all the numbers with zero Frequency
            // this is a variant used in ScanWindows
            // we may use direct indexing to check zero-frequency seeds
            for (int i = 0; i < smls.Count; i++)
                if (this[smls[i]] == 0)
                {
                    // if zero-frequence element is finded, Similars should be copied
                    // because it change its Count by shrinking
                    if (isNotCopied)
                    {
                        smls = smls.Copy();
                        isNotCopied = false;
                    }
                    smls[i] = Similars.UndefNumber;
                }
            return isNotCopied ? smls : smls.Shrink();
        }

        #endregion
        #region Output
        /// <summary>Outputs the name and extended frequency of an indexed pattern.</summary>
        /// <param name="index">The index of output pattern.</param>
        /// <returns>the name and extended frequency of an indexed pattern.</returns>
        public string ElemToString(int index)
        {
            return this[index].ToString();
        }

        #endregion
        #region DEBUG
#if DEBUG
        public void ShowSequance()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(string.Format("Count={0}  total={1}  ", Count, base.Count));
            for (int i = 0; i < Count; i++)
            {
                sb.Append(i.ToString("0: "));
                sb.Append(Words.WordToString(i, false) + "  ");
            }

            System.Windows.Forms.MessageBox.Show(
                String.Format("{0}", sb.ToString()), "",
                System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
        }

        static string GetName(byte[] name)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < name.Length; i++)
                sb.Append(Chars.GetChar(name[i]));
            return sb.ToString();
        }
#endif
        #endregion

    }   // end of class Patterns
    #endregion
}
