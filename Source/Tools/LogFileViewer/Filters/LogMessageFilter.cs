//******************************************************************************************************
//  LogMessageFilter.cs - Gbtc
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
//  12/01/2016 - Steven E. Chisholm
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Data;
using System.IO;
using System.Text;
using GSF.Diagnostics;
using GSF.IO;

namespace LogFileViewer.Filters
{
    public class LogMessageFilter
    {
        public FilterLevel FilterLevel;
        public TimestampMatching TimeFilter;
        public EnumMatchingFlags Classification;
        public EnumMatchingFlags Level;
        public EnumMatchingFlags Flags;
        public StringMatching Assembly;
        public StringMatching Type;
        public StringMatching RelatedType;
        public StackDetailsMatching StackDetails;
        public StackTraceMatching StackTraceDetails;
        public StringMatching EventName;
        public StringMatching MessageText;
        public StringMatching DetailsText;
        public StringMatching ExceptionText;

        public LogMessageFilter()
        {
            
        }
        public LogMessageFilter(Stream stream)
        {
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 1:
                    FilterLevel = (FilterLevel)stream.ReadNextByte();
                    if (stream.ReadBoolean())
                        TimeFilter = new TimestampMatching(stream);
                    if (stream.ReadBoolean())
                        Classification = new EnumMatchingFlags(stream);
                    if (stream.ReadBoolean())
                        Level = new EnumMatchingFlags(stream);
                    if (stream.ReadBoolean())
                        Flags = new EnumMatchingFlags(stream);
                    if (stream.ReadBoolean())
                        Assembly = new StringMatching(stream);
                    if (stream.ReadBoolean())
                        Type = new StringMatching(stream);
                    if (stream.ReadBoolean())
                        RelatedType = new StringMatching(stream);
                    if (stream.ReadBoolean())
                        StackDetails = new StackDetailsMatching(stream);
                    if (stream.ReadBoolean())
                        StackTraceDetails = new StackTraceMatching(stream);
                    if (stream.ReadBoolean())
                        EventName = new StringMatching(stream);
                    if (stream.ReadBoolean())
                        MessageText = new StringMatching(stream);
                    if (stream.ReadBoolean())
                        DetailsText = new StringMatching(stream);
                    if (stream.ReadBoolean())
                        ExceptionText = new StringMatching(stream);
                    break;
                default:
                    throw new VersionNotFoundException();
            }
        }

        public void Save(Stream stream)
        {
            stream.Write((byte)1);
            stream.Write((byte)FilterLevel);
            stream.Write(TimeFilter != null);
            TimeFilter?.Save(stream);
            stream.Write(Classification != null);
            Classification?.Save(stream);
            stream.Write(Level != null);
            Level?.Save(stream);
            stream.Write(Flags != null);
            Flags?.Save(stream);
            stream.Write(Assembly != null);
            Assembly?.Save(stream);
            stream.Write(Type != null);
            Type?.Save(stream);
            stream.Write(RelatedType != null);
            RelatedType?.Save(stream);
            stream.Write(StackDetails != null);
            StackDetails?.Save(stream);
            stream.Write(StackTraceDetails != null);
            StackTraceDetails?.Save(stream);
            stream.Write(EventName != null);
            EventName?.Save(stream);
            stream.Write(MessageText != null);
            MessageText?.Save(stream);
            stream.Write(DetailsText != null);
            DetailsText?.Save(stream);
            stream.Write(ExceptionText != null);
            ExceptionText?.Save(stream);
        }

        public bool IsMatch(LogMessage log)
        {
            if (TimeFilter != null)
            {
                if (!TimeFilter.IsMatch(log.UtcTime))
                    return false;
            }
            if (Classification != null)
            {
                if (!Classification.IsMatch(1 << (int)log.Classification))
                    return false;
            }
            if (Level != null)
            {
                if (!Level.IsMatch(1 << (int)log.Level))
                    return false;
            }
            if (Flags != null)
            {
                if (!Flags.IsMatch((int)log.Flags))
                    return false;
            }
            if (Assembly != null)
            {
                if (!Assembly.IsMatch(log.AssemblyName))
                    return false;
            }
            if (Type != null)
            {
                if (!Type.IsMatch(log.TypeName))
                    return false;
            }
            if (RelatedType != null)
            {
                if (!MatchRelatedType(log))
                    return false;
            }
            if (StackDetails != null)
            {
                if (!StackDetails.IsMatch(log))
                    return false;
            }
            if (StackTraceDetails != null)
            {
                if (!StackTraceDetails.IsMatch(log))
                    return false;
            }
            if (EventName != null)
            {
                if (!EventName.IsMatch(log.EventName))
                    return false;
            }
            if (MessageText != null)
            {
                if (!MessageText.IsMatch(log.Message))
                    return false;
            }
            if (DetailsText != null)
            {
                if (!DetailsText.IsMatch(log.Details))
                    return false;
            }
            if (ExceptionText != null)
            {
                if (!ExceptionText.IsMatch(log.ExceptionString))
                    return false;
            }
            return true;
        }

        private bool MatchRelatedType(LogMessage log)
        {
            if (RelatedType.IsMatch(log.TypeName))
                return true;
            foreach (var item in log.RelatedTypes)
            {
                if (RelatedType.IsMatch(item))
                    return true;
            }
            return false;
        }

        public string Description
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"({FilterLevel}) ");

                if (TimeFilter != null)
                {
                    sb.Append("Time: ");
                    sb.Append(TimeFilter.ToString());
                    sb.Append('|');
                }
                if (Classification != null)
                {
                    sb.Append("Class: ");
                    sb.Append(Classification.ToString());
                    sb.Append('|');
                }
                if (Level != null)
                {
                    sb.Append("Level: ");
                    sb.Append(Level.ToString());
                    sb.Append('|');
                }
                if (Flags != null)
                {
                    sb.Append("Flags: ");
                    sb.Append(Flags.ToString());
                    sb.Append('|');
                }
                if (Assembly != null)
                {
                    sb.Append("Assembly: ");
                    sb.Append(Assembly.ToString());
                    sb.Append('|');
                }
                if (Type != null)
                {
                    sb.Append("Type: ");
                    sb.Append(Type.ToString());
                    sb.Append('|');
                }
                if (RelatedType != null)
                {
                    sb.Append("Related Type: ");
                    sb.Append(RelatedType.ToString());
                    sb.Append('|');
                }
                if (StackDetails != null)
                {
                    sb.Append("StackDetails: ");
                    sb.Append(StackDetails.ToString());
                    sb.Append('|');
                }
                if (StackTraceDetails != null)
                {
                    sb.Append("StackTrace: ");
                    sb.Append(StackTraceDetails.ToString());
                    sb.Append('|');
                }
                if (EventName != null)
                {
                    sb.Append("Event: ");
                    sb.Append(EventName.ToString());
                    sb.Append('|');
                }
                if (MessageText != null)
                {
                    sb.Append("Message: ");
                    sb.Append(MessageText.ToString());
                    sb.Append('|');
                }
                if (DetailsText != null)
                {
                    sb.Append("Details: ");
                    sb.Append(DetailsText.ToString());
                    sb.Append('|');
                }
                if (ExceptionText != null)
                {
                    sb.Append("Exception: ");
                    sb.Append(ExceptionText.ToString());
                    sb.Append('|');
                }
                if (sb[sb.Length - 1] == '|')
                    sb.Length -= 1;
                return sb.ToString();
            }
        }
    }
}