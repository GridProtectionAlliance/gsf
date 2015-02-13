//******************************************************************************************************
//  ClientRequestHandler.cs - Gbtc
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
//  05/02/2007 - Pinal C. Patel
//       Generated original version of source code.
//  09/30/2008 - J. Ritchie Carroll
//       Converted to C#.
//  03/09/2009 - Pinal C. Patel
//       Edited code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.ServiceProcess
{
    /// <summary>
    /// Represents a handler for <see cref="ClientRequest"/>s sent by <see cref="ClientHelper"/>.
    /// </summary>
    /// <seealso cref="ClientHelper"/>
    /// <seealso cref="ClientRequest"/>
    /// <seealso cref="ServiceHelper"/>
    public class ClientRequestHandler
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRequestHandler"/> class.
        /// </summary>
        /// <param name="requestCommand">Command text that the <see cref="ClientRequestHandler"/> will process.</param>
        /// <param name="requestDescription">Description of the <see cref="ClientRequestHandler"/>.</param>
        /// <param name="handlerMethod"><see cref="Delegate"/> method that will be invoked for processing the <paramref name="requestCommand"/>.</param>
        /// <param name="aliases">Optional alias names to command.</param>
        public ClientRequestHandler(string requestCommand, string requestDescription, Action<ClientRequestInfo> handlerMethod, string[] aliases = null)
            : this(requestCommand, requestDescription, handlerMethod, true, aliases)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRequestHandler"/> class.
        /// </summary>
        /// <param name="requestCommand">Command text that the <see cref="ClientRequestHandler"/> will process.</param>
        /// <param name="requestDescription">Description of the <see cref="ClientRequestHandler"/>.</param>
        /// <param name="handlerMethod"><see cref="Delegate"/> method that will be invoked for processing the <paramref name="requestCommand"/>.</param>
        /// <param name="isAdvertised">true if the <see cref="ClientRequestHandler"/> is to be published by the <see cref="ServiceHelper"/>; otherwise false.</param>
        /// <param name="aliases">Optional alias names to command.</param>
        public ClientRequestHandler(string requestCommand, string requestDescription, Action<ClientRequestInfo> handlerMethod, bool isAdvertised, string[] aliases = null)
        {
            Command = requestCommand;
            CommandDescription = requestDescription;
            HandlerMethod = handlerMethod;
            IsAdvertised = isAdvertised;
            Aliases = aliases;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the command text that the <see cref="ClientRequestHandler"/> will process.
        /// </summary>
        public string Command
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets any aliases for the command.
        /// </summary>
        public string[] Aliases
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the description of the <see cref="ClientRequestHandler"/>.
        /// </summary>
        public string CommandDescription
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="Delegate"/> method that gets invoked for processing the <see cref="Command"/>.
        /// </summary>
        public Action<ClientRequestInfo> HandlerMethod
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a boolean value that indicates whether the <see cref="ClientRequestHandler"/> will be published by the <see cref="ServiceHelper"/>.
        /// </summary>
        public bool IsAdvertised
        {
            get;
            private set;
        }

        #endregion
    }
}
