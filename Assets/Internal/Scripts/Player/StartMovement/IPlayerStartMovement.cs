using System;

namespace Internal.Scripts.Player.StartMovement
{
    public interface IPlayerStartMovement
    {
        event Action<string> OnChooseNode;
        event Action<bool> OnSelectionStateChanged;
        bool IsChoosingTarget { get; }
        void SetCurrentPlayerNode(string node);
        void BeginSelection();
        void CancelSelection();
    }
}
