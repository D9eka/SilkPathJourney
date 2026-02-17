using System;

namespace Internal.Scripts.UI.Screens.Core.View
{
    public interface IScreenCloseRequestSource
    {
        event Action CloseRequested;
    }
}
