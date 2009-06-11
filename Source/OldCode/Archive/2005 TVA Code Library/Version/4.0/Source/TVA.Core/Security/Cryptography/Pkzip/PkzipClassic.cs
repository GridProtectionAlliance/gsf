//*******************************************************************************************************
//  PkzipClassic.cs
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
using System.Security.Cryptography;
using TVA.IO.Checksums;

namespace TVA.Security.Cryptography.Pkzip
{
	/// <summary>
	/// PkzipClassic embodies the classic or original encryption facilities used in Pkzip archives.
	/// While it has been superceded by more recent and more powerful algorithms, its still in use and 
	/// is viable for preventing casual snooping
	/// </summary>
	public abstract class PkzipClassic : SymmetricAlgorithm
	{
		/// <summary>
		/// Generates new encryption keys based on given seed
		/// </summary>
		/// <param name="seed">The seed value to initialize keys with.</param>
		/// <returns>A new key value.</returns>
		static public byte[] GenerateKeys(byte[] seed)
		{
			if ( seed == null ) {
				throw new ArgumentNullException("seed");
			}

			if ( seed.Length == 0 ) {
				throw new ArgumentException("Length is zero", "seed");
			}

			uint[] newKeys = new uint[] {
				0x12345678,
				0x23456789,
				0x34567890
			 };
			
			for (int i = 0; i < seed.Length; ++i) {
				newKeys[0] = Crc32.ComputeCrc32(newKeys[0], seed[i]);
				newKeys[1] = newKeys[1] + (byte)newKeys[0];
				newKeys[1] = newKeys[1] * 134775813 + 1;
				newKeys[2] = Crc32.ComputeCrc32(newKeys[2], (byte)(newKeys[1] >> 24));
			}

			byte[] result = new byte[12];
			result[0] = (byte)(newKeys[0] & 0xff);
			result[1] = (byte)((newKeys[0] >> 8) & 0xff);
			result[2] = (byte)((newKeys[0] >> 16) & 0xff);
			result[3] = (byte)((newKeys[0] >> 24) & 0xff);
			result[4] = (byte)(newKeys[1] & 0xff);
			result[5] = (byte)((newKeys[1] >> 8) & 0xff);
			result[6] = (byte)((newKeys[1] >> 16) & 0xff);
			result[7] = (byte)((newKeys[1] >> 24) & 0xff);
			result[8] = (byte)(newKeys[2] & 0xff);
			result[9] = (byte)((newKeys[2] >> 8) & 0xff);
			result[10] = (byte)((newKeys[2] >> 16) & 0xff);
			result[11] = (byte)((newKeys[2] >> 24) & 0xff);
			return result;
		}
	}




}
