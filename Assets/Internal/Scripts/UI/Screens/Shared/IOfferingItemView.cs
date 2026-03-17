using System;

namespace Internal.Scripts.UI.Screens.Shared
{
    public interface IOfferingItemView
    {
        void Initialize(OfferingItem item, Action onAction, Action onActionMax);
    }
}
