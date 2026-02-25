using Internal.Scripts.Economy.Generated;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Internal.Scripts.UI.Theme
{
    public class UiStaticColorBinder : MonoBehaviour
    {
        [SerializeField] private Biome _biome;
        [SerializeField] private ColorSlot _colorSlot;
        [SerializeField] private Graphic _target;

        [Inject(Optional = true)] private StaticColorController _controller;

        public Biome Biome => _biome;
        public ColorSlot Slot => _colorSlot;

        private void OnEnable()
        {
            if (_controller != null)
                _controller.Register(this);
        }

        public void Initialize(StaticColorController controller)
        {
            _controller = controller;
            _controller.Register(this);
        }

        public void SetBiome(Biome biome)
        {
            _biome = biome;
            if (_controller != null)
                _controller.Register(this);
        }

        public void SetColor(Color color)
        {
            if (_target != null) _target.color = color;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_target == null)
                _target = GetComponent<Graphic>();
        }
#endif
    }
}
