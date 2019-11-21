namespace Shipwreck.BlazorTypeahead
{
    internal interface IItem
    {
        string Html { get; }
        int HashCode { get; }
    }
}