using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace Shipwreck.BlazorTypeahead
{
    public class TypeaheadProxy<T> : ITypeaheadProxy, IDisposable
    {
        private class ItemList
        {
            public ItemList(ItemCache[] items)
            {
                Items = items;
                Timestamp = DateTime.Now;
            }

            public DateTime Timestamp { get; }
            public ItemCache[] Items { get; }
        }

        private class ItemCache : IItem
        {
            [JsonProperty("name")]
            public string Html { get; set; }

            [JsonProperty("hashCode")]
            public int HashCode => GetHashCode();

            [JsonIgnore]
            public T Value { get; set; }
        }

        private readonly IJSRuntime _Runtime;
        private readonly ElementReference _Element;

        private readonly List<ItemList> _History;
        private readonly int _MaximumHistory;

        #region Options

        private readonly T[] _Source;
        private readonly TypeaheadSourceCallback<T> _SourceCallback;
        private readonly int _Items;
        private readonly int _MinLength;
        private readonly HintBehavior _ShowHintOnFocus;
        private readonly int _ScrollHeight;

        private readonly Func<T, string, string> _Highlighter;
        private readonly Func<T, string> _DisplayText;
        private readonly bool _AutoSelect;
        private readonly Action<T> _AfterSelect;
        private readonly int _Delay;
        private readonly bool _FitToElement;
        private readonly bool _ChangeInputOnSelect;
        private readonly bool _ChangeInputOnMove;
        private readonly bool _OpenLinkInNewTab;
        private readonly bool _SelectOnBlur;
        private readonly bool _ShowCategoryHeader;

        #endregion Options

        public TypeaheadProxy(IJSRuntime runtime, ElementReference element, TypeaheadOptions<T> options)
        {
            _Runtime = runtime;
            _Element = element;

            _Source = options.Source?.ToArray();
            _SourceCallback = options.SourceCallback;
            _Items = options.Items;
            _MinLength = options.MinLength;
            _ShowHintOnFocus = options.ShowHintOnFocus;
            _ScrollHeight = options.ScrollHeight;
            _Highlighter = options.Highlighter;
            _DisplayText = options.DisplayText;
            _AutoSelect = options.AutoSelect;
            _AfterSelect = options.AfterSelect;
            _Delay = options.Delay;
            _FitToElement = options.FitToElement;
            _ChangeInputOnSelect = options.ChangeInputOnSelect;
            _ChangeInputOnMove = options.ChangeInputOnMove;
            _OpenLinkInNewTab = options.OpenLinkInNewTab;
            _SelectOnBlur = options.SelectOnBlur;
            _ShowCategoryHeader = options.ShowCategoryHeader;

            _History = new List<ItemList>();
            _MaximumHistory = 16;
        }

        public ValueTask InitializeAsync()
        {
            Typeahead.Register(this);

            return _Runtime.InvokeVoidAsync(
                "Shipwreck.BlazorTypeahead.initialize",
                _Element,
                GetHashCode(),
                JsonConvert.SerializeObject(new
                {
                    source = CacheItems(_Source),
                    sourceCallback = _SourceCallback != null,
                    items = _Items,
                    minLength = _MinLength,
                    showHintOnFocus = _ShowHintOnFocus == HintBehavior.Disabled ? false
                                    : _ShowHintOnFocus == HintBehavior.Enabled ? true
                                    : _ShowHintOnFocus == HintBehavior.All ? "all"
                                    : (object)null,
                    scrollHeight = _ScrollHeight,
                    autoSelect = _AutoSelect,
                    //AfterSelect;
                    delay = _Delay,
                    fitToElement = _FitToElement,
                    changeInputOnSelect = _ChangeInputOnSelect,
                    changeInputOnMove = _ChangeInputOnMove,
                    openLinkInNewTab = _OpenLinkInNewTab,
                    selectOnBlur = _SelectOnBlur,
                    showCategoryHeader = _ShowCategoryHeader,
                }));
        }

        async ValueTask<IEnumerable<IItem>> ITypeaheadProxy.QueryAsync(string text, int selectionStart, int selectionEnd)
        {
            if (_Source != null)
            {
                lock (_History)
                {
                    return _History.LastOrDefault()?.Items ?? CacheItems(_Source);
                }
            }
            if (_SourceCallback != null)
            {
                var items = await _SourceCallback(text, selectionStart, selectionEnd).ConfigureAwait(false);
                return CacheItems(items);
            }

            return Enumerable.Empty<IItem>();
        }

        #region Items

        private ItemCache[] CacheItems(IEnumerable<T> items)
        {
            if (items == null)
            {
                return null;
            }
            var vs = items.Select(e => new ItemCache
            {
                Html = ToHtml(e, null),
                Value = e
            }).ToArray();

            if (vs.Length != 0)
            {
                lock (_History)
                {
                    var dc = _History.Count - _MaximumHistory + 1;
                    if (dc > 0)
                    {
                        _History.RemoveRange(0, dc);
                    }

                    _History.Add(new ItemList(vs));
                }
            }
            return vs;
        }

        private string ToHtml(T item, string query)
        {
            if (_Highlighter != null)
            {
                return _Highlighter(item, query);
            }
            else
            {
                var text = GetItemText(item);

                return Regex.Replace(text, "[<>&]", m =>
                {
                    if (m.Value == "<")
                    {
                        return "&lt;";
                    }
                    if (m.Value == ">")
                    {
                        return "&gt;";
                    }
                    if (m.Value == "&")
                    {
                        return "&amp;";
                    }
                    return m.Value;
                });
            }
        }

        public string GetItemText(T item) => _DisplayText?.Invoke(item) ?? item?.ToString() ?? string.Empty;

        void ITypeaheadProxy.AfterSelect(int itemHashCode)
        {
            ItemCache selected = null;
            lock (_History)
            {
                for (var i = _History.Count - 1; i >= 0; i--)
                {
                    var l = _History[i];
                    foreach (var e in l.Items)
                    {
                        if (e.HashCode == itemHashCode)
                        {
                            selected = e;
                            i = 0;
                            break;
                        }
                    }
                }
            }

            if (selected != null)
            {
                if (_AfterSelect != null)
                {
                    _AfterSelect(selected.Value);
                }
                else
                {
                    UpdateElementAsync(text: GetItemText(selected.Value), selectionStart: int.MaxValue, selectionEnd: int.MaxValue);
                }
            }
        }

        #endregion Items

        public ValueTask DestroyAsync()
        {
            if (Typeahead.Unregister(this))
            {
                return _Runtime.InvokeVoidAsync("Shipwreck.BlazorTypeahead.destroy", _Element);
            }
            return default;
        }

        #region UpdateElementAsync

        public ValueTask SetTextAsync(string text)
            => UpdateElementAsync(text: text ?? string.Empty);

        public ValueTask FocusAsync()
            => UpdateElementAsync(focus: true);

        public ValueTask SetSelectionRangeAsync(int start, int end)
            => UpdateElementAsync(selectionStart: start, selectionEnd: end);

        public ValueTask SelectAsync()
            => UpdateElementAsync(selectionStart: 0, selectionEnd: int.MaxValue);

        public ValueTask UpdateElementAsync(string text = null, bool focus = false, int? selectionStart = null, int? selectionEnd = null)
              => _Runtime.InvokeVoidAsync("Shipwreck.BlazorTypeahead.update", _Element, text, focus, selectionStart, selectionEnd);

        #endregion UpdateElementAsync

        #region IDisposable

        protected bool IsDisposed { get; set; }

        protected virtual async void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                try
                {
                    await DestroyAsync().ConfigureAwait(false);
                }
                catch { }
            }
        }

        ~TypeaheadProxy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable
    }
}