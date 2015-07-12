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
    public class CirclePath : Path3D
    {
        #region Fields

        [SerializeField]
        public Circle LocalCircle;

        [SerializeField]
        public float MaxAngle = Circle.TwoPI;

        #endregion Fields

        #region Properties

        public Vector3 Center
        {
            get { return LocalSpaceTransform.TransformPoint(LocalCircle.Center); }
            set { LocalCircle.Center = LocalSpaceTransform.InverseTransformPoint(value); }
        }

        #endregion Properties

        #region Methods

        public CirclePath(Transform localSpace, float maxAngle = Circle.TwoPI)
            : base(localSpace)
        {
            LocalSpaceTransform = localSpace;
            MaxAngle = maxAngle;
        }

        public CirclePath(Transform localSpace, Circle localCircle, float maxAngle = Circle.TwoPI)
            : this(localSpace)
        {
            LocalCircle = localCircle;
            MaxAngle = maxAngle;
        }

        public override Vector3 Evaluate(float t)
        {
            return LocalSpaceTransform.TransformPoint(LocalCircle.InterpolateAngle(t * MaxAngle));
        }

        public override float Length()
        {
            Vector3 lossyScale = LocalSpaceTransform.lossyScale;

            if (lossyScale == Vector3.zero)
                return 0f;

            if (Mathf.Approximately(lossyScale.x, lossyScale.y))
            {
                Circle pathAsWorldCircle = new Circle(LocalCircle.Center, LocalCircle.Radius * lossyScale.x);
                return pathAsWorldCircle.Length() * Mathf.Abs(MaxAngle / Circle.TwoPI);
            }
            else
                return BruteLength();
        }

        //~ Because there are so many dynamics (center offset, scale), it is better to use base Tangent()
        //public override Vector3 Tangent(float t)
        //{
        //    return LocalSpaceTransform.TransformPoint(LocalSpaceCircle.Tangent(t * MaxAngle / Circle.TwoPI));
        //}

        #endregion Methods
    }
}