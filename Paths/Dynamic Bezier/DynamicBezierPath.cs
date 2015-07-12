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
    public class DynamicBezierPath : Path3D
    {
        #region Fields

        [SerializeField]
        public DynamicBezier LocalBezier;

        #endregion Fields

        #region Properties

        public DynamicBezier WorldSpaceBezier
        {
            get
            {
                var worldSpaceKnots = new List<Vector3>();
                for (int i = 0; i < LocalBezier.Knots.Count; i++)
                    worldSpaceKnots.Add(this.LocalSpaceTransform.TransformPoint(LocalBezier.Knots[i]));

                return new DynamicBezier(worldSpaceKnots.ToArray());
            }
        }

        #endregion Properties

        #region Methods

        public DynamicBezierPath(Transform localSpace)
            : base(localSpace) { }

        public DynamicBezierPath(Transform localSpace, DynamicBezier localBezier)
            : this(localSpace)
        {
            LocalBezier = localBezier;
        }

        public override Vector3 Evaluate(float t)
        {
            return LocalSpaceTransform.TransformPoint(LocalBezier.Evaluate(t));
        }

        public override Vector3 Tangent(float t)
        {
            return WorldSpaceBezier.Tangent(t);
        }

        public override float Length()
        {
            if (LocalSpaceTransform.lossyScale == Vector3.zero)
                return 0f;
            else
                return WorldSpaceBezier.Length;
        }

        #endregion Methods
    }
}