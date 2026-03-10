using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Internal.Scripts.UI.Localization
{
    public interface ILocalizationConsumer
    {
        void SetLocalization(LocalizationService service);
    }

    public class LocalizationService
    {
        public sealed class LocalizedTextHandle : IDisposable
        {
            private readonly TextMeshProUGUI _target;
            private readonly LocalizedString _localized;
            private readonly string _context;
            private readonly Func<string, string> _postProcess;
            private string _fallback;
            private bool _isBound;
            private LocalizedString.ChangeHandler _handler;

            internal LocalizedTextHandle(TextMeshProUGUI target, LocalizedString localized,
                string context, string fallback, Func<string, string> postProcess = null)
            {
                _target = target;
                _localized = localized;
                _context = context;
                _fallback = fallback ?? string.Empty;
                _postProcess = postProcess;
            }

            internal bool TryBind()
            {
                if (_target == null)
                {
                    Debug.LogWarning($"[SPJ] Missing Text target for {_context}.");
                    return false;
                }

                if (_localized == null)
                {
                    Debug.LogWarning($"[SPJ] Missing LocalizedString for {_context}.");
                    _target.text = _fallback;
                    return false;
                }

                if (IsEmpty(_localized))
                {
                    Debug.LogWarning($"[SPJ] LocalizedString is empty for {_context}.");
                    _target.text = _fallback;
                    return false;
                }

                _handler = value =>
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        _target.text = _fallback;
                        return;
                    }

                    _target.text = _postProcess != null ? _postProcess(value) : value;
                };

                _localized.StringChanged += _handler;
                _isBound = true;
                return true;
            }

            public void SetArguments(string fallback, params object[] args)
            {
                _fallback = fallback ?? string.Empty;
                if (_target == null)
                    return;

                if (_localized == null || IsEmpty(_localized))
                {
                    _target.text = _fallback;
                    return;
                }

                if (args != null && args.Length > 0)
                    _localized.Arguments = args;
                _localized.RefreshString();
            }

            public void Dispose()
            {
                if (!_isBound)
                    return;

                _localized.StringChanged -= _handler;
                _isBound = false;
            }
        }

        public sealed class LocalizedTextGroup : IDisposable
        {
            private readonly LocalizationService _service;
            private readonly List<LocalizedTextHandle> _handles = new();

            internal LocalizedTextGroup(LocalizationService service)
            {
                _service = service;
            }

            public void Bind(TextMeshProUGUI text, LocalizedString localized, string context)
            {
                if (text != null)
                    _handles.Add(_service.BindText(text, localized, context));
            }

            public void Dispose()
            {
                foreach (var h in _handles)
                    h.Dispose();
                _handles.Clear();
            }
        }

        public LocalizedTextGroup CreateTextGroup() => new(this);

        public LocalizedTextHandle BindText(TextMeshProUGUI target, LocalizedString localized, string context)
        {
            string fallback = target != null ? target.text : string.Empty;
            var handle = new LocalizedTextHandle(target, localized, context, fallback);
            if (handle.TryBind())
                handle.SetArguments(fallback);
            return handle;
        }

        public LocalizedTextHandle BindText(TextMeshProUGUI target, LocalizedString localized,
            string context, string fallback, Func<string, string> postProcess, params object[] args)
        {
            var handle = new LocalizedTextHandle(target, localized, context, fallback, postProcess);
            if (handle.TryBind())
                handle.SetArguments(fallback, args);
            return handle;
        }

        public LocalizedTextHandle BindText(TextMeshProUGUI target, LocalizedString localized,
            string context, Func<string, string> postProcess)
        {
            string fallback = target != null ? target.text : string.Empty;
            var handle = new LocalizedTextHandle(target, localized, context, fallback, postProcess);
            if (handle.TryBind())
                handle.SetArguments(fallback);
            return handle;
        }

        public static string ResolveString(LocalizedString localized, string fallback, string context, params object[] args)
        {
            if (localized == null)
            {
                Debug.LogWarning($"[SPJ] Missing LocalizedString for {context}.");
                return fallback;
            }

            if (IsEmpty(localized))
            {
                Debug.LogWarning($"[SPJ] LocalizedString is empty for {context}.");
                return fallback;
            }

            try
            {
                string value = args != null && args.Length > 0
                    ? localized.GetLocalizedString(args)
                    : localized.GetLocalizedString();

                if (string.IsNullOrWhiteSpace(value) || value.StartsWith("No translation found"))
                {
                    Debug.LogWarning($"[SPJ] LocalizedString resolved empty for {context}.");
                    return fallback;
                }

                return value;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SPJ] Failed to resolve LocalizedString for {context}: {e.Message}");
                return fallback;
            }
        }

        internal static bool IsEmpty(LocalizedString localized)
        {
            string table = localized.TableReference.TableCollectionName;
            string key = localized.TableEntryReference.Key;
            long keyId = localized.TableEntryReference.KeyId;
            return string.IsNullOrWhiteSpace(table) && string.IsNullOrWhiteSpace(key) && keyId == 0;
        }
    }
}
