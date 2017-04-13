//******************************************************************************************************
//  gsf.web.knockout.js - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  07/22/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable UndeclaredGlobalVariableUsing

// Grid Solutions Framework Core Web Knockout Extension Script Functions
"use strict";

// Field functions
function fieldHasValue(field, required) {
    if (field == null)
        return true;

    if (required === undefined)
        required = false;

    var isValid = true;

    if (required) {
        if (typeof field == "function")
            isValid = !isEmpty(field());
        else
            isValid = !isEmpty(field);
    }

    return isValid;
}

function fieldIsValid(field, required) {
    var isValid = fieldHasValue(field, required);

    if (isValid && typeof field.isValid == "function")
        isValid = field.isValid();

    return isValid;
}

// Custom knockout binding handers
ko.bindingHandlers.numeric = {
    init: function(element, valueAccessor, allBindings, viewModel, bindingContext) {
        $(element).on("keydown", function(event) {
            // Allow backspace, delete, tab, escape, and enter
            if (event.keyCode === 46 || event.keyCode === 8 || event.keyCode === 9 || event.keyCode === 27 || event.keyCode === 13 ||
                // Ctrl+A
                (event.keyCode === 65 && event.ctrlKey) ||
                // E + - . ,
                (event.keyCode === 69 || event.keyCode === 107 || event.keyCode === 109 || event.keyCode === 110 || event.keyCode === 190 || event.keyCode === 188) ||
                // home, end, left, right, up, down
                (event.keyCode >= 35 && event.keyCode <= 40)) {
                return;
            }
            else {
                // If value is not a number, stop key-press
                if (event.shiftKey || (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
                    event.preventDefault();
                }
            }
        });
    }
};

ko.bindingHandlers.integer = {
    init: function(element, valueAccessor, allBindings, viewModel, bindingContext) {
        $(element).on("keydown", function(event) {
            // Allow backspace, delete, tab, escape, and enter
            if (event.keyCode === 46 || event.keyCode === 8 || event.keyCode === 9 || event.keyCode === 27 || event.keyCode === 13 ||
                // Ctrl+A
                (event.keyCode === 65 && event.ctrlKey) ||
                // home, end, left, right, up, down
                (event.keyCode >= 35 && event.keyCode <= 40)) {
                return;
            }
            else {
                // If value is not a number, stop key-press
                if (event.shiftKey || (event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105)) {
                    event.preventDefault();
                }
            }
        });
    }
};

// Restrict binding to upper case letters - binding target specification required
ko.bindingHandlers.upperCase = {
    init: function(element, valueAccessor, allBindings, viewModel, bindingContext) {
        $(element).on("input", function(event) {
            const start = this.selectionStart;
            const end = this.selectionEnd;
            valueAccessor()(this.value = this.value.toUpperCase());
            this.setSelectionRange(start, end);
        });
    }
};

// Restrict binding to lower case letters - binding target specification required
ko.bindingHandlers.lowerCase = {
    init: function(element, valueAccessor, allBindings, viewModel, bindingContext) {
        $(element).on("input", function(event) {
            const start = this.selectionStart;
            const end = this.selectionEnd;
            valueAccessor()(this.value = this.value.toLowerCase());
            this.setSelectionRange(start, end);
        });
    }
};

// Restrict binding to upper case letters, numbers, '!', '-', '@', '#', '_' , '.' or '$' - binding target specification required
ko.bindingHandlers.acronym = {
    init: function(element, valueAccessor, allBindings, viewModel, bindingContext) {
        $(element).on("keydown", function(event) {
            // Allow backspace, delete, tab, escape, and enter
            if (event.keyCode === 46 || event.keyCode === 8 || event.keyCode === 9 || event.keyCode === 27 || event.keyCode === 13 ||
                // Ctrl+A
                (event.keyCode === 65 && event.ctrlKey) ||
                // home, end, left, right, up, down
                (event.keyCode >= 35 && event.keyCode <= 40)) {
                return;
            }
            else {
                // If value is not a number or letter, stop key-press
                if ((event.keyCode < 48 || event.keyCode > 57) && (event.keyCode < 96 || event.keyCode > 105) && (event.keyCode < 65 || event.keyCode > 90) && (event.keyCode < 189 || event.keyCode > 190)) {
                    event.preventDefault();
                }
            }
        }).on("input", function(event) {
            const start = this.selectionStart;
            const end = this.selectionEnd;
            valueAccessor()(this.value = this.value.toUpperCase());
            this.setSelectionRange(start, end);            
        });
    }
};

// Handle special validation error message location when using bootstrap add-on fields
ko.bindingHandlers.validationCore = {
    init: function(element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        const parent = $(element).parent().closest(".input-group");
        const message = document.createElement("em");
        message.className = "validationMessage small";

        if (parent.length > 0)
            $(parent).after(message);
        else
            $(element).after(message);

        ko.applyBindingsToNode(message, { validationMessage: valueAccessor() });
    }
};