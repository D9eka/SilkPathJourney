using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Internal.Scripts.InteractableObjects;
using Internal.Scripts.Road.Path;
using Internal.Scripts.UI.Arrow;
using Internal.Scripts.UI.Arrow.Controller;

namespace Internal.Scripts.Player.Input
{
    public class PlayerChoiceInputView : IPlayerChoiceInput
    {
        protected readonly IArrowsController _arrows;

        private UniTaskCompletionSource<RoadPathSegment> _tcs;
        private List<ArrowView> _arrowsToSubscribe;
        private CancellationTokenRegistration _cancelRegistration;

        public PlayerChoiceInputView(IArrowsController arrows)
        {
            _arrows = arrows;
        }

        public async UniTask<RoadPathSegment> WaitForChoiceAsync(CancellationToken ct)
        {
            _tcs = new UniTaskCompletionSource<RoadPathSegment>();
            _arrowsToSubscribe = _arrows.GetAllArrows();
            while (_arrowsToSubscribe.Count == 0)
            {
                await UniTask.WaitForFixedUpdate();
                _arrowsToSubscribe = _arrows.GetAllArrows();
            }
            foreach (ArrowView arrow in _arrowsToSubscribe)
            {
                arrow.OnClick += OnArrowClick;
            }
            
            _cancelRegistration = ct.Register(() =>
            {
                foreach (ArrowView arrow in _arrowsToSubscribe)
                {
                    arrow.OnClick -= OnArrowClick;
                }
                _arrowsToSubscribe.Clear();
                _tcs.TrySetCanceled(ct);
            });

            try
            {
                return await _tcs.Task;
            }
            finally
            {
                _cancelRegistration.Dispose();
                _cancelRegistration = default;
            }
        }

        private void OnArrowClick(IInteractableObject obj)
        {
            if (obj is not ArrowView arrowView)
            {
                return;
            }
            foreach (ArrowView arrow in _arrowsToSubscribe)
            {
                arrow.OnClick -= OnArrowClick;
            }
            _tcs.TrySetResult(arrowView.Segment);
        }
    }
    
    public interface IPlayerChoiceInput
    {
        UniTask<RoadPathSegment> WaitForChoiceAsync(CancellationToken ct);
    }
}
