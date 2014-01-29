#region [ Modification History ]
/*
 * 11/06/2012 Denis Kholine
 *  Generated Original version of source code.
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
using GSF.PhasorProtocols.IEEEC37_118;
using GSF.PhasorProtocols;
using GSF;
#endregion

namespace GSF.TestsSuite.PhasorProtocols.Cases.IEEE.C37118
{
    public class DataFrameCase
    {
        #region [ Members ]
        private DataFrame m_DataFrame;
        #endregion

        #region [ Properties ]
        public DataFrame DataFrame
        {
            get
            {
                return m_DataFrame;
            }
        }
        #endregion

        #region [ Constructors ]
        public DataFrameCase()
        {
            m_DataFrame = new DataFrame();
            m_DataFrame.Published = false;
            //m_DataFrame.Timestamp = DateTime.Parse("2012-11-05 16:27:13.366").Ticks;
            //DataFrameBase IDCore is read only in class but contains set property and definition need verification
            //m_DataFrame.IDCode = 1;
        }
        #endregion
    }
}
