using System.Collections.Generic;

namespace Internal.Scripts.UI.Screens.Archive.Tabs
{
    public interface IArchiveTabBuilder
    {
        ArchiveTab Tab { get; }
        IReadOnlyList<ArchiveListEntry> BuildItems();
        ArchiveDetailData BuildDetail(string selectedId);
    }
}
