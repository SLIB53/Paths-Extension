using System;
using Paths;
using UnityEngine;

public class PathsDemo_SphereLattice : MonoBehaviour
{
    public GameObject XPath;
    public GameObject YPath;
    public GameObject ZPath;
    private Path3D x;
    private Path3D z;
    public float Divisor = 0.25f;
    public float RelativeScale = 2.0f;
    public bool twoDimensional;
    public Material LatticeMat;

    private int count;
    public float TangentMod;

    private void Start()
    {
        for (float i = 0; i <= 1.0f; i += Divisor)
            for (float j = 0; j <= 1.0f; j += Divisor)
                for (float k = 0; k <= 1.0f; k += Divisor)
                {
                    // Create sphere 
                    GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                    // Scale sphere to quarter size of incrementation (quarter for decent margins) 
                    g.transform.localScale = Vector3.one * Divisor * RelativeScale;

                    // Set up space distortion component and appropriate value 
                    PathsDemo_SpaceDistortion gSpaceDistortion = g.AddComponent<PathsDemo_SpaceDistortion>();
                    gSpaceDistortion.XPath = this.XPath;
                    gSpaceDistortion.YPath = this.YPath;
                    gSpaceDistortion.ZPath = this.ZPath;
                    gSpaceDistortion.t = i;
                    gSpaceDistortion.u = j;
                    gSpaceDistortion.v = k;

                    // Attach material 
                    g.GetComponent<Renderer>().material = LatticeMat;

                    // Keep track of number of objects 
                    count++;

                    // don't generate along Z axis 
                    if (twoDimensional)
                        break;
                }

        Debug.Log("Number of spheres: " + count);
        //Destroy(this.gameObject);

        x = PathUtility.GetPath3D(XPath);
        z = PathUtility.GetPath3D(ZPath);

        GameObject.Find("Title").GetComponent<GUIText>().text += " " + count + " Objects";
        GameObject.Find("PathsDemo_Screen").GetComponent<PathsDemo_DemoScreen>().EVALCOUNT = count * 3;
    }

    private void OnGUI()
    {
        TangentMod = GUI.HorizontalSlider(new Rect(30, 150, 400, 50), TangentMod, -5f, 5f);
        //GUI.Label(new Rect(440, 250, 400, 50), TangentMod.ToString("0.00"));

        var xAsQdr = x as QuadraticBezierPath;
        var zAsQdr = z as QuadraticBezierPath;

        Action<QuadraticBezierPath> modifyMidTangent = (axisAsQdr) =>
        {
            if (axisAsQdr != null)
                axisAsQdr.MidTangent = Vector3.Lerp(axisAsQdr.StartPosition, axisAsQdr.EndPosition, 0.5f) + (Vector3.up * TangentMod);
        };
        modifyMidTangent(xAsQdr);
        modifyMidTangent(zAsQdr);
    }
}