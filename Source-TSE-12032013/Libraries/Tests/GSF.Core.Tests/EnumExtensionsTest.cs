//******************************************************************************************************
//  EnumExtensionsTest.cs - Gbtc
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
//  12/22/2010 - Pinal C. Patel
//       Generated original version of source code.
//  12/13/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GSF.Core.Tests
{
    [TestClass]
    public class EnumExtensionsTest
    {
        private const string TestEnumOneDescription = "Description of One.";

        private enum TestEnum
        {
            [System.ComponentModel.Description(TestEnumOneDescription)]
            One,
            Two
        }

        [TestMethod]
        public void GetDescription_WithDescription()
        {
            // Arrange
            TestEnum item = TestEnum.One;
            // Act
            string description = item.GetDescription();
            // Assert
            Assert.AreEqual(TestEnumOneDescription, description);
        }

        [TestMethod]
        public void GetDescription_WithoutDescription()
        {
            // Arrange
            TestEnum item = TestEnum.Two;
            // Act
            string description = item.GetDescription();
            // Assert
            Assert.AreEqual("Two", description);
        }

        [TestMethod]
        public void GetEnumFromDescription_Match()
        {
            // Arrange
            string description = TestEnumOneDescription;
            // Act
            TestEnum enumeration = (TestEnum)description.GetEnumValueByDescription(typeof(TestEnum));
            // Assert
            Assert.AreEqual(TestEnum.One, enumeration);
        }

        [TestMethod]
        public void GetEnumFromDescription_NoMatch()
        {
            // Arrange
            string description = "No such description";
            // Act
            object enumeration = description.GetEnumValueByDescription(typeof(TestEnum));
            // Assert
            Assert.AreEqual(null, enumeration);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public void GetEnumFromDescription_ThrowException()
        {
            // Arrange
            string description = "No such description";
            // Act
            object enumeration = description.GetEnumValueByDescription(typeof(object));
            // Assert
            Assert.AreEqual(null, enumeration);
        }
    }
}
