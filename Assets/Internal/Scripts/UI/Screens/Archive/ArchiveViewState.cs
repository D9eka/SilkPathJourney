using System.Collections.Generic;

namespace Internal.Scripts.UI.Screens.Archive
{
    public sealed class ArchiveViewState
    {
        public readonly ArchiveTab ActiveTab;
        public readonly IReadOnlyList<ArchiveListEntry> Items;
        public readonly string SelectedId;
        public readonly ArchiveDetailData Detail;

        public ArchiveViewState(ArchiveTab activeTab, IReadOnlyList<ArchiveListEntry> items, string selectedId, ArchiveDetailData detail)
        {
            ActiveTab = activeTab;
            Items = items;
            SelectedId = selectedId;
            Detail = detail;
        }
    }

    public readonly struct ArchiveListEntry
    {
        public readonly string Id;
        public readonly string DisplayName;
        public readonly string Description;

        public ArchiveListEntry(string id, string displayName, string description = null)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
        }
    }

    public readonly struct ArchiveDetailData
    {
        public readonly string Title;
        public readonly string Description;
        public readonly bool HasData;

        public ArchiveDetailData(string title, string description)
        {
            Title = title;
            Description = description;
            HasData = true;
        }

        public static ArchiveDetailData Empty => new ArchiveDetailData();
    }
}
