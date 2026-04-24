using System;
using Internal.Scripts.Input;
using Internal.Scripts.Travel.Hazards.Minigames;
using UnityEngine;

namespace Internal.Scripts.UI.Screens.HazardQte.Qte
{
    public abstract class MinigameBase : MonoBehaviour, IQteMinigameView
    {
        public event Action<bool> OnCompleted;
        public virtual bool DidPlayerSucceed() => Succeeded;

        protected bool IsAlive { get; private set; }
        protected bool Succeeded { get; private set; }

        public abstract void Show(IMinigameConfig config, IQteInput input);
        public abstract void Hide();

        protected void SetAlive(bool alive) => IsAlive = alive;

        protected void Complete(bool success)
        {
            if (!IsAlive) return;
            IsAlive = false;
            Succeeded = success;
            Hide();
            OnCompleted?.Invoke(success);
        }
    }
}
