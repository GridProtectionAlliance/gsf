//******************************************************************************************************
//  RecordViewModel.js - Gbtc
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
//  07/23/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

"use strict";

var RecordType = {
    DiscreteInput: 0,
    Coil: 1,
    InputRegister: 2,
    HoldingRegister: 3,
    DerivedValue: 4
}

function getRecordTypeCode(recordType) {
    switch (recordType) {
        case RecordType.DiscreteInput:
            return "DI";
        case RecordType.Coil:
            return "CO";
        case RecordType.InputRegister:
            return "IR";
        case RecordType.HoldingRegister:
            return "HR";
        case RecordType.DerivedValue:
            return "DV";
        default:
            return "??";
    }
}

// Define view model for sequence records
function RecordViewModel(parent, recordType, address, deserializedRecord) {
    const self = this;

    // Configuration fields
    self.parent = parent;

    // Observable fields
    self.recordType = ko.observable(recordType);
    self.selected = ko.observable(false);
    self.address = ko.observable(address);
    self.description = ko.observable("");
    self.dataValue = ko.observable();
    self.signalType = ko.observable(7);
    self.mapped = ko.observable(false);
        
    // Internal fields
    self._tagName = ko.observable("");
    self._tagDescription = ko.observable("");

    // If provided, construct from existing deserialized record
    if (deserializedRecord) {
        self.recordType(deserializedRecord.recordType);
        self.address(deserializedRecord.address);
        self.description(deserializedRecord.description);
        self.dataValue(deserializedRecord.dataValue);
    }

    // Setup address validation
    self.address.extend({ required: true });

    if (self.recordType() !== RecordType.DerivedValue)
        self.address.extend({ number: true }).extend({ min: 0 });

    self.addressValidation = ko.validatedObservable({ address: self.address });

    // Properties
    self.checkValidation = ko.pureComputed({
        read: function() {
            if (!self.addressValidation.isValid() || isEmpty(self.address()))
                return "has-error";

            return undefined;
        },
        owner: self
    });

    self.groupIndex = ko.pureComputed({
        read: function() {
            const groups = self.parent.sequenceGroups();
            const recordIndex = self.parent.sequenceRecords().indexOf(self);

            for (let i = 0; i < groups.length; i++) {
                const group = groups[i];

                if (recordIndex >= group.start && recordIndex <= group.stop)
                    return i;
            }

            return -1;
        },
        owner: self
    });

    self.groupCount = ko.pureComputed({
        read: function() {
            const groupIndex = self.groupIndex();

            if (groupIndex > -1) {
                const group = self.parent.sequenceGroups()[groupIndex];
                return (group.stop - group.start + 1);
            }

            return -1;
        },
        owner: self
    });

    self.isGroupFirst = ko.pureComputed({
        read: function() {
            const groupIndex = self.groupIndex();

            if (groupIndex > -1) {
                const group = self.parent.sequenceGroups()[groupIndex];
                return self.parent.sequenceRecords.indexOf(self) === group.start;
            }

            return false;
        },
        owner: self
    });

    self.isGroupLast = ko.pureComputed({
        read: function() {
            const groupIndex = self.groupIndex();

            if (groupIndex > -1) {
                const group = self.parent.sequenceGroups()[groupIndex];
                return self.parent.sequenceRecords.indexOf(self) === group.stop;
            }

            return false;
        },
        owner: self
    });

    self.groupBorders = ko.pureComputed({
        read: function() {
            const records = self.parent.sequenceRecords();
            const borderExpression = "\"border-{0}-color\": \"{1}\", \"border-{0}-style\": \"{2}\", \"border-{0}-width\": \"{3}\"";
            const recordIndex = records.indexOf(self);
            const style = "solid";
            const width = "3px";
            var color = "black";
            var topWidth = width;
            var bottomStyle = "none";

            if (self.recordType() !== RecordType.DerivedValue && self.groupCount() > 100)
                color = "yellow";

            var topColor = color;

            if (recordIndex > 0) {
                const previous = records[recordIndex - 1];

                if (previous.recordType() === self.recordType()) {
                    if (self.recordType() === RecordType.DerivedValue) {
                        topWidth = "1px";
                        topColor = "#ddd";
                    } else if ((isEmpty(previous.address()) && isEmpty(self.address())) || parseInt(previous.address()) + 1 === parseInt(self.address())) {
                        topWidth = "1px";
                        topColor = "#ddd";
                    }
                }
            }

            if (recordIndex === records.length - 1) {
                bottomStyle = "solid";
            }

            var borderStyles = "{ " + String.format(borderExpression, "top", topColor, style, topWidth);
            borderStyles += ", " + String.format(borderExpression, "left", color, style, width);
            borderStyles += ", " + String.format(borderExpression, "bottom", color, bottomStyle, width);
            borderStyles += ", \"border-right-style\": \"none\" }";

            return JSON.parse(borderStyles);
        },
        owner: self
    });

    self.isDuplicate = ko.pureComputed({
        read: function() {
            const records = self.parent.sequenceRecords();
            const recordIndex = records.indexOf(self);

            if (self.recordType() === RecordType.DerivedValue) {
                if (recordIndex < records.length - 1) {
                    const next = records[recordIndex + 1];

                    if (next.recordType() === self.recordType() && notNull(next.address()).toUpperCase() === notNull(self.address()).toUpperCase())
                        return true;
                }

                if (recordIndex > 0) {
                    const previous = records[recordIndex - 1];

                    if (previous.recordType() === self.recordType() && notNull(previous.address()).toUpperCase() === notNull(self.address()).toUpperCase())
                        return true;
                }

                return false;
            }

            if (recordIndex < records.length - 1) {
                const next = records[recordIndex + 1];

                if (self.recordType() === next.recordType() && parseInt(self.address()) === parseInt(next.address()))
                    return true;
            }

            if (recordIndex > 0) {
                const previous = records[recordIndex - 1];

                if (previous.recordType() === self.recordType() && parseInt(previous.address()) === parseInt(self.address()))
                    return true;
            }

            return false;
        },
        owner: self
    });

    self.signalReferenceFormat = ko.pureComputed({
        read: function() {
            if (self.recordType() === RecordType.DerivedValue) {
                // Format derived values like: {DeviceName}-{RecordTypeCode}!{upper(DerivedType)}@{Address0}#{Address1}[...#{Address(n)}]
                const address = self.address().replaceAll("(", "@").replaceAll(",", "#").trim().toUpperCase();
                return "{0}-" + String.format("{0}!{1}", getRecordTypeCode(self.recordType()), address.substr(0, address.length - 1));
            }

            return "{0}-" + getRecordTypeCode(self.recordType()) + self.address();
        },
        owner: self
    });
    
    self.signalReference = ko.pureComputed({
        read: function() {
            return String.format(self.signalReferenceFormat(), notNull(viewModel.deviceName(), "<DeviceName>"));
        },
        owner: self
    });

    self.tagName = ko.pureComputed({
        read: function() {
            var tagName;

            if (isEmpty(self._tagName()))
                tagName = self.signalReference();
            else
                tagName = self._tagName().toUpperCase();

            // Database defines maximum length of tag names and signal references to 200 characters
            if (tagName.length > 200 || self.signalReference().length > 200)
                self.mapped(false);

            return tagName;
        },
        write: function(value) {
            self._tagName(notNull(value).toUpperCase());
        },
        owner: self
    });

    self.tagDescription = ko.pureComputed({
        read: function() {
            if (isEmpty(self._tagDescription()))
                return String.format("{0} {1}", notNull(viewModel.deviceName(), "<DeviceName>"), notNull(self.description()));

            return self._tagDescription();
        },
        write: function(value) {
            self._tagDescription(notNull(value));
        },
        owner: self
    });

    self.deviceConnectionValues = ko.validatedObservable({
        tagName: self.tagName.extend({
            required: true,
            pattern: { 
                message: "Only upper case letters, numbers, '!', '-', '@', '#', '_' , '.'and '$' are allowed.", 
                params: "^[A-Z0-9\\-!_\\.@#\\$]+$"
            }
        })}, {
        tagDescription: self.tagDescription.extend({
            required: true
        })}
    );
}
