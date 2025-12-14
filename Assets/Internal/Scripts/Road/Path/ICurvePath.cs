using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Road.Paths
{
    public interface ICurvePath
    {
        float Length { get; }

        Vector3 GetPosition(float distance);

        Vector3 GetTangent(float distance);

        IReadOnlyList<Vector3> SampleByDistance(int segments);
    }
}