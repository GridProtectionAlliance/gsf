//******************************************************************************************************
//  StackDetailsMatching.cs - Gbtc
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

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using GSF.Diagnostics;
using GSF.IO;

namespace LogFileViewer.Filters
{
    public class StackDetailsMatching
    {
        private readonly List<KeyValuePair<string, string>> m_items;

        public StackDetailsMatching(List<KeyValuePair<string, string>> items)
        {
            m_items = items;
        }

        public StackDetailsMatching()
        {
            m_items = new List<KeyValuePair<string, string>>();
        }

        public StackDetailsMatching(Stream stream)
        {
            m_items = new List<KeyValuePair<string, string>>();
            byte version = stream.ReadNextByte();
            switch (version)
            {
                case 2:
                    while (stream.ReadBoolean())
                    {
                        string key = stream.ReadString();
                        string value = stream.ReadString();
                        m_items.Add(new KeyValuePair<string, string>(key, value));
                    }
                    break;
                default:
                    throw new VersionNotFoundException();
            }
        }

        public void Save(Stream stream)
        {
            stream.Write((byte)2);
            foreach (var items in m_items)
            {
                stream.Write(true);
                stream.Write(items.Key);
                stream.Write(items.Value);
            }
            stream.Write(false);
        }

        public bool IsMatch(LogMessage log)
        {
            foreach (var item in m_items)
            {
                if (string.Equals(log.CurrentStackMessages[item.Key],item.Value))
                {

                }
                else if (string.Equals(log.InitialStackMessages[item.Key], item.Value))
                {

                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return string.Join(";", m_items.Select(x => $"{x.Key}={x.Value}"));
        }

    }
}