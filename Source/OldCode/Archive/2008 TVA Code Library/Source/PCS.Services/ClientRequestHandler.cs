//*******************************************************************************************************
//  ClientRequestHandlerInfo.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/02/2007 - Pinal C. Patel
//       Generated original version of source code.
//  09/30/2008 - James R Carroll
//       Converted to C#.
//  03/09/2009 - Pinal C. Patel
//       Edited code comments.
//
//*******************************************************************************************************

using System;

namespace PCS.Services
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
        /// <param name="handlerMethod"><see cref="Delegate"/> method that will be invoked for processing the <paramref name="command"/>.</param>
        public ClientRequestHandler(string requestCommand, string requestDescription, Action<ClientRequest.Info> handlerMethod)
            : this(requestCommand, requestDescription, handlerMethod, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRequestHandler"/> class.
        /// </summary>
        /// <param name="requestCommand">Command text that the <see cref="ClientRequestHandler"/> will process.</param>
        /// <param name="requestDescription">Description of the <see cref="ClientRequestHandler"/>.</param>
        /// <param name="handlerMethod"><see cref="Delegate"/> method that will be invoked for processing the <paramref name="command"/>.</param>
        /// <param name="isAdvertised">true if the <see cref="ClientRequestHandler"/> is to be published by the <see cref="ServiceHelper"/>; otherwise false.</param>
        public ClientRequestHandler(string requestCommand, string requestDescription, Action<ClientRequest.Info> handlerMethod, bool isAdvertised)
        {
            Command = requestCommand;
            CommandDescription = requestDescription;
            HandlerMethod = handlerMethod;
            IsAdvertised = isAdvertised;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the command text that the <see cref="ClientRequestHandler"/> will process.
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// Gets the description of the <see cref="ClientRequestHandler"/>.
        /// </summary>
        public string CommandDescription { get; private set; }

        /// <summary>
        /// Gets the <see cref="Delegate"/> method that gets invoked for processing the <see cref="Command"/>.
        /// </summary>
        public Action<ClientRequest.Info> HandlerMethod { get; private set; }

        /// <summary>
        /// Gets a boolean value that indicates whether the <see cref="ClientRequestHandler"/> will be published by the <see cref="ServiceHelper"/>.
        /// </summary>
        public bool IsAdvertised { get; private set; }

        #endregion
    }
}
