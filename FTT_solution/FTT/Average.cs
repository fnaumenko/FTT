using System;

namespace FTT
{
    /// <summary>Represents an average Frequence and Standart Deviation of the pattern.</summary>
    /// <remarks>Needs for Individual Avaraging Choice rate only.</remarks>
    public struct Average
    {
        /// <summary>The average Frequence.</summary>
        public readonly float Mean;
        /// <summary>The Standart Deviation of the pattern: SD = SQRT( sum(SQR) / count - mean^2 )</summary>
        public readonly float SD;

        //Average (float mean, float sd)
        //{
        //    Mean = mean;
        //    SD = sd;
        //}
        /// <summary>Creates the new Average structure instance.</summary>
        /// <param name="sumCounter">The sum & sum of squares of values.</param>
        public Average  (SumCounter sumCounter)
        {
            if (sumCounter.IsEmpty)
            {
                Mean = 0;
                SD = 0;
            }
            else
            {
                Mean = sumCounter.Mean;
                SD = sumCounter.SD(Mean);
                if (SD == float.NaN)
                    Console.Beep();
                //System.Windows.Forms.MessageBox.Show(
                //    "sumSqr=" + sumSqr.ToString() + "  count=" + count.ToString()
                //    + "  mean=" + mean.ToString());
            }
        }

        //static public Average Empty
        //{
        //    get { return new Average(0, 0); }
        //}

        /// <summary>Gets the Coefficient of Variation: CV = SD/mean, where SD - Standart Deviation, mean - the average Frequence.</summary>
        /// <returns></returns>
        public float	CV
        {
            get { return Mean != 0 ? SD / Mean : 0; }
        }

        /// <summary>Returns the "Fluffing Factor" for given frequency.</summary>
        /// <param name="freq">Frequency.</param>
        /// <returns></returns>
        public float F  (int freq)
        {
            return SD == 0 ? 0 : (float)((freq - Mean) / SD);
        }

        #region SumCounter structure
        /// <summary>Represents an accumulative values of sum & sum of squares.</summary>
        public struct SumCounter
        {
            /// <summary>Accruing sum.</summary>
            long sum;
            /// <summary>Accruing sum of squares.</summary>
            float sumSqr;
            /// <summary>Total number of accumulations.</summary>
            int  count;

            /// <summary>The number of accumulations..</summary>
            public bool IsEmpty
            {
                get { return count==0; }
            }

            /// <summary>The average value.</summary>
            public float Mean
            {
                get { return (float)sum / count; }
            }

            ///// <summary>The Standart Deviation of the pattern: SD = SQRT( sum(SQRT) / count - mean^2 )</summary>
            //public float SD
            //{
            //    get
            //    {
            //        float mean = Mean;
            //        return (float)Math.Sqrt((float)sumSqr / (float)count - mean * mean);
            //    }
            //}

            /// <summary>The Standart Deviation of the pattern: SD = SQRT( sum(SQRT) / count - mean^2 )</summary>
            ///<param name="mean">Mean.</param>
            public float SD (float mean)
            {
                return (float)Math.Sqrt(sumSqr / count - mean * mean);
            //    float res = (float)Math.Sqrt( (float)sumSqr / count - mean * mean);
            //    if (float.IsNaN(res))
            //        System.Windows.Forms.MessageBox.Show(
            //            "sumSqr=" + sumSqr.ToString() + "  count=" + count.ToString()
            //            + "  mean^2=" + (mean*mean).ToString());
            //    return res;
            }

            /// <summary>Set all the fields to 0.</summary>
            public void Reset()
            {
                sum = count = 0;
                sumSqr = 0;
            }

            public static SumCounter operator + (SumCounter sumCounter, int val)
            {
                sumCounter.sum += val;
                sumCounter.sumSqr += val * val;
                sumCounter.count++;
                return sumCounter;
            }

         }  // end of structure SumCounter
        #endregion
    }   // end of structure Average
}
