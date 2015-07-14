using UnityEngine;

namespace Paths.Cache
{
    /// <summary>
    /// Caches spline evaluations.
    /// </summary>
    public class EvaluationCache : PathCache<Vector3>, ISpline
    {
        public EvaluationCache(Vector3[] values)
            : base(values) { }

        public EvaluationCache(ISpline spline, int cacheSize)
            : base(cacheSize)
        {
            for (int i = 0; i < this.Values.Length; i++)
                this.Values[i] = spline.Evaluate((float)i / (Values.Length - 1));
        }

        public Vector3 Evaluate(float t)
        {
            var indexInfo = GetCacheIndexInfo(t);

            return Vector3.Lerp(
                this.Values[indexInfo.LowerIndex],
                this.Values[indexInfo.UpperIndex],
                indexInfo.PercentOfIndexes);
        }

        public Vector3 Evaluate(float t, out PathCacheIndexInfo indexInfo)
        {
            indexInfo = GetCacheIndexInfo(t);

            return Vector3.Lerp(
                this.Values[indexInfo.LowerIndex],
                this.Values[indexInfo.UpperIndex],
                indexInfo.PercentOfIndexes);
        }

        public Vector3 Tangent(float t)
        {
            PathCacheIndexInfo evalInfo;
            Evaluate(t, out evalInfo);

            return this.Values[evalInfo.UpperIndex] - this.Values[evalInfo.LowerIndex];
        }
    }
}