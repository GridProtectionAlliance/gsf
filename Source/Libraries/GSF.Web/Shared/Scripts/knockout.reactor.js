/*! ko-reactor v1.4.0
 * The MIT License (MIT)
 * Copyright (c) 2018 Ziad Jeeroburkhan */
// Deep observer plugin for Knockout http://knockoutjs.com/
// (c) Ziad Jeeroburkhan
// License: MIT (http://www.opensource.org/licenses/mit-license.php)
; (function (factory) {
    // CommonJS
    if (typeof require === 'function' && typeof exports === 'object' && typeof module === 'object') {
        factory(require('knockout'));
        // AMD
    } else if (typeof define === 'function' && define.amd) {
        define(['knockout'], factory);
        // Normal script tag
    } else {
        factory(window.ko);
    }
}(function (ko) {
    ko.subscribable.fn['watch'] = function (targetOrCallback, options, evaluatorCallback, context) {
        /// <summary>
        ///     Track and manage changes within the chained observable down to any given level. 
        /// </summary>
        /// <param name="targetOrCallback">
        ///      The subscription callback function or an object containing the subscribables to be watched.
        /// </param>
        /// <param name="options" type="object">
        ///     false -> Disables tracking on the chained observable.
        ///     { depth: 2 } -> Track all nested subscribables down to the 2nd level(default is 1).<br/>
        ///     { depth: -1 } -> Track all nested subscribables.<br/>
        ///     { hide: [...] } -> Property or array of properties to be ignored.<br/>
        ///     { hideArrays: true } -> Ignore all nested arrays.<br/>
        ///     { hideWrappedValues: true } -> Ignore observables wrapped under yet another parent observable.<br/>
        ///     { mutable: true } -> Dynamically adapt to changes made to the target structure through any subscribable.<br/>
        ///     { watchedOnly: true } -> Watch only subscribables tagged with .watch().<br/>
        ///     { beforeWatch: function(parents, child) {...} } -> Function called prior to creating a subscription. Returning false aborts the operation and ignores its children.<br/>
        ///     { wrap: true } -> Wrap all fields into observables. This happens on the fly for new array items(or child objects when mutable is set to true).<br/>
        ///     { beforeWrap: function(parents, field, value) {...} } -> Function called prior to wrapping a value into an observable. Returning false leaves it as it is.<br/>
        ///     { tagFields: true } -> Add the property '_fieldName' under each property for textual identification.<br/>
        ///     { tagFields: 'parentsOnly' } -> Same as above except that it is limited to parent properties only.<br/>
        ///     { oldValues: 3 } -> Keep the last three values for each subscribable under the property 'oldValues'.<br/>
        ///     { async: false } -> Start watching new objects synchronously
        ///     { splitArrayChanges: false } -> receive a single notification for array changes as an array of "items" instead of multiple notifications
        ///     { seal: true } -> Prevent any subsequent watcher from watching the target again.<br/>
        ///     { unloop: true } -> Avoid circular paths through the use of a breadcrumb property '_watcher' set at each node level.<br/>
        /// </param>
        /// <param name="evaluatorCallback" type="function">
        ///     The  callback function called during changes. Any return value is assigned to the chained observable.
        /// </param>

        var targetType = typeof targetOrCallback;

        if (targetType === 'boolean' || targetType === 'undefined') {
            // Turn on or off the watcher for the specified target along with any of its children.
            ko.watch(this, { enabled: targetOrCallback !== false });
        } else if (targetType === 'function' && !ko.isSubscribable(targetOrCallback)) {
            // Target the chained subscribable itself if no target subscribable or object was passed.
            ko.watch(this, options || {}, targetOrCallback, context || this);
        } else {
            ko.watch(targetOrCallback, options, evaluatorCallback, context || this);
        }

        return this;
    };

    ko['watch'] = function (target, options, evaluatorCallback, context) {
        /// <summary>
        ///     Track and manage changes within a specific target object down to any given level.
        /// </summary>
        /// <param name="target">
        ///     An object or function containing the targeted subscribable(s).
        /// </param>
        /// <param name="options" type="object">
        ///     { depth: 2 } -> Track all nested subscribables down to the 2nd level(default is 1).<br/>
        ///     { depth: -1 } -> Track all nested subscribables.<br/>
        ///     { hide: [...] } -> Property or array of properties to be ignored.<br/>
        ///     { hideArrays: true } -> Ignore all nested arrays.<br/>
        ///     { hideWrappedValues: true } -> Ignore observables wrapped under yet another parent observable.<br/>
        ///     { mutable: true } -> Dynamically adapt to changes made to the target structure through any subscribable.<br/>
        ///     { watchedOnly: true } -> Watch only subscribables tagged with .watch().<br/>
        ///     { beforeWatch: function(parents, child) {...} } -> Function called prior to creating a subscription. Returning false aborts the operation and ignores its children.<br/>
        ///     { wrap: true } -> Wrap all fields into observables. This happens on the fly for new array items(or child objects when mutable is set to true).<br/>
        ///     { beforeWrap: function(parents, field, value) {...} } -> Function called prior to wrapping a value into an observable. Returning false leaves it as it is.<br/>
        ///     { tagFields: true } -> Add the property '_fieldName' under each property for textual identification.<br/>
        ///     { tagFields: 'parentsOnly' } -> Same as above except that it is limited to parent properties only.<br/>
        ///     { oldValues: 3 } -> Keep the last three values for each subscribable under the property 'oldValues'.<br/>
        ///     { seal: true } -> Prevent any subsequent watcher from watching the target again.<br/>
        ///     { unloop: true } -> Avoid circular paths through the use of a breadcrumb property '_watcher' set at each node level.<br/>
        ///     { getter: function(parents, child, property) {...} } -> Function used to retrieve the property value from the given child. False can be returned to ignore the property.<br/>
        /// </param>
        /// <param name="evaluatorCallback" type="function">
        ///     The callback function called during changes.
        /// </param>

        if (typeof options === 'function') {
            context = context || evaluatorCallback;
            evaluatorCallback = options;
            options = {};
        }

        context = context || this;

        function watchChildren(child, parent, grandParents, unwatch, keepOffParentList, fieldName) {
            if (child && options.depth !== 0 && (options.depth === -1 || grandParents.length < (options.depth || 1))) {

                // Proceed on watched children only when in watched-only mode.
                if (options.watchedOnly && !child.watchable && child != target)
                    return;

                // Setting the target as false prevents it from being watched later on.
                if (options.enabled === false || options.enabled === true)
                    child.watchable = options.enabled;

                // Ignore watch-disabled objects.
                if (child.watchable === false)
                    return;

                // Prevent subsequent watchers from watching the target when sealed.
                if (options.seal === true)
                    child.watchable = false;

                var type = typeof child;

                if (type === 'object' || type === 'function') {
                    // Bypass circular references.
                    if (child._watcher === context)
                        return;

                    // Ignore hidden objects. Also applies to any of their children.
                    if (options.hide)
                        if (ko.utils.arrayIndexOf(options.hide, child) > -1)
                            return;

                    // Merge parents. Using a fresh array so it is not referenced in the next recursion if any.
                    var parents = [].concat(grandParents, parent && parent !== target ? parent : []);

                    if (type === 'function') {
                        if (typeof child['notifySubscribers'] == 'function') {
                            // Target is a subscribable. Watch it.
                            if (evaluatorCallback) {
                                if (options.enabled === true && child.watchable === false)
                                    // Only waking up an existing watcher. Let's not add another.
                                    return;

                                if (unwatch || !options.beforeWatch || options.beforeWatch.call(context, parents, child, fieldName) !== false) {
                                    var isArray = typeof child.pop === 'function';

                                    if (unwatch) {
                                        disposeWatcher(child);
                                    } else {
                                        assignWatcher(child, isArray, parents, keepOffParentList);
                                    }

                                    if (isArray) {
                                        watchChildren(child(), keepOffParentList ? null : child, parents, unwatch, true);
                                        return true;
                                    } else {
                                        if (options.hideWrappedValues !== true)
                                            return watchChildren(child(), keepOffParentList ? null : child, parents, unwatch, true);
                                    }
                                }
                            }
                        }
                    } else {
                        if (Object.prototype.toString.call(child) === '[object Object]') {
                            ko.utils.objectForEach(child, function (property, sub) {
                                sub = options.getter ? options.getter.call(context, parents, child, property) : sub;
                                if (sub) {
                                    if (options.wrap) {
                                        // Wrap simple objects and arrays into observables.
                                        var type = Object.prototype.toString.call(sub);
                                        if (type !== '[object Function]' && type !== '[object Object]') {
                                            if (!options.beforeWrap || options.beforeWrap.call(context, parents, child, sub) !== false) {
                                                sub = child[property] = type === '[object Array]'
                                                    ? ko.observableArray(sub)
                                                    : ko.observable(sub);
                                            }
                                        }
                                    }

                                    if (options.unloop)
                                        sub._watcher = unwatch ? undefined : context;

                                    var hasChildren = watchChildren(sub, keepOffParentList ? null : child, parents, unwatch, null, property);

                                    if (options.tagFields && sub._fieldName === undefined)
                                        if (hasChildren
                                            || (options.tagFields !== 'parentsOnly' && typeof sub === 'function' || typeof sub === 'object'))
                                            sub._fieldName = property;
                                }
                            });
                        } else { // '[object Array]'
                            if (options.hideArrays !== true)
                                for (var i = 0; i < child.length; i++)
                                    watchChildren(child[i], keepOffParentList ? null : child, parents, unwatch);
                        }

                        return true;
                    }
                }
            }
        }

        // Subscriptions are stored under either the _subscriptions field for the debug version
        // or the F, H or M fields when minified depending on the version used.
        // NOTE: we used to use ko.DEBUG to detect the debug versionbut it was removed in 3.4.0+,
        //       so we now check the existence of a "subscription" function.
        var subscriptionsField;
        switch (typeof ko.subscription == 'function' || ko.version) {
            case true: subscriptionsField = '_subscriptions'; break;
            case "3.0.0": subscriptionsField = 'F'; break;
            case "3.1.0": subscriptionsField = 'H'; break;
            case "3.2.0": subscriptionsField = 'M'; break;
            case "3.3.0": subscriptionsField = 'G'; break;
            case "3.4.0": subscriptionsField = 'K'; break;
            case "3.4.1": subscriptionsField = 'K'; break;
            case "3.4.2": subscriptionsField = 'F'; break;
            case "3.5.0-beta": subscriptionsField = 'S'; break;
            default: throw "Unsupported Knockout version. Only v3.0.0 to v3.5.0-beta are supported when minified. Current version is " + ko.version;
        }

        function disposeWatcher(child) {
            var subsc = child[subscriptionsField];

            if (subsc) {
                if (subsc.change)
                    for (var i = subsc.change.length - 1; i >= 0; i--)
                        if (subsc.change[i]._watcher === context)
                            subsc.change[i].dispose();

                if (subsc.beforeChange && (options.mutable || options.oldValues > 0))
                    // Also clean up any before-change subscriptions used for tracking old values.
                    for (var i = subsc.beforeChange.length - 1; i >= 0; i--)
                        if (subsc.beforeChange[i]._watcher === context)
                            subsc.beforeChange[i].dispose();

                if (subsc.arrayChange)
                    for (var i = subsc.arrayChange.length - 1; i >= 0; i--)
                        if (subsc.arrayChange[i]._watcher === context)
                            subsc.arrayChange[i].dispose();
            } else {
                throw "Subscriptions field (." + subscriptionsField + ") not defined for observable child " + (child._fieldName || "");
            }
        }

        function assignWatcher(child, isArray, parents, keepOffParentList) {
            if (isArray) {
                // Child is an observable array. Watch all changes within it.
                child.subscribe(function (changes) {
                    var returnValue;
                    if (options.splitArrayChanges === false) {
                        returnValue = evaluatorCallback.call(context, parents, child, changes);
                        if (returnValue !== undefined)
                            context(returnValue);
                    }
                    ko.utils.arrayForEach(changes, function (item) {
                        if (options.splitArrayChanges !== false) {
                            var returnValue = evaluatorCallback.call(context, parents, child, item);
                            if (returnValue !== undefined)
                                context(returnValue);
                        }

                        if (!item.moved) {
                            // Deleted or brand new item. Unwatch or watch it accordingly.
                            if (options.async === false) {
                                watchChildren(item.value, (keepOffParentList ? null : child), parents, item.status === 'deleted');
                            } else {
                                setTimeout(function () {
                                    watchChildren(item.value, (keepOffParentList ? null : child), parents, item.status === 'deleted');
                                });
                            }
                        }
                    });
                }, undefined, 'arrayChange')._watcher = context;

            } else {
                child.subscribe(function () {
                    if (child.watchable !== false) {
                        var returnValue = evaluatorCallback.call(context, parents, child);

                        if (returnValue !== undefined)
                            context(returnValue);

                        if (options.mutable && typeof child() === 'object') {
                            // Watch the new comer.
                            if (options.async === false) {
                                watchChildren(child(), (keepOffParentList ? null : child), parents, false, true);
                            } else {
                                setTimeout(function () {
                                    watchChildren(child(), (keepOffParentList ? null : child), parents, false, true);
                                });
                            }
                        }
                    }

                }, null, 'change')._watcher = context;

                if (options.oldValues > 0 || options.mutable) {
                    child.subscribe(function (oldValue) {
                        if (options.oldValues > 0) {
                            // Add old value to history list before every update.
                            var values = (child['oldValues']
                                ? child['oldValues']
                                : child['oldValues'] = []);

                            values.unshift(oldValue);

                            while (values.length > options.oldValues)
                                values.pop();
                        }

                        if (options.mutable && typeof oldValue === 'object')
                            // Clean up all subscriptions for the old child object.
                            watchChildren(oldValue, (keepOffParentList ? null : child), parents, true, true);

                    }, null, 'beforeChange')._watcher = context;
                }
            }
        }

        // Use a computed when targeting a non-watchable function.
        if (typeof target === 'function' && !ko.isSubscribable(target))
            return ko.computed(target, evaluatorCallback, options);

        watchChildren(target, null, []);

        return {
            dispose: function () {
                watchChildren(target, null, [], true);
            }
        };
    };

}));