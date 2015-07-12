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

using UnityEngine;

namespace Paths
{
    [System.Serializable]
    public struct QuadraticBezier : ISpline
    {
        #region Fields & Properties

        public const int GoodNumMidPoints = 32;
        public const float TangentEpsilon = 0.00001f;

        private bool recalcLength;

        [SerializeField]
        private Vector3 _startPosition;

        public Vector3 StartPosition
        {
            get
            {
                return _startPosition;
            }
            set
            {
                _startPosition = value;
                recalcLength = true;
            }
        }

        [SerializeField]
        private Vector3 _midTangent;

        public Vector3 MidTangent
        {
            get
            {
                return _midTangent;
            }
            set
            {
                _midTangent = value;
                recalcLength = true;
            }
        }

        [SerializeField]
        private Vector3 _endPosition;

        public Vector3 EndPosition
        {
            get
            {
                return _endPosition;
            }
            set
            {
                _endPosition = value;
                recalcLength = true;
            }
        }

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

        public QuadraticBezier(Vector3 startPosition, Vector3 tangent, Vector3 endPosition)
        {
            _startPosition = startPosition;
            _midTangent = tangent;
            _endPosition = endPosition;
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
            Vector3 a = Vector3.Lerp(StartPosition, MidTangent, t);
            Vector3 b = Vector3.Lerp(MidTangent, EndPosition, t);
            return Vector3.Lerp(a, b, t);
        }

        /// <summary>
        /// Calculates the length of the curve. Use when Length property is inaccurate. 
        /// </summary>
        public float CalculateLength(float divisions)
        {
            float t = 1.0f / divisions;
            float increment = t;
            Vector3 p1 = StartPosition;
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

        public override bool Equals(object obj)
        {
            if (obj is QuadraticBezier)
                return this == (QuadraticBezier)obj;

            return false;
        }

        public override int GetHashCode()
        {
            return
                StartPosition.x.GetHashCode() ^ StartPosition.y.GetHashCode() ^ StartPosition.z.GetHashCode() ^
                MidTangent.x.GetHashCode() ^ MidTangent.y.GetHashCode() ^ MidTangent.z.GetHashCode() ^
                EndPosition.x.GetHashCode() ^ EndPosition.y.GetHashCode() ^ EndPosition.z.GetHashCode();
        }

        public static bool operator ==(QuadraticBezier a, QuadraticBezier b)
        {
            return
                a.StartPosition == b.StartPosition &&
                a.MidTangent == b.MidTangent &&
                a.EndPosition == b.EndPosition;
        }

        public static bool operator !=(QuadraticBezier a, QuadraticBezier b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return
                StartPosition.ToString() +
                MidTangent.ToString() +
                EndPosition.ToString();
        }

        #endregion Methods
    }
}