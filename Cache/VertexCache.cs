using UnityEngine;

namespace Paths.Cache
{
    /// <summary>
    /// A vertex cache that caches spline evaluations.
    /// </summary>
    public class VertexCache : PathCache<Vector3>, ISpline
    {
        public VertexCache(ISpline spline, int numMidPoints)
            : base(numMidPoints + 2)
        {
            for (int i = 0; i < Values.Length; i++)
                Values[i] = spline.Evaluate((float)i / (Values.Length - 1));
        }

        public Vector3 Evaluate(float t)
        {
            var indexInfo = GetCacheIndex(t);

            return Vector3.Lerp(
                Values[indexInfo.LowerIndex],
                Values[indexInfo.LowerIndex + 1],
                indexInfo.PercentOfIndexes);
        }

        public Vector3 Evaluate(float t, out PathCacheIndexInfo indexInfo)
        {
            indexInfo = GetCacheIndex(t);

            return Vector3.Lerp(
                Values[indexInfo.LowerIndex],
                Values[indexInfo.LowerIndex + 1],
                indexInfo.PercentOfIndexes);
        }

        public Vector3 Tangent(float t)
        {
            PathCacheIndexInfo evalInfo;
            Evaluate(t, out evalInfo);

            return Values[evalInfo.LowerIndex + 1] - Values[evalInfo.LowerIndex];
        }
    }
}