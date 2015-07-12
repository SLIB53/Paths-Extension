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
using Paths;
using PathsEditor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CubicBezierPathComponent))]
public class CubicBezierPathComponentEditor : Editor
{
    private CubicBezierPathComponent targetScript;
    private CubicBezierPath cubicBezierPath;
    private ToolSettings settings;
    private bool warnedTransform = false;

    private void OnEnable()
    {
        targetScript = (CubicBezierPathComponent)target;
        cubicBezierPath = targetScript.Path;
        settings = targetScript.EditorOnlyToolSettings;

        if (!Safety())
        {
            SceneView.currentDrawingSceneView.ShowNotification(new GUIContent(PathEditorUtility.EditorUnavailable));
            return;
        }
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        Safety();

        var newStartPosition = cubicBezierPath.LocalBezier.StartPosition;
        var newStartTangent = cubicBezierPath.LocalBezier.StartTangent;
        var newEndPosition = cubicBezierPath.LocalBezier.EndPosition;
        var newEndTangent = cubicBezierPath.LocalBezier.EndTangent;

        #region Path Inspector

        settings.PathFoldout =
            EditorGUILayout.Foldout(settings.PathFoldout, "Path");
        if (settings.PathFoldout)
        {
            EditorGUI.indentLevel++;

            //~ Knots
            //cubicBezierPath.LocalBezier.StartPosition = EditorGUILayout.Vector3Field("Start Position", cubicBezierPath.LocalBezier.StartPosition);
            //cubicBezierPath.LocalBezier.StartTangent = EditorGUILayout.Vector3Field("Start Tangent", cubicBezierPath.LocalBezier.StartTangent);
            //cubicBezierPath.LocalBezier.EndPosition = EditorGUILayout.Vector3Field("End Position", cubicBezierPath.LocalBezier.EndPosition);
            //cubicBezierPath.LocalBezier.EndTangent = EditorGUILayout.Vector3Field("End Tangent", cubicBezierPath.LocalBezier.EndTangent);

            newStartPosition = EditorGUILayout.Vector3Field("Start Position", cubicBezierPath.LocalBezier.StartPosition);
            newStartTangent = EditorGUILayout.Vector3Field("Start Tangent", cubicBezierPath.LocalBezier.StartTangent);
            newEndPosition = EditorGUILayout.Vector3Field("End Position", cubicBezierPath.LocalBezier.EndPosition);
            newEndTangent = EditorGUILayout.Vector3Field("End Tangent", cubicBezierPath.LocalBezier.EndTangent);

            EditorGUI.indentLevel--;
        }

        #endregion Path Inspector

        #region Tool Settings Inspector

        PathEditorUtility.Inspector(settings, 1);

        #endregion Tool Settings Inspector

        #region Advanced

        settings.AdvancedFoldout = EditorGUILayout.Foldout(settings.AdvancedFoldout, "Advanced");
        if (settings.AdvancedFoldout)
        {
            //~ LocalSpaceTransform field
            cubicBezierPath.LocalSpaceTransform =
                (Transform)EditorGUILayout.ObjectField(
                    "Local Space Transform",
                    cubicBezierPath.LocalSpaceTransform,
                    typeof(Transform),
                    allowSceneObjects: true
                    );
        }

        #endregion Advanced

        #region Undo

        if (newStartPosition != cubicBezierPath.LocalBezier.StartPosition
                || newStartTangent != cubicBezierPath.LocalBezier.StartTangent
                || newEndPosition != cubicBezierPath.LocalBezier.EndPosition
                || newEndTangent != cubicBezierPath.LocalBezier.EndTangent)
        {
            Undo.RecordObject(target, "Edit Cubic Bezier");
            cubicBezierPath.LocalBezier.StartPosition = newStartPosition;
            cubicBezierPath.LocalBezier.StartTangent = newStartTangent;
            cubicBezierPath.LocalBezier.EndPosition = newEndPosition;
            cubicBezierPath.LocalBezier.EndTangent = newEndTangent;
            EditorUtility.SetDirty(target);
        }

        #endregion Undo

        SceneView.RepaintAll();
    }

    private void OnSceneGUI()
    {
        if (!Safety())
        {
            if (warnedTransform == false)
            {
                Debug.Log("[" + targetScript.name + "]\t" + PathEditorUtility.EditorUnavailable);
                warnedTransform = true;
            }
            return;
        }

        if (settings.HideHandles || Tools.current == Tool.View)
            goto Draw;

        if (Tools.current == Tool.Move)
            MoveToolSceneGUI();
Draw:
        Draw();
    }

    private bool Safety()
    {
        bool safe = true;

        //~ Fixes first run issues
        if (settings == null)
        {
            targetScript.Reset();
        }
        cubicBezierPath = targetScript.Path;
        settings = targetScript.EditorOnlyToolSettings;

        //~ Fixes deleted transform issues
        if (cubicBezierPath.LocalSpaceTransform == null)
        {
            cubicBezierPath.LocalSpaceTransform = null;
            safe = false;
        }

        return safe;
    }

    private void MoveToolSceneGUI()
    {
        // Undo world transformation before writing values to Path's bezier curve 
        Func<Vector3, Vector3> handleToLocal = (handlePoint) =>
        { return PathEditorUtility.HandleToLocalPosition(handlePoint, cubicBezierPath.LocalSpaceTransform); };

        Vector3 newLocalStartPosition = handleToLocal(cubicBezierPath.StartPosition);
        Vector3 newLocalStartTangent = handleToLocal(cubicBezierPath.StartTangent);
        Vector3 newLocalEndPosition = handleToLocal(cubicBezierPath.EndPosition);
        Vector3 newLocalEndTangent = handleToLocal(cubicBezierPath.EndTangent);

        // Write values to Path's bezier curve 
        if (newLocalStartPosition != cubicBezierPath.LocalBezier.StartPosition
            || newLocalStartTangent != cubicBezierPath.LocalBezier.StartTangent
            || newLocalEndPosition != cubicBezierPath.LocalBezier.EndPosition
            || newLocalEndTangent != cubicBezierPath.LocalBezier.EndTangent)
        {
            Undo.RecordObject(target, "Edit Cubic Bezier");

            //~ Before storing new values, prevent lossyScale from deleting values
            Vector3 finalStartPosition = cubicBezierPath.LocalBezier.StartPosition;
            Vector3 finalStartTangent = cubicBezierPath.LocalBezier.StartTangent;
            Vector3 finalEndPosition = cubicBezierPath.LocalBezier.EndPosition;
            Vector3 finalEndTangent = cubicBezierPath.LocalBezier.EndTangent;
            bool guardX = Mathf.Approximately(targetScript.transform.lossyScale.x, 0.0f);
            bool guardY = Mathf.Approximately(targetScript.transform.lossyScale.y, 0.0f);
            bool guardZ = Mathf.Approximately(targetScript.transform.lossyScale.z, 0.0f);

            if (!guardX)
            {
                finalStartPosition.x = newLocalStartPosition.x;
                finalStartTangent.x = newLocalStartTangent.x;
                finalEndPosition.x = newLocalEndPosition.x;
                finalEndTangent.x = newLocalEndTangent.x;
            }
            if (!guardY)
            {
                finalStartPosition.y = newLocalStartPosition.y;
                finalStartTangent.y = newLocalStartTangent.y;
                finalEndPosition.y = newLocalEndPosition.y;
                finalEndTangent.y = newLocalEndTangent.y;
            }
            if (!guardZ)
            {
                finalStartPosition.z = newLocalStartPosition.z;
                finalStartTangent.z = newLocalStartTangent.z;
                finalEndPosition.z = newLocalEndPosition.z;
                finalEndTangent.z = newLocalEndTangent.z;
            }

            cubicBezierPath.LocalBezier.StartPosition = finalStartPosition;
            cubicBezierPath.LocalBezier.StartTangent = finalStartTangent;
            cubicBezierPath.LocalBezier.EndPosition = finalEndPosition;
            cubicBezierPath.LocalBezier.EndTangent = finalEndTangent;

            EditorUtility.SetDirty(target);
        }
    }

    private void Draw()
    {
        CubicBezier worldBezier = cubicBezierPath.WorldSpaceBezier;

        // draw bezier 
        Vector3[] cPointCache = PathUtility.BuildCache(worldBezier, CubicBezier.GoodNumMidPoints);
        /*Handles.color = Color.yellow;
        Handles.DrawAAPolyLine(cPointCache);
        */
        for (int i = 0; i < cPointCache.Length - 1; i++)
        {
            Handles.color = Color.Lerp(Color.yellow, Color.magenta, (float)i / cPointCache.Length);
            var lineSegment = new Vector3[2];
            lineSegment[0] = cPointCache[i];
            lineSegment[1] = cPointCache[i + 1];
            Handles.DrawAAPolyLine(lineSegment);
        }

        // draw direction cone cap 
        if (!settings.HideDirectionCones && targetScript.transform.lossyScale != Vector3.zero) //check for zero vector, since LookRotation logs messages
        {
            float startConeSize = PathEditorUtility.Nice3DHandleSize(worldBezier.Evaluate(0f));
            float endConeSize = PathEditorUtility.Nice3DHandleSize(worldBezier.Evaluate(1f));

            Handles.color = Color.yellow;
            Handles.ConeCap(0, worldBezier.Evaluate(0f), Quaternion.LookRotation(worldBezier.Tangent(0f)), startConeSize);
            Handles.color = Color.magenta;
            Handles.ConeCap(0, worldBezier.Evaluate(1f), Quaternion.LookRotation(worldBezier.Tangent(1f)), endConeSize);
        }

        // draw tangent lines 
        if (settings.DrawTangentLines)
        {
            Handles.color = Color.cyan;
            Handles.DrawDottedLine(worldBezier.StartPosition, worldBezier.StartTangent, 7.5f);
            Handles.DrawDottedLine(worldBezier.EndPosition, worldBezier.EndTangent, 7.5f);
        }

        // test t 
        if (settings.TestInterpolate)
            PathEditorUtility.DrawTestInterpolate(worldBezier, settings.EditorData.T);

        // draw GUI 
        InterpolateSceneGUI();
        ToolShelf();
    }

    private void InterpolateSceneGUI()
    {
        Handles.BeginGUI(); //Note: GUILayout.BeginArea() deprecates Handles, however it behaves incorrectly (debug lines offset)
        {
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            {
                GUILayout.Space(20.0f);
                settings.TestInterpolate = GUILayout.Toggle(settings.TestInterpolate, "Test Interpolate", GUILayout.ExpandWidth(false));
                if (settings.TestInterpolate)
                {
                    settings.EditorData.T = GUILayout.HorizontalSlider(settings.EditorData.T, 0f, 1.0f, GUILayout.Width(100.0f));
                    GUILayout.Label(settings.EditorData.T.ToString(), GUILayout.Width(75.0f));
                    GUILayout.Label("Custom: ", GUILayout.Width(60.0f));
                    settings.EditorData.CustomT = GUILayout.TextField(settings.EditorData.CustomT, GUILayout.Width(100.0f));
                    if (Event.current.keyCode == KeyCode.Return)
                    {
                        float tempT;
                        if (System.Single.TryParse(settings.EditorData.CustomT, out tempT))
                            settings.EditorData.T = tempT;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        Handles.EndGUI();
    }

    private void ToolShelf()
    {
        Handles.BeginGUI();
        {
            GUILayout.BeginArea(new Rect(Screen.width - 150f, Screen.height - 100f, 150f - 20f, 300f - 20f));
            {
                if (GUILayout.Button("Reverse Direction"))
                {
                    Undo.RecordObject(target, "Reverse Cubic Bezier");

                    // swap positions 
                    Vector3 tempPos = cubicBezierPath.StartPosition;
                    cubicBezierPath.StartPosition = cubicBezierPath.EndPosition;
                    cubicBezierPath.EndPosition = tempPos;

                    //swap tangents
                    Vector3 tempTngt = cubicBezierPath.StartTangent;
                    cubicBezierPath.StartTangent = cubicBezierPath.EndTangent;
                    cubicBezierPath.EndTangent = tempTngt;

                    EditorUtility.SetDirty(target);
                }
                if (GUILayout.Button("Apply Transform"))
                {
                    string title = "Are you sure you want to Apply Transform?";
                    string message = "Applying transform will reset \"" + cubicBezierPath.LocalSpaceTransform.name + "\" transform values. \n\nThis cannot be undone.";
                    if (EditorUtility.DisplayDialog(title, message, ok: "Apply Transform", cancel: "Cancel"))
                    {
                        cubicBezierPath.LocalBezier = cubicBezierPath.WorldSpaceBezier;
                        cubicBezierPath.LocalSpaceTransform.position = Vector3.zero;
                        cubicBezierPath.LocalSpaceTransform.rotation = Quaternion.identity;
                        cubicBezierPath.LocalSpaceTransform.localScale = Vector3.one;

                        EditorUtility.SetDirty(target);
                    }
                }
            }
            GUILayout.EndArea();
        }
        Handles.EndGUI();
    }

    [MenuItem(PathEditorUtility.Menu_Create + "Cubic Bezier")]
    public static void CreatePath()
    {
        var newPathObject = new GameObject("Cubic Bezier Path", typeof(CubicBezierPathComponent));
        Selection.activeGameObject = newPathObject;
    }

    [MenuItem(PathEditorUtility.Menu_Components + "Cubic Bezier Path")]
    public static void AddComponent()
    {
        var activeObj = Selection.activeGameObject;
        if (activeObj == null)
            Debug.Log("Can't add component. No game objects are selected.");
        else
            activeObj.AddComponent<CubicBezierPathComponent>();
    }
}