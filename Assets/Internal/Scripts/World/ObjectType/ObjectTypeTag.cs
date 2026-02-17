using UnityEngine;

namespace Internal.Scripts.World.ObjectType
{
    [DisallowMultipleComponent]
    public class ObjectTypeTag : MonoBehaviour
    {
        [field: SerializeField] public string ObjectType { get; private set; }

        public void EditorSetType(string type) => ObjectType = type;
    }
}
