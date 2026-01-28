using Internal.Scripts.World.State;

namespace Internal.Scripts.World.VisualObjects
{
    public interface IVisualObject
    {
        public WorldDetailLevel ViewMode { get; }
        
        public void Show();
        public void Hide();
    }
}
