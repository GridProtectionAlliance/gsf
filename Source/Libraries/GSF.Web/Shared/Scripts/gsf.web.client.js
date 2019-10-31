//******************************************************************************************************
//  gsf.web.client.js - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/26/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************
// ReSharper disable NativeTypePrototypeExtending
// ReSharper disable UndeclaredGlobalVariableUsing

// Grid Solutions Framework Core Web Client Script Functions
"use strict";

if (typeof jQuery === "undefined") {
    throw new Error("gsf.web.client script requires jQuery - make sure jquery.js is loaded first");
}

const isIE = detectIE();
var textMetrics;

$(function() {
    // Create a canvas object that will be used for text metrics calculations
    $("<canvas id=\"textMetricsCanvas\" height=\"1px\" width=\"1px\" style=\"visibility: hidden\"></canvas>")
        .appendTo("body");

    // Get text metrics canvas context
    textMetrics = document.getElementById("textMetricsCanvas").getContext("2d");
});

// Miscellaneous functions
function notNull(value, nonNullValue) {
    return value || (nonNullValue || "");
}

function detectIE() {
    const ua = window.navigator.userAgent;
    const msie = ua.indexOf("MSIE ");

    if (msie > 0) {
        // IE 10 or older => return version number
        return parseInt(ua.substring(msie + 5, ua.indexOf(".", msie)), 10);
    }

    const trident = ua.indexOf("Trident/");

    if (trident > 0) {
        // IE 11 => return version number
        const rv = ua.indexOf("rv:");
        return parseInt(ua.substring(rv + 3, ua.indexOf(".", rv)), 10);
    }

    const edge = ua.indexOf("Edge/");

    if (edge > 0) {
        // Edge (IE 12+) => return version number
        return parseInt(ua.substring(edge + 5, ua.indexOf(".", edge)), 10);
    }

    // Other browser
    return false;
}

function isNumber(val) {
    return !isNaN(parseFloat(val)) && isFinite(val);
}

function toHex(val, leftPadding) {
    if (leftPadding === undefined)
        leftPadding = 4;

    const isNegative = val < 0;
    return (isNegative ? "-" : "") + "0x" + Math.abs(val).toString(16).padLeft(leftPadding, "0").toUpperCase();
}

function isBool(val) {
    if (typeof value === "boolean")
        return true;

    const lval = val.toString().toLowerCase();
    return lval === "true" || lval === "false";
}

function getBool(val) {
    const num = +val;
    return !isNaN(num) ? !!num : !!String(val).toLowerCase().replace(false, "");
}

function getParameterByName(name, url) {
    if (!url)
        url = window.location.href;

    name = name.replace(/[\[\]]/g, "\\$&");

    const regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"), results = regex.exec(url);

    if (!results)
        return null;

    if (!results[2])
        return "";

    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

function clearCachedCredentials(securedUrl, successCallback) {
    if (isIE) {
        document.execCommand("ClearAuthenticationCache", "false");

        if (successCallback)
            successCallback(true);
    } else {
        const xhr = new XMLHttpRequest();

        xhr.open("GET", securedUrl, true);

        xhr.setRequestHeader("Content-type", "application/json");

        // Send in an invalid set of credentials, i.e., base64 encoded _logout:_logout:
        xhr.setRequestHeader("Authorization", "Basic X2xvZ291dDpfbG9nb3V0");

        if (successCallback)
            xhr.onreadystatechange = function() {
                if (this.readyState === XMLHttpRequest.DONE)
                    successCallback(this.status === 401 || this.status === 403);
            };

        xhr.send();
    }
}

const persistentStorage = (function() {
    function storageAvailable(type) {
        var storage = [];

        try {
            storage = window[type];
            const x = "__storage_test__";
            storage.setItem(x, x);
            storage.removeItem(x);
            return true;
        } catch (e) {
            return e instanceof DOMException &&
                (
                    // everything except Firefox
                    e.code === 22 ||
                        // Firefox
                        e.code === 1014 ||
                        // test name field too, because code might not be present
                        // everything except Firefox
                        e.name === "QuotaExceededError" ||
                        // Firefox
                        e.name === "NS_ERROR_DOM_QUOTA_REACHED"
                ) &&
                // acknowledge QuotaExceededError only if there's something already stored
                storage.length !== 0;
        }
    }

    function fakeStorage() {
        return {
            getItem: function() {},
            setItem: function() {},
            removeItem: function() {}
        };
    }

    if (storageAvailable("localStorage"))
        return localStorage;
    else
        return fakeStorage();
})();

// Number functions
Number.prototype.truncate = function() {
    if (typeof Math.trunc !== "function")
        return parseInt(this.toString());

    return Math.trunc(this);
};

Number.prototype.padLeft = function(totalWidth, paddingChar) {
    return this.truncate().toString().padLeft(totalWidth, paddingChar || "0");
};

Number.prototype.padRight = function(totalWidth, paddingChar) {
    return this.truncate().toString().padRight(totalWidth, paddingChar || "0");
};

// Array functions

// Combines a dictionary of key-value pairs into a string. Values will be escaped within startValueDelimiter and endValueDelimiter
// to contain nested key/value pair expressions like the following: "normalKVP=-1; nestedKVP={p1=true; p2=0.001}" when either the
// parameterDelimiter or the keyValueDelimiter are detected in the value of the key/value pair.
function joinKeyValuePairs(source, parameterDelimiter, keyValueDelimiter, startValueDelimiter, endValueDelimiter) {
    if (!parameterDelimiter)
        parameterDelimiter = ";";

    if (!keyValueDelimiter)
        keyValueDelimiter = "=";

    if (!startValueDelimiter)
        startValueDelimiter = "{";

    if (!endValueDelimiter)
        endValueDelimiter = "}";

    const values = [];

    for (let key in source) {
        if (source.hasOwnProperty(key)) {
            let value = source[key];

            if (isBool(value))
                value = value.toString().toLowerCase();
            else
                value = value ? value.toString() : "";

            if (value.indexOf(parameterDelimiter) >= 0 ||
                value.indexOf(keyValueDelimiter) >= 0 ||
                value.indexOf(startValueDelimiter) >= 0 ||
                value.indexOf(endValueDelimiter) >= 0)
                value = startValueDelimiter + value + endValueDelimiter;

            values.push(key + keyValueDelimiter + value);
        }
    }

    return values.join(parameterDelimiter + " ");
}

Array.prototype.joinKeyValuePairs =
    function(parameterDelimiter, keyValueDelimiter, startValueDelimiter, endValueDelimiter) {
        return joinKeyValuePairs(this, parameterDelimiter, keyValueDelimiter, startValueDelimiter, endValueDelimiter);
    };

Array.prototype.forEachWithDelay = function(iterationCallback, timeout, completedCallback, thisArg) {
    var index = 0,
        count = this.length,
        self = this,
        next = function() {
            if (self[index])
                iterationCallback.call(thisArg || self, self[index], index, self);

            index++;

            if (index < count)
                setTimeout(next, timeout);
            else
                completedCallback.call(thisArg || self, self);
        };

    next();
};

if (!Array.prototype.any) {

    if (Array.prototype.some) {
        Array.prototype.any = Array.prototype.some;
    } else {
        Array.prototype.any = function(callback, thisArg) {
            var args;

            if (this === null)
                throw new TypeError("this is null or not defined");

            const array = Object(this);
            const length = array.length >>> 0; // Convert array length to positive integer

            if (typeof callback !== "function")
                throw new TypeError();

            if (arguments.length > 1)
                args = thisArg;
            else
                args = undefined;

            var index = 0;

            while (index < length) {
                if (index in array) {
                    const element = array[index];

                    if (callback.call(args, element, index, array))
                        return true;
                }

                index++;
            }

            return false;
        };
    }
}

// Represents a dictionary style class with case-insensitive keys
function Dictionary(source) {
    const self = this;

    if (!source)
        source = [];

    self._keys = [];
    self._values = [];

    self.count = function() {
        var size = 0;

        for (let property in self._values) {
            if (self._values.hasOwnProperty(property))
                size++;
        }

        return size;
    };

    self.keys = function() {
        const keys = [];

        for (let property in self._keys) {
            if (self._keys.hasOwnProperty(property))
                keys.push(self._keys[property]);
        }

        return keys;
    };

    self.values = function() {
        const values = [];

        for (let property in self._values) {
            if (self._keys.hasOwnProperty(property))
                values.push(self._values[property]);
        }

        return values;
    };

    self.get = function(key) {
        return self._values[String(key).toLowerCase()];
    };

    self.set = function(key, value) {
        const lkey = String(key).toLowerCase();

        if (!self._keys[lkey] || self._keys[lkey].toLowerCase() !== key)
            self._keys[lkey] = key;

        if (isBool(value))
            self._values[lkey] = getBool(value);
        else
            self._values[lkey] = value;
    };

    self.remove = function(key) {
        const lkey = String(key).toLowerCase();
        delete self._keys[lkey];
        delete self._values[lkey];
    };

    self.containsKey = function(key) {
        const lkey = String(key).toLowerCase();

        for (let property in self._values) {
            if (self._values.hasOwnProperty(property) && property === lkey)
                return true;
        }

        return false;
    };

    self.containsValue = function(value) {
        for (let property in self._values) {
            if (self._values.hasOwnProperty(property) && self._values[property] === value)
                return true;
        }

        return false;
    };

    self.clear = function() {
        self._keys = [];
        self._values = [];
    };

    self.joinKeyValuePairs = function(parameterDelimiter, keyValueDelimiter, startValueDelimiter, endValueDelimiter) {
        const keyValuePairs = [];

        for (let property in self._values) {
            if (self._values.hasOwnProperty(property))
                keyValuePairs[self._keys[property]] = self._values[property];
        }

        return keyValuePairs.joinKeyValuePairs(parameterDelimiter,
            keyValueDelimiter,
            startValueDelimiter,
            endValueDelimiter);
    };

    self.pushAll = function(source) {
        for (let property in source)
            if (source.hasOwnProperty(property))
                self.set(property, source[property]);
    };

    self.toObservableDictionary = function(useLowerKeys) {
        // See ko.observableDictionary.js
        const observableDictionary = new ko.observableDictionary();

        for (let property in self._values) {
            if (self._values.hasOwnProperty(property))
                observableDictionary.push(useLowerKeys ? property : self._keys[property], self._values[property]);
        }

        return observableDictionary;
    };

    self.updateObservableDictionary = function(observableDictionary, useLowerKeys) {
        for (let property in self._values) {
            if (self._values.hasOwnProperty(property))
                observableDictionary.set(useLowerKeys ? property : self._keys[property], self._values[property]);
        }
    };

    // Construction
    if (source instanceof Dictionary) {
        for (let property in source._values)
            if (source._values.hasOwnProperty(property))
                self.set(source._keys[property], source._values[property]);
    } else {
        for (let property in source) {
            if (source.hasOwnProperty(property))
                self.set(property, source[property]);
        }
    }
}

Dictionary.fromObservableDictionary = function(observableDictionary) {
    const dictionary = new Dictionary();
    dictionary.pushAll(observableDictionary.toJSON());
    return dictionary;
};

// String functions
function isEmpty(str) {
    return !str || String(str).length === 0;
}

String.prototype.trimLeft = String.prototype.trimLeft ||
    function() {
        return this.replace(/^\s+/, "");
    };

String.prototype.truncate = function(limit) {
    const text = this.trim();

    if (text.length > limit)
        return text.substr(0, limit - 3) + "...";

    return text;
};

String.prototype.replaceAll = function(findText, replaceWith, ignoreCase) {
    return this.replace(
            new RegExp(findText.replace(/([\/\,\!\\\^\$\{\}\[\]\(\)\.\*\+\?\|\<\>\-\&])/g, "\\$&"),
                ignoreCase ? "gi" : "g"),
            typeof replaceWith === "string")
        ? replaceWith.replace(/\$/g, "$$$$")
        : replaceWith;
};

if (!String.prototype.endsWith) {
    String.prototype.endsWith = function(searchString, position) {
        const subjectString = this.toString();

        if (typeof position !== 'number' ||
            !isFinite(position) ||
            Math.floor(position) !== position ||
            position > subjectString.length)
            position = subjectString.length;

        position -= searchString.length;

        const lastIndex = subjectString.lastIndexOf(searchString, position);

        return lastIndex !== -1 && lastIndex === position;
    };
}

if (!String.format) {
    String.format = function(format) {
        var text = format;

        for (let i = 1; i < arguments.length; i++)
            text = text.replaceAll("{" + (i - 1) + "}", arguments[i]);

        return text;
    };
}

String.prototype.padLeft = function(totalWidth, paddingChar) {
    if (totalWidth > this.length)
        return Array(totalWidth - this.length + 1).join(paddingChar || " ") + this;

    return this;
};

String.prototype.padRight = function(totalWidth, paddingChar) {
    if (totalWidth > this.length)
        return this + Array(totalWidth - this.length + 1).join(paddingChar || " ");

    return this;
};

String.prototype.countOccurrences = function(searchString) {
    return this.split(searchString).length - 1;
};

if (!String.prototype.startsWith) {
    String.prototype.startsWith = function(searchString, position) {
        position = position || 0;
        return this.substr(position, searchString.length) === searchString;
    };
}

String.prototype.regexEncode = function() {
    return "\\u" + this.charCodeAt(0).toString(16).padLeft(4, "0");
};

// Returns a Dictionary of the parsed key/value pair expressions from a string. Parameter pairs are delimited by keyValueDelimiter
// and multiple pairs separated by parameterDelimiter. Supports encapsulated nested expressions.
String.prototype.parseKeyValuePairs = function(parameterDelimiter,
    keyValueDelimiter,
    startValueDelimiter,
    endValueDelimiter,
    ignoreDuplicateKeys) {
    if (!parameterDelimiter)
        parameterDelimiter = ";";

    if (!keyValueDelimiter)
        keyValueDelimiter = "=";

    if (!startValueDelimiter)
        startValueDelimiter = "{";

    if (!endValueDelimiter)
        endValueDelimiter = "}";

    if (ignoreDuplicateKeys === undefined)
        ignoreDuplicateKeys = true;

    if (parameterDelimiter === keyValueDelimiter ||
        parameterDelimiter === startValueDelimiter ||
        parameterDelimiter === endValueDelimiter ||
        keyValueDelimiter === startValueDelimiter ||
        keyValueDelimiter === endValueDelimiter ||
        startValueDelimiter === endValueDelimiter)
        throw "All delimiters must be unique";

    const escapedParameterDelimiter = parameterDelimiter.regexEncode();
    const escapedKeyValueDelimiter = keyValueDelimiter.regexEncode();
    const escapedStartValueDelimiter = startValueDelimiter.regexEncode();
    const escapedEndValueDelimiter = endValueDelimiter.regexEncode();
    const backslashDelimiter = "\\".regexEncode();

    var keyValuePairs = new Dictionary();
    var escapedValue = [];
    var valueEscaped = false;
    var delimiterDepth = 0;

    // Escape any parameter or key/value delimiters within tagged value sequences
    //      For example, the following string:
    //          "normalKVP=-1; nestedKVP={p1=true; p2=false}")
    //      would be encoded as:
    //          "normalKVP=-1; nestedKVP=p1\\u003dtrue\\u003b p2\\u003dfalse")
    for (let i = 0; i < this.length; i++) {
        var character = this[i];

        if (character === startValueDelimiter) {
            if (!valueEscaped) {
                valueEscaped = true;
                continue; // Don't add tag start delimiter to final value
            }

            // Handle nested delimiters
            delimiterDepth++;
        }

        if (character === endValueDelimiter) {
            if (valueEscaped) {
                if (delimiterDepth > 0) {
                    // Handle nested delimiters
                    delimiterDepth--;
                } else {
                    valueEscaped = false;
                    continue; // Don't add tag stop delimiter to final value
                }
            } else {
                throw "Failed to parse key/value pairs: invalid delimiter mismatch. Encountered end value delimiter \"" +
                    endValueDelimiter +
                    "\" before start value delimiter \"" +
                    startValueDelimiter +
                    "\".";
            }
        }

        if (valueEscaped) {
            // Escape any delimiter characters inside nested key/value pair
            if (character === parameterDelimiter)
                escapedValue.push(escapedParameterDelimiter);
            else if (character === keyValueDelimiter)
                escapedValue.push(escapedKeyValueDelimiter);
            else if (character === startValueDelimiter)
                escapedValue.push(escapedStartValueDelimiter);
            else if (character === endValueDelimiter)
                escapedValue.push(escapedEndValueDelimiter);
            else if (character === "\\")
                escapedValue.push(backslashDelimiter);
            else
                escapedValue.push(character);
        } else {
            if (character === "\\")
                escapedValue.push(backslashDelimiter);
            else
                escapedValue.push(character);
        }
    }

    if (delimiterDepth !== 0 || valueEscaped) {
        // If value is still escaped, tagged expression was not terminated
        if (valueEscaped)
            delimiterDepth = 1;

        throw "Failed to parse key/value pairs: invalid delimiter mismatch. Encountered more " +
            (delimiterDepth > 0
                ? "start value delimiters \"" + startValueDelimiter + "\""
                : "end value delimiters \"" + endValueDelimiter + "\"") +
            " than " +
            (delimiterDepth < 0
                ? "start value delimiters \"" + startValueDelimiter + "\""
                : "end value delimiters \"" + endValueDelimiter + "\"") +
            ".";
    }

    // Parse key/value pairs from escaped value
    var pairs = escapedValue.join("").split(parameterDelimiter);

    for (let i = 0; i < pairs.length; i++) {
        // Separate key from value
        var elements = pairs[i].split(keyValueDelimiter);

        if (elements.length === 2) {
            // Get key
            var key = elements[0].trim();

            // Get unescaped value
            var unescapedValue = elements[1].trim().replaceAll(escapedParameterDelimiter, parameterDelimiter)
                .replaceAll(escapedKeyValueDelimiter, keyValueDelimiter)
                .replaceAll(escapedStartValueDelimiter, startValueDelimiter)
                .replaceAll(escapedEndValueDelimiter, endValueDelimiter).replaceAll(backslashDelimiter, "\\");

            // Add key/value pair to dictionary
            if (ignoreDuplicateKeys) {
                // Add or replace key elements with unescaped value
                keyValuePairs.set(key, unescapedValue);
            } else {
                // Add key elements with unescaped value throwing an exception for encountered duplicate keys
                if (keyValuePairs.containsKey(key))
                    throw "Failed to parse key/value pairs: duplicate key encountered. Key \"" +
                        key +
                        "\" is not unique within the string: \"" +
                        this +
                        "\"";

                keyValuePairs.set(key, unescapedValue);
            }
        }
    }

    return keyValuePairs;
};

// Renders URLs and e-mail addresses as clickable links
function renderHotLinks(sourceText, target) {
    if (target === undefined)
        target = "_blank";

    sourceText = sourceText.toString();

    var replacedText;

    // URLs starting with http://, https://, or ftp://
    const replacePattern1 = /(\b(https?|ftp):\/\/[-A-Z0-9+&@#\/%?=~_|!:,.;]*[-A-Z0-9+&@#\/%=~_|])/gim;
    replacedText = sourceText.replace(replacePattern1, "<a href=\"$1\" target=\"" + target + "\">$1</a>");

    // URLs starting with "www." without // before it
    const replacePattern2 = /(^|[^\/])(www\.[\S]+(\b|$))/gim;
    replacedText = replacedText.replace(replacePattern2, "$1<a href=\"http://$2\" target=\"" + target + "\">$2</a>");

    // Change e-mail addresses to mailto: links
    const replacePattern3 = /(([a-zA-Z0-9\-\_\.])+@[a-zA-Z\_]+?(\.[a-zA-Z]{2,6})+)/gim;
    replacedText = replacedText.replace(replacePattern3, "<a href=\"mailto:$1\">$1</a>");

    return replacedText;
}

// Date Functions
Date.prototype.addDays = function(days) {
    return new Date(this.setDate(this.getDate() + days));
};

Date.prototype.toUTC = function() {
    this.setMinutes(this.getMinutes() - this.getTimezoneOffset());
    return this;
};

Date.prototype.daysBetween = function(startDate) {
    const millisecondsPerDay = 24 * 60 * 60 * 1000;
    return (this.toUTC() - startDate.toUTC()) / millisecondsPerDay;
};

String.prototype.toDate = function() {
    return new Date(Date.parse(this));
};

String.prototype.formatDate = function(format, utc) {
    return formatDate(this.toDate(), format, utc);
};

Date.prototype.formatDate = function(format, utc) {
    return formatDate(this, format, utc);
};

function formatDate(date, format, utc) {
    if (typeof date === "string")
        return formatDate(date.toDate(), format, utc);

    if (format === undefined && DateTimeFormat !== undefined)
        format = DateTimeFormat;

    if (utc === undefined)
        utc = true;

    function ii(i, len) {
        var ss = i + "";
        len = len || 2;
        while (ss.length < len) ss = "0" + ss;
        return ss;
    }

    if (!(format === null || format === undefined)) {
        var MMMM = [
            "\x00", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
            "November", "December"
        ];
        var MMM = ["\x01", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
        var dddd = ["\x02", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
        var ddd = ["\x03", "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

        var y = utc ? date.getUTCFullYear() : date.getFullYear();
        format = format.replace(/(^|[^\\])yyyy+/g, "$1" + y);
        format = format.replace(/(^|[^\\])yy/g, "$1" + y.toString().substr(2, 2));
        format = format.replace(/(^|[^\\])y/g, "$1" + y);

        var M = (utc ? date.getUTCMonth() : date.getMonth()) + 1;
        format = format.replace(/(^|[^\\])MMMM+/g, "$1" + MMMM[0]);
        format = format.replace(/(^|[^\\])MMM/g, "$1" + MMM[0]);
        format = format.replace(/(^|[^\\])MM/g, "$1" + ii(M));
        format = format.replace(/(^|[^\\])M/g, "$1" + M);

        var d = utc ? date.getUTCDate() : date.getDate();
        format = format.replace(/(^|[^\\])dddd+/g, "$1" + dddd[0]);
        format = format.replace(/(^|[^\\])ddd/g, "$1" + ddd[0]);
        format = format.replace(/(^|[^\\])dd/g, "$1" + ii(d));
        format = format.replace(/(^|[^\\])d/g, "$1" + d);

        var H = utc ? date.getUTCHours() : date.getHours();
        format = format.replace(/(^|[^\\])HH+/g, "$1" + ii(H));
        format = format.replace(/(^|[^\\])H/g, "$1" + H);

        var h = H > 12 ? H - 12 : H === 0 ? 12 : H;
        format = format.replace(/(^|[^\\])hh+/g, "$1" + ii(h));
        format = format.replace(/(^|[^\\])h/g, "$1" + h);

        var m = utc ? date.getUTCMinutes() : date.getMinutes();
        format = format.replace(/(^|[^\\])mm+/g, "$1" + ii(m));
        format = format.replace(/(^|[^\\])m/g, "$1" + m);

        var s = utc ? date.getUTCSeconds() : date.getSeconds();
        format = format.replace(/(^|[^\\])ss+/g, "$1" + ii(s));
        format = format.replace(/(^|[^\\])s/g, "$1" + s);

        var f = utc ? date.getUTCMilliseconds() : date.getMilliseconds();
        format = format.replace(/(^|[^\\])fff+/g, "$1" + ii(f, 3));
        f = Math.round(f / 10);
        format = format.replace(/(^|[^\\])ff/g, "$1" + ii(f));
        f = Math.round(f / 10);
        format = format.replace(/(^|[^\\])f/g, "$1" + f);

        var T = H < 12 ? "AM" : "PM";
        format = format.replace(/(^|[^\\])TT+/g, "$1" + T);
        format = format.replace(/(^|[^\\])T/g, "$1" + T.charAt(0));

        var t = T.toLowerCase();
        format = format.replace(/(^|[^\\])tt+/g, "$1" + t);
        format = format.replace(/(^|[^\\])t/g, "$1" + t.charAt(0));

        var tz = -date.getTimezoneOffset();
        var K = utc || !tz ? "Z" : tz > 0 ? "+" : "-";
        if (!utc) {
            tz = Math.abs(tz);
            var tzHrs = Math.floor(tz / 60);
            var tzMin = tz % 60;
            K += ii(tzHrs) + ":" + ii(tzMin);
        }

        format = format.replace(/(^|[^\\])K/g, "$1" + K);
        var day = (utc ? date.getUTCDay() : date.getDay()) + 1;

        format = format.replace(new RegExp(dddd[0], "g"), dddd[day]);
        format = format.replace(new RegExp(ddd[0], "g"), ddd[day]);

        format = format.replace(new RegExp(MMMM[0], "g"), MMMM[M]);
        format = format.replace(new RegExp(MMM[0], "g"), MMM[M]);

        format = format.replace(/\\(.)/g, "$1");

        return format;
    }

    return null;
}

// jQuery extensions
$.fn.enable = function() {
    return this.each(function() {
        this.disabled = false;
    });
};

$.fn.disable = function() {
    return this.each(function() {
        this.disabled = true;
    });
};

$.fn.visible = function() {
    return this.each(function() {
        $(this).css("visibility", "visible");
    });
};

$.fn.invisible = function() {
    return this.each(function() {
        $(this).css("visibility", "hidden");
    });
};

$.fn.readCookie = function(name) {
    $(this).val(Cookies.get(name));
};

$.fn.writeCookie = function(name, expiration) {
    if (expiration) // Date or number of days
        Cookies.set(name, $(this).val(), { expires: expiration });
    else
        Cookies.set(name, $(this).val());
};

$.fn.removeCookie = function(name) {
    Cookies.remove(name);
};

$.fn.paddingHeight = function() {
    return this.outerHeight(true) - this.height();
};

$.fn.paddingWidth = function() {
    return this.outerWidth(true) - this.width();
};

// Call function once at start-up to auto-size drop-down to selected option contents
$.fn.autoSizeSelect = function() {
    return this.each(function() {
        $(this).change(function() {
            const arrowWidth = 30;
            const $this = $(this);

            // Create test element
            const text = $this.find("option:selected").text();
            const $test = $("<span>").html(text);

            // Add to body, get width, and get out
            $test.appendTo("body");
            const width = $test.width();
            $test.remove();

            // Set select width
            $this.width(width + arrowWidth);
        }).change();
    });
};

// Cell truncations should only be used with .table-cell-hard-wrap style
$.fn.truncateToWidth = function(text, rows) {
    if (isEmpty(text))
        return "";

    if (!rows)
        rows = 1;

    textMetrics.font = this.css("font");

    var targetWidth = this.innerWidth();

    if (rows > 1)
        targetWidth *= (isIE ? 0.45 : 0.75) * rows;

    if (targetWidth > 30) {
        let textWidth = textMetrics.measureText(text).width;
        let limit = Math.min(text.length, Math.ceil(targetWidth / (textWidth / text.length)));

        while (textWidth > targetWidth && limit > 3) {
            limit--;
            text = text.truncate(limit);
            textWidth = textMetrics.measureText(text).width;
        }
    } else {
        text = text.charAt(0) + "...";
    }

    return text;
};

// The following target arrays of promises
$.fn.whenAny = function() {
    var finish = $.Deferred();

    if (this.length === 0)
        finish.resolve();
    else
        $.each(this,
            function(index, deferred) {
                deferred.done(finish.resolve);
            });

    return finish.promise();
};

$.fn.whenAll = function() {
    if (this.length > 0)
        return $.when.apply($, this);

    return $.Deferred().resolve().promise();
};

// Fix for IE11 not having this function
// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/from
if (!Array.from) {
    Array.from = (function() {
        var toStr = Object.prototype.toString;
        var isCallable = function(fn) {
            return typeof fn === 'function' || toStr.call(fn) === '[object Function]';
        };
        var toInteger = function(value) {
            var number = Number(value);
            if (isNaN(number)) {
                return 0;
            }
            if (number === 0 || !isFinite(number)) {
                return number;
            }
            return (number > 0 ? 1 : -1) * Math.floor(Math.abs(number));
        };
        var maxSafeInteger = Math.pow(2, 53) - 1;
        var toLength = function(value) {
            var len = toInteger(value);
            return Math.min(Math.max(len, 0), maxSafeInteger);
        };

        // The length property of the from method is 1.
        return function from(arrayLike /*, mapFn, thisArg */) {
            // 1. Let C be the this value.
            var C = this;

            // 2. Let items be ToObject(arrayLike).
            var items = Object(arrayLike);

            // 3. ReturnIfAbrupt(items).
            if (arrayLike === null) {
                throw new TypeError('Array.from requires an array-like object - not null or undefined');
            }

            // 4. If mapfn is undefined, then let mapping be false.
            var mapFn = arguments.length > 1 ? arguments[1] : void undefined;
            var T;
            if (typeof mapFn !== 'undefined') {
                // 5. else
                // 5. a If IsCallable(mapfn) is false, throw a TypeError exception.
                if (!isCallable(mapFn)) {
                    throw new TypeError('Array.from: when provided, the second argument must be a function');
                }

                // 5. b. If thisArg was supplied, let T be thisArg; else let T be undefined.
                if (arguments.length > 2) {
                    T = arguments[2];
                }
            }

            // 10. Let lenValue be Get(items, "length").
            // 11. Let len be ToLength(lenValue).
            var len = toLength(items.length);

            // 13. If IsConstructor(C) is true, then
            // 13. a. Let A be the result of calling the [[Construct]] internal method 
            // of C with an argument list containing the single item len.
            // 14. a. Else, Let A be ArrayCreate(len).
            var A = isCallable(C) ? Object(new C(len)) : new Array(len);

            // 16. Let k be 0.
            var k = 0;
            // 17. Repeat, while k < len… (also steps a - h)
            var kValue;
            while (k < len) {
                kValue = items[k];
                if (mapFn) {
                    A[k] = typeof T === 'undefined' ? mapFn(kValue, k) : mapFn.call(T, kValue, k);
                } else {
                    A[k] = kValue;
                }
                k += 1;
            }
            // 18. Let putStatus be Put(A, "length", len, true).
            A.length = len;
            // 20. Return A.
            return A;
        };
    }());
}

// Fix for IE11 not having this function
// https://tc39.github.io/ecma262/#sec-array.prototype.find
// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/find
if (!Array.prototype.find) {
    Object.defineProperty(Array.prototype,
        'find',
        {
            value: function(predicate) {
                // 1. Let O be ? ToObject(this value).
                if (this === null) {
                    throw new TypeError('"this" is null or not defined');
                }

                var o = Object(this);

                // 2. Let len be ? ToLength(? Get(O, "length")).
                var len = o.length >>> 0;

                // 3. If IsCallable(predicate) is false, throw a TypeError exception.
                if (typeof predicate !== 'function') {
                    throw new TypeError('predicate must be a function');
                }

                // 4. If thisArg was supplied, let T be thisArg; else let T be undefined.
                var thisArg = arguments[1];

                // 5. Let k be 0.
                var k = 0;

                // 6. Repeat, while k < len
                while (k < len) {
                    // a. Let Pk be ! ToString(k).
                    // b. Let kValue be ? Get(O, Pk).
                    // c. Let testResult be ToBoolean(? Call(predicate, T, « kValue, k, O »)).
                    // d. If testResult is true, return kValue.
                    var kValue = o[k];
                    if (predicate.call(thisArg, kValue, k, o)) {
                        return kValue;
                    }
                    // e. Increase k by 1.
                    k++;
                }

                // 7. Return undefined.
                return undefined;
            },
            configurable: true,
            writable: true
        });
}

// Fix for IE11 not having this function
// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Array/fill

if (!Array.prototype.fill) {
    Object.defineProperty(Array.prototype,
        'fill',
        {
            value: function(value) {

                // Steps 1-2.
                if (this === null) {
                    throw new TypeError('this is null or not defined');
                }

                var O = Object(this);

                // Steps 3-5.
                var len = O.length >>> 0;

                // Steps 6-7.
                var start = arguments[1];
                var relativeStart = start >> 0;

                // Step 8.
                var k = relativeStart < 0 ? Math.max(len + relativeStart, 0) : Math.min(relativeStart, len);

                // Steps 9-10.
                var end = arguments[2];
                var relativeEnd = end === undefined ? len : end >> 0;

                // Step 11.
                var final = relativeEnd < 0 ? Math.max(len + relativeEnd, 0) : Math.min(relativeEnd, len);

                // Step 12.
                while (k < final) {
                    O[k] = value;
                    k++;
                }

                // Step 13.
                return O;
            }
        });
}

// Fix for IE11 not having this function
// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/parseFloat
if (Number.parseFloat === void 0)
    Number.parseFloat = parseFloat;

// Fix for IE11 not having this function
// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/isInteger
Number.isInteger = Number.isInteger ||
    function(value) {
        return typeof value === 'number' &&
            isFinite(value) &&
            Math.floor(value) === value;
    };