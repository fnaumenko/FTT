using System;
using System.Collections;
//using System.Collections.Generic;

namespace FTT
{
    #region class ArraySorter for descending order
    //static public class ArraySorter<T>
    //where T : IComparable
    //{
    //    static public void SortDescending(T[] array)
    //    {
    //        Array.Sort<T>(array, s_Comparer);
    //    }
    //    static private ReverseComparer s_Comparer = new ReverseComparer();
    //    private class ReverseComparer : IComparer<T>
    //    {
    //        public int Compare(T object1, T object2)
    //        {
    //            return -((IComparable)object1).CompareTo(object2);
    //        }
    //    }
    //}
    #endregion
    /// <summary>The generic collection of structures.</summary>
    public class StructArray<T> : IEnumerable
    {
        ///// <summary>Occurs when the progress bar is increased by one step.</summary>
        //public static event EventHandler PrgBarIncreased;
        /// <summary>Occurs when the plot should be drowned.</summary>
        //public static event EventHandler<DrawEventArgs> Draw;
        // By using the generic EventHandler<T> event type we do not need to declare a separate delegate type.

        ///// <summary>The mode of sorting the collection.</summary>
        //protected enum SortMode : byte
        //{
        //    /// <summary>Not sorting.</summary>
        //    Non = 0,
        //    /// <summary>Sorting by default.</summary>
        //    ByDef = 0x1,
        //    /// <summary>Sorting by IComparer.</summary>
        //    ByIComp = 0x2
        //}

        // The mode of sorting the collection.
        /// <summary>Not sorting.</summary>
        protected const byte NON_SORT = 0;
        /// <summary>Sorting by default (ascending order).</summary>
        protected const byte BY_DEF = 0x1;
        /// <summary>Sorting by descending order.</summary>
        protected const byte BY_DESCEND = 0x2;
        /// <summary>Sorting by IComparer 1.</summary>
        protected const byte BY_ICOMP1 = 0x4;

        /// <summary>Counter of accumulating events for progress bar step.</summary>
        static int _counter;

        protected T[] _coll = null;
        private byte _signSorted = NON_SORT; // (byte)SortMode.Non;

        /// <summary>Constructor of StructArray.</summary>
		/// <param name="count">The total count of the elements.</param>
        public  StructArray    (int count)
		{
			_coll = new T[count];
		}

        /// <summary>Constructor of StructArray.</summary>
        /// <param name="count">The total count of the elements.</param>
        /// <param name="sortMode">Current sorting mode.</param>
        protected StructArray(int count, byte sortMode)
        {
            _coll = new T[count];
            _signSorted = sortMode;
        }

        /// <summary>Gets the number of elements contained in the instance.</summary>
        public int Count
        {
            get {
                
                return _coll.Length; }
        }

		/// <summary>Gets or sets the element by index.</summary>
		public  T this [int index]
		{
			get	{ return _coll[index]; }
			set	{ _coll[index] = value;}
		}

        public IEnumerator GetEnumerator()
        {
            return _coll.GetEnumerator();
        }

        /// <summary>Clears the collection and set default state.</summary>
        protected void Clear()
        {
            Array.Clear(_coll, 0, Count);
            _signSorted = NON_SORT;
        }

        ///// <summary>Returns value indicated if collection is sorted by default.</summary>
        //protected bool IsUnsorted_
        //{
        //    get { return (_signSorted & BY_DEF) == 0; }
        //}

        /// <summary>Sorts the collection by default if it is not sorted yet.</summary>
        protected void Sort()
        {
            if ((_signSorted & BY_DEF) == 0)
            {
                Array.Sort(_coll);
                _signSorted = BY_DEF;
            }
        }

        /// <summary>Sorts the collection by default if it is not sorted yet.</summary>
        protected void SortReverse()
        {
            if ((_signSorted & BY_DESCEND) == 0)
            {
                Array.Sort(_coll);
                Array.Reverse(_coll);
                _signSorted = BY_DESCEND;
                //System.Console.Beep();
            }
        }

        /// <summary>Sorts the collection using the specified IComparer interface if it is not sorted yet.</summary>
        /// <param name="comparer">IComparer interface.</param>
        protected void Sort(IComparer comparer)
        {
            Sort(comparer, BY_ICOMP1);
        }

        /// <summary>Sorts the collection using the specified IComparer interface if it is not sorted yet.</summary>
        /// <param name="comparer">IComparer interface.</param>
        /// <param name="sortMode">Sorting mode.</param>
        protected void Sort(IComparer comparer, byte sortMode)
        {
            if ((_signSorted & BY_ICOMP1) == 0)
            {
                Array.Sort(_coll, comparer);
                _signSorted = sortMode;
            }
        }

        /// <summary>Raises the ProgressChanged event every specified times.</summary>
        /// <param name="worker">The current BackgroundWorker raised event.</param>
        /// <param name="timesPerStep">The count of calls after that one PrgBarIncreased event is raised;
        /// 0 if miss Progress Bar treatment.</param>
        protected static void OnPrgBarIncreased(
            System.ComponentModel.BackgroundWorker worker, int timesPerStep)
        {
            if (timesPerStep > 0)
            {
                //System.Windows.Forms.MessageBox.Show(timesPerStep.ToString() + counter.ToString("  0"));
                if (_counter == timesPerStep)
                {
                    // the receiver doesn't processed the parameter. Progress Bar counts percentage by himself.
                    worker.ReportProgress(_counter = 1);
                }
                else
                    _counter++;
            }
        }

        /// <summary>Reset progress bar step counter.</summary>
        protected static void OnPrgBarReseat()
        {
            _counter = 1;
        }

        /// <summary>Copies collection to the another collection.</summary>
        /// <param name="destColl">The destination collection</param>
        /// <param name="count">The count of non-zero elements of array to copy</param>
        /// <returns>The new copy collection</returns>
        protected void CloneTo  (StructArray<T> destColl, int count)
        {
            Array.Copy(_coll, destColl._coll, count);
            //destColl._signSorted = _signSorted;
        }
    }
}
