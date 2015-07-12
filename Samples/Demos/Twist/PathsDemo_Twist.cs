using Paths;
using UnityEngine;

public class PathsDemo_Twist : MonoBehaviour
{
    public GameObject CorePathObj;
    public GameObject TwistPathObj;
    private Path3D core;
    private Path3D twist;

    [Range(0f, 1f)]
    [SerializeField]
    private float t;

    private void Start()
    {
        core = PathUtility.GetPath3D(CorePathObj);
        twist = PathUtility.GetPath3D(TwistPathObj);
    }

    private void Update()
    {
        this.transform.position = core.Evaluate(t);
        this.transform.rotation = PathUtility.Twist(core, t, twist, t);
    }

    private void OnGUI()
    {
        float top = 250;
        t = GUI.HorizontalSlider(new Rect(30, top, 400, 50), t, 0f, 1f);
        GUI.Label(new Rect(440, top, 400, 50), t.ToString("0.00"));
    }
}