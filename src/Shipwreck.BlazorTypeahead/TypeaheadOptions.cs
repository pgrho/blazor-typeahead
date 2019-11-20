using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shipwreck.BlazorTypeahead
{
    public class TypeaheadOptions<T>
    {
        public IList<T> Source { get; set; }

        public Func<string, Task<IList<T>>> SourceCallback { get; set; }

        public int Items { get; set; } = 8;

        public int MinLength { get; set; } = 1;

        public HintBehavior ShowHintOnFocus { get; set; }

        public int ScrollHeight { get; set; }

        // matcher

        // sorter

        // updater

        public Func<T, string, string> Highlighter { get; set; }

        public Func<T, string> DisplayText { get; set; }

        public bool AutoSelect { get; set; } = true;

        public Action<T> AfterSelect { get; set; }
        public int Delay { get; set; }

        // public ElementReference AppendTo { get; set; }

        public bool FitToElement { get; set; }

        // addItem

        public bool ChangeInputOnSelect { get; set; } = true;

        public bool ChangeInputOnMove { get; set; } = true;

        public bool OpenLinkInNewTab { get; set; }

        public bool SelectOnBlur { get; set; } = true;

        public bool ShowCategoryHeader { get; set; } = true;
    }
}