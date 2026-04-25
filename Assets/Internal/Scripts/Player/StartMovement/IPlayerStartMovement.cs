using System;
using Internal.Scripts.Economy.Cities;
using UnityEngine;

namespace Internal.Scripts.Player.StartMovement
{
    public interface IPlayerStartMovement
    {
        event Action<string> OnChooseNode;
        event Action<bool> OnSelectionStateChanged;
        event Action<CityData, Vector3> OnCityPreview;
        bool IsChoosingTarget { get; }
        void SetCurrentPlayerNode(string node);
        void BeginSelection();
        void CancelSelection();
        void ConfirmSelection();
        void CancelPreview();
        void RequestCityPreview(CityData city);
    }
}
