using UnityEngine;

namespace Paths.Cache
{
    //public class LengthCacheOLD
    //{
    //    public readonly float[] Lengths;

    //    public LengthCacheOLD(ISpline spline, float divisions)
    //    {
    //        float t = 1.0f / divisions;
    //        float increment = t;
    //        Vector3 p1 = spline.Evaluate(0f);
    //        Vector3 p2;
    //        float sum = 0.0f;
    //        var sums = new List<float>();

    //        for (int i = 0; t < 1.0f; i++)
    //        {
    //            p2 = spline.Evaluate(t);
    //            sum += Vector3.Distance(p1, p2);
    //            sums.Add(sum);
    //            t += increment;
    //            p1 = p2;
    //        }

    //        Lengths = sums.ToArray();
    //    }
    //}

    public class LengthCache : PathCache<float>
    {
        public LengthCache(ISpline spline, int cacheSize)
            : base(cacheSize)
        {
            Vector3 p1 = spline.Evaluate(0f);
            Vector3 p2;

            Values[0] = 0f;

            for (int i = 1; i < Values.Length; i++)
            {
                p2 = spline.Evaluate((float)i / (Values.Length - 1));
                Values[i] = Values[i - 1] + Vector3.Distance(p1, p2);
                p1 = p2;
            }
        }

        public float Interpolate(float t)
        {
            PathCacheIndexInfo indexInfo = this.GetCacheIndexInfo(t);

            return Mathf.Lerp(
                Values[indexInfo.LowerIndex],
                Values[indexInfo.UpperIndex],
                indexInfo.PercentOfIndexes);
        }
    }
}