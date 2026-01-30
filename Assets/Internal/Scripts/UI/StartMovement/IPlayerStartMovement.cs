using System;

namespace Internal.Scripts.UI.StartMovement
{
    public interface IPlayerStartMovement
    {
        event Action<string> OnChooseNode;
        bool IsChoosingTarget { get; }
        void SetCurrentPlayerNode(string node);
        void SetStartButtonEnabled(bool enabled);
        void CancelSelection();
        void FinishPath();
    }
}
