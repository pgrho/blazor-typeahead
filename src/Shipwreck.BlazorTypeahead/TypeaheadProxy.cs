using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Shipwreck.BlazorTypeahead
{
    public class TypeaheadProxy<T> : IDisposable
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

        private class ItemCache
        {
            public string Html { get; set; }
            public int HashCode => GetHashCode();
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
        private readonly string _AppendToSelector;
        private readonly ElementReference _AppendTo;
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
            _AppendTo = options.AppendTo;
            _AppendToSelector = options.AppendToSelector;
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
            string json;
            using (var ms = new MemoryStream())
            using (var jw = new Utf8JsonWriter(ms))
            {
                jw.WriteStartObject();

                if (_Source != null)
                {
                    jw.WritePropertyName("source");
                    jw.WriteStartArray();
                    foreach (var e in CacheItems(_Source))
                    {
                        jw.WriteStartObject();

                        jw.WriteString("name", e.Html);

                        jw.WriteNumber("hashCode", e.HashCode);

                        jw.WriteEndObject();
                    }
                    jw.WriteEndArray();
                }

                jw.WriteBoolean("sourceCallback", _SourceCallback != null);
                jw.WriteNumber("items", _Items);
                jw.WriteNumber("minLength", _MinLength);

                switch (_ShowHintOnFocus)
                {
                    case HintBehavior.Disabled:
                    case HintBehavior.Enabled:
                        jw.WriteBoolean("showHintOnFocus", _ShowHintOnFocus == HintBehavior.Enabled);
                        break;

                    case HintBehavior.All:
                        jw.WriteString("showHintOnFocus", "all");
                        break;
                }

                jw.WriteNumber("scrollHeight", _ScrollHeight);
                jw.WriteBoolean("autoSelect", _AutoSelect);
                jw.WriteNumber("delay", _Delay);
                jw.WriteString("appendTo", _AppendToSelector);
                jw.WriteBoolean("fitToElement", _FitToElement);
                jw.WriteBoolean("changeInputOnSelect", _ChangeInputOnSelect);
                jw.WriteBoolean("changeInputOnMove", _ChangeInputOnMove);
                jw.WriteBoolean("openLinkInNewTab", _OpenLinkInNewTab);
                jw.WriteBoolean("selectOnBlur", _SelectOnBlur);
                jw.WriteBoolean("showCategoryHeader", _ShowCategoryHeader);

                jw.WriteEndObject();
                jw.Flush();

                json = Encoding.UTF8.GetString(ms.ToArray());
            }

            return _Runtime.InvokeVoidAsync(
                "Shipwreck.BlazorTypeahead.initialize",
                _Element,
                DotNetObjectReference.Create(this),
                _AppendTo.Id == null ? null : (object)_AppendTo,
                json);
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

        #endregion Items

        public ValueTask DestroyAsync()
            => _Runtime.InvokeVoidAsync("Shipwreck.BlazorTypeahead.destroy", _Element);

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

        [JSInvokable]
        public async Task<string> GetJsonItemsAsync(string text, int selectionStart, int selectionEnd)
        {
            var items = await QueryAsync(text, selectionStart, selectionEnd).ConfigureAwait(false);

            using (var ms = new MemoryStream())
            using (var jw = new Utf8JsonWriter(ms))
            {
                jw.WriteStartArray();
                foreach (var e in items)
                {
                    jw.WriteStartObject();

                    jw.WriteString("name", e.Html);

                    jw.WriteNumber("hashCode", e.HashCode);

                    jw.WriteEndObject();
                }
                jw.WriteEndArray();

                jw.Flush();

                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        private async ValueTask<IEnumerable<ItemCache>> QueryAsync(string text, int selectionStart, int selectionEnd)
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

            return Enumerable.Empty<ItemCache>();
        }

        [JSInvokable]
        public void OnItemSelected(int itemHashCode)
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
    }
}