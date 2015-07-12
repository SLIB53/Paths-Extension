using PathsEditor;
using UnityEditor;
using UnityEngine;

namespace Paths
{
    internal static class PathEditorUtility
    {
        public const string EditorUnavailable
            = "Path editor unavailable. No Local Space Transform found for this path.";

        public const string Menu_Root
            = "Paths/";

        public const string Menu_Create
            = Menu_Root + "Create/";

        public const string Menu_Components
            = Menu_Root + "Add Component/";

        public static float Nice3DHandleSize(Vector3 point)
        {
            Vector3 cameraPosition = SceneView.currentDrawingSceneView.camera.transform.position;
            return Vector3.Distance(cameraPosition, point) * (1f / 64f);
        }

        public static Vector3 HandleToLocalPosition(Vector3 handlePosition, Transform localSpace)
        {
            return localSpace.InverseTransformPoint(Handles.PositionHandle(handlePosition, Tools.handleRotation));
        }

        public static void Inspector(ToolSettings settings, int pathType = 0)
        {
            EditorGUILayout.Separator();
            settings.SettingsFoldout = EditorGUILayout.Foldout(settings.SettingsFoldout, "Editor Settings");

            if (settings.SettingsFoldout)
            {
                EditorGUI.indentLevel++;
                settings.TestInterpolate = EditorGUILayout.Toggle("Test Interpolate", settings.TestInterpolate);
                settings.DrawBezierDebugLines = EditorGUILayout.Toggle("Draw Debug Lines", settings.DrawBezierDebugLines);
                if (pathType == 1)
                    settings.DrawTangentLines = EditorGUILayout.Toggle("Draw Tangent Lines", settings.DrawTangentLines);
                settings.HideHandles = EditorGUILayout.Toggle("Hide Handles", settings.HideHandles);
                settings.HideDirectionCones = EditorGUILayout.Toggle("Hide Direction Cones", settings.HideDirectionCones);

                EditorGUI.indentLevel--;
            }
        }

        public static void DrawSplineInScene(Vector3[] pointCache)
        {
            for (int i = 0; i < pointCache.Length - 1; i++)
            {
                Handles.color = GetTColor((float)i / pointCache.Length);
                var lineSegment = new Vector3[2];
                lineSegment[0] = pointCache[i];
                lineSegment[1] = pointCache[i + 1];
                Handles.DrawAAPolyLine(lineSegment);
            }
        }

        public static void DrawTestInterpolate(ISpline worldSpline, float t)
        {
            Vector3 targetPoint = worldSpline.Evaluate(t);
            float sphereSize = PathEditorUtility.Nice3DHandleSize(targetPoint);

            Handles.color = GetTColor(t);
            Handles.SphereCap(0, targetPoint, Quaternion.identity, sphereSize);
        }

        public static Color GetTColor(float t)
        {
            return Color.Lerp(Color.yellow, Color.magenta, t);
        }

        public static bool VectorNotApproximately(Vector3 a, Vector3 b)
        {
            return !Mathf.Approximately(a.x, b.x) || !Mathf.Approximately(a.y, b.y) || !Mathf.Approximately(a.z, b.z);
        }
    }
}