//******************************************************************************************************
//  SerialException.cs - Gbtc
//
//  Copyright © 2019, Grid Protection Alliance.  All Rights Reserved.
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
//  12/10/2019 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.IO.Ports;
using System.Runtime.Serialization;

namespace GSF.Communication
{
    /// <summary>
    /// Defines an exception for errors related <see cref="SerialClient"/> connections.
    /// </summary>
    [Serializable]
    public class SerialException : Exception
    {
        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialException"/> class.
        /// </summary>
        /// <param name="serialError">The <see cref="System.IO.Ports.SerialError"/> associated with the exception.</param>
        /// <param name="message">The error message that explains the reason for the exception, or <see langword="null"/> to use the default message associated with <paramref name="serialError"/>.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or <see langword="null"/> if no inner exception is specified.</param>
        public SerialException(SerialError serialError, string message = null, Exception innerException = null) : base(message ?? GetMessage(serialError), innerException)
        {
            SerialError = serialError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException"> <paramref name="info"/> is <see langword="null"/>.</exception>
        protected SerialException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            
            SerialError = (SerialError)info.GetValue(nameof(SerialError), typeof(SerialError));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the <see cref="System.IO.Ports.SerialError"/> associated with the <see cref="SerialException"/>.
        /// </summary>
        public SerialError SerialError { get; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is <see langword="null"/>.</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(SerialError), SerialError, typeof(SerialError));
        }

        #endregion

        #region [ Static ]

        // Static Methods
        private static string GetMessage(SerialError serialError)
        {
            switch (serialError)
            {
                case SerialError.Frame:
                    return "The hardware detected a framing error.";
                case SerialError.Overrun:
                    return "A character-buffer overrun has occurred. The next character is lost.";
                case SerialError.RXOver:
                    return "An input buffer overflow has occurred. There is either no room in the input buffer, or a character was received after the end-of-file (EOF) character.";
                case SerialError.RXParity:
                    return "The hardware detected a parity error.";
                case SerialError.TXFull:
                    return "The application tried to transmit a character, but the output buffer was full.";
                default:
                    throw new ArgumentOutOfRangeException(nameof(serialError), serialError, $"Unexpected serial error encountered: {serialError}");
            }
        }

        #endregion
    }
}
