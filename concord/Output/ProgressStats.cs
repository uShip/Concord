using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace concord.Output
{
    public class ProgressStats : IEnumerable<int>
    {
        private readonly int[] _totalToRun;

        public ProgressStats(int totalToRun)
        {
            _totalToRun = new int[totalToRun];
        }

        public int GetProgressCount(ProgressState state)
        {
            return _totalToRun.Count(x => x == (int) state);
        }

        /// <summary>
        /// Count of all tests that is a completed state (includes errors)
        /// </summary>
        /// <returns></returns>
        public int GetCompletedCount()
        {
            return _totalToRun.Count(x => x >= (int)ProgressState.Finished);
        }


        public void IncrementIndex(int index)
        {
            Interlocked.Increment(ref _totalToRun[index]);
        }

        //public void SetIndexTo(int index, ProgressState state)
        //{
        //    Interlocked.Exchange(ref _totalToRun[index], (int)state);
        //}

        public IEnumerator<int> GetEnumerator()
        {
            return ((IEnumerable<int>)_totalToRun).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get { return _totalToRun.Length; }
        }
    }
}