using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVA.Services
{
    /// <summary>
    /// Represents information about a <see cref="ClientRequest"/> sent by <see cref="ClientHelper"/>.
    /// </summary>
    /// <seealso cref="ClientInfo"/>
    /// <seealso cref="ClientRequest"/>
    public class ClientRequestInfo
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientRequestInfo"/> class.
        /// </summary>
        /// <param name="sender"><see cref="ClientInfo"/> object of the <paramref name="request"/> sender.</param>
        /// <param name="request"><see cref="ClientRequest"/> object sent by the <paramref name="sender"/>.</param>
        public ClientRequestInfo(ClientInfo sender, ClientRequest request)
        {
            Request = request;
            Sender = sender;
            ReceivedAt = DateTime.Now;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="ClientInfo"/> object of the <see cref="Request"/> sender.
        /// </summary>
        public ClientInfo Sender { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ClientRequest"/> object sent by the <see cref="Sender"/>.
        /// </summary>
        public ClientRequest Request { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> when the <see cref="Request"/> was received from the <see cref="Sender"/>.
        /// </summary>
        public DateTime ReceivedAt { get; set; }

        #endregion
    }
}
