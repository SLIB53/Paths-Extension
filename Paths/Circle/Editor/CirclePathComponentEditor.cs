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
using Paths.Cache;
using PathsEditor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CirclePathComponent))]
public class CirclePathComponentEditor : Editor
{
    private CirclePathComponent targetScript;
    private CirclePath circlePath;
    private ToolSettings settings;
    private bool warnedTransform = false;

    private void OnEnable()
    {
        targetScript = (CirclePathComponent)target;
        circlePath = targetScript.Path;
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

        var newCenter = circlePath.LocalCircle.Center;
        var newRadius = circlePath.LocalCircle.Radius;
        var newMaxAngle = circlePath.MaxAngle;

        #region Path Inspector

        settings.PathFoldout
            = EditorGUILayout.Foldout(settings.PathFoldout, "Path");
        if (settings.PathFoldout)
        {
            EditorGUI.indentLevel++;

            //~ Fields
            newCenter = EditorGUILayout.Vector3Field("Local Center", circlePath.LocalCircle.Center);
            newRadius = EditorGUILayout.FloatField("Radius", circlePath.LocalCircle.Radius);
            newMaxAngle = EditorGUILayout.FloatField("Max Angle (Degrees)", circlePath.MaxAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;

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
            circlePath.LocalSpaceTransform
                = (Transform)EditorGUILayout.ObjectField(
                    "Local Space Transform",
                    circlePath.LocalSpaceTransform,
                    typeof(Transform),
                    allowSceneObjects: true
                    );
        }

        #endregion Advanced

        #region Undo

        if (newCenter != circlePath.LocalCircle.Center
            || newRadius != circlePath.LocalCircle.Radius
            || newMaxAngle != circlePath.MaxAngle)
        {
            Undo.RecordObject(target, "Edit Circle");
            circlePath.LocalCircle.Center = newCenter;
            circlePath.LocalCircle.Radius = newRadius;
            circlePath.MaxAngle = newMaxAngle;
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

    public bool Safety()
    {
        bool safe = true;

        //~ Fixes first run issues
        if (settings == null)
        {
            targetScript.Reset();
            safe = false;
        }
        circlePath = targetScript.Path;
        settings = targetScript.EditorOnlyToolSettings;

        //~ Fixes deleted transform issues
        if (circlePath.LocalSpaceTransform == null)
        {
            circlePath.LocalSpaceTransform = null;
            safe = false;
        }

        return safe;
    }

    private void MoveToolSceneGUI()
    {
        //~ Center handle
        circlePath.Center = Handles.PositionHandle(circlePath.Center, Tools.handleRotation);
    }

    private void Draw()
    {
        Vector3 startPoint = circlePath.Evaluate(0f);
        Vector3 endPoint = circlePath.Evaluate(1f);
        CirclePath reducedCirclePath = new CirclePath(
            circlePath.LocalSpaceTransform,
            new Circle(circlePath.LocalCircle.Center, circlePath.LocalCircle.Radius),
            circlePath.MaxAngle % Circle.TwoPI // remove loops
            );
        if (circlePath.MaxAngle >= Circle.TwoPI) // add back a single loop if it loops
            reducedCirclePath.MaxAngle += Circle.TwoPI;
        else if (Mathf.Abs(circlePath.MaxAngle) >= Circle.TwoPI) // if negative maxAngle and is still at least one loop
            reducedCirclePath.MaxAngle -= Circle.TwoPI;

        // draw circle
        var cPointCache = new VertexCache(reducedCirclePath, 128).Values; //PathUtility.BuildCache(reducedCirclePath, 128);
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
        if (!settings.HideDirectionCones
            && circlePath.LocalSpaceTransform.lossyScale != Vector3.zero
            && circlePath.LocalCircle.Radius != 0f
            && Mathf.Abs(reducedCirclePath.MaxAngle * Mathf.Rad2Deg) > 10.0f) // also hide cones if virtually a dot
        {
            float startConeSize = PathEditorUtility.Nice3DHandleSize(startPoint);
            float endConeSize = PathEditorUtility.Nice3DHandleSize(endPoint);

            Handles.color = Color.yellow;
            Handles.ConeCap(0, startPoint, Quaternion.LookRotation(circlePath.Tangent(0f)), startConeSize);
            Handles.color = Color.magenta;
            Handles.ConeCap(0, endPoint, Quaternion.LookRotation(circlePath.Tangent(1f)), endConeSize);
        }

        // test t
        if (settings.TestInterpolate)
            PathEditorUtility.DrawTestInterpolate(circlePath, settings.EditorData.T);

        #region Scene View GUI

        Handles.BeginGUI(); //Note: GUILayout.BeginArea() deprecates Handles, however it behaves incorrectly (debug lines offset)
        {
            GUILayout.BeginHorizontal(GUILayout.Width(Screen.width));
            {
                GUILayout.Space(20.0f);
                targetScript.EditorOnlyToolSettings.TestInterpolate
                    = GUILayout.Toggle(targetScript.EditorOnlyToolSettings.TestInterpolate, "Test Interpolate", GUILayout.ExpandWidth(false));
                if (targetScript.EditorOnlyToolSettings.TestInterpolate)
                {
                    settings.EditorData.T
                        = GUILayout.HorizontalSlider(settings.EditorData.T, 0f, 1.0f, GUILayout.Width(100.0f));
                    GUILayout.Label(settings.EditorData.T.ToString(), GUILayout.Width(75.0f));
                    GUILayout.Label("Custom: ", GUILayout.Width(60.0f));
                    settings.EditorData.CustomT
                        = GUILayout.TextField(settings.EditorData.CustomT, GUILayout.Width(100.0f));
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

        #endregion Scene View GUI

        ToolShelf();
    }

    private void ToolShelf()
    {
        Handles.BeginGUI();
        {
            GUILayout.BeginArea(new Rect(Screen.width - 150f, Screen.height - 120f, 150f - 20f, 300f - 20f));
            {
                if (GUILayout.Button("Reverse Direction"))
                {
                    string title = "Are you sure you want to Reverse Direction?";
                    string message = "'Reverse Direction' will change the local z rotation.\nThis cannot be undone.";
                    bool unevenScale;
                    {
                        Vector3 lossyScale = circlePath.LocalSpaceTransform.lossyScale;
                        unevenScale = !Mathf.Approximately(lossyScale.x, lossyScale.y) || !Mathf.Approximately(lossyScale.y, lossyScale.z);
                    }
                    if (unevenScale)
                        message += "\n\nWARNING:\nThe lossy scale is not a (n,n,n) vector. The operation will be unsuccessful.";

                    if (EditorUtility.DisplayDialog(title, message, ok: "Reverse Direction", cancel: "Cancel"))
                    {
                        circlePath.MaxAngle *= -1f;
                        Vector3 newEulerRotation = circlePath.LocalSpaceTransform.localEulerAngles;
                        newEulerRotation.z -= circlePath.MaxAngle * Mathf.Rad2Deg;

                        Vector3 centerWorldPosition = circlePath.LocalSpaceTransform.TransformPoint(circlePath.LocalCircle.Center);
                        circlePath.LocalSpaceTransform.localEulerAngles = newEulerRotation;
                        circlePath.LocalCircle.Center = circlePath.LocalSpaceTransform.InverseTransformPoint(centerWorldPosition);

                        string warning = "";
                        if (unevenScale)
                            warning += "Operation may be unsuccessful because the lossy scale is not (n,n,n).\n";
                        warning += "Use 'Reverse Direction' again to undo.";
                        SceneView.currentDrawingSceneView.ShowNotification(new GUIContent(warning));

                        EditorUtility.SetDirty(target);
                    }
                }
                if (GUILayout.Button("Apply Local Position"))
                {
                    string title = "Are you sure you want to Apply Local Position?";
                    string message = "'Apply Local Position' will set the Local Center to (0, 0, 0). This will change \"" + circlePath.LocalSpaceTransform.name + "\" transform values. \n\nThis cannot be undone.";
                    if (EditorUtility.DisplayDialog(title, message, ok: "Apply Local Position", cancel: "Cancel"))
                    {
                        var temp = circlePath.Center;
                        circlePath.LocalCircle.Center = Vector3.zero;
                        circlePath.LocalSpaceTransform.position = temp;

                        EditorUtility.SetDirty(target);
                    }
                }
                if (GUILayout.Button("Apply Position"))
                {
                    string title = "Are you sure you want to Apply Position?";
                    string message = "'Apply Position' will change \"" + circlePath.LocalSpaceTransform.name + "\" transform values. \n\nThis cannot be undone.";
                    if (EditorUtility.DisplayDialog(title, message, ok: "Apply Position", cancel: "Cancel"))
                    {
                        var temp = circlePath.Center;
                        circlePath.LocalSpaceTransform.position = Vector3.zero;
                        circlePath.Center = temp;

                        EditorUtility.SetDirty(target);
                    }
                }
            }
            GUILayout.EndArea();
        }
        Handles.EndGUI();
    }

    [MenuItem(PathEditorUtility.Menu_Create + "Circle")]
    public static void CreatePath()
    {
        var newPathObject = new GameObject("Circle Path", typeof(CirclePathComponent));
        Selection.activeGameObject = newPathObject;
    }

    [MenuItem(PathEditorUtility.Menu_Components + "Circle Path")]
    public static void AddComponent()
    {
        var activeObj = Selection.activeGameObject;
        if (activeObj == null)
            Debug.Log("Can't add component. No game objects are selected.");
        else
            activeObj.AddComponent<CirclePathComponent>();
    }
}