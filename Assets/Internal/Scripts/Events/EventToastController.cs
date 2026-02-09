using Internal.Scripts.Events.Data;

namespace Internal.Scripts.Events
{
    public interface IEventToastView
    {
        void Show(EventData eventData);
    }

    public class EventToastController
    {
        private IEventToastView _view;

        public void RegisterView(IEventToastView view) => _view = view;

        public void ShowToast(EventData eventData)
        {
            _view?.Show(eventData);
        }
    }
}
