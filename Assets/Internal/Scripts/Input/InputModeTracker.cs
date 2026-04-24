using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Zenject;

namespace Internal.Scripts.Input
{
    public sealed class InputModeTracker : IInitializable, IDisposable
    {
        public InputMode CurrentMode { get; private set; }
        public event Action<InputMode> ModeChanged;

        public void Initialize()
        {
            InputSystem.onEvent += OnInputEvent;
        }

        public void Dispose()
        {
            InputSystem.onEvent -= OnInputEvent;
        }

        private void OnInputEvent(InputEventPtr evt, InputDevice device)
        {
            if (!evt.IsA<StateEvent>() && !evt.IsA<DeltaStateEvent>()) return;
            SetMode(device is Gamepad ? InputMode.Gamepad : InputMode.KeyboardMouse);
        }

        private void SetMode(InputMode mode)
        {
            if (mode == CurrentMode) return;
            CurrentMode = mode;
            ModeChanged?.Invoke(CurrentMode);
        }
    }
}
