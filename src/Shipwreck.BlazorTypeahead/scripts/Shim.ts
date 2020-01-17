/// <reference path="../node_modules/@dotnet/jsinterop/dist/Microsoft.JSInterop.d.ts" />
/// <reference path="../node_modules/@types/jquery/index.d.ts" />
namespace Shipwreck.BlazorTypeahead {
    export function initialize(element: HTMLInputElement, obj, appendTo: Element, optionsJson: string) {
        var options = JSON.parse(optionsJson);
        var ops = {
            source: undefined,
            items: options.items,
            minLength: options.minLength,
            showHintOnFocus: options.showHintOnFocus,
            scrollHeight: options.scrollHeight,
            matcher: options.sourceCallback ? function () { return true; } : null,
            highlighter: function (text, item) {
                return item.name;
            },
            afterSelect: function (item) {
                obj.invokeMethodAsync("OnItemSelected", item.hashCode);
            },
            autoSelect: options.autoSelect,
            delay: options.delay,
            appendTo: appendTo || (options.appendTo ? jQuery(options.appendTo) : null),
            fitToElement: options.fitToElement,
            changeInputOnSelect: options.changeInputOnSelect,
            changeInputOnMove: options.changeInputOnMove,
            openLinkInNewTab: options.openLinkInNewTab,
            selectOnBlur: options.selectOnBlur,
            showCategoryHeader: options.showCategoryHeader
        };
        if (options.source) {
            ops.source = options.source;
        }
        else {
            ops.source = function (query, process) {
                obj.invokeMethodAsync("GetJsonItemsAsync", element.value, element.selectionStart, element.selectionEnd)
                    .then(function (json) { return process(JSON.parse(json)); });
            };
        }
        (<any>jQuery)(element).typeahead(ops);
    }
    export function destroy(element) {
        (<any>jQuery)(element).typeahead('destroy');
    }
    export function update(element, text, focus, selectionStart, selectionEnd) {
        if (element) {
            if (text !== null && text !== undefined) {
                element.value = text;
            }
            if (focus) {
                element.focus();
            }
            if (selectionStart !== null && selectionStart !== undefined
                && selectionEnd !== null && selectionEnd !== undefined) {
                element.setSelectionRange(selectionStart, selectionEnd);
            }
        }
    }
}  
