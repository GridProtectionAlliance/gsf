//******************************************************************************************************
//  SubscriberInstance.h - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  03/21/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __SUBSCRIBERINSTANCE_H
#define __SUBSCRIBERINSTANCE_H

#include <string>
#include <vector>
#include <ctime>

#include "DataSubscriber.h"

namespace GSF {
namespace TimeSeries {
namespace Transport
{
    class SubscriberInstance  // NOLINT
    {
    private:
        // Subscription members
        std::string m_hostname;
        uint16_t m_port;
        uint16_t m_udpPort;
        bool m_autoReconnect;
        bool m_autoParseMetadata;
        int16_t m_maxRetries;
        int16_t m_retryInterval;
        std::string m_filterExpression;
        std::string m_metadataFilters;
        DataSubscriber m_subscriber;
        SubscriptionInfo m_subscriptionInfo;
        std::string m_startTime;
        std::string m_stopTime;
        EndianConverter m_endianConverter;
        void* m_userData;

        GSF::StringMap<DeviceMetadataPtr> m_devices;
        std::unordered_map<Guid, MeasurementMetadataPtr> m_measurements;
        GSF::StringMap<ConfigurationFramePtr> m_configurationFrames;

        Mutex m_configurationUpdateLock;
        void SendMetadataRefreshCommand();

        static void ConstructConfigurationFrames(const GSF::StringMap<DeviceMetadataPtr>& devices, const std::unordered_map<Guid, MeasurementMetadataPtr>& measurements, GSF::StringMap<ConfigurationFramePtr>& configurationFrames);
        static bool TryFindMeasurement(const std::vector<MeasurementMetadataPtr>& measurements, SignalKind kind, uint16_t index, MeasurementMetadataPtr& measurementMetadata);
        static bool TryFindMeasurement(const std::vector<MeasurementMetadataPtr>& measurements, SignalKind kind, MeasurementMetadataPtr& measurementMetadata);
        static uint16_t GetSignalKindCount(const std::vector<MeasurementMetadataPtr>& measurements, SignalKind kind);

        // Internal subscription event handlers
        static void HandleResubscribe(DataSubscriber* source);
        static void HandleStatusMessage(DataSubscriber* source, const std::string& message);
        static void HandleErrorMessage(DataSubscriber* source, const std::string& message);
        static void HandleDataStartTime(DataSubscriber* source, int64_t startTime);
        static void HandleMetadata(DataSubscriber* source, const std::vector<uint8_t>& payload);
        static void HandleNewMeasurements(DataSubscriber* source, const std::vector<MeasurementPtr>& measurements);
        static void HandleProcessingComplete(DataSubscriber* source, const std::string& message);
        static void HandleConfigurationChanged(DataSubscriber* source);
        static void HandleConnectionTerminated(DataSubscriber* source);

    protected:
        virtual void SetupSubscriberConnector(SubscriberConnector& connector);
        virtual SubscriptionInfo CreateSubscriptionInfo();
        virtual void StatusMessage(const std::string& message);	// Defaults output to cout
        virtual void ErrorMessage(const std::string& message);	// Defaults output to cerr
        virtual void DataStartTime(time_t unixSOC, uint16_t milliseconds);
        virtual void DataStartTime(DateTime startTime);
        virtual void ReceivedMetadata(const std::vector<uint8_t>& payload);
        virtual void ParsedMetadata();
        virtual void ReceivedNewMeasurements(const std::vector<MeasurementPtr>& measurements);
        virtual void ConfigurationChanged();
        virtual void HistoricalReadComplete();
        virtual void ConnectionEstablished();
        virtual void ConnectionTerminated();

        SubscriberInstance();

    public:
        virtual ~SubscriberInstance();

        // Constants
        static constexpr const char* SubscribeAllExpression = "FILTER ActiveMeasurements WHERE ID IS NOT NULL";
        static constexpr const char* SubscribeAllNoStatsExpression = "FILTER ActiveMeasurements WHERE SignalType <> 'STAT'";
        static constexpr const char* FilterMetadataStatsExpression = "FILTER MeasurementDetail WHERE SignalAcronym <> 'STAT'";

        // Iterator handler delegates
        typedef void(*DeviceMetadataIteratorHandlerFunction)(const DeviceMetadataPtr&, void* userData);
        typedef void(*MeasurementMetadataIteratorHandlerFunction)(const MeasurementMetadataPtr&, void* userData);
        typedef void(*ConfigurationFrameIteratorHandlerFunction)(const ConfigurationFramePtr&, void* userData);

        // Subscription functions

        // Initialize a connection with host name, port. To enable UDP for data channel,
        // optionally specify a UDP receive port. This function must be called before
        // calling the Connect method.
        void Initialize(const std::string& hostname, uint16_t port, uint16_t udpPort = 0);

        // Gets or sets flag that determines if auto-reconnect is enabled
        bool GetAutoReconnect() const;
        void SetAutoReconnect(bool autoReconnect);

        // Gets or sets flag that determines if metadata should be automatically
        // parsed. When true, metadata will be requested upon connection before
        // subscription; otherwise, metadata will not be manually requested and
        // subscribe will happen upon connection.
        bool GetAutoParseMetadata() const;
        void SetAutoParseMetadata(bool autoParseMetadata);

        // Gets or sets maximum connection retries
        int16_t GetMaxRetries() const;
        void SetMaxRetries(int16_t maxRetries);

        // Gets or sets delay between connection retries
        int16_t GetRetryInterval() const;
        void SetRetyInterval(int16_t retryInterval);

        // The following are example filter expression formats:
        //
        // - Signal ID list -
        // subscriber.SetFilterExpression("7aaf0a8f-3a4f-4c43-ab43-ed9d1e64a255;"
        //						"93673c68-d59d-4926-b7e9-e7678f9f66b4;"
        //						"65ac9cf6-ae33-4ece-91b6-bb79343855d5;"
        //						"3647f729-d0ed-4f79-85ad-dae2149cd432;"
        //						"069c5e29-f78a-46f6-9dff-c92cb4f69371;"
        //						"25355a7b-2a9d-4ef2-99ba-4dd791461379");
        //
        // - Measurement key list pattern -
        // subscriber.SetFilterExpression("PPA:1;PPA:2;PPA:3;PPA:4;PPA:5;PPA:6;PPA:7;PPA:8;PPA:9;PPA:10;PPA:11;PPA:12;PPA:13;PPA:14");
        //
        // - Filter pattern -
        // subscriber.SetFilterExpression("FILTER ActiveMeasurements WHERE ID LIKE 'PPA:*'");
        // subscriber.SetFilterExpression("FILTER ActiveMeasurements WHERE Device = 'SHELBY' AND SignalType = 'FREQ'");

        // Define a filter expression to control which points to receive. The filter expression
        // defaults to all non-static points available. When specified before the Connect function,
        // this filter expression will be used for the initial connection. Updating the filter
        // expression while a subscription is active will cause a resubscribe with new expression.
        const std::string& GetFilterExpression() const;
        void SetFilterExpression(const std::string& filterExpression);

        // Define any metadata filters to be applied to incoming metadata. Each separate filter should
        // be separated by semi-colons.
        const std::string& GetMetadataFilters() const;
        void SetMetadataFilters(const std::string& metadataFilters);

        // Starts the connection cycle to a GEP publisher. Upon connection, meta-data will be requested,
        // when received, a subscription will be established
        void Connect();
        void ConnectAsync();

        // Disconnects from the GEP publisher
        void Disconnect();

        // Historical subscription functions

        // Defines the desired time-range of data from the GEP publisher, if the publisher supports
        // historical queries. If specified, this function must be called before Connect.
        void EstablishHistoricalRead(const std::string& startTime, const std::string& stopTime);

        // Dynamically controls replay speed - can be updated while historical data is being received
        void SetHistoricalReplayInterval(int32_t replayInterval);

        // Gets or sets user defined data reference for SubscriberInstance
        void* GetUserData() const;
        void SetUserData(void* userData);

        // Gets or sets value that determines whether
        // payload data is compressed using TSSC.
        bool IsPayloadDataCompressed() const;
        void SetPayloadDataCompressed(bool compressed);

        // Gets or sets value that determines whether the
        // metadata transfer is compressed using GZip.
        bool IsMetadataCompressed() const;
        void SetMetadataCompressed(bool compressed);

        // Gets or sets value that determines whether the
        // signal index cache is compressed using GZip.
        bool IsSignalIndexCacheCompressed() const;
        void SetSignalIndexCacheCompressed(bool compressed);

        // Statistical functions
        uint64_t GetTotalCommandChannelBytesReceived() const;
        uint64_t GetTotalDataChannelBytesReceived() const;
        uint64_t GetTotalMeasurementsReceived() const;
        bool IsConnected() const;
        bool IsSubscribed() const;

        // Metadata iteration functions - note that full lock will be maintained on source collections
        // for the entire call, so keep work time minimized or clone collection before work
        void IterateDeviceMetadata(DeviceMetadataIteratorHandlerFunction iteratorHandler, void* userData);
        void IterateMeasurementMetadata(MeasurementMetadataIteratorHandlerFunction iteratorHandler, void* userData);
        void IterateConfigurationFrames(ConfigurationFrameIteratorHandlerFunction iteratorHandler, void* userData);

        // Safely get list of device acronyms (accessed from metadata after successful auto-parse),
        // vector will be cleared then appended to, returns true if any devices were added
        bool TryGetDeviceAcronyms(std::vector<std::string>& deviceAcronyms);

        // Safely get parsed device metadata (accessed after successful auto-parse),
        // vector will be cleared then appended to with copy of all parsed data
        void GetParsedDeviceMetadata(std::map<std::string, DeviceMetadataPtr>& devices);

        // Safely get parsed measurements (accessed after successful auto-parse),
        // vector will be cleared then appended to with copy of all parsed data
        void GetParsedMeasurementMetadata(std::map<Guid, MeasurementMetadataPtr>& measurements);

        // Metadata record lookup functions (post-parse)
        bool TryGetDeviceMetadata(const std::string& deviceAcronym, DeviceMetadataPtr& deviceMetadata);
        bool TryGetMeasurementMetdata(const Guid& signalID, MeasurementMetadataPtr& measurementMetadata);
        bool TryGetConfigurationFrame(const std::string& deviceAcronym, ConfigurationFramePtr& configurationFrame);
        bool TryFindTargetConfigurationFrame(const Guid& signalID, ConfigurationFramePtr& targetFrame);

        // Configuration frame limits the required search range for measurement metadata,
        // searching the frame members for a matching signal ID should normally be much
        // faster than executing a lookup in the full measurement map cache.
        static bool TryGetMeasurementMetdataFromConfigurationFrame(const Guid& signalID, const ConfigurationFramePtr& sourceFrame, MeasurementMetadataPtr& measurementMetadata);
    };
}}}

#endif