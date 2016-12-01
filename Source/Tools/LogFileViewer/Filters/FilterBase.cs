//******************************************************************************************************
//  FilterBase.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
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
//  11/01/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Data;
using System.IO;
using GSF.Diagnostics;
using GSF.IO;

namespace LogFileViewer.Filters
{
    public static class FilterLevelExtensions
    {
        public static string Name(this FilterLevel level)
        {
            switch (level)
            {
                case FilterLevel.Lowest:
                    return "Lowest";
                case FilterLevel.Low:
                    return "Low";
                case FilterLevel.BelowNormal:
                    return "BelowNormal";
                case FilterLevel.Normal:
                    return "Normal";
                case FilterLevel.AboveNormal:
                    return "AboveNormal";
                case FilterLevel.High:
                    return "High";
                case FilterLevel.Highest:
                    return "Highest";
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }

    public abstract class FilterBase
    {
        public FilterLevel FilterLevel { get; set; }

        public string Description
        {
            get
            {
                return $"({FilterLevel}) {DescriptionInternal}";
            }
        }

        protected abstract string DescriptionInternal { get; }

        public abstract bool IsMatch(LogMessage log);

        public void Save(Stream stream)
        {
            stream.Write((byte)1);

            Type ownerType = GetType();
            if (ownerType == typeof(TimestampFilter))
            {
                stream.Write((byte)FilterType.Timestamp);
            }
            else if (ownerType == typeof(TypeFilter))
            {
                stream.Write((byte)FilterType.Type);
            }
            else
            {
                throw new Exception("Type not identified.");
            }

            stream.Write((int)FilterLevel);
            SaveInternal(stream);
        }

        public static FilterBase Load(Stream stream)
        {
            switch (stream.ReadNextByte())
            {
                case 1:
                    FilterType filterType = (FilterType)stream.ReadNextByte();
                    int filterLevel = stream.ReadInt32();
                    FilterBase filter;
                    switch (filterType)
                    {
                        case FilterType.Timestamp:
                            filter = new TimestampFilter(stream);
                            break;
                        case FilterType.Type:
                            filter = new TypeFilter(stream);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    filter.FilterLevel = (FilterLevel)filterLevel;
                    return filter;
                default:
                    throw new VersionNotFoundException();
            }
        }

        protected abstract void SaveInternal(Stream stream);
    }
}