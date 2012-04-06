#******************************************************************************************************
#  Makefile - Gbtc
#
#  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
#
#  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
#  the NOTICE file distributed with this work for additional information regarding copyright ownership.
#  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
#  not use this file except in compliance with the License. You may obtain a copy of the License at:
#
#      http:#www.opensource.org/licenses/eclipse-1.0.php
#
#  Unless agreed to in writing, the subject software distributed under the License is distributed on an
#  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
#  License for the specific language governing permissions and limitations.
#
#  Code Modification History:
#  ----------------------------------------------------------------------------------------------------
#  04/05/2012 - Stephen C. Wills
#       Generated original version of source code.
#
#******************************************************************************************************

CC := gcc
CFLAGS := -Wall
OUTPUT := Common/EndianConverter.o Transport/CompactMeasurementParser.o Transport/DataSubscriber.o Transport/SignalIndexCache.o Samples/SimpleSubscribe Samples/AdvancedSubscribe

Samples: SimpleSubscribe AdvancedSubscribe
SimpleSubscribe: Samples/SimpleSubscribe
AdvancedSubscribe: Samples/AdvancedSubscribe

Samples/SimpleSubscribe: Common/EndianConverter.o Transport/CompactMeasurementParser.o Transport/DataSubscriber.o Transport/SignalIndexCache.o Samples/SimpleSubscribe.cpp
	$(CC) $(CFLAGS) $^ -lboost_system -lboost_thread -o $@
	
Samples/AdvancedSubscribe: Common/EndianConverter.o Transport/CompactMeasurementParser.o Transport/DataSubscriber.o Transport/SignalIndexCache.o Samples/AdvancedSubscribe.cpp
	$(CC) $(CFLAGS) $^ -lboost_system -lboost_thread -o $@

%.o : %.cpp
	$(CC) $(CFLAGS) -c $< -o $@

clean:
	rm -rf $(OUTPUT)