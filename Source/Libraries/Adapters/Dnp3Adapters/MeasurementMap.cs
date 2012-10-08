//******************************************************************************************************
//  MeasurementMap.cs - Gbtc
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
//  10/05/2012 - Adam Crain
//       Generated original version of source code.
//  10/8/2012 - Danyelle Gilliam
//        Modified Header
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace Dnp3Adapters
{
    public class Mapping
    {
        public Mapping()
        {
            this.tsfId = 0;
            this.dnpIndex = 0;
        }

        public Mapping(uint id, String source, UInt32 index)
        {
            this.tsfId = id;
            this.tsfSource = source;
            this.dnpIndex = index;
        }

        public uint tsfId;
        public String tsfSource;        
        public UInt32 dnpIndex;
    }

    public class MeasurementMap
    {
        public List<Mapping> binaryMap = new List<Mapping>();
        public List<Mapping> analogMap = new List<Mapping>();
        public List<Mapping> counterMap = new List<Mapping>();
        public List<Mapping> controlStatusMap = new List<Mapping>();
        public List<Mapping> setpointStatusMap = new List<Mapping>();
    }

   
}
