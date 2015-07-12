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
    public abstract class Path3D : ISpline
    {
        public Transform LocalSpaceTransform;

        public Path3D(Transform localSpace)
        {
            LocalSpaceTransform = localSpace;
        }

        public abstract Vector3 Evaluate(float t);

        public abstract float Length();

        public virtual Vector3 Tangent(float t)
        {
            float tangentMargin = 0.001f;
            return Evaluate(t + tangentMargin) - Evaluate(t - tangentMargin);
        }

        protected float BruteLength(float divisions = 32.0f)
        {
            float t = 1.0f / divisions;
            float increment = t;
            Vector3 l1 = Evaluate(0.0f);
            Vector3 l2;
            float sum = 0.0f;

            while (t < 1.0f)
            {
                l2 = Evaluate(t);
                sum += Vector3.Distance(l1, l2);
                t += increment;
                l1 = l2;
            }

            return sum;
        }
    }
}