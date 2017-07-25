//******************************************************************************************************
//  ProgressUpdate.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
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
//  05/25/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace ModbusAdapters.Model
{
    public enum ProgressState
    {
        Queued,
        Processing,
        PartialSuccess,
        Success,
        Fail
    }

    public class ProgressUpdate
    {
        #region [ Constructors ]

        public ProgressUpdate()
        {
            Timestamp = DateTime.UtcNow;
        }

        #endregion

        #region [ Properties ]

        public DateTime Timestamp
        {
            get;
            private set;
        }

        public ProgressState? State
        {
            get;
            set;
        }

        public long? OverallProgress
        {
            get;
            set;
        }

        public long? OverallProgressTotal
        {
            get;
            set;
        }

        public long? Progress
        {
            get;
            set;
        }

        public long? ProgressTotal
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public string ErrorMessage
        {
            get;
            set;
        }

        public string Summary
        {
            get;
            set;
        }

        #endregion

        #region [ Methods ]

        public object AsExpandoObject()
        {
            dynamic obj = new ExpandoObject();

            if (State != null)
                obj.State = State;

            if (OverallProgress != null)
                obj.OverallProgress = OverallProgress;

            if (OverallProgressTotal != null)
                obj.OverallProgressTotal = OverallProgressTotal;

            if (Progress != null)
                obj.Progress = Progress;

            if (ProgressTotal != null)
                obj.ProgressTotal = ProgressTotal;

            if (Message != null)
                obj.Message = $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {Message}";

            if (ErrorMessage != null)
                obj.ErrorMessage = $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {ErrorMessage}";

            if (Summary != null)
                obj.Summary = Summary;

            return obj;
        }

        #endregion

        #region [ Static ]

        // Static Methods
        public static List<ProgressUpdate> Flatten(List<ProgressUpdate> updates)
        {
            ProgressUpdate flatState = new ProgressUpdate();
            List<ProgressUpdate> flattenedUpdates = new List<ProgressUpdate>() { flatState };
            string lastMessage = null;

            foreach (ProgressUpdate update in updates)
            {
                if (update.State == ProgressState.Queued)
                {
                    flatState = new ProgressUpdate();
                    flattenedUpdates.Add(flatState);
                }

                if (update.State != null)
                    flatState.State = update.State;

                if (update.OverallProgress != null)
                    flatState.OverallProgress = update.OverallProgress;

                if (update.OverallProgressTotal != null)
                    flatState.OverallProgressTotal = update.OverallProgressTotal;

                if (update.Progress != null)
                    flatState.Progress = update.Progress;

                if (update.ProgressTotal != null)
                    flatState.ProgressTotal = update.ProgressTotal;

                if (update.Summary != null)
                    flatState.Summary = update.Summary;

                if (update.Message != null && update.Message != lastMessage)
                {
                    lastMessage = update.Message;

                    flattenedUpdates.Add(new ProgressUpdate()
                    {
                        Timestamp = update.Timestamp,
                        Message = update.Message
                    });
                }

                if (update.ErrorMessage != null && update.ErrorMessage != lastMessage)
                {
                    lastMessage = update.ErrorMessage;

                    flattenedUpdates.Add(new ProgressUpdate()
                    {
                        Timestamp = update.Timestamp,
                        ErrorMessage = update.ErrorMessage
                    });
                }
            }

            return flattenedUpdates;
        }

        #endregion
    }
}