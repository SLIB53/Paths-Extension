using System;
using UnityEngine;

namespace Paths.Cache
{
    public static class PathCacheUtility
    {
        /// <summary>
        /// Converts the percentLength of a spline to the common t parameter.
        /// The spline is represented by its LengthCache.
        /// </summary>
        public static float t_PercentLength(float percentLength, LengthCache lengthTable)
        {
            float[] lengthValues = lengthTable.Values;

            percentLength = Mathf.Clamp01(percentLength);

            //// Early Out
            if (Mathf.Approximately(percentLength, 0f))
                return 0f;
            else if (Mathf.Approximately(percentLength, 1.0f))
                return 1f;

            //// Search for t that matches the length percent
            float tAsLength = percentLength * lengthValues[lengthValues.Length - 1];
            int nearestIndex = Array.BinarySearch<float>(lengthValues, tAsLength);
            float finalT;

            //// Get the final output
            if (nearestIndex < 0)
            {
                int lowerValueIndex = ~nearestIndex - 1;
                float lowerValue = (lowerValueIndex < 0) ? 0f : lengthValues[lowerValueIndex];
                float approximateOffset = (tAsLength - lowerValue);
                approximateOffset /= (lengthValues[lowerValueIndex + 1] - lowerValue);

                finalT = (lowerValueIndex + approximateOffset) / lengthValues.Length;
            }
            else
            {
                finalT = (float)nearestIndex / lengthValues.Length;
            }

            return finalT;
        }

        /// <summary>
        /// Evaluates a spline based on the integrated length of the spline.
        /// </summary>
        public static Vector3 EvenlyEvaluate(ISpline spline, float percentLength, LengthCache splineLengthCache)
        {
            return spline.Evaluate(t_PercentLength(percentLength, splineLengthCache));
        }

        /// <summary>
        /// Builds a cache from the results of evenly evaluating a spline.
        /// </summary>
        public static EvaluationCache EvenEvaluationCache(int cacheSize, ISpline spline, LengthCache splineLengthCache)
        {
            var points = new Vector3[cacheSize];

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = EvenlyEvaluate(
                    spline,
                    (float)i / (points.Length - 1),
                    splineLengthCache);
            }

            return new EvaluationCache(points);
        }
    }
}