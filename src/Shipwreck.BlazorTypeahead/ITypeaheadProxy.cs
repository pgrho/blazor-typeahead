namespace Shipwreck.BlazorTypeahead
{
    internal interface ITypeaheadProxy
    {
        void AfterSelect(int itemHashCode);
    }
}