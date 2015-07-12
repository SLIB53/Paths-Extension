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

using System;
using UnityEngine;

namespace Paths
{
    [System.Serializable]
    public class LinePath : Path3D
    {
        #region Fields

        [SerializeField]
        public Vector3 LocalStartPosition;

        [SerializeField]
        public Vector3 LocalEndPosition;

        #endregion Fields

        #region Properties

        public Vector3 StartPosition
        {
            get { return LocalSpaceTransform.TransformPoint(LocalStartPosition); }
            set { LocalStartPosition = LocalSpaceTransform.InverseTransformPoint(value); }
        }

        public Vector3 EndPosition
        {
            get { return LocalSpaceTransform.TransformPoint(LocalEndPosition); }
            set { LocalEndPosition = LocalSpaceTransform.InverseTransformPoint(value); }
        }

        #endregion Properties

        #region Methods

        public LinePath(Transform localSpace)
            : base(localSpace) { }

        public LinePath(Transform localSpace, Vector3 localStartPosition, Vector3 localEndPosition)
            : this(localSpace)
        {
            LocalStartPosition = localStartPosition;
            LocalEndPosition = localEndPosition;
        }

        public override Vector3 Evaluate(float t)
        {
            return LocalSpaceTransform.TransformPoint(Vector3.Lerp(LocalStartPosition, LocalEndPosition, t));
        }

        public override float Length()
        {
            if (LocalSpaceTransform.lossyScale == Vector3.zero)
                return 0f;
            else
                return Vector3.Distance(StartPosition, EndPosition);
        }

        public Vector3 Tangent()
        {
            return EndPosition - StartPosition;
        }

        // Hide Tangent(float t), because a line wouldn't use t 
        new private Vector3 Tangent(float t)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}