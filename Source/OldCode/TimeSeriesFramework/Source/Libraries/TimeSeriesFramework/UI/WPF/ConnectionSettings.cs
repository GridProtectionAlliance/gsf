//******************************************************************************************************
//  ConnectionSettings.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  05/25/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using TVA.Communication;
using TVA.PhasorProtocols;

namespace TimeSeriesFramework.UI
{
    /// <summary>
    /// Represents connection information defined in PmuConnection file.
    /// </summary>
    [Serializable()]
    public class ConnectionSettings
    {
        public PhasorProtocol PhasorProtocol;
        public TransportProtocol TransportProtocol;
        public string ConnectionString;
        public int PmuID;
        public int FrameRate;
        public bool AutoRepeatPlayback;
        public int ByteEncodingDisplayFormat;
        public object ConnectionParameters;
        public string ConfigurationFileName;
        public bool RefreshConfigurationFileOnChange;
        public bool ParseWordCountFromByte;

        //#region [ Members ]

        //// Fields
        //private PhasorProtocol m_phasorProtocol;
        //private TransportProtocol m_transportProtocol;
        //private string m_connectionString;
        //private int m_pmuId;
        //private int m_frameRate;
        //private bool m_autoRepeatPlayback;
        //private int m_byteEncodingDisplayFormat;
        //private object m_connectionParameters;
        //private string m_configurationFileName;
        //private bool m_refreshConfigurationFileOnChange;
        //private bool m_parseWordCountFromByte;

        //#endregion

        //#region [ Properties ]

        //public PhasorProtocol PhasorProtocol
        //{
        //    get
        //    {
        //        return m_phasorProtocol;
        //    }
        //    set
        //    {
        //        m_phasorProtocol = value;
        //    }
        //}

        //public TransportProtocol TransportProtocol
        //{
        //    get
        //    {
        //        return m_transportProtocol;
        //    }
        //    set
        //    {
        //        m_transportProtocol = value;
        //    }
        //}

        //public string ConnectionString
        //{
        //    get
        //    {
        //        return m_connectionString;
        //    }
        //    set
        //    {
        //        m_connectionString = value;
        //    }
        //}

        //public int PmuId
        //{
        //    get
        //    {
        //        return m_pmuId;
        //    }
        //    set
        //    {
        //        m_pmuId = value;
        //    }
        //}

        //public int FrameRate
        //{
        //    get
        //    {
        //        return m_frameRate;
        //    }
        //    set
        //    {
        //        m_frameRate = value;
        //    }
        //}

        //public bool AutoRepeatPlayback
        //{
        //    get
        //    {
        //        return m_autoRepeatPlayback;
        //    }
        //    set
        //    {
        //        m_autoRepeatPlayback = value;
        //    }
        //}

        //public int ByteEncodingDisplayFormat
        //{
        //    get
        //    {
        //        return m_byteEncodingDisplayFormat;
        //    }
        //    set
        //    {
        //        m_byteEncodingDisplayFormat = value;
        //    }
        //}

        //public object ConnectionParameters
        //{
        //    get
        //    {
        //        return m_connectionParameters;
        //    }
        //    set
        //    {
        //        m_connectionParameters = value;
        //    }
        //}

        //public string ConfigurationFileName
        //{
        //    get
        //    {
        //        return m_configurationFileName;
        //    }
        //    set
        //    {
        //        m_configurationFileName = value;
        //    }
        //}

        //public bool RefreshConfigurationFileOnChange
        //{
        //    get
        //    {
        //        return m_refreshConfigurationFileOnChange;
        //    }
        //    set
        //    {
        //        m_refreshConfigurationFileOnChange = value;
        //    }
        //}

        //public bool ParseWordCountFromByte
        //{
        //    get
        //    {
        //        return m_parseWordCountFromByte;
        //    }
        //    set
        //    {
        //        m_parseWordCountFromByte = value;
        //    }
        //}

        //#endregion

    }
}
