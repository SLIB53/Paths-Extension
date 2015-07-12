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

using Paths;
using PathsEditor;
using UnityEngine;

public class DynamicBezierPathComponent : MonoBehaviour
{
    [SerializeField]
    public DynamicBezierPath Path;

#if UNITY_EDITOR

    public ToolSettings EditorOnlyToolSettings = new ToolSettings();

    private void Update()
    {
        EditorOnlyToolSettings.Update(Path);
    }

    public void Reset()
    {
        EditorOnlyToolSettings = new ToolSettings();

        var dynamicBezier = new DynamicBezier(
            new Vector3(0.5f, 0f, 0f),
            new Vector3(0f, 0.5f, 0.5f),
            new Vector3(0.5f, 1.0f, 0.5f),
            new Vector3(1.5f, 1f, 1f),
            new Vector3(1f, 1.5f, 1.5f)
            );
        Path = new DynamicBezierPath(transform, dynamicBezier);
    }

#endif

    //public GameObject Target;
    //public DynamicBezierPath test;
    //[Range(0, 1)]
    //public float t;

    //void Start()
    //{
    //}

    //void Update()
    //{
    //    Target.transform.position = test.Evaluate(t);

    //    var cache = PathUtility.BuildCache(test, DynamicBezier.GoodNumMidPoints);
    //    for (int i = 0; i < cache.Length - 1; i++)
    //        Debug.DrawLine(cache[i], cache[i + 1]);
    //}
}