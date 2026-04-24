using System;
using UnityEngine;

namespace Internal.Scripts.Input
{
    public interface IUiInput
    {
        event Action<Vector2> OnNavigate;
        event Action OnSubmit;
        event Action OnSubmitAll;
        event Action OnBack;
        event Action OnAction;
        event Action OnNextArea;
        event Action OnPrevArea;
        Vector2 NavigateValue { get; }
    }
}
