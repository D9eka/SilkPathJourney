using System;

namespace Internal.Scripts.Player.UI.StartMovement
{
    public interface IPlayerStartMovement
    {
        event Action<string> OnChooseNode;
        bool IsChoosingTarget { get; }
        void SetCurrentPlayerNode(string node);
        void FinishPath();
    }
}
