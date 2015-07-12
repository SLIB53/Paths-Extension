using Paths;
using UnityEngine;

public class PathsDemo_SpaceDistortion : MonoBehaviour
{
    public GameObject XPath;
    public GameObject YPath;
    public GameObject ZPath;
    private Path3D x;
    private Path3D y;
    private Path3D z;

    [Range(0, 1)]
    public float t;

    [Range(0, 1)]
    public float u;

    [Range(0, 1)]
    public float v;

    public float tangentMod;

    private void Start()
    {
        x = PathUtility.GetPath3D(XPath);
        y = PathUtility.GetPath3D(YPath);
        z = PathUtility.GetPath3D(ZPath);
    }

    private void Update()
    {
        // Early out, if any paths are null 
        if (!XPath || !YPath || !ZPath)
            return;

        Vector3 targetPos = x.Evaluate(t) + y.Evaluate(u) + z.Evaluate(v);

        transform.position = targetPos;
    }
}