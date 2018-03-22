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

#include <iostream>
#include <string>
#include <vector>
#include <ctime>

#include "../Common/Convert.h"
#include "../Common/Measurement.h"
#include "../Transport/DataSubscriber.h"

namespace gsfts = GSF::TimeSeries;
namespace tst = gsfts::Transport;

class SubscriberInstance
{
private:
	// Subscription members
	std::string m_hostname;
	gsfts::uint16_t m_port;
	gsfts::uint16_t m_udpPort;
	std::string m_filterExpression;
	tst::DataSubscriber m_subscriber;
	tst::SubscriptionInfo m_info;
	std::string m_startTime;
	std::string m_stopTime;
	gsfts::EndianConverter m_endianConverter;
	void* m_userData;

	// Internal subscription event handlers
	static void HandleResubscribe(tst::DataSubscriber* source);
	static void HandleStatusMessage(tst::DataSubscriber* source, std::string message);
	static void HandleErrorMessage(tst::DataSubscriber* source, std::string message);
	static void HandleDataStartTime(tst::DataSubscriber* source, gsfts::int64_t startTime);
	static void HandleMetadata(tst::DataSubscriber* source, std::vector<uint8_t> payload);
	static void HandleNewMeasurements(tst::DataSubscriber* source, std::vector<gsfts::Measurement> measurements);
	static void HandleProcessingComplete(tst::DataSubscriber* source, std::string message);
	static void HandleConfigurationChanged(tst::DataSubscriber* source);
	static void HandleConnectionTerminated(tst::DataSubscriber* source);

protected:
	virtual tst::SubscriberConnector CreateSubscriberConnector();
	virtual tst::SubscriptionInfo CreateSubscriptionInfo();
	virtual void StatusMessage(std::string message);
	virtual void ErrorMessage(std::string message);
	virtual void DataStartTime(std::time_t unixSOC, int milliseconds);
	virtual void ReceivedMetadata(std::vector<uint8_t> payload);
	virtual void ReceivedNewMeasurements(std::vector<gsfts::Measurement> measurements);
	virtual void ConfigurationChanged();
	virtual void HistoricalReadComplete();
	virtual void ConnectionEstablished();
	virtual void ConnectionTerminated();

public:
	SubscriberInstance();
	~SubscriberInstance();

	// Constants
	static constexpr const char* SubscribeAllExpression = "FILTER ActiveMeasurements WHERE ID IS NOT NULL";
	static constexpr const char* SubscribeAllNoStatsExpression = "FILTER ActiveMeasurements WHERE SignalType <> 'STAT'";

	// Subscription functions

	// Initialize a connection with host name, port. To enable UDP for data channel,
	// optionally specify a UDP receive port. This function must be called before
	// calling the Connect method.
	void Initialize(std::string hostname, gsfts::uint16_t port, gsfts::uint16_t udpPort = 0);

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
	void SetFilterExpression(std::string filterExpression);

	// Starts the connection cycle to a GEP publisher. Upon connection, meta-data will be requested,
	// when received, a subscription will be established
	void Connect();
	
	// Disconnects from the GEP publisher
	void Disconnect();

	// Historical subscription functions
	
	// Defines the desired time-range of data from the GEP publisher, if the publisher supports
	// historical queries. If specified, this function must be called before Connect.
	void EstablishHistoricalRead(std::string startTime, std::string stopTime);

	// Dynamically controls replay speed - can be updated while historical data is being received
	void SetHistoricalReplayInterval(int32_t replayInterval);

	// Gets or sets user defined data reference for SubscriberInstance
	void* GetUserData() const;
	void SetUserData(void* userData);

	// Gets or sets value that determines if metadata transfer will be compressed,
	// when true, metadata payload will be zlib compressed
	bool IsMetadataCompressed() const;
	void SetMetadataCompressed(bool compressed);

	// Statistical functions
	long GetTotalCommandChannelBytesReceived() const;
	long GetTotalDataChannelBytesReceived() const;
	long GetTotalMeasurementsReceived() const;
	bool IsConnected() const;
	bool IsSubscribed() const;
};