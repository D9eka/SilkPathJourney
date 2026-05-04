using System.Collections.Generic;
using System.Linq;

namespace Internal.Scripts.Meta.Unlocks
{
    public sealed class UnlockRepository
    {
        private readonly UnlockData[] _all;

        public UnlockRepository(UnlockData[] all)
        {
            _all = all ?? System.Array.Empty<UnlockData>();
        }

        public IEnumerable<UnlockData> GetAll() => _all;

        public IEnumerable<UnlockData> GetByCategory(UnlockCategory cat)
            => _all.Where(u => u.Category == cat);

        public UnlockData GetById(string id)
            => _all.FirstOrDefault(u => u.Id == id);
    }
}
