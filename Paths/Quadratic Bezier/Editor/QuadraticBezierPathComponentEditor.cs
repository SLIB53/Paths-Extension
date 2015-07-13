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
using Paths.Cache;
using PathsEditor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuadraticBezierPathComponent))]
public class QuadraticBezierPathComponentEditor : Editor
{
    private QuadraticBezierPathComponent targetScript;
    private QuadraticBezierPath qdrBezierPath;
    private ToolSettings settings;
    private bool warnedTransform = false;

    private void OnEnable()
    {
        targetScript = (QuadraticBezierPathComponent)target;
        qdrBezierPath = targetScript.Path;
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

        var newStartPosition = qdrBezierPath.LocalBezier.StartPosition;
        var newMidTangent = qdrBezierPath.LocalBezier.MidTangent;
        var newEndPosition = qdrBezierPath.LocalBezier.EndPosition;

        #region Path Inspector

        settings.PathFoldout
            = EditorGUILayout.Foldout(settings.PathFoldout, "Path");
        if (settings.PathFoldout)
        {
            EditorGUI.indentLevel++;

            //~ Knots
            newStartPosition = EditorGUILayout.Vector3Field("Start Position", qdrBezierPath.LocalBezier.StartPosition);
            newMidTangent = EditorGUILayout.Vector3Field("Mid Tangent", qdrBezierPath.LocalBezier.MidTangent);
            newEndPosition = EditorGUILayout.Vector3Field("End Position", qdrBezierPath.LocalBezier.EndPosition);

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
            qdrBezierPath.LocalSpaceTransform
                = (Transform)EditorGUILayout.ObjectField(
                    "Local Space Transform",
                    qdrBezierPath.LocalSpaceTransform,
                    typeof(Transform),
                    allowSceneObjects: true
                    );
        }

        #endregion Advanced

        #region Undo

        if (newStartPosition != qdrBezierPath.LocalBezier.StartPosition
            || newMidTangent != qdrBezierPath.LocalBezier.MidTangent
            || newEndPosition != qdrBezierPath.LocalBezier.EndPosition)
        {
            Undo.RecordObject(target, "Edit Quadratic Bezier");
            qdrBezierPath.LocalBezier.StartPosition = newStartPosition;
            qdrBezierPath.LocalBezier.MidTangent = newMidTangent;
            qdrBezierPath.LocalBezier.EndPosition = newEndPosition;
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
        qdrBezierPath = targetScript.Path;
        settings = targetScript.EditorOnlyToolSettings;

        //~ Fixes deleted transform issues
        if (qdrBezierPath.LocalSpaceTransform == null)
        {
            qdrBezierPath.LocalSpaceTransform = null;
            safe = false;
        }

        //if(!safe)
        //    SceneView.currentDrawingSceneView.ShowNotification(new GUIContent(PathEditorUtility.EditorUnavailable));

        return safe;
    }

    private void MoveToolSceneGUI()
    {
        // Undo world transformation before writing values to Path's bezier curve
        Func<Vector3, Vector3> handleToLocal = (handlePoint) =>
        { return PathEditorUtility.HandleToLocalPosition(handlePoint, qdrBezierPath.LocalSpaceTransform); };

        Vector3 newLocalStartPosition = handleToLocal(qdrBezierPath.StartPosition);
        Vector3 newLocalMidTangent = handleToLocal(qdrBezierPath.MidTangent);
        Vector3 newLocalEndPosition = handleToLocal(qdrBezierPath.EndPosition);

        // Write values to Path's bezier curve
        if (newLocalStartPosition != qdrBezierPath.LocalBezier.StartPosition
            || newLocalMidTangent != qdrBezierPath.LocalBezier.MidTangent
            || newLocalEndPosition != qdrBezierPath.LocalBezier.EndPosition)
        {
            Undo.RecordObject(target, "Edit Quadratic Bezier");

            //~ Before storing new values, prevent lossyScale from deleting values
            Vector3 finalStartPosition = qdrBezierPath.LocalBezier.StartPosition;
            Vector3 finalMidTangent = qdrBezierPath.LocalBezier.MidTangent;
            Vector3 finalEndPosition = qdrBezierPath.LocalBezier.EndPosition;
            bool guardX = Mathf.Approximately(targetScript.transform.lossyScale.x, 0.0f);
            bool guardY = Mathf.Approximately(targetScript.transform.lossyScale.y, 0.0f);
            bool guardZ = Mathf.Approximately(targetScript.transform.lossyScale.z, 0.0f);

            if (!guardX)
            {
                finalStartPosition.x = newLocalStartPosition.x;
                finalMidTangent.x = newLocalMidTangent.x;
                finalEndPosition.x = newLocalEndPosition.x;
            }
            if (!guardY)
            {
                finalStartPosition.y = newLocalStartPosition.y;
                finalMidTangent.y = newLocalMidTangent.y;
                finalEndPosition.y = newLocalEndPosition.y;
            }
            if (!guardZ)
            {
                finalStartPosition.z = newLocalStartPosition.z;
                finalMidTangent.z = newLocalMidTangent.z;
                finalEndPosition.z = newLocalEndPosition.z;
            }

            qdrBezierPath.LocalBezier.StartPosition = finalStartPosition;
            qdrBezierPath.LocalBezier.MidTangent = finalMidTangent;
            qdrBezierPath.LocalBezier.EndPosition = finalEndPosition;

            EditorUtility.SetDirty(target);
        }
    }

    private void Draw()
    {
        QuadraticBezier worldBezier = qdrBezierPath.WorldSpaceBezier;
        var cPointCache = new VertexCache(worldBezier, QuadraticBezier.GoodNumMidPoints).Values; //PathUtility.BuildCache(worldBezier, DynamicBezier.GoodNumMidPoints);

        // draw bezier
        PathEditorUtility.DrawSplineInScene(cPointCache);

        // draw direction cone cap
        if (!settings.HideDirectionCones
                && targetScript.transform.lossyScale != Vector3.zero)  // also hide cones if virtually a dot
        {
            float startConeSize = PathEditorUtility.Nice3DHandleSize(worldBezier.Evaluate(0f));
            float endConeSize = PathEditorUtility.Nice3DHandleSize(worldBezier.Evaluate(1f));

            Handles.color = Color.yellow;
            Handles.ConeCap(0, worldBezier.Evaluate(0f), Quaternion.LookRotation(worldBezier.Evaluate(0.01f) - worldBezier.Evaluate(0f)), startConeSize);
            Handles.color = Color.magenta;
            Handles.ConeCap(0, worldBezier.Evaluate(1f), Quaternion.LookRotation(worldBezier.Evaluate(1f) - worldBezier.Evaluate(1f - 0.01f)), endConeSize);
        }

        // draw tangent lines
        if (settings.DrawTangentLines)
        {
            Handles.color = Color.cyan;
            Handles.DrawDottedLine(worldBezier.StartPosition, worldBezier.MidTangent, 7.5f);
            Handles.DrawDottedLine(worldBezier.MidTangent, worldBezier.EndPosition, 7.5f);
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
                settings.TestInterpolate
                    = GUILayout.Toggle(settings.TestInterpolate, "Test Interpolate", GUILayout.ExpandWidth(false));
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
                    Undo.RecordObject(target, "Reverse Quadratic Bezier");

                    Vector3 temp = qdrBezierPath.StartPosition;
                    qdrBezierPath.StartPosition = qdrBezierPath.EndPosition;
                    qdrBezierPath.EndPosition = temp;

                    EditorUtility.SetDirty(target);
                }
                if (GUILayout.Button("Apply Transform"))
                {
                    string title = "Are you sure you want to Apply Transform?";
                    string message = "Applying transform will reset \"" + qdrBezierPath.LocalSpaceTransform.name + "\" transform values. \n\nThis cannot be undone.";
                    if (EditorUtility.DisplayDialog(title, message, ok: "Apply Transform", cancel: "Cancel"))
                    {
                        qdrBezierPath.LocalBezier = qdrBezierPath.WorldSpaceBezier;
                        qdrBezierPath.LocalSpaceTransform.position = Vector3.zero;
                        qdrBezierPath.LocalSpaceTransform.rotation = Quaternion.identity;
                        qdrBezierPath.LocalSpaceTransform.localScale = Vector3.one;

                        EditorUtility.SetDirty(target);
                    }
                }
            }
            GUILayout.EndArea();
        }
        Handles.EndGUI();
    }

    [MenuItem(PathEditorUtility.Menu_Create + "Quadratic Bezier")]
    public static void CreatePath()
    {
        var newPathObject = new GameObject("Quadratic Bezier Path", typeof(QuadraticBezierPathComponent));
        Selection.activeGameObject = newPathObject;
    }

    [MenuItem(PathEditorUtility.Menu_Components + "Quadratic Bezier Path")]
    public static void AddComponent()
    {
        var activeObj = Selection.activeGameObject;
        if (activeObj == null)
            Debug.Log("Can't add component. No game objects are selected.");
        else
            activeObj.AddComponent<QuadraticBezierPathComponent>();
    }
}