using System;

namespace Internal.Scripts.Player
{
    public interface IPlayerStateEvents
    {
        event Action<string> OnCurrentNodeChanged;
        event Action<string> OnDestinationChanged;
    }
}
