using System;
using UnityEngine.InputSystem;

namespace Internal.Scripts.Input
{
    public interface IQteInput
    {
        event Action OnClick;
        event Action OnClickCanceled;
        event Action OnLeft;
        event Action OnRight;
        event Action OnDown;
        event Action OnUp;

        InputAction ClickAction { get; }
        InputAction LeftAction  { get; }
        InputAction RightAction { get; }
        InputAction DownAction  { get; }
        InputAction UpAction    { get; }

        void Enable();
        void Disable();
    }
}
