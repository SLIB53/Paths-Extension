/*
 * Copyright (c) 2014 Akilram Krishnan

 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR
 * A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 * Akilram.com
 */

using System.Collections.Generic;
using UnityEngine;

namespace Paths
{
    [System.Serializable]
    public struct DynamicBezier : ISpline
    {
        #region Fields & Properties

        public const int GoodNumMidPoints = 32;
        public const float TangentEpsilon = 0.00001f;

        private bool recalcLength;

        [SerializeField]
        public List<Vector3> Knots;

        private float _length;

        public float Length
        {
            get
            {
                if (recalcLength)
                {
                    _length = CalculateLength(GoodNumMidPoints);
                    recalcLength = false;
                }

                return _length;
            }
        }

        #endregion Fields & Properties

        #region Methods

        public DynamicBezier(IEnumerable<Vector3> knots)
        {
            Knots = new List<Vector3>(knots);
            recalcLength = true;
            _length = float.NaN;
        }

        public DynamicBezier(params Vector3[] knots)
        {
            Knots = new List<Vector3>(knots);
            recalcLength = true;
            _length = float.NaN;
        }

        /// <summary>
        /// Evaluates the point at t for this Bezier curve. 
        /// </summary>
        /// <param name="t">
        /// Interpolant value between 0 and 1. t is implicitly clamped between 0 and 1.
        /// </param>
        public Vector3 Evaluate(float t)
        {
            //~ Build tree from bottom to top. Root is the returned value
            Vector3[] lowerOrder = Knots.ToArray();

            //~ Tree evaluation
            for (int level = Knots.Count; level > 0; level--)
            {
                //~ Exit loop when root is reached
                if (level == 1)
                    break;

                //~ Create next level of tree
                Vector3[] higherOrder = new Vector3[level - 1];
                for (int i = 0; i < higherOrder.Length; i++) //~ i < lowerOrder.Length - 1
                {
                    higherOrder[i] = Vector3.Lerp(lowerOrder[i], lowerOrder[i + 1], t);
                }
                lowerOrder = higherOrder;
            }
            return lowerOrder[0];
        }

        /// <summary>
        /// Calculates the length of the curve. Use when Length property is inaccurate. 
        /// </summary>
        public float CalculateLength(float divisions)
        {
            float t = 1.0f / divisions;
            float increment = t;
            Vector3 p1 = Knots[0];
            Vector3 p2;
            float sum = 0.0f;

            while (t < 1.0f)
            {
                p2 = this.Evaluate(t);
                sum += Vector3.Distance(p1, p2);
                t += increment;
                p1 = p2;
            }

            return sum;
        }

        /// <summary>
        /// Calculates the length of the curve and returns a table of lengths. 
        /// </summary>
        public float[] CalculateLengthTable()
        {
            return CalculateLengthTable(GoodNumMidPoints);
        }

        public float[] CalculateLengthTable(float divisions)
        {
            float t = 1.0f / divisions;
            float increment = t;
            Vector3 p1 = Knots[0];
            Vector3 p2;
            float sum = 0.0f;
            var sums = new List<float>();

            for (int i = 0; t < 1.0f; i++)
            {
                p2 = this.Evaluate(t);
                sum += Vector3.Distance(p1, p2);
                sums.Add(sum);
                t += increment;
                p1 = p2;
            }

            return sums.ToArray();
        }

        /// <summary>
        /// Calculates the tangent vector at the point Interpolate(t). The vector returned is not normalized. 
        /// </summary>
        public Vector3 Tangent(float t)
        {
            return Tangent(t, TangentEpsilon);
        }

        public Vector3 Tangent(float t, float tangentEpsilon)
        {
            return Evaluate(t + tangentEpsilon) - Evaluate(t - tangentEpsilon);
        }

        #endregion Methods
    }
}