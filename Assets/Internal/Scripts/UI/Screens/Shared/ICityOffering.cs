using Internal.Scripts.Economy;
using Internal.Scripts.Economy.Save;

namespace Internal.Scripts.UI.Screens.Shared
{
    public interface ICityOffering
    {
        OfferingItem Build(PlayerResourceState state, int money);
        void Execute(PlayerResourceRepository repo, bool toMax);
    }
}
