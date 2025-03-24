//******************************************************************************************************
//  DataMonitorExporter.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/04/2007 - Pinal C. Patel
//       Original version of source code generated.
//  04/17/2009 - Pinal C. Patel
//       Converted to C#.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/11/2010 - Mihir Brahmbhatt
//       Updated new header and license agreement.
//  11/30/2011 - J. Ritchie Carroll
//       Modified to support buffer optimized ISupportBinaryImage.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using GSF.Collections;
using GSF.Communication;
using GSF.Historian.Packets;
using GSF.Parsing;

// ReSharper disable ClassNeverInstantiated.Local

namespace GSF.Historian.Exporters;

/// <summary>
/// Represents an exporter that can export real-time time-series data over a TCP server socket.
/// </summary>
/// <example>
/// Definition of a sample <see cref="Export"/> that can be processed by <see cref="DataMonitorExporter"/>:
/// <code>
/// <![CDATA[
/// <?xml version="1.0" encoding="utf-16"?>
/// <Export xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
///   <Name>DataMonitorExport</Name>
///   <Type>RealTime</Type>
///   <Interval>0</Interval>
///   <Exporter>DataMonitorExporter</Exporter>
///   <Settings>
///     <ExportSetting>
///       <Name>ServerPort</Name>
///       <Value>8500</Value>
///     </ExportSetting>
///     <ExportSetting>
///       <Name>LegacyMode</Name>
///       <Value>True</Value>
///     </ExportSetting>
///   </Settings>
///   <Records>
///     <ExportRecord>
///       <Instance>P2</Instance>
///       <Identifier>1885</Identifier>
///     </ExportRecord>  
///     <ExportRecord>
///       <Instance>P2</Instance>
///       <Identifier>2711</Identifier>
///     </ExportRecord>
///   </Records> 
/// </Export>
/// ]]>
/// </code>
/// <para>
/// Description of custom settings required by <see cref="DataMonitorExporter"/> in an <see cref="Export"/>:
/// <list type="table">
///     <listheader>
///         <term>Setting Name</term>
///         <description>Setting Description</description>
///     </listheader>
///     <item>
///         <term>ServerPort</term>
///         <description>TCP server socket port number over which export data is to be transmitted.</description>
///     </item>
///     <item>
///         <term>LegacyMode (Optional)</term>
///         <description>True if export data is to be transmitted in <see cref="PacketType1"/> and False if export data is to be transmitted in <see cref="PacketType101"/>.</description>
///     </item>
/// </list>
/// </para>
/// </example>
/// <seealso cref="Export"/>
public class DataMonitorExporter : RebroadcastExporter
{
    #region [ Members ]

    // Nested Types

    /// <summary>
    /// A class for storing runtime information of an <see cref="Export"/>.
    /// </summary>
    private class ExportContext
    {
        /// <summary>
        /// <see cref="IServer"/> used for transmitting the time-series data.
        /// </summary>
        public IServer Socket;

        /// <summary>
        /// Number of time-series data points to be transmitted in a single packet.
        /// </summary>
        public int DataPerPacket;

        /// <summary>
        /// <see cref="Delegate"/> to invoke for transmitting the time-series data.
        /// </summary>
        public Action<ExportContext, IList<IDataPoint>> TransmitHandler;
    }

    // Fields
    private readonly Dictionary<string, ExportContext> m_contexts;

    #endregion

    #region [ Constructors ]

    /// <summary>
    /// Initializes a new instance of the <see cref="DataMonitorExporter"/> class.
    /// </summary>
    public DataMonitorExporter()
        : this(nameof(DataMonitorExporter))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataMonitorExporter"/> class.
    /// </summary>
    /// <param name="name"><see cref="ExporterBase.Name"/> of the exporter.</param>
    protected DataMonitorExporter(string name)
        : base(name)
    {
        m_contexts = new Dictionary<string, ExportContext>();

        // Register handlers.
        ExportAddedHandler = CreateContext;
        ExportRemovedHandler = RemoveContext;
        ExportUpdatedHandler = UpdateContext;
    }

    #endregion

    #region [ Methods ]

    /// <summary>
    /// Performs the transmission of time-series data for the <paramref name="export"/>.
    /// </summary>
    /// <param name="export"><see cref="Export"/> whose time-series data os to be transmitted.</param>
    /// <param name="dataToTransmit">Collection of time-series data to be transmitted.</param>
    protected override void TransmitData(Export export, IList<IDataPoint> dataToTransmit)
    {
        // Retrieve the export context.
        ExportContext context;

        lock (m_contexts)
            m_contexts.TryGetValue(export.Name, out context);

        // Transmit export data if export context exists and socket is connected.
        if (context is not null && context.Socket.CurrentState == ServerState.Running)
            context.TransmitHandler(context, dataToTransmit);
    }

    /// <summary>
    /// Transmits export data in <see cref="PacketType1"/>
    /// </summary>
    private void TransmitPacketType1(ExportContext context, IList<IDataPoint> dataToTransmit)
    {
        byte[] dataBuffer = new byte[context.DataPerPacket * PacketType1.FixedLength];

        for (int i = 0; i < dataToTransmit.Count; i += context.DataPerPacket)
        {
            // Transmit the data at the maximum allowed rate.
            int dataCount = 0;

            for (int j = i; j < i + (dataToTransmit.Count - i < context.DataPerPacket ? dataToTransmit.Count - i : context.DataPerPacket); j++)
            {
                // Prepare binary image of the data.
                new PacketType1(dataToTransmit[j]).GenerateBinaryImage(dataBuffer, dataCount * PacketType1.FixedLength);
                dataCount++;
            }

            // Transmit the prepared binary image.
            context.Socket.MulticastAsync(dataBuffer, 0, dataCount * PacketType1.FixedLength);
        }
    }

    /// <summary>
    /// Transmits export data in <see cref="PacketType101"/>
    /// </summary>
    private void TransmitPacketType101(ExportContext context, IList<IDataPoint> dataToTransmit)
    {
        PacketType101 packet;

        if (dataToTransmit.Count <= context.DataPerPacket)
        {
            // Transmit all the data.
            packet = new PacketType101(dataToTransmit);
            context.Socket.MulticastAsync(packet.BinaryImage(), 0, packet.BinaryLength);
        }
        else
        {
            // Transmit data at the maximum allowed rate.
            for (int i = 0; i < dataToTransmit.Count; i += context.DataPerPacket)
            {
                packet = new PacketType101(dataToTransmit.GetRange(i, dataToTransmit.Count - i < context.DataPerPacket ? dataToTransmit.Count - i : context.DataPerPacket));
                context.Socket.MulticastAsync(packet.BinaryImage(), 0, packet.BinaryLength);
            }
        }
    }

    private void CreateContext(Export export)
    {
        lock (m_contexts)
        {
            if (m_contexts.TryGetValue(export.Name, out ExportContext context))
                return;
            
            // Context for the export does not exist, so we create one.
            ExportSetting serverPortSetting = export.FindSetting("ServerPort");
            ExportSetting legacyModeSetting = export.FindSetting("LegacyMode");

            if (serverPortSetting is null || string.IsNullOrEmpty(serverPortSetting.Value))
                return;

            // Create the server socket for export context.
            context = new ExportContext { Socket = ServerBase.Create($"Protocol=TCP;Port={serverPortSetting.Value}") };
            context.Socket.Start(); // Start the server; need to do only one time.
            context.Socket.ClientConnected += CommunicationServer_ClientConnected;
            context.Socket.ClientDisconnected += CommunicationServer_ClientDisconnected;

            // Determine the format of transmission.
            if (legacyModeSetting is not null && Convert.ToBoolean(legacyModeSetting.Value))
            {
                // Data is to be transmitted using PacketType1.
                context.DataPerPacket = 50;
                context.TransmitHandler = TransmitPacketType1;
            }
            else
            {
                // Data is to be transmitted using PacketType101.
                context.DataPerPacket = 1000000;
                context.TransmitHandler = TransmitPacketType101;
            }

            // Save export context
            m_contexts.Add(export.Name, context);

            // Provide a status update.
            OnStatusUpdate($"TCP server created for export {export.Name}");
        }
    }

    private void RemoveContext(Export export)
    {
        lock (m_contexts)
        {
            if (!m_contexts.TryGetValue(export.Name, out ExportContext context))
                return;
            
            // Context for the export exists, so remove it from the list.
            context.Socket.ClientConnected -= CommunicationServer_ClientConnected;
            context.Socket.ClientDisconnected -= CommunicationServer_ClientDisconnected;
            context.Socket.Dispose();
            
            m_contexts.Remove(export.Name);

            // Provide a status update.
            OnStatusUpdate($"TCP server of export {export.Name} removed");
        }
    }

    private void UpdateContext(Export export)
    {
        RemoveContext(export);   // Remove context.
        CreateContext(export);   // Create context.
    }

    private void CommunicationServer_ClientConnected(object sender, EventArgs<Guid> e)
    {
        TcpServer server = (TcpServer)sender;

        if (!server.TryGetClient(e.Argument, out TransportProvider<Socket> client))
            return;
        
        IPEndPoint serverEndPoint = (IPEndPoint)server.Server.LocalEndPoint;
        IPEndPoint clientEndPoint = (IPEndPoint)client.Provider.RemoteEndPoint;

        // Notify that a new client has connected.
        OnStatusUpdate($"Remote TCP client {clientEndPoint.Address} connected to port {serverEndPoint.Port}");
    }

    private void CommunicationServer_ClientDisconnected(object sender, EventArgs<Guid> e)
    {
        TcpServer server = (TcpServer)sender;
        Dictionary<string, string> configString = server.ConfigurationString.ParseKeyValuePairs();

        // Notify that existing client has disconnected.
        OnStatusUpdate($"Remote TCP client disconnected from port {configString["port"]}");
    }

    #endregion
}