//******************************************************************************************************
//  PublisherHandler.h - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
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
//  03/27/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __PUBLISHER_HANDLER_H
#define __PUBLISHER_HANDLER_H

#include "../Common/CommonTypes.h"
#include "../Transport/PublisherInstance.h"

class PublisherHandler : public GSF::TimeSeries::Transport::PublisherInstance
{
private:
    std::string m_name;
    uint64_t m_processCount;
	GSF::TimerPtr m_publishTimer;
	int32_t m_metadataVersion;
	std::vector<GSF::TimeSeries::DeviceMetadataPtr> m_deviceMetadata;
	std::vector<GSF::TimeSeries::MeasurementMetadataPtr> m_measurementMetadata;
	std::vector<GSF::TimeSeries::PhasorMetadataPtr> m_phasorMetadata;

    static GSF::Mutex s_coutLock;

	void DefineMetadata();

protected:
    void StatusMessage(const std::string& message) override;
    void ErrorMessage(const std::string& message) override;
	void ClientConnected(const GSF::TimeSeries::Transport::SubscriberConnectionPtr& connection) override;
	void ClientDisconnected(const GSF::TimeSeries::Transport::SubscriberConnectionPtr& connection) override;

public:
	PublisherHandler(std::string name, uint16_t port, bool ipV6);

	void Start();
	void Stop() const;
};

typedef GSF::SharedPtr<PublisherHandler> PublisherHandlerPtr;

#endif