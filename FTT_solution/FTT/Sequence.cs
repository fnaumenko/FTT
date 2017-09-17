using System;
using System.IO;

namespace FTT
{
    /// <summary>Represents an input sequence of letters.</summary>
    class Sequence
    {
        /// <summary>Original input sequance. Constant for calculation session.</summary>
         public static byte[] Original;

        /// <summary>Gets the length of original input sequence.</summary>
        public static int Length
        {
            get { return Original.Length; }
        }

        /// <summary>Reads and initialises input sequance from a file.</summary>
        /// <param name="fileName">The full name of file.</param>
        /// <returns>True if input sequence is not empty.</returns>
        public static bool Read(string fileName)
        {
            //string res = string.Empty;
            long lcount = 0, fl = new FileInfo(fileName).Length;
            byte[] inputArray = new byte[fl];
            using (StreamReader sr = File.OpenText(fileName))
            {
                string tmp = sr.ReadLine();
                // check for header string
                if (tmp != null && tmp.Length > 0 && tmp[0] != Abbr.SPEC)
                    ParseString(ref inputArray, ref lcount, tmp);

                while ((tmp = sr.ReadLine()) != null)
                    ParseString(ref inputArray, ref lcount, tmp);
            }
            if (lcount == 0)
                return false;
            Array.Resize(ref inputArray, (int)lcount);
            Original = inputArray;
            return true;
        }

        /// <summary>Fills the input array by the input string line.</summary>
        /// <param name="inpArray">The input array.</param>
        /// <param name="seek">Current writing position.</param>
        /// <param name="str">The string line.</param>
        static void ParseString(ref byte[] inpArray, ref long seek, string str)
        {
            int length = str.Length;
            str = str.ToUpper();
            for (int i = 0; i < length; i++)
            {
                if (str[i] != 'N')
                    if ((inpArray[seek++] = Chars.GetIndex(str[i])) == Chars.Undef)
                        throw new ApplicationException(Abbr.Incorrect + Abbr.FastaData);
            }
        }

        /// <summary>Fills the input array by the input string.</summary>
        /// <param name="inputStr">The input string.</param>
        /// <remarks>Provides full check for the correct data.</remarks>
        public static byte[] Parse  (string inputStr)
        {
            byte[] inputArray = new byte[inputStr.Length];
            long seek = 0;
            ParseString(ref inputArray, ref seek, inputStr);
            return inputArray;
        }

        /// <summary>Shakes the range of elements in the input array.</summary>
        /// <param name="startIndex">The index in the inputArray at which shaking begins.</param>
        /// <param name="length">The number of elements to shake.</param>
        /// <param name="worker">The current BackgroundWorker raised event.</param>
        /// <param name="cntSteps">The number of ProgressBar steps: from 0 to 3</param>
        /// <returns>The length-sized array with the random order of initial elements on the range.</returns>
        public static byte[] Shake(int startIndex, int length, System.ComponentModel.BackgroundWorker worker, int cntSteps)
        {
            byte[] res = new byte[length];
            Array.Copy(Original, startIndex, res, 0, length);
            //if (cntSteps > 2)
            //    worker.ReportProgress(1);
            byte[] b = new Byte[length];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            //new Random().NextBytes(b);    this class generate bad random by fast processor
            //if (cntSteps > 1)
            //    worker.ReportProgress(1);
            Array.Sort(b, res);
            //if (cntSteps > 0)
            //    worker.ReportProgress(1);
            return res;
        }

#if DEBUG
        /// <summary>Shakes the range of elemetns in the input string.</summary>
        /// <param name="input">The string to seek</param>
        /// <returns>The array with the random order of initial elements.</returns>
        public static string Shake  (string input)
        {
            char[] res = input.ToCharArray();
            byte[] b = new Byte[input.Length];
            new Random().NextBytes(b);
            Array.Sort(b, res);
            return new string(res);
        }

        /// <summary>Fills the input array by the input string.</summary>
        /// <param name="inputArray">The input array.</param>
        /// <remarks>Provides full check for the correct data.</remarks>
        public static string ToString   (byte[] inputArray)
        {
            int length = inputArray.Length;
            char[] res = new char[length];
            for (int i = 0; i < length; i++)
                res[i] = Chars.GetChar(inputArray[i]);
            return new string(res);
        }

#endif
    }
}
