using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shipwreck.BlazorTypeahead
{
    internal interface ITypeaheadProxy
    {
        ValueTask<IEnumerable<IItem>> QueryAsync(string text, int selectionStart, int selectionEnd);
        void AfterSelect(int itemHashCode);
    }
}