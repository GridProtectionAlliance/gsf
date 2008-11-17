//*******************************************************************************************************
//  IFrameParser.vb - Frame parsing interface
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2008
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

using System;
using PCS;

namespace PCS.Parsing
{
    /// <summary>
    /// This interface represents the protocol independent representation of a frame parser.
    /// </summary>
    public interface IFrameParser : ISupportLifecycle, IStatusProvider
    {
        //event EventHandler<EventArgs<IBinaryImageConsumer>> ReceivedImage;
        //event EventHandler<EventArgs<IBinaryImageConsumer>> ReceivedUndeterminedImage;
        //event EventHandler<EventArgs<Exception>> DataStreamException;

        void Start();

        void Stop();

        bool ExecuteParseOnSeparateThread
        {
            get;
            set;
        }

        int QueuedBuffers
        {
            get;
        }

        void Write(byte[] buffer, int offset, int count);
    }
}