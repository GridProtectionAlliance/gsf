//******************************************************************************************************
//  MapViewModel.js - Gbtc
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

var SequenceType = {
    Read: 0,
    Write: 1
}
var sequenceDomIndex = 0;

function MapViewModel() {
    const self = this;

    // Observable fields
    self.sequences = ko.observableArray();
    self.totalReadsAndWrites = ko.observable(0);
    self.unitID = ko.observable(255).extend({ required: true }).extend({ number: true }).extend({ min: 0 }).extend({ max: 255 });
    self.pollingRate = ko.observable(2000).extend({ required: true }).extend({ number: true }).extend({ min: 100 });
    self.interSequenceGroupPollDelay = ko.observable(250).extend({ required: true }).extend({ number: true }).extend({ min: 100 });
    self.ipPort = ko.observable(502).extend({ required: true }).extend({ number: true }).extend({ min: 1 }).extend({ max: 65535 });
    self.hostName = ko.observable("localhost").extend({ required: true });
    self.interface = ko.observable("0.0.0.0").extend({ required: true }).extend({ pattern: { message: 'Must be a valid IP address', params: /^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$/ } });
    self.comPort = ko.observable(1).extend({ required: true }).extend({ number: true }).extend({ min: 1 }).extend({ max: 65535 });
    self.dataBits = ko.observable(8).extend({ required: true }).extend({ number: true }).extend({ min: 5 }).extend({ max: 8 });
    self.connectionString = ko.observable("").extend({ required: true });
    self.adapterConnectionString = ko.observable("").extend({ required: true });
    self.exists = ko.observable(false);

    // Internal fields
    self._isDirty = ko.observable(false);
    self._frameFormat = ko.observable("TCP");
    self._transport = ko.observable("TCP");
    self._connectionString = ko.observable("");
    self._deviceName = ko.observable("");

    // Properties
    self.isDirty = ko.pureComputed({
        read: self._isDirty,
        write: function(value) {
            if (value === undefined)
                value = true;

            self._isDirty(value);
        },
        owner: self
    });

    self.frameFormat = ko.pureComputed({
        read: self._frameFormat,
        write: function(value) {
            if (value === "TCP") {
                if (self.transport() === "SERIAL")
                    self.transport("TCP");
            } else {
                if (self.transport() !== "SERIAL")
                    self.transport("SERIAL");
            }

            self._frameFormat(value);
        },
        owner: self
    });

    self.transport = ko.pureComputed({
        read: self._transport,
        write: function(value) {
            if (value !== self._transport()) {
                self._transport(value);
                self.connectionString("");
            }
        },
        owner: self
    });

    self.connectionString = ko.pureComputed({
        read: function() {
            const parameters = notNull(self._connectionString()).parseKeyValuePairs();
            parameters.set("frameFormat", self.frameFormat());
            parameters.set("transport", self.transport());
            parameters.set("unitID", self.unitID());
            self._connectionString(parameters.joinKeyValuePairs());
            return self._connectionString();
        },
        write: function(value) {
            self._connectionString(value);
        },
        owner: self
    });

    self.totalSequenceGroups = ko.pureComputed({
        read: function() {
            var total = 0;

            for (let i = 0; i < self.sequences().length; i++)
                total += self.sequences()[i].sequenceGroups().length;

            return total;
        },
        owner: self
    });

    self.pollingRateWarning = ko.pureComputed({
        read: function() {
            return self.totalSequenceGroups() * self.interSequenceGroupPollDelay() > self.pollingRate();
        },
        owner: self
    });

    self.deviceName = ko.pureComputed({
        read: function() {
            return self._deviceName().toUpperCase();
        },
        write: function(value) {
            self._deviceName(notNull(value).toUpperCase());

            if (hubIsConnected) {
                dataHub.queryDevice(self._deviceName()).done(function(device) {
                   self.exists(device.ID > 0); 
                });
            }
        },
        owner: self
    });

    self.deviceConnectionValues = ko.validatedObservable({
        deviceName: self.deviceName.extend({
            required: true,
            pattern: { 
                message: "Only upper case letters, numbers, '!', '-', '@', '#', '_' , '.'and '$' are allowed.", 
                params: "^[A-Z0-9\\-!_\\.@#\\$]+$"
            }
        })}, {
        adapterConnectionString: self.adapterConnectionString.extend({
            required: true
        })}, {
        sequences: self.sequences
        }
    );

    // Delegates
    self.reorderSequencePanels = function() { };
    self.loadMappingComplete = function() { };

    self.setReorderSequencePanels = function(reorderSequencePanelsFunction) {
        self.reorderSequencePanels = reorderSequencePanelsFunction;
    }

    self.setLoadMappingComplete = function(loadMappingCompleteFunction) {
        self.loadMappingComplete = loadMappingCompleteFunction;
    }

    // Methods
    self.addNewSequence = function(sequenceType, expanded) {
        const domIndex = sequenceDomIndex;
        const sequence = new SequenceViewModel(sequenceType, domIndex, self.sequences().length, expanded);

        self.sequences.push(sequence);

        sequenceDomIndex++;

        return sequence;
    }

    self.removeSequence = function(sequence) {
        if (sequence.sequenceRecords().length === 0 || confirm("Are you sure you want to delete sequence \"" + sequence.sequenceName() + "\"?")) {
            const sequences = self.sequences();
            const index = sequences.indexOf(sequence);

            if (index !== -1)
                sequences.splice(index, 1);

            // Fully re-render reorderable sequences when one is removed
            self.sequences([]);

            setTimeout(function() {
                self.sequences(sequences);
                self.reorderSequencePanels();
            }, 250);
        }
    }

    self.newSequenceMap = function() {
        self.sequences([]);
        self.isDirty(false);
    }

    self.loadSequenceMap = function(fileBlob, appendMode) {
        var reader = new FileReader();

        reader.onload = function() {
            const data = JSON.parse(reader.result);

            if (!appendMode)
                self.sequences([]);

            data.sequences.forEach(function(parsedSequence) {
                var sequence = self.addNewSequence(parsedSequence.type, false);
                sequence.sequenceName(parsedSequence.name);

                // Restore full field set, only a partial set gets serialized
                parsedSequence.records.forEach(function(record) {
                    sequence.addNewSequenceRecord(null,
                        {
                            recordType: record.recordType,
                            selected: false,
                            address: record.address,
                            description: record.description,
                            dataValue: record.dataValue
                        },
                        false);
                });
            });

            self.isDirty(appendMode);
        };

        reader.onloadend = function() {
            if (reader.error && reader.error.message)
                alert("Failed to load sequence file: " + reader.error.message);

            self.loadMappingComplete();
        };

        reader.readAsText(fileBlob);
    }

    self.saveSequenceMap = function(fileName) {
        const data = JSON.stringify(self.serializeMapping(), null, 4);
        const anchor = $("#saveMappingFileLink");

        if (typeof anchor[0].download != "undefined") {
            anchor.attr("href", "data:text/json;charset=utf-8," + encodeURIComponent(data));
            anchor.attr("download", fileName);
            anchor[0].click();
        } else {
            if (isIE)
                window.navigator.msSaveBlob(new Blob([data]), fileName);
            else
                window.open("data:text/json;charset=utf-8," + encodeURIComponent(data), "_blank", "");
        }

        self.isDirty(false);
    }

    self.readSequences = function() {
        const sequences = self.sequences();
        var readSequences = [];

        if (sequences) {
            sequences.forEach(function(sequence) {
                if (sequence.sequenceType === SequenceType.Read)
                    readSequences.push(sequence);
            });
        }    

        return readSequences;
    }
    
    self.writeSequences = function() {
        const sequences = self.sequences();
        var writeSequences = [];

        if (sequences) {
            sequences.forEach(function(sequence) {
                if (sequence.sequenceType === SequenceType.Write)
                    writeSequences.push(sequence);
            });
        }    

        return writeSequences;
    }

    self.serializeMapping = function() {
        const sequences = self.sequences();
        var serializedSequences = [];

        if (sequences) {
            sequences.forEach(function(sequence) {
                serializedSequences.push(sequence.serializeSequence());
            });
        }

        return {
            sequences: serializedSequences
        };
    }
}
