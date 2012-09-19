//******************************************************************************************************
//  CActionAdapter.h - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/27/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::ComponentModel;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Adapters;
using namespace GSF;

namespace CSampleAdapters
{
    /// <summary>
    /// Represents a C++ action adapter implementation sample.
    /// </summary>
    [Description("Represents a C++ action adapter implementation sample.")]
    public ref class CActionAdapter : public ActionAdapterBase
    {
        #pragma region [ Properties ]

    public:

        /// <summary>
        /// Gets the flag indicating if this <see cref="CActionAdapter"/> supports temporal processing.
        /// </summary>
        property bool SupportsTemporalProcessing
        {
            virtual bool get() override;
        };

        #pragma endregion

        #pragma region [ Methods ]

    public:

        /// <summary>
        /// Initializes <see cref="CActionAdapter"/>.
        /// </summary>
        virtual void Initialize() override;

        /// <summary>
        /// Gets a short one-line status of this <see cref="CActionAdapter"/>.
        /// </summary>
        virtual String^ GetShortStatus(int maxLength) override;

    protected:

        /// <summary>
        /// Publish <see cref="IFrame"/> of time-aligned collection of <see cref="IMeasurement"/> values that arrived within the
        /// concentrator's defined <see cref="LagTime"/>.
        /// </summary>
        /// <param name="frame"><see cref="IFrame"/> of measurements with the same timestamp that arrived within <see cref="LagTime"/> that are ready for processing.</param>
        /// <param name="index">Index of <see cref="IFrame"/> within a second ranging from zero to <c><see cref="FramesPerSecond"/> - 1</c>.</param>
        /// <remarks>
        /// If user implemented publication function consistently exceeds available publishing time (i.e., <c>1 / <see cref="FramesPerSecond"/></c> seconds),
        /// concentration will fall behind. A small amount of this time is required by the <see cref="ConcentratorBase"/> for processing overhead, so actual total time
        /// available for user function process will always be slightly less than <c>1 / <see cref="FramesPerSecond"/></c> seconds.
        /// </remarks>
        virtual void PublishFrame(IFrame^ frame, int index) override;

        #pragma endregion
    };
}
