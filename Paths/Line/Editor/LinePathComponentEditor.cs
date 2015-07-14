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
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LinePathComponent))]
public class LinePathComponentEditor : Editor
{
    private LinePathComponent targetScript;
    private LinePath linePath;
    private ToolSettings settings;
    private bool warnedTransform = false;

    private void OnEnable()
    {
        targetScript = (LinePathComponent)target;
        linePath = targetScript.Path;
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

        var newStartPosition = linePath.StartPosition;
        var newEndPosition = linePath.EndPosition;

        #region Path Inspector

        settings.PathFoldout
            = EditorGUILayout.Foldout(settings.PathFoldout, "Path");
        if (settings.PathFoldout)
        {
            EditorGUI.indentLevel++;

            //~ Knots
            newStartPosition = EditorGUILayout.Vector3Field("Start Position", linePath.LocalStartPosition);
            newEndPosition = EditorGUILayout.Vector3Field("End Position", linePath.LocalEndPosition);

            EditorGUI.indentLevel--;
        }

        #endregion Path Inspector

        #region Tool Settings Inspector

        PathEditorUtility.Inspector(settings);

        #endregion Tool Settings Inspector

        #region Advanced

        settings.AdvancedFoldout = EditorGUILayout.Foldout(settings.AdvancedFoldout, "Advanced");
        if (settings.AdvancedFoldout)
        {
            //~ LocalSpaceTransform field
            linePath.LocalSpaceTransform
                = (Transform)EditorGUILayout.ObjectField(
                    "Local Space Transform",
                    linePath.LocalSpaceTransform,
                    typeof(Transform),
                    allowSceneObjects: true
                    );
        }

        #endregion Advanced

        #region Undo

        if (newStartPosition != linePath.StartPosition || newEndPosition != linePath.EndPosition)
        {
            Undo.RecordObject(target, "Edit Line");
            linePath.LocalStartPosition = newStartPosition;
            linePath.LocalEndPosition = newEndPosition;
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
        {
            Vector3 newLocalStartPosition = PathEditorUtility.HandleToLocalPosition(linePath.StartPosition, linePath.LocalSpaceTransform);
            Vector3 newLocalEndPosition = PathEditorUtility.HandleToLocalPosition(linePath.EndPosition, linePath.LocalSpaceTransform);

            bool guardX = Mathf.Approximately(linePath.LocalSpaceTransform.lossyScale.x, 0f);
            bool guardY = Mathf.Approximately(linePath.LocalSpaceTransform.lossyScale.y, 0f);
            bool guardZ = Mathf.Approximately(linePath.LocalSpaceTransform.lossyScale.z, 0f);

            if (newLocalStartPosition != linePath.LocalStartPosition
                || newLocalEndPosition != linePath.LocalEndPosition)
            {
                Undo.RecordObject(target, "Edit Line");
                if (!guardX)
                {
                    linePath.LocalStartPosition.x = newLocalStartPosition.x;
                    linePath.LocalEndPosition.x = newLocalEndPosition.x;
                }
                if (!guardY)
                {
                    linePath.LocalStartPosition.y = newLocalStartPosition.y;
                    linePath.LocalEndPosition.y = newLocalEndPosition.y;
                }
                if (!guardZ)
                {
                    linePath.LocalStartPosition.z = newLocalStartPosition.z;
                    linePath.LocalEndPosition.z = newLocalEndPosition.z;
                }
                EditorUtility.SetDirty(target);
            }
        }

Draw:
        Draw();
    }

    public bool Safety()
    {
        bool safe = true;

        //~ Fixes first run issues
        if (settings == null)
        {
            targetScript.Reset();
        }
        linePath = targetScript.Path;
        settings = targetScript.EditorOnlyToolSettings;

        //~ Fixes deleted transform issues
        if (linePath.LocalSpaceTransform == null)
        {
            linePath.LocalSpaceTransform = null;
            safe = false;
        }

        return safe;
    }

    private void Draw()
    {
        Vector3 startPoint = linePath.Evaluate(0f);
        Vector3 endPoint = linePath.Evaluate(1f);

        // draw line
        var cPointCache = new Vector3[32];

        for (int i = 0; i < cPointCache.Length; i++)
            cPointCache[i] = Vector3.Lerp(startPoint, endPoint, (float)i / (cPointCache.Length - 1));

        for (int i = 0; i < cPointCache.Length - 1; i++)
        {
            Handles.color = Color.Lerp(Color.yellow, Color.magenta, (float)i / cPointCache.Length);
            var lineSegment = new Vector3[2];
            lineSegment[0] = cPointCache[i];
            lineSegment[1] = cPointCache[i + 1];
            Handles.DrawAAPolyLine(lineSegment);
        }

        // draw direction cone cap
        if (!settings.HideDirectionCones
            && linePath.LocalSpaceTransform.lossyScale != Vector3.zero
            && linePath.StartPosition != linePath.EndPosition) // also hide cones if virtually a dot
        {
            float startConeSize = PathEditorUtility.Nice3DHandleSize(startPoint);
            float endConeSize = PathEditorUtility.Nice3DHandleSize(endPoint);

            Handles.color = Color.yellow;
            Handles.ConeCap(0, startPoint, Quaternion.LookRotation(linePath.Tangent()), startConeSize);
            Handles.color = Color.magenta;
            Handles.ConeCap(0, endPoint, Quaternion.LookRotation(linePath.Tangent()), endConeSize);
        }

        // test t
        Handles.color = PathEditorUtility.GetTColor(settings.EditorData.T);
        if (settings.TestInterpolate)
        {
            Vector3 targetPoint = linePath.Evaluate(settings.EditorData.T);
            float sphereSize = PathEditorUtility.Nice3DHandleSize(targetPoint);
            Handles.SphereCap(0, targetPoint, Quaternion.identity, sphereSize);
        }

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
                    Undo.RecordObject(target, "Reverse Line");

                    Vector3 temp = linePath.StartPosition;
                    linePath.StartPosition = linePath.EndPosition;
                    linePath.EndPosition = temp;

                    EditorUtility.SetDirty(target);
                }
                if (GUILayout.Button("Apply Transform"))
                {
                    string title = "Are you sure you want to Apply Transform?";
                    string message = "Applying transform will reset \"" + linePath.LocalSpaceTransform.name + "\" transform values. \n\nThis cannot be undone.";
                    if (EditorUtility.DisplayDialog(title, message, ok: "Apply Transform", cancel: "Cancel"))
                    {
                        linePath.LocalStartPosition = linePath.StartPosition;
                        linePath.LocalEndPosition = linePath.EndPosition;
                        linePath.LocalSpaceTransform.position = Vector3.zero;
                        linePath.LocalSpaceTransform.rotation = Quaternion.identity;
                        linePath.LocalSpaceTransform.localScale = Vector3.one;

                        EditorUtility.SetDirty(target);
                    }
                }
            }
            GUILayout.EndArea();
        }
        Handles.EndGUI();
    }

    [MenuItem(PathEditorUtility.Menu_Create + "Line")]
    public static void CreatePath()
    {
        var newPathObject = new GameObject("Line Path", typeof(LinePathComponent));
        Selection.activeGameObject = newPathObject;
    }

    [MenuItem(PathEditorUtility.Menu_Components + "Line Path")]
    public static void AddComponent()
    {
        var activeObj = Selection.activeGameObject;
        if (activeObj == null)
            Debug.Log("Can't add component. No game objects are selected.");
        else
            activeObj.AddComponent<LinePathComponent>();
    }
}