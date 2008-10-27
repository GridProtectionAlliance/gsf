//*******************************************************************************************************
//  PkzipClassicCryptoBase.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  09/25/2008 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

// PkzipClassic encryption
//
// Copyright 2004 John Reilly
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
// 
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

using System;
using PCS.IO.Checksums;

namespace PCS.Security.Cryptography.Pkzip
{
    /// <summary>
    /// PkzipClassicCryptoBase provides the low level facilities for encryption
    /// and decryption using the PkzipClassic algorithm.
    /// </summary>
    class PkzipClassicCryptoBase
    {
        /// <summary>
        /// Transform a single byte 
        /// </summary>
        /// <returns>
        /// The transformed value
        /// </returns>
        protected byte TransformByte()
        {
            uint temp = ((keys[2] & 0xFFFF) | 2);
            return (byte)((temp * (temp ^ 1)) >> 8);
        }

        /// <summary>
        /// Set the key schedule for encryption/decryption.
        /// </summary>
        /// <param name="keyData">The data use to set the keys from.</param>
        protected void SetKeys(byte[] keyData)
        {
            if (keyData == null)
            {
                throw new ArgumentNullException("keyData");
            }

            if (keyData.Length != 12)
            {
                throw new InvalidOperationException("Key length is not valid");
            }

            keys = new uint[3];
            keys[0] = (uint)((keyData[3] << 24) | (keyData[2] << 16) | (keyData[1] << 8) | keyData[0]);
            keys[1] = (uint)((keyData[7] << 24) | (keyData[6] << 16) | (keyData[5] << 8) | keyData[4]);
            keys[2] = (uint)((keyData[11] << 24) | (keyData[10] << 16) | (keyData[9] << 8) | keyData[8]);
        }

        /// <summary>
        /// Update encryption keys 
        /// </summary>		
        protected void UpdateKeys(byte ch)
        {
            keys[0] = Crc32.ComputeCrc32(keys[0], ch);
            keys[1] = keys[1] + (byte)keys[0];
            keys[1] = keys[1] * 134775813 + 1;
            keys[2] = Crc32.ComputeCrc32(keys[2], (byte)(keys[1] >> 24));
        }

        /// <summary>
        /// Reset the internal state.
        /// </summary>
        protected void Reset()
        {
            keys[0] = 0;
            keys[1] = 0;
            keys[2] = 0;
        }

        #region Instance Fields
        uint[] keys;
        #endregion
    }
}
