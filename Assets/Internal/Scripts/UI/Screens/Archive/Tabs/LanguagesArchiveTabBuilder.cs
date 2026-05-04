using System;
using System.Collections.Generic;
using Internal.Scripts.Economy.Buildings.Archive;
using Internal.Scripts.Player.Languages;
using Internal.Scripts.Player.Languages.Generated;
using Internal.Scripts.UI.Localization;

namespace Internal.Scripts.UI.Screens.Archive.Tabs
{
    internal sealed class LanguagesArchiveTabBuilder : IArchiveTabBuilder
    {
        private readonly ArchiveService _archiveService;

        public ArchiveTab Tab => ArchiveTab.Languages;

        public LanguagesArchiveTabBuilder(ArchiveService archiveService)
        {
            _archiveService = archiveService;
        }

        public IReadOnlyList<ArchiveListEntry> BuildItems()
        {
            var languages = _archiveService.GetLanguages();
            var result = new List<ArchiveListEntry>(languages.Count);
            foreach (var entry in languages)
                result.Add(new ArchiveListEntry(entry.Language.ToString(), entry.Name, ResolveProficiency(entry.Proficiency)));
            return result;
        }

        public ArchiveDetailData BuildDetail(string selectedId)
        {
            if (string.IsNullOrEmpty(selectedId))
                return ArchiveDetailData.Empty;
            if (!Enum.TryParse(selectedId, out LanguageType lang))
                return ArchiveDetailData.Empty;

            string name = LocalizationService.Resolve("UI", $"UI.Language.{lang}.Name", lang.ToString());
            string desc = LocalizationService.Resolve("UI", $"UI.Language.{lang}.Description", string.Empty);
            return new ArchiveDetailData(name, desc);
        }

        private static string ResolveProficiency(LanguageProficiency proficiency)
        {
            if (proficiency == LanguageProficiency.None)
                return LocalizationService.Resolve("UI", "ui.archive.languages.not_learned", "Не изучен");

            string levelName = LocalizationService.Resolve("UI", $"UI.Language.Proficiency.{proficiency}", proficiency.ToString());
            return LocalizationService.Resolve("UI", "ui.archive.languages.proficiency_label", $"Уровень: {levelName}", levelName);
        }
    }
}
