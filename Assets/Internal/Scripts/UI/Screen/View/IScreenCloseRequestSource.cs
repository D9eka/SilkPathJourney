using System;

namespace Internal.Scripts.UI.Screen.View
{
    public interface IScreenCloseRequestSource
    {
        event Action CloseRequested;
    }
}
