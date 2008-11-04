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

namespace PhasorProtocols
{
    public delegate void ReceivedCommandFrameEventHandler(ICommandFrame frame);
    public delegate void ReceivedConfigurationFrameEventHandler(IConfigurationFrame frame);
    public delegate void ReceivedDataFrameEventHandler(IDataFrame frame);
    public delegate void ReceivedFrameBufferImageEventHandler(FundamentalFrameType frameType, byte[] binaryImage, int offset, int length);
    public delegate void ReceivedHeaderFrameEventHandler(IHeaderFrame frame);
    public delegate void ReceivedUndeterminedFrameEventHandler(IChannelFrame frame);
    public delegate void ConfigurationChangedEventHandler();
    public delegate void DataStreamExceptionEventHandler(Exception ex);

    // TODO: Implement the following: ISupportLifecycle, IStatusProvider

    /// <summary>This interface represents the protocol independent representation of a frame parser.</summary>
    public interface IFrameParser : IDisposable
    {
        event ReceivedCommandFrameEventHandler ReceivedCommandFrame;
        event ReceivedConfigurationFrameEventHandler ReceivedConfigurationFrame;
        event ReceivedDataFrameEventHandler ReceivedDataFrame;
        event ReceivedFrameBufferImageEventHandler ReceivedFrameBufferImage;
        event ReceivedHeaderFrameEventHandler ReceivedHeaderFrame;
        event ReceivedUndeterminedFrameEventHandler ReceivedUndeterminedFrame;
        event ConfigurationChangedEventHandler ConfigurationChanged;
        event DataStreamExceptionEventHandler DataStreamException;

        void Start();

        void Stop();

        bool Enabled
        {
            get;
            set;
        }

        bool ExecuteParseOnSeparateThread
        {
            get;
            set;
        }

        int QueuedBuffers
        {
            get;
        }

        IConfigurationFrame ConfigurationFrame
        {
            get;
            set;
        }

        void Write(byte[] buffer, int offset, int count);

        string Status
        {
            get;
        }

        IConnectionParameters ConnectionParameters
        {
            get;
            set;
        }
    }
}