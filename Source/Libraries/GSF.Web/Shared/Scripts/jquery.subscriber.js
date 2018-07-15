//******************************************************************************************************
//  jquery.subscriber.js - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  07/12/2018 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

/*
 * Subscriber API usage:
 *   var subscriber = $.subscriber();
 *
 *   // Subscriber callbacks
 *   subscriber.connectionEstablished = function () { }
 *   subscriber.connectionTerminated = function () { }
 *   subscriber.newMeasurements = function (measurements) { }
 *   subscriber.metadataReceived = function () { }
 *   subscriber.configurationChanged = function () { }
 *   subscriber.statusMessage = function (message) { }
 *   subscriber.processException = function (exception) { }
 *
 *   // API calls
 *   subscriber.connect().done(function () { });
 *   subscriber.sendCommand(commandCode, message).done(function () { });
 *   subscriber.getMetadata(tableName, whereExpression, sortField, takeCount).done(function (metadata) { });
 *   subscriber.subscribe(subscriptionInfo).done(function() { });
 *   subscriber.unsubscribe().done(function() { });
 *   subscriber.disconnect().done(function() { });
 *   subscriber.dispose().done(function() { });
 */
(function ($) {
    var server = $.connection.subscriberHub.server;
    var client = $.connection.subscriberHub.client;

    var subscribers = {};
    var nextSubscriberID = 0;

    client.connectionEstablished = function (subscriberID) {
        var subscriber = subscribers[subscriberID];

        if (subscriber === undefined)
            return;

        if (typeof (subscriber.connectionEstablished) === "function")
            subscriber.connectionEstablished();
    }

    client.connectionTerminated = function (subscriberID) {
        var subscriber = subscribers[subscriberID];

        if (subscriber === undefined)
            return;

        if (typeof (subscriber.connectionTerminated) === "function")
            subscriber.connectionTerminated();
        else if (typeof (subscriber.Start) === "function")
            subscriber.Start();
    }

    /**
     * measurements: an array of measurement objects
     *               { signalID, timestamp, value }
     */
    client.newMeasurements = function (subscriberID, measurements) {
        var subscriber = subscribers[subscriberID];

        if (subscriber === undefined)
            return;

        if (typeof (subscriber.newMeasurements) === "function")
            subscriber.newMeasurements(measurements);
    }

    client.metadataReceived = function (subscriberID) {
        var subscriber = subscribers[subscriberID];

        if (subscriber === undefined)
            return;

        if (typeof (subscriber.metadataReceived) === "function")
            subscriber.metadataReceived();
    }

    client.configurationChanged = function (subscriberID) {
        var subscriber = subscribers[subscriberID];

        if (subscriber === undefined)
            return;

        if (typeof (subscriber.configurationChanged) === "function")
            subscriber.configurationChanged();
    }

    /**
     * message: an information string for logging interactions with the data publisher
     */
    client.statusMessage = function (subscriberID, message) {
        var subscriber = subscribers[subscriberID];

        if (subscriber === undefined)
            return;

        if (typeof (subscriber.statusMessage) === "function")
            subscriber.statusMessage(message);
    }

    /**
     * exception: an object containing information about an error
     *            between the subscriber and publisher
     *            { Message, StackTrace, ... }
     */
    client.processException = function (subscriberID, exception) {
        var subscriber = subscribers[subscriberID];

        if (subscriber === undefined)
            return;

        if (typeof (subscriber.processException) === "function")
            subscriber.processException(exception);
    }

    $.subscriber = function () {
        var subscriberID = (nextSubscriberID++).toString();
        var subscriber = {};

        subscriber.connect = function () {
            return server.connect(subscriberID);
        }

        /**
         * Most command codes will not be useful.
         * The following lists the commands that might be the most useful.
         *
         * Command codes (string):
         *   - MetaDataRefresh
         *   - DefineOperationalModes
         *   - UpdateProcessingInterval
         *
         * message: an optional string to send along with the command
         */
        subscriber.sendCommand = function (commandCode, message) {
            return server.sendCommand(subscriberID, commandCode, message);
        }

        /**
         * Table names:
         *   DeviceDetail {
         *       NodeID,
         *       UniqueID,
         *       OriginalSource,
         *       IsConcentrator,
         *       Acronym,
         *       Name,
         *       AccessID,
         *       ParentAcronym,
         *       ProtocolName,
         *       FramesPerSecond,
         *       CompanyAcronym,
         *       VendorAcronym,
         *       VendorDeviceName,
         *       Longitude,
         *       Latitude,
         *       InterconnectionName,
         *       ContactList,
         *       Enabled,
         *       UpdatedOn,
         *   }
         *
         *   MeasurementDetail {
         *       DeviceAcronym,
         *       ID,
         *       SignalID,
         *       PointTag,
         *       SignalReference,
         *       SignalAcronym,
         *       PhasorSourceIndex,
         *       Description,
         *       Internal,
         *       Enabled,
         *       UpdatedOn
         *   }
         *
         *   PhasorDetail {
         *       ID,
         *       DeviceAcronym,
         *       Label,
         *       Type,
         *       Phase,
         *       DestinationPhasorID,
         *       SourceIndex,
         *       UpdatedOn
         *   }
         *
         *   SchemaVersion { VersionNumber }
         *
         * Other parameters:
         *   whereExpression: filter applied to the table records (SQL-like syntax)
         *   sortField: the field in the selected table by which to sort the metadata
         *   takeCount: the number of values to be provided by the metadata query
         *
         * Callback parameters:
         *   metadata: an array of objects matching the table definitions above
         */
        subscriber.getMetadata = function (tableName, whereExpression, sortField, takeCount) {
            if (takeCount === undefined)
                takeCount = 0;

            return server.getMetadata(subscriberID, tableName, whereExpression, sortField, takeCount);
        }

        /**
         * subscriptionInfo {
         *     Synchronized - determines whether measurements are concentrated (default: false)
         *     FilterExpression - expression used to select measurements for subscription
         *     LagTime - allowed past time deviation tolerance in seconds (default: 10)
         *     LeadTime - allowed future time deviation tolerance in seconds (default: 5)
         *     UseLocalClockAsRealTime - determines whether the server's local clock is used as real-time (default: false)
         *     UseMillisecondResolution - determines whether measurement timestamps use millisecond resolution (default: false)
         *     RequestNaNValueFilter - determines whether to request that the publisher remove NaNs from the data stream (default: false)
         *     StartTime - start time of the requested temporal session for streaming historic data (default: no temporal session)
         *     EndTime - stop time of the requested temporal session for streaming historic data (default: no temporal session)
         *     ConstraintParameters - additional constraint parameters supplied to temporal adapters in a temporal session
         *     ProcessingInterval - interval at which measurements will be provided; 0 indicates "as fast as possible" (default: -1)
         *     ExtraConnectionStringParameters - connection string defining custom parameters for further subscription customization
         *
         *     // Only applies to synchronized subscriptions:
         *     RemotelySynchronized - determines whether data is concentrated remotely by the publisher or locally by the subscriber
         *     FramesPerSecond - the rate of data publication (default: 30)
         *     DownsamplingMethod - { LastReceived, Closest, Filtered, BestQuality } (default: LastReceived)
         *     AllowPreemptivePublishing - determines whether data should be sent ASAP or at the end of the lag time (default: true)
         *     AllowSortsByArrival - determines whether to allow measurements to be sorted by time of arrival (default: true)
         *     IgnoreBadTimestamps - determines whether to ignore data with bad timestamps when concentrating (default: false)
         *     TimeResolution - the maximum time resolution, in ticks, when sorting measurements by timestamp (default: 10000)
         *
         *     // Only applies to unsynchronized subscriptions:
         *     Throttled - determines whether to adjust the data publication rate (default: false)
         *     PublishInterval - the interval at which data should be published for throttled subscriptions (default: -1)
         *     IncludeTime - determines whether timestamps are included (default: true)
         * }
         */
        subscriber.subscribe = function (subscriptionInfo) {
            return server.subscribe(subscriberID, subscriptionInfo);
        }

        subscriber.unsubscribe = function () {
            return server.unsubscribe(subscriberID);
        }

        subscriber.disconnect = function () {
            return server.disconnect(subscriberID);
        }

        subscriber.dispose = function () {
            subscribers[subscriberID] = undefined;
            return server.dispose(subscriberID);
        }

        subscribers[subscriberID] = subscriber;
        return subscriber;
    }
})(jQuery);