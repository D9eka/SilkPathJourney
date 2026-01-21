using System;

namespace Internal.Scripts.Player.UI.StartMovement
{
    public interface IPlayerStartMovement
    {
        event Action<string> OnChooseNode;
        void SetCurrentPlayerNode(string node);
        void FinishPath();
    }
}