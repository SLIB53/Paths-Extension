using UnityEngine;

namespace Paths.Cache
{
    public abstract class PathCache<T>
    {
        public readonly T[] Values;

        protected PathCache(int cacheSize)
        {
            if (cacheSize < 2)
            {
                throw new System.ArgumentOutOfRangeException(
                    "cacheSize",
                    "Cache size cannot be less than or equal to 2.");
            }

            Values = new T[cacheSize];
        }

        public PathCacheIndexInfo GetCacheIndex(float t)
        {
            return new PathCacheIndexInfo(Values.Length, t);
        }
    }

    /// <summary>
    /// Builds cache index values from an interpolant. Used to interpolate caches.
    /// </summary>
    public struct PathCacheIndexInfo
    {
        public readonly int LowerIndex;

        /// <summary>
        /// Interpolant between cache[LowerIndex] and cache[LowerIndex+1].
        /// </summary>
        public readonly float PercentOfIndexes;

        public PathCacheIndexInfo(int cacheLength, float t)
        {
            float percentOfIndexes;
            int lowerIndex;

            //~ Make sure t is a valid number
            t = Mathf.Clamp01(t);

            if (Mathf.Approximately(t, 1f)) // avoid index out of bounds
            {
                lowerIndex = cacheLength - 2;
                percentOfIndexes = 1f;
            }
            else
            {
                percentOfIndexes = t * (cacheLength - 1);
                lowerIndex = Mathf.FloorToInt(percentOfIndexes);
                if (lowerIndex != 0) // avoid NaN from indexInterpolant % 0
                    percentOfIndexes = percentOfIndexes % lowerIndex; // store value that is inbetween 0 and 1.
            }

            LowerIndex = lowerIndex;
            PercentOfIndexes = percentOfIndexes;
        }

        public int UpperIndex { get { return LowerIndex + 1; } }
    }
}