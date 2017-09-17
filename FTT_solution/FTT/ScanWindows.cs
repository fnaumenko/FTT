using System;
using System.Windows.Forms;
using System.Collections;
using System.IO;

namespace FTT
{
    #region ScanWindow structure

    /// <summary>Represents the range of input sequance ('window'), maximum F and holding it pattern,
    /// and CV in the range.</summary>
    public struct ScanWindow : IComparable
    {
        /// <summary>A start window index.</summary>
        public readonly int StartIndex;
        /// <summary>The lenght of window.</summary>
        public readonly int Length;
        ///// <summary>The pattern with the max F in the window.</summary>
        //readonly Pattern Pattern;
        /// <summary>The pattern with the max F in the window.</summary>
        public readonly int WordNumber;
        /// <summary>The max Fluffing Factor in the window.</summary>
        public readonly float F;
        /// <summary>The CV in the window.</summary>
        public float CV;

        /// <summary>Creates the new ScanWindow structure instance.</summary>
        /// <param name="startIndex">A start window index.</param>
        /// <param name="length">The lenght of window.</param>
        /// <param name="pattern">The pattern with the max F in the window.</param>
        /// <param name="f">The max Fluffing Factor in the window.</param>
        /// <param name="cv">The CV in the window.</param>
        public ScanWindow(int startIndex, int length, int wordNumber, float f, float cv)
        //public ScanWindow(int startIndex, int length, Pattern pattern, float f, float cv)
        {
            StartIndex = startIndex;
            Length = length;
            WordNumber = wordNumber;
            //Pattern = pattern;
            F = f;
            CV = cv;
        }

        /// <summary>Compares by default the current instance with another ScanWindow by F.</summary>
        /// <param name="obj"></param>
        /// <returns>1 if obj amount is more, ;
        /// <br>-1 if obj amount is less;</br>
        /// <br>0 if both are equal.</br></returns>
        int IComparable.CompareTo(object obj)
        {
            return Math.Sign(((ScanWindow)obj).F - F);
        }

        /// <summary>Outputs the ScanWindow structure in a string.</summary>
        public override string ToString()
        {
            string formatF = F >= 100 ? "{2,-6:0.#}" :
                (F >= 0 && F < 10 ? "  {2,-4:0.#}" : " {2,-5:0.#}");

            return String.Format("{0,7} {1,4}  " + formatF + " {3,-5:0.##} {4}",
                StartIndex, Length, F, CV, Words.WordToString(WordNumber, false));
        }
    }
    #endregion
    #region ScanWindows class
    /// <summary>The Collection of ScanWindow.</summary>
    public class ScanWindows : StructArray<ScanWindow>
	{
        #region Sorting class
        class SortByInd : IComparer
        {
            int IComparer.Compare(object obj1, object obj2)
            {
                if (((ScanWindow)obj1).StartIndex > ((ScanWindow)obj2).StartIndex) return -1;
                if (((ScanWindow)obj1).StartIndex < ((ScanWindow)obj2).StartIndex) return 1;
                return 0;
            }
        }
        #endregion

        #region Constructor and main algorithm

        /// <summary>Constructor of ScanWindows.</summary>
        /// <param name="sequence">The input sequence.</param>
        /// <param name="count">The total count of the elements.</param>
        public ScanWindows(int count) : base(count) { }

        /// <summary>Constructor of ScanWindows.</summary>
        /// <param name="count">The total count of the elements.</param>
        /// <param name="sortMode">Current sorting mode.</param>
        ScanWindows(int count, byte sortMode) : base(count, sortMode) { }

        /// <summary>Creates and calculate an instance of ScanWindows class.</summary>
        /// <param name="worker">BackgroundWorker in wich Processing is run.</param>
        /// <param name="prgBar">ProgressBar to modify its Maximum.</param>
        /// <param name="shakeCnt">Number of shakes.</param>
        /// <param name="winStartLenght">Initial length of current scanned window.</param>
        /// <param name="winStopLenght">Final length of of current scanned window.</param>
        /// <param name="winIncr">Incriment of length of current scanned window.</param>
        /// <param name="winShift">Shift of current scanned window.</param>
        /// <param name="scanCnt">Nnumber of scanned window.</param>
        /// <returns>The sorted ScanWindow's collection.</returns>
        public static ScanWindows GetTreatedWindows(
            System.ComponentModel.BackgroundWorker worker, 
            ProgressBar prgBar,
            short shakeCnt,
            short winStartLenght, short winStopLenght, short winIncr, short winShift,
            int scanCnt )
        {
            int j, length, startInd = 0, maxFreqInd, maxFreqNumber = 0;
            int winCnt = (winStopLenght - winStartLenght) / winIncr + 1;    // the amount of windows from start to stop lengths
            int loopsPerStep = scanCnt * winCnt / prgBar.Maximum;// ProgressBar.DefaultMaximum;
            if (loopsPerStep < 10)   // modify Maximum in case of appreciable cumulative error
                prgBar.Maximum = scanCnt * winCnt / loopsPerStep;
            float F, maxF;
            Patterns ptns;      // threated patterns within inner loop
            ScanWindow sWin;    // scanWindow with max Frequency
            int[] maxFreqNumbers = new int[winCnt];         // array for keeping maxFreqNumber within inner loop
            Patterns[] ptnsArray = new Patterns[winCnt];    // array for keeping Patterns within inner loop
            Average[] avrgs = new Average[1];               // single Average from threated patterns
            ScanWindows sWins = new ScanWindows(winCnt);    // ScanWindows within inner loop
            ScanWindows sWinsRes = new ScanWindows(scanCnt);// result ScanWindows

            OnPrgBarReseat();
            for (int i = 0; i < scanCnt; i++)
            {
                length = winStartLenght;
                // choose patterns with max F within inner loop (among increasing subsequenses)
                for (maxF = 0, maxFreqInd = 0, j = 0; j < winCnt; length += winIncr, j++)
                {
                    ptnsArray[j] = ptns = Patterns.GetTreatedPatterns(
                        worker, startInd, length, shakeCnt, 
                        ref avrgs,
                        ref maxFreqNumber, 0, false);
                    // ptns is unsorted
                    maxFreqNumbers[j] = maxFreqNumber;
                    sWins[j] = new ScanWindow(
                        startInd, length, maxFreqNumber,
                        F = avrgs[0].F(ptns[maxFreqNumber]),
                        0);
                    // keep index of maxF element to avoid sorting by F at the end
                    if (F > maxF) { maxF = F; maxFreqInd = j; }
                    // increase progress bar
                    OnPrgBarIncreased(worker, loopsPerStep);
                }
                // calculate CV for sWin with max F
                maxFreqNumber = maxFreqNumbers[maxFreqInd];
                sWin = sWins[maxFreqInd];
                sWin.CV = ptnsArray[maxFreqInd].
                    GetSimilars(maxFreqNumber).
                    GetCV(sWin.StartIndex, sWin.Length, maxFreqNumber);

                // save sWin with max F
                sWinsRes[i] = sWin;
                startInd += winShift;

                // corrects startInd on the last step
                if (startInd + winStopLenght > Sequence.Length)
                    startInd = Sequence.Length - winStopLenght;
            }
            sWinsRes.Sort();
            return sWinsRes;
        }

        /// <summary>
        /// Gets the array of floating-points represented collection of points on the painting surface.
        /// </summary>
        public System.Drawing.PointF[] Points
        {
            get
            {
                int cnt = Count;
                System.Drawing.PointF[] pts = new System.Drawing.PointF[cnt];
                Sort(new ScanWindows.SortByInd());
                for (int i = 0; i < cnt; i++)
                {
                    pts[i].X = this[i].StartIndex;
                    pts[i].Y = this[i].F;
                }
                return pts;
            }
        }

        #endregion
        #region Input-output
        /// <summary>
        /// Converts the array of strings represented ScanWindows to its instance.
        /// </summary>
        /// <param name="inputs">Array of strings represented ScanWindows to convert.</param>
        /// <returns></returns>
        static ScanWindows Parse(ArrayList inputs)
        {
            int i = 0;
            string[] images;
            byte[] name;

            // read the name of first pattern and inizialise session by given word length
            string tmp = inputs[0] as string;
            Words.Init( (byte)tmp.Substring(tmp.LastIndexOf(Abbr.TAB) + 1).Length, 0, false);

            ScanWindows sWins = new ScanWindows(inputs.Count, BY_DEF);   // Default sort mode was set by writing to a file
            // initialises sWins member
            foreach (string str in inputs)
            {
                // each string contains 6 number separated by TAB
                images = str.Split(Abbr.TAB);
                name = Sequence.Parse(images[4]);
                sWins[i++] = new ScanWindow(
                    int.Parse(images[0]),
                    int.Parse(images[1]),
                    //new Pattern(Words.GetNumber(name) * Words.WordLength, 1),
                    Words.GetNumber(name),
                    float.Parse(images[2]),
                    float.Parse(images[3]));
            }

            return sWins;
        }

        /// <summary>Reads ScanWindows data saved in a file.</summary>
        /// <param name="fileName">The name of file</param>
        /// <param name="header">The string buffer to which data header is write.</param>
        /// <returns></returns>
        public static ScanWindows Read  (string fileName, ref string header)
        {
            ScanWindows sWins = null;
            using (StreamReader sr = File.OpenText(fileName))
            {
                // first line
                string tmp = sr.ReadLine();
                // pre check of format
                if (tmp.IndexOf(Abbr.FTT) < 0
                || tmp.IndexOf(Abbr.Global, Abbr.FTT.Length, 30) < 0)
                    throw new ApplicationException(Abbr.Incorrect + Abbr.SavedData);

                bool isHeader = true;
                header = string.Empty;
                ArrayList issue = new ArrayList();

                while ((tmp = sr.ReadLine()) != null)
                    if (isHeader)
                    {
                        if( tmp.Length > 0)                                             // if string is not empty
                            if (tmp[0] != Abbr.SEP_LINE && tmp[0] != Abbr.SEP_LINE0)    // check for separte line
                                header += tmp + Abbr.EOL;                               // grown header string
                            else
                                isHeader = false;
                    }
                    else
                        issue.Add(tmp);                     // grown issues (body) array

                sWins = ScanWindows.Parse(issue);
            }
            return sWins;
        }

        /// <summary>Gets the caption of outputs.</summary>
        public string Caption
        {
            get
            {
                return Abbr.AddSepLine(string.Format(" Start  Len {0,4} {1,5}   {2}",
                    Abbr.F, Abbr.CV, Abbr.Pattern));
            }
        }

        /// <summary>Outputs the collection in a string array.</summary>
        /// <param name="format">A composite float format string.</param>
        /// <returns></returns>
		public	string[]	ToStrings	()
		{
            string[] strs = new string[Count];
            int i = 0;
            foreach (ScanWindow sw in this)
                strs[i++] = sw.ToString();
			return strs;
		}

		/// <summary>Outputs the filtered collection in a string array.</summary>
        /// <param name="format">A composite float format string.</param>
        /// <param name="FMin">The minimum value of F filter.</param>
        /// <param name="FMax">The maximum value of F filter.</param>
        /// <param name="cvMin">The minimum value of CV filter.</param>
        /// <param name="cvMax">The maximum value of CV filter.</param>
        /// <returns></returns>
		public	string[]	ToStrings	(string format, float FMin, float FMax, float cvMin, float cvMax)
		{
			int i = 0, selectCnt = 0;
            string[] strs = new string[Count];
            foreach (ScanWindow sw in this)
            {
                if (sw.F > FMin && sw.F < FMax && sw.CV > cvMin && sw.CV < cvMax)
                {
                    strs[i] = sw.ToString();
                    selectCnt++;
                }
                i++;
            }
            if (selectCnt < Count)
			{
                string[] tmp = new string[selectCnt];
                selectCnt = 0;
                foreach (string str in strs)
                    if (str != null)
                        tmp[selectCnt++] = str;
				strs = tmp;
			}
			return strs;
		}
        #endregion
    }
    #endregion
}
