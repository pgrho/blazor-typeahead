/// <reference path="../node_modules/@dotnet/jsinterop/dist/Microsoft.JSInterop.d.ts" />
var Shipwreck;
(function (Shipwreck) {
    var BlazorTypeahead;
    (function (BlazorTypeahead) {
        function initialize(element, hashCode, optionsJson) {
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
                    DotNet.invokeMethodAsync("Shipwreck.BlazorTypeahead", "OnItemSelected", hashCode, item.hashCode);
                },
                autoSelect: options.autoSelect,
                delay: options.delay,
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
                    DotNet.invokeMethodAsync("Shipwreck.BlazorTypeahead", "GetJsonItemsAsync", hashCode, element.value, element.selectionStart, element.selectionEnd)
                        .then(function (json) { return process(JSON.parse(json)); });
                };
            }
            window.jQuery(element).typeahead(ops);
        }
        BlazorTypeahead.initialize = initialize;
        function destroy(element) {
            window.jQuery(element).typeahead('destroy');
        }
        BlazorTypeahead.destroy = destroy;
        function update(element, text, focus, selectionStart, selectionEnd) {
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
        BlazorTypeahead.update = update;
    })(BlazorTypeahead = Shipwreck.BlazorTypeahead || (Shipwreck.BlazorTypeahead = {}));
})(Shipwreck || (Shipwreck = {}));
