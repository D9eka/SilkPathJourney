using System.Collections.Generic;

namespace Internal.Scripts.Events
{
    public sealed class RecentEventHistory
    {
        private readonly Queue<string> _ids = new();
        private const int CAPACITY = 5;

        public void Register(string id)
        {
            _ids.Enqueue(id);
            while (_ids.Count > CAPACITY) _ids.Dequeue();
        }

        public bool Contains(string id) => _ids.Contains(id);
    }
}
