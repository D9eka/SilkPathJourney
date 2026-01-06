using System.Collections.Generic;

namespace Internal.Scripts.World.VisualObjects
{
    public interface IVisualObject
    {
        public List<WorldDetailLevel> ViewMode { get; }
        
        public void Show();
        public void Hide();
    }
}