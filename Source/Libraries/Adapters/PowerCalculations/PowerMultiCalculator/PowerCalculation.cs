//******************************************************************************************************
//  PowerCalculation.cs - Gbtc
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

using GSF.TimeSeries;

namespace PowerCalculations.PowerMultiCalculator
{
	/// <summary>
	/// Model class for power calculations stored in the configuration repository
	/// </summary>
	public class PowerCalculation
	{
		#region [ Properties ]

		/// <summary>
		/// ID field for the <see cref="PowerCalculation"/>
		/// </summary>
		public int PowerCalculationID { get; set; }

		/// <summary>
		/// Circuit Description field for the <see cref="PowerCalculation"/>
		/// </summary>
		public string CircuitDescription { get; set; }

		/// <summary>
		/// Measurement key from the <see cref="PowerCalculation"/>'s Voltage Angle <see cref="PowerMeasurement"/> 
		/// </summary>
		public MeasurementKey VoltageAngleMeasurementKey { get; set; }

		/// <summary>
		/// Measurement key from the <see cref="PowerCalculation"/>'s Voltage Magnitude <see cref="PowerMeasurement"/> 
		/// </summary>
		public MeasurementKey VoltageMagnitudeMeasurementKey { get; set; }

		/// <summary>
		/// Measurement key from the <see cref="PowerCalculation"/>'s Current Angle <see cref="PowerMeasurement"/> 
		/// </summary>
		public MeasurementKey CurrentAngleMeasurementKey { get; set; }

		/// <summary>
		/// Measurement key from the <see cref="PowerCalculation"/>'s Current Magnitude <see cref="PowerMeasurement"/> 
		/// </summary>
		public MeasurementKey CurrentMagnitudeMeasurementKey { get; set; }

		/// <summary>
		/// Measurement template to be used for active power output values from the <see cref="PowerCalculation"/>
		/// </summary>
		public IMeasurement ActivePowerOutputMeasurement { get; set; }

		/// <summary>
		/// Measurement template to be used for reactive power output values from the <see cref="PowerCalculation"/>
		/// </summary>
		public IMeasurement ReactivePowerOutputMeasurement { get; set; }

		/// <summary>
		/// Measurement template to be used for apparent power output values from the <see cref="PowerCalculation"/>
		/// </summary>
		public IMeasurement ApparentPowerOutputMeasurement { get; set; }

		#endregion
	}
}
