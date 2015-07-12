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
    public struct Circle : ISpline
    {
        #region Fields

        public const float TwoPI = 2f * Mathf.PI;
        public Vector3 Center;
        public float Radius;

        #endregion Fields

        #region Methods

        public Circle(float radius)
            : this(Vector3.zero, radius)
        {
        }

        public Circle(Vector3 center)
            : this(center, 1.0f)
        {
        }

        public Circle(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// Evaluates the point at the value t for this circle. 
        /// </summary>
        /// <param name="t">
        /// Interpolant value between 0 and 1. Values out of range will simply loop.
        /// </param>
        public Vector3 Evaluate(float t)
        {
            return InterpolateAngle(t * TwoPI);
        }

        /// <summary>
        /// Evaluates the point at an angle for this circle. The angle value loops. 
        /// </summary>
        /// <param name="angle">
        /// Interpolant value between 0 and 2 * pi radians. Values out of range will simply loop.
        /// </param>
        public Vector3 InterpolateAngle(float angle)
        {
            return (Center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f)) * Radius;
        }

        /// <summary>
        /// Circumference of the circle. 
        /// </summary>
        public float Length()
        {
            return Mathf.PI * Radius * 2.0f;
        }

        /// <summary>
        /// Calculates the tangent to a point on the circle at point Interpolate(t). Returned vector
        /// is not normalized.
        /// </summary>
        /// <param name="t">
        /// Interpolant value between 0 and 1. Values out of range will simply loop.
        /// </param>
        public Vector3 Tangent(float t)
        {
            var i = Evaluate(t);
            return new Vector3(-i.y, i.x, i.z); // negative inverse
        }

        public override bool Equals(object obj)
        {
            if (obj is Circle)
                return this == (Circle)obj;

            return false;
        }

        public override int GetHashCode()
        {
            return Center.x.GetHashCode() ^ Center.y.GetHashCode() ^ Center.z.GetHashCode() ^ Radius.GetHashCode();
        }

        public static bool operator ==(Circle a, Circle b)
        {
            return a.Center == b.Center && a.Radius == b.Radius;
        }

        public static bool operator !=(Circle a, Circle b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return Radius + " " + Center;
        }

        #endregion Methods
    }
}