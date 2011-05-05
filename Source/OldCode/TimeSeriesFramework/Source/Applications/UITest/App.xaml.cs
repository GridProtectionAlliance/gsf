//******************************************************************************************************
//  App.xaml.cs - Gbtc
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
//  04/01/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Security.Principal;
using System.Windows;
using TimeSeriesFramework.UI;

namespace UITest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region [ Members ]

        // Fields
        private Guid m_nodeValue;

        #endregion

        #region [ Properties ]

        public Guid NodeValue
        {
            get
            {
                return m_nodeValue;
            }
            set
            {
                m_nodeValue = value;
            }
        }

        #endregion

        #region [ Constructor ]

        public App()
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
            m_nodeValue = Guid.Parse("e7a5235d-cb6f-4864-a96e-a8686f36e599");
            CommonFunctions.CurrentNodeID = NodeValue;
        }

        #endregion
    }
}
