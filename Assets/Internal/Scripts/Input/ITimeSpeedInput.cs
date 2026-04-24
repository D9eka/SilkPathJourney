using System;

namespace Internal.Scripts.Input
{
    public interface ITimeSpeedInput
    {
        event Action OnPause;
        event Action OnSpeed1;
        event Action OnSpeed2;
        event Action OnSpeed3;
    }
}
