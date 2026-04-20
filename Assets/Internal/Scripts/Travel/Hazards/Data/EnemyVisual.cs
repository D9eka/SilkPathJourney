using System;
using UnityEngine;

namespace Internal.Scripts.Travel.Hazards.Data
{
    [Serializable]
    public struct EnemyVisual
    {
        [field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField] public Color Color { get; private set; }
    }
}
