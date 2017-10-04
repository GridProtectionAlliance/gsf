//******************************************************************************************************
//  AdapterTest.cs - Gbtc
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
//  02/01/2011 - Denis Kholine
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#region [ University of Illinois/NCSA Open Source License ]
/*
Copyright © <2012> <University of Illinois> 
All rights reserved.

Developed by: 		<ITI>
                      	<University of Illinois>
                        <http://www.iti.illinois.edu/>
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal with the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
•	Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimers.
•	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimers in the documentation and/or other materials provided with the distribution.
•	Neither the names of <Name of Development Group, Name of Institution>, nor the names of its contributors may be used to endorse or promote products derived from this Software without specific prior written permission.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE SOFTWARE.
*/
#endregion

using System;
using GSF.Adapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests
{

    /// <summary>
    ///This is a test class for AdapterTest and is intended
    ///to contain all AdapterTest Unit Tests
    ///</summary>
    [TestClass]
    public class AdapterTest : IDisposable
    {
        #region DISPOSE
        private bool isDisposed;
        ~AdapterTest()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool isDisposing)
        {
            if (!isDisposed)
            {
                if (isDisposing)
                {
                    target.Dispose();
                }
                isDisposed = false;
            }
        }
        #endregion

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes

        Adapter target;
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        [TestInitialize]
        public void MyTestInitialize()
        {
            target = new Adapter();
        }

        //Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            target.Dispose();
        }

        #endregion

        /// <summary>
        /// A test for Adapter Constructor
        /// Initializes a new instance of the <see cref="Adapter"/>.
        /// </summary>
        [TestMethod]
        public void AdapterConstructorTest()
        {
            Assert.IsInstanceOfType(target, typeof(Adapter));
            Assert.IsNotNull(target);
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void DisposeTest()
        {
            try
            {
                target.Dispose();
            }
            catch
            {
                Assert.Inconclusive("Disposing error.");
            }
        }

        /// <summary>
        /// A test for Initialize
        /// Initializes the <see cref="Adapter"/>.
        /// </summary> 
        [TestMethod]
        public void InitializeTest()
        {
            try
            {
                target.Initialize();
            }
            catch
            {
                Assert.Inconclusive("Can't initialize Adapter instance.");
            }

        }

        /// <summary>
        ///A test for LoadSettings
        ///</summary>
        [TestMethod]
        public void LoadSettingsTest()
        {
            try
            {
                target.LoadSettings();
            }
            catch
            {
                Assert.Inconclusive("Can't load settings.");
            }
        }

        /// <summary>
        /// A test for OnDisposed
        /// Raises the <see cref="Disposed"/> event.
        /// </summary>       
        [TestMethod]
        [DeploymentItem("GSF.Core.dll")]
        public void OnDisposedTest()
        {
            try
            {
                target.Dispose();
            }
            catch
            {
                Assert.Fail();
            }
        }

        ///// <summary>
        /////A test for OnExecutionException
        /////</summary>
        //[TestMethod]
        //[DeploymentItem("GSF.Core.dll")]
        //public void OnExecutionExceptionTest()
        //{
        //    // Creation of the private accessor for 'Microsoft.VisualStudio.TestTools.TypesAndSymbols.Assembly' failed
        //    Assert.Inconclusive("Exception verification implementation needed");
        //}

        ///// <summary>
        ///// A test for OnStatusUpdate
        ///// Raises the <see cref="StatusUpdate"/> event.
        ///// </summary>
        ///// <param name="updateType"><see cref="UpdateType"/> to send to <see cref="StatusUpdate"/> event.</param>
        ///// <param name="updateMessage">Update message to send to <see cref="StatusUpdate"/> event.</param>
        ///// <param name="args">Arguments to be used when formatting the <paramref name="updateMessage"/>.</param>       
        //[TestMethod]
        //[DeploymentItem("GSF.Core.dll")]
        //public void OnStatusUpdateTest()
        //{
        //    // Creation of the private accessor for 'Microsoft.VisualStudio.TestTools.TypesAndSymbols.Assembly' failed
        //    Assert.Inconclusive("Creation of the private accessor for \'Microsoft.VisualStudio.TestTools.TypesAndSy" +
        //            "mbols.Assembly\' failed");
        //}

        /// <summary>
        /// A test for SaveSettings
        /// Saves <see cref="Adapter"/> settings to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception>        
        [TestMethod]
        public void SaveSettingsTest()
        {
            target.PersistSettings = true;
            try
            {
                target.SaveSettings();
            }
            catch
            {
                Assert.Inconclusive("Configuration error exception");
            }
        }

        /// <summary>
        /// A test for Domain
        /// Gets the <see cref="AppDomain"/> in which the <see cref="Adapter"/> is executing.
        /// </summary>
        [TestMethod]
        public void DomainTest()
        {
            AppDomain actual;
            actual = target.Domain;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        /// A test for Enabled
        /// Gets or sets a boolean value that indicates whether the <see cref="Adapter"/> is currently enabled.
        /// </summary>
        [TestMethod]
        public void EnabledTest()
        {
            bool expected = false;
            bool actual;
            target.Enabled = expected;
            actual = target.Enabled;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for HostFile
        /// Gets or sets the path to the file where the <see cref="Adapter"/> is housed.
        /// </summary>
        /// <remarks>
        /// This can be used to update the <see cref="Adapter"/> when changes are made to the file where it is housed.
        /// </remarks> 
        [TestMethod]
        public void HostFileTest()
        {
            string expected = "Host File";
            string actual;
            target.HostFile = expected;
            actual = target.HostFile;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for MemoryUsage
        /// Gets the memory utilzation of the <see cref="Adapter"/> in bytes if executing in a separate <see cref="AppDomain"/>, otherwise <see cref="Double.NaN"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="MemoryUsage"/> gets updated only after a full blocking collection by <see cref="GC"/> (e.g. <see cref="GC.Collect()"/>).
        /// </para>
        /// <para>
        /// This method always returns <c><see cref="Double.NaN"/></c> under Mono deployments.
        /// </para>
        /// </remarks> 
        [TestMethod]
        public void MemoryUsageTest()
        {
            double actual;
            if (AppDomain.MonitoringIsEnabled == false)
            {
                AppDomain.MonitoringIsEnabled = true;
            }
            actual = target.MemoryUsage;
            Assert.AreNotEqual(Double.NaN, actual);

        }

        /// <summary>
        /// A test for Name
        /// Gets the unique identifier of the <see cref="Adapter"/>.
        /// </summary> 
        [TestMethod]
        public void NameTest()
        {
            string actual;
            actual = target.Name;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        /// A test for PersistSettings
        /// Gets or sets a boolean value that indicates whether <see cref="Adapter"/> settings are to be saved to the config file.
        /// </summary>
        [TestMethod]
        public void PersistSettingsTest()
        {
            bool expected = false;
            bool actual;
            target.PersistSettings = expected;
            actual = target.PersistSettings;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for ProcessorUsage
        /// Gets the % processor utilization of the <see cref="Adapter"/> if executing in a separate <see cref="AppDomain"/> otherwise <see cref="Double.NaN"/>.
        /// </summary>
        /// <remarks>
        /// This method always returns <c><see cref="Double.NaN"/></c> under Mono deployments.
        /// </remarks> 
        [TestMethod]
        public void ProcessorUsageTest()
        {
            double actual;
            if (AppDomain.MonitoringIsEnabled == false)
            {
                AppDomain.MonitoringIsEnabled = true;
            }
            actual = target.ProcessorUsage;
            Assert.AreNotEqual(Double.NaN, actual);
        }

        /// <summary>
        /// A test for SettingsCategory
        /// Saves <see cref="Adapter"/> settings to the config file if the <see cref="PersistSettings"/> property is set to true.
        /// </summary>
        /// <exception cref="ConfigurationErrorsException"><see cref="SettingsCategory"/> has a value of null or empty string.</exception> 
        [TestMethod]
        public void SettingsCategoryTest()
        {

            string expected = "Config File";
            string actual;
            target.SettingsCategory = expected;
            actual = target.SettingsCategory;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for Status
        /// Gets the descriptive status of the <see cref="Adapter"/>.
        /// </summary> 
        [TestMethod]
        public void StatusTest()
        {
            string actual;
            actual = target.Status;
            Assert.IsNotNull(actual);
        }

        /// <summary>
        /// A test for TypeName
        /// Gets or sets the text representation of the <see cref="Adapter"/>'s <see cref="TypeName"/>.
        /// </summary>
        /// <remarks>
        /// This can be used for looking up the <see cref="TypeName"/> of the <see cref="Adapter"/> when deserializing it using <see cref="XmlSerializer"/>.
        /// </remarks> 
        [TestMethod]
        public void TypeNameTest()
        {
            string expected = "GSF.Adapters.Adapter, GSF.Core";
            string actual;
            target.TypeName = expected;
            actual = target.TypeName;
            Assert.AreEqual(expected, actual);

        }
    }
}
