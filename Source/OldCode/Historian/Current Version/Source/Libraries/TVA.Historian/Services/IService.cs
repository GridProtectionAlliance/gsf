//*******************************************************************************************************
//  IService.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  08/21/2009 - Pinal C. Patel
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.ServiceModel.Web;
using TVA.Configuration;

namespace TVA.Historian.Services
{
    #region [ Enumerations ]

    /// <summary>
    /// Indicates the direction in which data will be flowing from a web service.
    /// </summary>
    public enum DataFlowDirection
    {
        /// <summary>
        /// Data will be flowing in to the web service.
        /// </summary>
        Incoming,
        /// <summary>
        /// Data will be flowing out from the web service.
        /// </summary>
        Outgoing,
        /// <summary>
        /// Data will be flowing both in and out from the web service.
        /// </summary>
        BothWays
    }

    #endregion

    /// <summary>
    /// Defines a web service that can send and receive data over REST (Representational State Transfer) interface.
    /// </summary>
    public interface IService : ISupportLifecycle, IPersistSettings
    {
        #region [ Members ]

        /// <summary>
        /// Occurs when the <see cref="ServiceHost"/> has been created for the specified <see cref="ServiceUri"/>.
        /// </summary>
        event EventHandler ServiceHostCreated;

        /// <summary>
        /// Occurs when the <see cref="ServiceHost"/> can process requests via all of its endpoints.
        /// </summary>
        event EventHandler ServiceHostStarted;

        /// <summary>
        /// Occurs when an <see cref="Exception"/> is encountered when processing a request.
        /// </summary>
        event EventHandler<EventArgs<Exception>> ServiceProcessError;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="IArchive"/> used by the web service for its data.
        /// </summary>
        IArchive Archive { get; set; }

        /// <summary>
        /// Gets or sets the URI where the web service is to be hosted.
        /// </summary>
        string ServiceUri { get; set; }

        /// <summary>
        /// Gets or sets the contract interface implemented by the web service.
        /// </summary>
        string ServiceContract { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataFlowDirection"/> of the web service.
        /// </summary>
        DataFlowDirection ServiceDataFlow { get; set; }

        /// <summary>
        /// Gets the <see cref="WebServiceHost"/> hosting the web service.
        /// </summary>
        WebServiceHost ServiceHost { get; }

        #endregion
    }
}
