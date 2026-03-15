using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Internal.Scripts.Save
{
    public sealed class AutoSaveController : ITickable
    {
        private const float INTERVAL = 60f;
        private const int MAX_SLOTS = 5;
        private const string PREFIX = "autosave_";

        private readonly SaveRepository _saveRepository;
        private readonly ISaveService _saveService;
        private float _elapsed;

        public AutoSaveController(SaveRepository saveRepository, ISaveService saveService)
        {
            _saveRepository = saveRepository;
            _saveService = saveService;
        }

        public void Tick()
        {
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed < INTERVAL)
                return;

            _elapsed = 0f;
            PerformAutoSave();
        }

        private void PerformAutoSave()
        {
            List<SaveMetadata> allSaves = _saveService.GetAllSaves();
            var autoSlots = allSaves.FindAll(s => s.IsAutoSave);

            while (autoSlots.Count >= MAX_SLOTS)
            {
                var oldest = autoSlots[autoSlots.Count - 1];
                _saveService.Delete(oldest.SlotId);
                autoSlots.RemoveAt(autoSlots.Count - 1);
            }

            string newSlotId = PREFIX + Guid.NewGuid().ToString("N");
            _saveRepository.SaveToSlot(newSlotId, isAutoSave: true);
        }
    }
}
