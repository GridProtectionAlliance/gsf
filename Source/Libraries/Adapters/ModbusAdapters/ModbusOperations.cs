//******************************************************************************************************
//  ModbusOperations.cs - Gbtc
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
//  08/17/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Threading.Tasks;
using GSF;
using GSF.Web.Hubs;
using Microsoft.AspNet.SignalR.Hubs;

namespace ModbusAdapters
{
    /// <summary>
    /// Defines an interface for using <see cref="ModbusOperations"/> within a SignalR hub.
    /// </summary>
    /// <remarks>
    /// This interface makes sure all hub methods needed by ModbusConfig.cshtml get properly defined.
    /// </remarks>
    public interface IModbusOperations
    {
        Task<bool> ModbusConnect(string connectionString);

        void ModbusDisconnect();

        Task<bool[]> ReadDiscreteInputs(ushort startAddress, ushort pointCount);

        Task<bool[]> ReadCoils(ushort startAddress, ushort pointCount);

        Task<ushort[]> ReadInputRegisters(ushort startAddress, ushort pointCount);

        Task<ushort[]> ReadHoldingRegisters(ushort startAddress, ushort pointCount);

        Task WriteCoils(ushort startAddress, bool[] data);

        Task WriteHoldingRegisters(ushort startAddress, ushort[] data);

        string DeriveString(ushort[] values);

        float DeriveSingle(ushort highValue, ushort lowValue);

        double DeriveDouble(ushort b3, ushort b2, ushort b1, ushort b0);

        int DeriveInt32(ushort highValue, ushort lowValue);

        uint DeriveUInt32(ushort highValue, ushort lowValue);

        long DeriveInt64(ushort b3, ushort b2, ushort b1, ushort b0);

        ulong DeriveUInt64(ushort b3, ushort b2, ushort b1, ushort b0);
    }

    /// <summary>
    /// Represents hub operations for using <see cref="DataSubscriptionHubClient"/> instances.
    /// </summary>
    /// <remarks>
    /// This hub client operations class makes sure a Modbus connection is created per SignalR session and only created when needed.
    /// </remarks>
    public class ModbusOperations : HubClientOperationsBase<ModbusHubClient>, IModbusOperations
    {
        /// <summary>
        /// Creates a new <see cref="ModbusOperations"/> instance.
        /// </summary>
        /// <param name="hub">Parent hub.</param>
        /// <param name="logStatusMessageFunction">Delegate to use to log status messages, if any.</param>
        /// <param name="logExceptionFunction">Delegate to use to log exceptions, if any.</param>
        public ModbusOperations(IHub hub, Action<string, UpdateType> logStatusMessageFunction = null, Action<Exception> logExceptionFunction = null) : base(hub, logStatusMessageFunction, logExceptionFunction)
        {
        }

        public Task<bool> ModbusConnect(string connectionString)
        {
            return HubClient.Connect(connectionString);
        }

        public void ModbusDisconnect()
        {
            HubClient.Disconnect();
        }

        public Task<bool[]> ReadDiscreteInputs(ushort startAddress, ushort pointCount)
        {
            return HubClient.ReadDiscreteInputs(startAddress, pointCount);
        }

        public Task<bool[]> ReadCoils(ushort startAddress, ushort pointCount)
        {
            return HubClient.ReadCoils(startAddress, pointCount);
        }

        public Task<ushort[]> ReadInputRegisters(ushort startAddress, ushort pointCount)
        {
            return HubClient.ReadInputRegisters(startAddress, pointCount);
        }

        public Task<ushort[]> ReadHoldingRegisters(ushort startAddress, ushort pointCount)
        {
            return HubClient.ReadHoldingRegisters(startAddress, pointCount);
        }

        public Task WriteCoils(ushort startAddress, bool[] data)
        {
            return HubClient.WriteCoils(startAddress, data);
        }

        public Task WriteHoldingRegisters(ushort startAddress, ushort[] data)
        {
            return HubClient.WriteHoldingRegisters(startAddress, data);
        }

        public string DeriveString(ushort[] values)
        {
            return HubClient.DeriveString(values);
        }

        public float DeriveSingle(ushort highValue, ushort lowValue)
        {
            return HubClient.DeriveSingle(highValue, lowValue);
        }

        public double DeriveDouble(ushort b3, ushort b2, ushort b1, ushort b0)
        {
            return HubClient.DeriveDouble(b3, b2, b1, b0);
        }

        public int DeriveInt32(ushort highValue, ushort lowValue)
        {
            return HubClient.DeriveInt32(highValue, lowValue);
        }

        public uint DeriveUInt32(ushort highValue, ushort lowValue)
        {
            return HubClient.DeriveUInt32(highValue, lowValue);
        }

        public long DeriveInt64(ushort b3, ushort b2, ushort b1, ushort b0)
        {
            return HubClient.DeriveInt64(b3, b2, b1, b0);
        }

        public ulong DeriveUInt64(ushort b3, ushort b2, ushort b1, ushort b0)
        {
            return HubClient.DeriveUInt64(b3, b2, b1, b0);
        }
    }
}