using System;
using Internal.Scripts.UI.WorldLabel;
using R3;
using UnityEngine;

namespace Internal.Scripts.Npc.City
{
    public class CityNpcQuestIndicator : MonoBehaviour
    {
        [SerializeField] private CityNpcView _npcView;

        private IDisposable _subscription;

        public void Bind(IQuestAvailabilityProvider provider)
        {
            _subscription?.Dispose();

            _subscription = provider.HasAvailableQuest.Subscribe(hasQuest =>
            {
                if (_npcView.Label == null) return;

                if (hasQuest && _npcView.QuestIconSprite != null)
                    _npcView.Label.SetIcon(_npcView.QuestIconSprite);
                else
                    _npcView.Label.HideIcon();
            });
        }

        private void OnDestroy()
        {
            _subscription?.Dispose();
        }
    }
}
