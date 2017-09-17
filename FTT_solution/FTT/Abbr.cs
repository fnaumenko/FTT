using System;
using System.Text;

namespace FTT
{
    /// <summary>Represents common abbreviation.</summary>
    public static class Abbr
    {
        public const string FTT = "FTT";
        //public const string YEAR = "2011";

        public const string Incorrect = "Wrong ";
        public const string FastaData = "FASTA format";
        public const string SavedData = "saved data format";
        public const string Global = "Global";
        public const string Local = "Local";
        public const string F = "F";
        public const string CV = "CV";
        public const string SD = "SD";
        public const string Patt = "Patt";
        public const string Pattern = "Pattern";
        public const string Frequency = "Frequency";
        public const string Freq = "Freq";
        public const string Mean = "Mean";
        public const string OUTPUTDATA = " output data";

        public const string SmallInput = "The input sequance is too small";

        public const string Tab = "\t";
        public const string EoL = "\n";
        public const string Tab_ = "\t ";
        public const char TAB = '\t';
        public const char EOL = '\n';
        public const char SPEC = '>';
        public const char SEP_LINE = '—';
        public const char SEP_LINE0 = '=';

        /// <summary>Returns a string with added new separate line below initial string value.</summary>
        /// <param name="caption">Initial string represented caption.</param>
        /// <returns></returns>
        public static string AddSepLine (string caption)
        {
            caption += Abbr.EoL;
            return caption.PadRight(2 * caption.Length, SEP_LINE);
        }

        /// <summary>Returns a string array that contains the substrings in this instance 
        /// that are delimited by separate line. The separate line is removed.</summary>
        /// <param name="input">The input string.</param>
        /// <returns></returns>
        public static string SplitBySepLine   (ref string input)
        {
            int sep = input.IndexOf(SEP_LINE);
            string res = input.Substring(0, sep);  // string before separate line
            sep = input.IndexOf(EOL, sep);
            input = input.Substring(sep + 1);     // string with separate line and below
            return res;
        }

    }
}
