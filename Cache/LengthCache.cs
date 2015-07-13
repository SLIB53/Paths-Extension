using System.Collections.Generic;
using UnityEngine;

namespace Paths.Cache
{
    //TODO: Extend PathCache, construct based on numMidPoints, not divisions.
    public class LengthCache
    {
        public readonly float[] Lengths;

        public LengthCache(ISpline spline, float divisions)
        {
            float t = 1.0f / divisions;
            float increment = t;
            Vector3 p1 = spline.Evaluate(0f);
            Vector3 p2;
            float sum = 0.0f;
            var sums = new List<float>();

            for (int i = 0; t < 1.0f; i++)
            {
                p2 = spline.Evaluate(t);
                sum += Vector3.Distance(p1, p2);
                sums.Add(sum);
                t += increment;
                p1 = p2;
            }

            Lengths = sums.ToArray();
        }
    }
}