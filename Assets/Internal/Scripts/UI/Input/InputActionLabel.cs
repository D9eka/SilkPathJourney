using Internal.Scripts.Input;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Internal.Scripts.UI.Input
{
    public sealed class InputActionLabel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _target;
        [SerializeField] private InputActionReference _action;

        private InputModeTracker _tracker;

        [Inject]
        public void Construct(InputModeTracker tracker) => _tracker = tracker;

        private void OnEnable()
        {
            _tracker.ModeChanged += OnModeChanged;
            Refresh();
        }

        private void OnDisable()
        {
            _tracker.ModeChanged -= OnModeChanged;
        }

        private void OnModeChanged(InputMode _) => Refresh();

        private void Refresh()
        {
            var action = _action?.action;
            if (action == null) { _target.text = string.Empty; return; }

            int index = FindMatchingBindingIndex(action);
            if (index < 0) { _target.text = string.Empty; return; }

            _target.text = action.GetBindingDisplayString(index,
                InputBinding.DisplayStringOptions.DontIncludeInteractions);
        }

        private int FindMatchingBindingIndex(InputAction action)
        {
            bool gamepad = _tracker.CurrentMode == InputMode.Gamepad;
            var bindings = action.bindings;
            int fallback = -1;
            for (int i = 0; i < bindings.Count; i++)
            {
                var b = bindings[i];
                if (b.isComposite) continue;
                if (string.IsNullOrEmpty(b.path)) continue;
                bool isGamepadPath = b.path.Contains("<Gamepad>");
                if (isGamepadPath == gamepad) return i;
                if (fallback < 0) fallback = i;
            }
            return fallback;
        }
    }
}
