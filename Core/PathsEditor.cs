/*
 * This file is needed for the editor to function correctly. It is not for build/runtime use.
 * Note: moving this to an 'Editor' folder will make the project fail to compile.
 */

using Paths;

namespace PathsEditor
{
    public struct ToolData
    {
        public float T;
        public string CustomT;
    }

    [System.Serializable]
    public class ToolSettings
    {
        public bool TestInterpolate;
        public bool DrawBezierDebugLines;
        public bool HideTangentLines;
        public bool HideHandles;
        public bool HideDirectionCones;
        public bool PathFoldout;
        public bool SettingsFoldout;
        public bool AdvancedFoldout;
        public ToolData EditorData;

        public ToolSettings(
            bool testInterpolate = false,
            bool drawDebugLines = false,
            bool hideTangentLines = false,
            bool hideHandles = false,
            bool hideDirectionCones = false,
            bool pathFoldout = true,
            bool settingsFoldout = false,
            bool advancedFoldout = false
            )
        {
            TestInterpolate = testInterpolate;
            DrawBezierDebugLines = drawDebugLines;
            HideTangentLines = hideTangentLines;
            HideHandles = hideHandles;
            HideDirectionCones = hideDirectionCones;
            PathFoldout = pathFoldout;
            SettingsFoldout = settingsFoldout;
            AdvancedFoldout = advancedFoldout;
            EditorData = new ToolData();
            EditorData.CustomT = string.Empty;
        }

        public void Update(Path3D path)
        {
            if (DrawBezierDebugLines)
                PathUtility.DebugDrawSpline(path, 32);
        }
    }
}