using System;
using System.Collections.Generic;
using UnityEngine;

namespace Internal.Scripts.Travel.Pickups
{
    public static class PickupDebugLog
    {
        public const int MaxLines = 64;

        private static readonly Queue<string> _log = new();

        public static IReadOnlyCollection<string> Entries => _log;
        public static event Action Changed;

        public static void Push(string line)
        {
            _log.Enqueue($"[{Time.time:F1}] {line}");
            while (_log.Count > MaxLines)
                _log.Dequeue();
            Changed?.Invoke();
        }

        public static void Clear()
        {
            _log.Clear();
            Changed?.Invoke();
        }
    }
}
