using UnityEngine;

namespace Paths.Cache
{
    /// <summary>
    /// Base class for path caches, and parallel caches.
    /// </summary>
    /// <typeparam name="T">Type of cached values.</typeparam>
    public abstract class PathCache<T>
    {
        public readonly T[] Values;

        protected PathCache(T[] values) { Values = values; }

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

        /// <summary>
        /// Converts the common t spline parameter to PathCache.Values indexes.
        /// </summary>
        public PathCacheIndexInfo GetCacheIndexInfo(float t)
        {
            return new PathCacheIndexInfo(Values.Length, t);
        }
    }

    /// <summary>
    /// Builds cache index values from an interpolant. Used to interpolate caches.
    /// </summary>
    public struct PathCacheIndexInfo
    {
        /// <summary>
        /// Lower bound index for interpolant.
        /// </summary>
        public readonly int LowerIndex;

        /// <summary>
        /// Interpolant between cache[LowerIndex] and cache[LowerIndex+1].
        /// </summary>
        public readonly float PercentOfIndexes;

        public PathCacheIndexInfo(int cacheSize, float t)
        {
            float percentOfIndexes;
            int lowerIndex;

            //~ Make sure t is a valid number
            t = Mathf.Clamp01(t);

            if (Mathf.Approximately(t, 1f)) // avoid index out of bounds
            {
                lowerIndex = cacheSize - 2;
                percentOfIndexes = 1f;
            }
            else
            {
                percentOfIndexes = t * (cacheSize - 1);
                lowerIndex = Mathf.FloorToInt(percentOfIndexes);
                if (lowerIndex != 0) // avoid NaN from indexInterpolant % 0
                    percentOfIndexes = percentOfIndexes % lowerIndex; // store value that is inbetween 0 and 1.
            }

            LowerIndex = lowerIndex;
            PercentOfIndexes = percentOfIndexes;
        }

        /// <summary>
        /// Upper bound index for interpolant.
        /// </summary>
        public int UpperIndex { get { return LowerIndex + 1; } }
    }
}