//******************************************************************************************************
//  RunningAverage.cs - Gbtc
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
//  11/2/2015 - Ryan McCoy
//       Generated original version of source code.
//
//******************************************************************************************************

namespace PowerCalculations.PowerMultiCalculator
{
	/// <summary>
	/// Calculates running average of a value
	/// </summary>
	public class RunningAverage
	{
		private double m_numberOfValues = 0;
		private double m_average = 0;

		/// <summary>
		/// Average calculated on values provided so far
		/// </summary>
		public double Average
		{
			get { return m_average; }
		}

		/// <summary>
		/// Calculates running average based on previous values and the new value
		/// </summary>
		/// <param name="value">Value to be added to the running average</param>
		/// <returns>New running average</returns>
		public double AddValue(double value)
		{
			var total = m_numberOfValues * m_average + value;
			m_numberOfValues++;
			m_average = total / m_numberOfValues;
			return m_average;
		}
	}
}
