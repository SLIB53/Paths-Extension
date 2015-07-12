using UnityEngine;

namespace Paths
{
    public interface ISpline
    {
        Vector3 Evaluate(float t);

        Vector3 Tangent(float t);
    }
}