#region [ Modification History ]
/*
 * 11/01/2012 Denis Kholine
 *  Generated Original version of source code.
 * 11/14/2012 Denis Kholine
 *  Update namespace
 */
#endregion

#region  [ UIUC NCSA Open Source License ]
/*
Copyright © <2012> <University of Illinois>
All rights reserved.

Developed by: <ITI>
<University of Illinois>
<http://www.iti.illinois.edu/>
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal with the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
• Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimers.
• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimers in the documentation and/or other materials provided with the distribution.
• Neither the names of <Name of Development Group, Name of Institution>, nor the names of its contributors may be used to endorse or promote products derived from this Software without specific prior written permission.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE CONTRIBUTORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS WITH THE SOFTWARE.
*/
#endregion

#region [ Using ]
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GSF.TestsSuite.PhasorProtocols.Wrappers.IEEE.C37118;
using GSF.TestsSuite.PhasorProtocols.Wrappers;
using System.Runtime.Serialization;
using GSF.PhasorProtocols.IEEEC37_118;
#endregion

namespace GSF.TestsSuite.PhasorProtocols.Cases.IEEE.C37118
{
    public class CommandFrameCase
    {
        #region [ Members ]
        private string m_commandframe;
        /// <summary>
        /// Typical data stream synchrnonization byte.
        /// </summary>
        private const byte SyncByte = 0xAA;
        private CommandFrame m_CommandFrame;
        private byte[] m_buffer;
        private int m_startIndex;
        private int m_length;
        #endregion

        #region [ Properties ]
        public byte[] buffer
        {
            get
            {
                return m_buffer;
            }
        }
        public int startIndex
        {
            get
            {
                return m_startIndex;
            }
        }
        public int length
        {
            get
            {
                return m_length;
            }
        }
        public CommandFrame CommandFrame
        {
            get
            {
                return m_CommandFrame;
            }
        }
        #endregion

        #region [ Constructors ]
        public CommandFrameCase()
        {
            m_commandframe = "170 065 000 018 000 000 080 198 012 170 000 000 178 180 000 001 056 058";
            m_buffer = new byte[18];
            m_startIndex = 0;
            m_buffer = m_commandframe.Split(' ').Select(x => Convert.ToByte(x)).ToArray();
            m_length = m_buffer.Length;
            m_CommandFrame = new CommandFrame(m_buffer, m_startIndex, m_length);
        }
        #endregion
    }
}
