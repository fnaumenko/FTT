using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace FTT
{
    /// <summary>
    /// Provides input sequance's methods, launch of the main processes, 
    /// general management of outputs and progress bar.
	/// </summary>
	public class Processing
	{
        /// <summary>The BackgroundWorker in wich Processing is run.</summary>
        BackgroundWorker _worker;
        /// <summary>The progress bar indicated executing.</summary>
        ProgressBar _prgBar;
        /// <summary>True if a plot should be drawing.</summary>
        bool _isDrawPlot;

        /// <summary>Initialises a new instance of the Processing class.</summary>
        /// <param name="worker">The BackgroundWorker in wich Processing is run.</param>
        /// <param name="prgBar">The progress bar indicated executing.</param>
        /// <param name="wordLength">The length of word.</param>
		/// <param name="mismatchCnt">The amount of mismatches.</param>
        /// <param name="isAllPossible">True if all possible words are processed.</param>
        /// <param name="isTurbo">True if Turbo mode is defined.</param>
        /// <param name="isDrawPlot">True if a plot should be drawing; otherwise, false.</param>
        public Processing(BackgroundWorker worker, ProgressBar prgBar, byte wordLength, byte mismatchCnt, bool isAllPossible, bool isTurbo, bool isDrawPlot)
        {
            _worker = worker;
            _prgBar = prgBar;
            _isDrawPlot = isDrawPlot;
            Words.Init(wordLength, mismatchCnt, isTurbo);
            Patterns.Init(isAllPossible);
        }

        /// <summary>Calculates the patterns frequency.</summary>
        /// <param name="shakeCnt">The amount of shakes.</param>
        /// <param name="isAvrgIndividual">True if List Averaging Choice is an Individual.</param>
        public TrimmedPatterns ScanLocal(short shakeCnt, bool isAvrgIndividual)
		{
            return new TrimmedPatterns(
                _worker, shakeCnt, 
                (shakeCnt > 0) && isAvrgIndividual,
               ((float)_prgBar.Maximum - 60 / Words.WordLength) / (shakeCnt + 1), // why 60 ?
                _isDrawPlot
            );
		}

        /// <summary>Calculates the patterns frequency.</summary>
        /// <param name="shakeCnt">The amount of shakes.</param>
        /// <param name="winStartLenght">The start lenght of window.</param>
        /// <param name="winStopLenght">The stop lenght of window.</param>
        /// <param name="winIncr">TYhe increment of the window.</param>
        /// <param name="winShift">The shift of the window.</param>
        public ScanWindows ScanGlobal(
            short shakeCnt, short winStartLenght, short winStopLenght, short winIncr, short winShift)
		{
            // check input sequence
            int seqLength = Sequence.Length;
            if (winStopLenght >= seqLength)
				throw new ApplicationException(Abbr.SmallInput);

            // calculate total number of scannings
            int scanCnt = (seqLength - winStopLenght) / winShift + 1;
            if (((seqLength - winStopLenght) % winShift) > 0)
                scanCnt++;

            return ScanWindows.GetTreatedWindows(
                _worker, _prgBar, shakeCnt,
                winStartLenght, winStopLenght, winIncr, winShift, scanCnt
                );
        }
    }
}
