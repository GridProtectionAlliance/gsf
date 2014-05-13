//******************************************************************************************************
//  Service.cs - Gbtc
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
//  11/15/2011 - Pinal C. Patel
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace GSF.ServiceModel
{
    /// <summary>
    /// Defines helper methods related to WCF-based web services.
    /// </summary>
    public static class Service
    {
        private const string HostedAspNetEnvironment = "System.ServiceModel.Activation.HostedAspNetEnvironment, System.ServiceModel.Activation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

        /// <summary>
        /// Creates a <see cref="Binding"/> based on the specified <paramref name="address"/>.
        /// </summary>
        /// <param name="address">The URI that is used to determine the type of <see cref="Binding"/> to be created.</param>
        /// <param name="enableSecurity">A boolean value that indicated whether security is to be enabled on the <see cref="Binding"/>.</param>
        /// <returns>An <see cref="Binding"/> object if a valid <paramref name="address"/> is specified; otherwise null.</returns>
        /// <remarks>
        /// This list shows all valid address schemes that can be specified in the <paramref name="address"/>:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Address Scheme</term>
        ///         <description>Usage</description>
        ///     </listheader>
        ///     <item>
        ///         <term><b>http://</b> or <b>http.soap11://</b></term>
        ///         <description>An <paramref name="address"/> of <b>http.soap11://localhost:2929</b> will create an <see cref="BasicHttpBinding"/> and update the <paramref name="address"/> to <b>http://localhost:2929</b>.</description>
        ///     </item>
        ///     <item>
        ///         <term><b>http.soap12://</b></term>
        ///         <description>An <paramref name="address"/> of <b>http.soap12://localhost:2929</b> will create an <see cref="WSHttpBinding"/> and update the <paramref name="address"/> to <b>http://localhost:2929</b>.</description>
        ///     </item>
        ///     <item>
        ///         <term><b>http.duplex://</b></term>
        ///         <description>An <paramref name="address"/> of <b>http.duplex://localhost:2929</b> will create an <see cref="WSDualHttpBinding"/> and update the <paramref name="address"/> to <b>http://localhost:2929</b>.</description>
        ///     </item>
        ///     <item>
        ///         <term><b>http.rest://</b></term>
        ///         <description>An <paramref name="address"/> of <b>http.rest://localhost:2929</b> will create an <see cref="WebHttpBinding"/> and update the <paramref name="address"/> to <b>http://localhost:2929</b>.</description>
        ///     </item>
        ///     <item>
        ///         <term><b>net.tcp://</b></term>
        ///         <description>An <paramref name="address"/> of <b>net.tcp://localhost:2929</b> will create an <see cref="NetTcpBinding"/> and leave the <paramref name="address"/> unchanged.</description>
        ///     </item>
        ///     <item>
        ///         <term><b>net.p2p://</b></term>
        ///         <description>An <paramref name="address"/> of <b>net.p2p://localhost:2929</b> will create an <see cref="NetPeerTcpBinding"/> and leave the <paramref name="address"/> unchanged.</description>
        ///     </item>
        ///     <item>
        ///         <term><b>net.pipe://</b></term>
        ///         <description>An <paramref name="address"/> of <b>net.pipe://localhost:2929</b> will create an <see cref="NetNamedPipeBinding"/> and leave the <paramref name="address"/> unchanged.</description>
        ///     </item>
        ///     <item>
        ///         <term><b>net.msmq://</b></term>
        ///         <description>An <paramref name="address"/> of <b>net.msmq://localhost:2929</b> will create an <see cref="NetMsmqBinding"/> and leave the <paramref name="address"/> unchanged.</description>
        ///     </item>
        /// </list>
        /// <para>
        /// The <paramref name="enableSecurity"/> parameter value is ignored Mono deployments since security bindings are not implemented.
        /// </para>
        /// </remarks>
        public static Binding CreateServiceBinding(ref string address, bool enableSecurity)
        {
            address = address.Trim();
            if (string.IsNullOrEmpty(address))
                return null;

            int index = address.IndexOf("://");
            string scheme = address.Substring(0, index >= 0 ? index : address.Length);
            switch (scheme.ToLower())
            {
                case "http":
                case "http.soap11":
                    // Format address.
                    address = address.Replace("http.soap11", "http");
                    // Create binding.
                    BasicHttpBinding soap11Binding = new BasicHttpBinding();

#if !MONO
                    if (enableSecurity)
                    {
                        // Enable security.
                        soap11Binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                        soap11Binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
                    }
                    else
                    {
                        // Disable security.
                        soap11Binding.Security.Mode = BasicHttpSecurityMode.None;
                        soap11Binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                    }
#endif

                    return soap11Binding;
                case "http.soap12":
                    // Format address.
                    address = address.Replace("http.soap12", "http");
                    // Create binding.
                    WSHttpBinding soap12Binding = new WSHttpBinding();

#if !MONO
                    if (enableSecurity)
                    {
                        // Enable security.
                        soap12Binding.Security.Mode = SecurityMode.Transport;
                        soap12Binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
                    }
                    else
                    {
                        // Disable security.
                        soap12Binding.Security.Mode = SecurityMode.None;
                        soap12Binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                    }
#endif

                    return soap12Binding;
                case "http.duplex":
                    // Format address.
                    address = address.Replace("http.duplex", "http");
                    // Create binding.
                    WSDualHttpBinding duplexBinding = new WSDualHttpBinding();

#if !MONO
                    if (enableSecurity)
                    {
                        // Enable security.
                        duplexBinding.Security.Mode = WSDualHttpSecurityMode.Message;
                    }
                    else
                    {
                        // Disable security.
                        duplexBinding.Security.Mode = WSDualHttpSecurityMode.None;
                    }
#endif

                    return duplexBinding;
                case "http.rest":
                    // Format address.
                    address = address.Replace("http.rest", "http");
                    // Create binding.
                    WebHttpBinding restBinding = new WebHttpBinding();

#if !MONO
                    if (enableSecurity)
                    {
                        // Enable security.
                        restBinding.Security.Mode = WebHttpSecurityMode.TransportCredentialOnly;
                        restBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
                    }
                    else
                    {
                        // Disable security.
                        restBinding.Security.Mode = WebHttpSecurityMode.None;
                        restBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                    }
#endif

                    return restBinding;
                case "net.tcp":
                    // Create binding.
                    NetTcpBinding tcpBinding = new NetTcpBinding();

#if !MONO
                    if (enableSecurity)
                    {
                        // Enable security.
                        tcpBinding.Security.Mode = SecurityMode.Transport;
                        tcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                    }
                    else
                    {
                        // Disable sercurity.
                        tcpBinding.Security.Mode = SecurityMode.None;
                        tcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
                    }
#endif

                    return tcpBinding;
                case "net.p2p":
                    // Create binding.
#pragma warning disable 612, 618
                    NetPeerTcpBinding p2pBinding = new NetPeerTcpBinding();

#if !MONO
                    if (enableSecurity)
                    {
                        // Enable security.
                        p2pBinding.Security.Mode = SecurityMode.Transport;
                    }
                    else
                    {
                        // Disable security.
                        p2pBinding.Security.Mode = SecurityMode.None;
                    }
#endif

                    return p2pBinding;
                case "net.pipe":
                    // Create binding.
                    NetNamedPipeBinding pipeBinding = new NetNamedPipeBinding();

#if !MONO
                    if (enableSecurity)
                    {
                        // Enable security.
                        pipeBinding.Security.Mode = NetNamedPipeSecurityMode.Transport;
                    }
                    else
                    {
                        // Disable security.
                        pipeBinding.Security.Mode = NetNamedPipeSecurityMode.None;
                    }
#endif

                    return pipeBinding;
                case "net.msmq":
                    // Create binding.
                    NetMsmqBinding msmqBinding = new NetMsmqBinding();

#if !MONO
                    if (enableSecurity)
                    {
                        // Enable security.
                        msmqBinding.Security.Mode = NetMsmqSecurityMode.Transport;
                    }
                    else
                    {
                        // Disable security.
                        msmqBinding.Security.Mode = NetMsmqSecurityMode.None;
                    }
#endif

                    return msmqBinding;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the security setting of the hosting environment (For example: IIS web site or virtual directory).
        /// </summary>
        public static AuthenticationSchemes GetAuthenticationSchemes(Uri baseAddress)
        {
            Type type = Type.GetType(HostedAspNetEnvironment);
            object instance = Activator.CreateInstance(type, true);

            return (AuthenticationSchemes)instance.GetType().InvokeMember("GetAuthenticationSchemes", BindingFlags.InvokeMethod, null, instance, new object[] { baseAddress });
        }

        /// <summary>
        /// Gets the contract that the service implements.
        /// </summary>
        /// <param name="serviceType"><see cref="Type"/> of the service.</param>
        /// <returns><see cref="Type"/> of the service contract if found, otherwise null.</returns>
        public static Type GetServiceContract(Type serviceType)
        {
            // Find contract interface by service type name.
            Type contract = serviceType.GetInterface("I" + serviceType.Name);

            if ((object)contract == null)
            {
                // Fall back to the first interface the service implements.
                Type[] interfaces = serviceType.GetInterfaces();
                if (interfaces.Length > 0)
                    contract = interfaces[0];
            }

            return contract;
        }
    }
}
