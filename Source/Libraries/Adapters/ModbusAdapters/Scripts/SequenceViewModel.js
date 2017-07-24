//******************************************************************************************************
//  SequenceViewModel.js - Gbtc
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

// Define view model for sequences
function SequenceViewModel(sequenceType, domIndex, index, expanded) {
    const self = this;

    // Configuration fields
    self.sequenceType = sequenceType;
    self.domIndex = domIndex;

    // Observable fields
    self.index = ko.observable(index);
    self.sequenceName = ko.observable();
    self.sequenceRecords = ko.observableArray();
    self.expanded = ko.observable(expanded);
    self.dataAsHex = ko.observable(false);
    self.derivedValue = ko.observable("");

    // Properties
    self.selectedRecordCount = ko.pureComputed({
        read: function() {
            const records = self.sequenceRecords();
            var selectedCount = 0;

            if (records) {
                for (let i = 0; i < records.length; i++) {
                    if (records[i].selected())
                        selectedCount++;
                }
            }

            return selectedCount;
        },
        owner: self
    }).extend({ notify: "always" });

    self.canAddDerivedValue = ko.pureComputed({
        read: function() {
            return self.selectedRecordCount() > 0 && $("input[name=interpretAs" + self.domIndex + "]:checked").length > 0;
        },
        owner: self
    });

    self.sequenceTypeDescription = ko.pureComputed({
        read: function() {
            return self.sequenceType === SequenceType.Read ? "Read" : "Write";
        },
        owner: self
    });

    self.sequenceGroups = ko.pureComputed({
        read: function() {
            const records = self.sequenceRecords();
            const groups = [];
            var group = { start: 0, stop: -1, type: -1 };

            if (records.length > 0) {
                for (let i = 1; i < records.length; i++) {
                    const previous = records[i - 1];
                    const current = records[i];

                    if (current.recordType() !== previous.recordType()) {
                        group.stop = i - 1;
                        group.type = previous.recordType();
                        groups.push(group);
                        group = { start: i, stop: -1, type: -1 };
                    } else if (current.recordType() !== RecordType.DerivedValue && !((isEmpty(current.address()) && isEmpty(previous.address())) || parseInt(current.address()) === parseInt(previous.address()) + 1)) {
                        group.stop = i - 1;
                        group.type = previous.recordType();
                        groups.push(group);
                        group = { start: i, stop: -1, type: -1 };
                    }
                }

                group.stop = records.length - 1;
                group.type = records[group.stop].recordType();
                groups.push(group);
            }

            return groups;
        },
        owner: self
    });

    self.groupLengthWarning = ko.pureComputed({
        read: function() {
            const groups = self.sequenceGroups();

            for (let i = 0; i < groups.length; i++) {
                const group = groups[i];

                if (group.type !== RecordType.DerivedValue && group.stop - group.start + 1 > 100)
                    return true;
            }

            return false;
        },
        owner: self
    });

    // Methods
    self.addNewSequenceRecord = function(recordType, deserializedRecord, expand) {
        var address = "";

        if (!deserializedRecord && self.sequenceRecords().length > 0) {
            const records = self.sequenceRecords();
            let lastAddress = null;

            for (let i = records.length - 1; i >= 0; i--) {
                if (records[i].recordType() === recordType) {
                    lastAddress = records[i].address();
                    break;
                }
            }

            if (isNumber(lastAddress))
                address = parseInt(lastAddress) + 1;
        }


        self.sequenceRecords.push(new RecordViewModel(self, recordType, address, deserializedRecord));
        self.sortSequenceRecords();

        // Make sure sequence is expanded when adding a record
        if (expand)
            $("#sequenceContent" + domIndex).collapse("show");
    }

    self.sortSequenceRecords = function() {
        // Keep sequence map sorted by record type, then address
        self.sequenceRecords.sort(function(a, b) {
            const firstOrderSort = a.recordType() - b.recordType();

            if (firstOrderSort !== 0)
                return firstOrderSort;

            var addra = a.address();
            var addrb = b.address();

            if (isEmpty(addra))
                addra = 0;

            if (isEmpty(addrb))
                addrb = 0;

            if (isNumber(addra) && isNumber(addrb))
                return parseInt(addra) - parseInt(addrb);

            addra = addra.toUpperCase();
            addrb = addrb.toUpperCase();

            return addra < addrb ? -1 : (addra > addrb ? 1 : 0);
        });
    }

    self.removeSequenceRecord = function(record) {
        self.sequenceRecords.remove(record);
    }

    self.findSequenceRecord = function(recordType, address, parseAsInt) {
        const records = self.sequenceRecords();

        if (parseAsInt === undefined)
            parseAsInt = true;

        for (let i = 0; i < records.length; i++) {
            const record = records[i];

            if (record.recordType() === recordType) {
                if (parseAsInt) {
                    if (parseInt(record.address()) === parseInt(address))
                        return record;
                    else if (record.address() === address)
                        return record;
                }
            }
        }

        return null;
    }

    self.refreshSequenceRecords = function() {
        // Sort and fully re-render sequence records upon request
        self.sortSequenceRecords();
        const records = self.sequenceRecords();

        self.sequenceRecords([]);

        setTimeout(function() {
            self.sequenceRecords(records);
        }, 250);
    }

    self.interpretAsSelectionChanged = function() {
        self.sequenceRecords.valueHasMutated();
    }

    self.selectNone = function() {
        const records = self.sequenceRecords();

        if (records) {
            for (let i = 0; i < records.length; i++)
                records[i].selected(false);
        }
    }

    self.selectOne = function(selectedIndex) {
        const records = self.sequenceRecords();

        if (records) {
            for (let i = 0; i < records.length; i++)
                records[i].selected(i === selectedIndex);
        }
    }

    self.serializeSequence = function() {
        var records = [];

        // Only serializing recordType, address, description and dataValue
        self.sequenceRecords().forEach(function(record) {
            records.push({
                recordType: record.recordType(),
                address: record.address().toString(),
                description: record.description(),
                dataValue: record.recordType() === RecordType.DerivedValue ? record.dataValue() : ""
            });
        });

        return {
            type: self.sequenceType,
            name: self.sequenceName(),
            records: records
        };
    }
}
