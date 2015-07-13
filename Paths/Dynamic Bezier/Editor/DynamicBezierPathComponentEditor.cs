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
using System.Linq;
using Paths;
using Paths.Cache;
using PathsEditor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DynamicBezierPathComponent))]
public class DynamicBezierPathComponentEditor : Editor
{
    private DynamicBezierPathComponent targetScript;

    private DynamicBezierPath targetPath
    {
        get { return targetScript.Path; }
    }

    private ToolSettings settings;
    private bool warnedTransform = false;

    private void OnEnable()
    {
        targetScript = (DynamicBezierPathComponent)target;
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

        var newKnots = new List<Vector3>(targetPath.LocalBezier.Knots);

        #region Path Inspector

        settings.PathFoldout = EditorGUILayout.Foldout(settings.PathFoldout, "Path");
        if (settings.PathFoldout)
        {
            // EditorGUI.indentLevel++;

            if (newKnots.Count <= 0)
            {
                if (GUILayout.Button("+"))
                {
                    newKnots.Add(Vector3.right);
                    goto UndoHistory;
                }
            }

            for (int i = 0; i < newKnots.Count; i++)
            {
                newKnots[i] =
                    EditorGUILayout.Vector3Field(i.ToString(), newKnots[i]);

                EditorGUILayout.BeginHorizontal(EditorStyles.inspectorDefaultMargins);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(25f)))
                {
                    newKnots.RemoveAt(i);
                    goto UndoHistory;
                }

                if (GUILayout.Button("+", GUILayout.Width(25f)))
                {
                    Vector3 newKnot;
                    var currentKnot = newKnots[i];

                    if (i == newKnots.Count - 1) // last knot
                    {
                        if (newKnots.Count == 1) // this is the only knot
                            newKnot = newKnots[0] + Vector3.right;
                        else
                        {
                            var previousKnot = newKnots[i - 1];
                            newKnot = currentKnot + (currentKnot - previousKnot);
                        }
                    }
                    else
                    {
                        var nextKnot = newKnots[i + 1];
                        newKnot = Vector3.Lerp(currentKnot, nextKnot, 0.5f);
                    }

                    newKnots.Insert(i + 1, newKnot);
                    goto UndoHistory;
                }
                EditorGUILayout.EndHorizontal();
            }

            //EditorGUI.indentLevel--;
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
            targetPath.LocalSpaceTransform =
                (Transform)EditorGUILayout.ObjectField(
                    "Local Space Transform",
                    targetPath.LocalSpaceTransform,
                    typeof(Transform),
                    allowSceneObjects: true
                    );
        }

        #endregion Advanced

        #region Undo

UndoHistory:

        if (!targetPath.LocalBezier.Knots.SequenceEqual(newKnots) || targetPath.LocalBezier.Knots.Count != newKnots.Count)
        {
            Undo.RecordObject(target, "Edit Dynamic Bezier");
            targetPath.LocalBezier.Knots = newKnots;
            EditorUtility.SetDirty(target);
        }

        SceneView.RepaintAll();

        #endregion Undo
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

        if (targetPath.LocalBezier.Knots.Count <= 0)
            return;

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
        settings = targetScript.EditorOnlyToolSettings;

        //~ Fixes deleted transform issues
        if (targetPath.LocalSpaceTransform == null)
        {
            targetPath.LocalSpaceTransform = null;
            safe = false;
        }

        return safe;
    }

    private void MoveToolSceneGUI()
    {
        for (int i = 0; i < targetPath.LocalBezier.Knots.Count; i++)
        {
            // Undo world transformation before writing values to Path's bezier curve
            Vector3 newLocalPosition;
            {
                var handlePosition = targetPath.WorldSpaceBezier.Knots[i];
                newLocalPosition = PathEditorUtility.HandleToLocalPosition(
                                    handlePosition, targetPath.LocalSpaceTransform);
            }

            // Write value to Path's bezier curve
            //if (newLocalPosition != dynamicBezierPath.LocalBezier.Knots[i])
            if (PathEditorUtility.VectorNotApproximately(newLocalPosition, targetPath.LocalBezier.Knots[i]))
            {
                Undo.RecordObject(target, "Edit Dynamic Bezier");

                //~ Before storing new values, prevent lossyScale from deleting values
                var finalPosition = targetPath.LocalBezier.Knots[i];
                bool guardX = Mathf.Approximately(targetScript.transform.lossyScale.x, 0.0f);
                bool guardY = Mathf.Approximately(targetScript.transform.lossyScale.y, 0.0f);
                bool guardZ = Mathf.Approximately(targetScript.transform.lossyScale.z, 0.0f);

                if (!guardX)
                    finalPosition.x = newLocalPosition.x;
                if (!guardY)
                    finalPosition.y = newLocalPosition.y;
                if (!guardZ)
                    finalPosition.z = newLocalPosition.z;

                targetPath.LocalBezier.Knots[i] = finalPosition;

                EditorUtility.SetDirty(target);
            }
        }
    }

    private void Draw()
    {
        var worldBezier = targetPath.WorldSpaceBezier;
        var vertexCount = DynamicBezier.GoodNumMidPoints * worldBezier.Knots.Count;
        vertexCount /= 2;
        var cPointCache = new VertexCache(worldBezier, vertexCount).Values;//PathUtility.BuildCache(worldBezier, vertexCount);

        // draw bezier
        PathEditorUtility.DrawSplineInScene(cPointCache);

        // draw direction cone cap
        if (!settings.HideDirectionCones
            && targetScript.transform.lossyScale != Vector3.zero
            && targetPath.LocalBezier.Knots.Count > 1)  // also hide cones if virtually a dot
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
            for (int i = 0; i < worldBezier.Knots.Count - 1; i++)
                Handles.DrawDottedLine(worldBezier.Knots[i], worldBezier.Knots[i + 1], 7.5f);
        }

        // Draw knot labels
        var knotLabelStyle = new GUIStyle();
        knotLabelStyle.fontStyle = FontStyle.Bold;
        knotLabelStyle.fontSize = 17;
        knotLabelStyle.alignment = TextAnchor.UpperRight;
        knotLabelStyle.contentOffset = new Vector2(25f, -50f);

        for (int i = 0; i < worldBezier.Knots.Count; i++)
        {
            knotLabelStyle.normal.textColor = PathEditorUtility.GetTColor((float)i / worldBezier.Knots.Count);
            Handles.Label(worldBezier.Knots[i], i.ToString(), knotLabelStyle);
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
                    Undo.RecordObject(target, "Reverse Dynamic Bezier");

                    targetPath.LocalBezier.Knots.Reverse();

                    EditorUtility.SetDirty(target);
                }

                if (GUILayout.Button("Apply Transform"))
                {
                    string title = "Are you sure you want to Apply Transform?";
                    string message = "Applying transform will reset \"" + targetPath.LocalSpaceTransform.name + "\" transform values. \n\nThis cannot be undone.";
                    if (EditorUtility.DisplayDialog(title, message, ok: "Apply Transform", cancel: "Cancel"))
                    {
                        targetPath.LocalBezier = targetPath.WorldSpaceBezier;
                        targetPath.LocalSpaceTransform.position = Vector3.zero;
                        targetPath.LocalSpaceTransform.rotation = Quaternion.identity;
                        targetPath.LocalSpaceTransform.localScale = Vector3.one;

                        EditorUtility.SetDirty(target);
                    }
                }
            }
            GUILayout.EndArea();
        }
        Handles.EndGUI();
    }

    [MenuItem(PathEditorUtility.Menu_Create + "Dynamic Bezier")]
    public static void CreatePath()
    {
        var newPathObject = new GameObject("Dynamic Bezier Path", typeof(DynamicBezierPathComponent));
        Selection.activeGameObject = newPathObject;
    }

    [MenuItem(PathEditorUtility.Menu_Components + "Dynamic Bezier Path")]
    public static void AddComponent()
    {
        var activeObj = Selection.activeGameObject;
        if (activeObj == null)
            Debug.Log("Can't add component. No game objects are selected.");
        else
            activeObj.AddComponent<DynamicBezierPathComponent>();
    }
}