using System;
using UnityEngine;

namespace Internal.Scripts.Input
{
    public interface ICameraInput
    {
        event Action<float>   OnChangeSize;
        event Action<Vector2> OnChangePosition;
    }
}
