using UnityEngine;

public class PathsDemo_DemoScreen : MonoBehaviour
{
    public Summary DemoSummary;
    public GUIText Title;
    public GUIText Instructions;
    private const float margin = 30.0f;

    public enum Summary { Twist, Bezier_Performance }

    public int EVALCOUNT;

    private void OnGUI()
    {
        //~ Title
        Title.pixelOffset = new Vector2(-(Screen.width / 2.0f) + margin, (Screen.height / 2.0f) - margin);
        Title.anchor = TextAnchor.UpperLeft;
        Title.alignment = TextAlignment.Left;

        //~ Summary
        string summaryText;
        switch (DemoSummary)
        {
            case Summary.Twist:
                summaryText = "Twist takes in two paths and a floating point interpolant for each path (the slider gives the value for both parameters in this demo). Twist returns a rotation that tilts such that the X axis points to the second path. The Z axis will always point down the tangent of the first path, so the tilted cube's X axis does not always point exactly at the second path (magenta path).\n\nYou can think of the second path as a rotation guide.\n\nThis is useful for designing rotations along a path, such as when controlling a camera motion. Because it produces predictable rotations, it can give better results than Quaternion.LookRotation, or Transform.LookAt. It is also worth noting that the Y axis won't flip unpredictably because the rotation is based on two controlled vectors (first path tangent & vector pointing to guide path).";
                break;

            case Summary.Bezier_Performance:
                summaryText = "In this demo, a lattice of spheres get their positions by evaluating three basis bezier curves and then adding the results. That means per frame, each object does three Bezier evaluations (that is " + EVALCOUNT + " evaluations per frame). Move the slider up and down to change the MidTangent knot of the X and Z basis beziers.";
                break;

            default:
                summaryText = string.Empty;
                break;
        }
        GUI.Label(new Rect(margin, 80f, Screen.width * 0.75f, Screen.height), summaryText);

        //~ Instructions
        Instructions.pixelOffset = new Vector2(0f, -(Screen.height / 2.0f) + margin);
    }
}