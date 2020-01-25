//******************************************************************************************************
//  GrafanaAlarm.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  01/23/2020 - C. Lackner
//       Generated original version of source code.
//
//******************************************************************************************************

using GSF.Data.Model;
using System;

namespace GrafanaAdapters
{
	/// <summary>
	/// Defines a Grafana Alarm. Similar to the GSF Alarm but without <see cref="AlarmState"/> location request.
	/// </summary>
	[TableName("Alarm")]
	public class GrafanaAlarm
	{
		public Guid NodeID { get; set; }

		[PrimaryKey]
		public int ID { get; set; }

		public string TagName { get; set; }

		public Guid SignalID { get; set; }

		public Guid AssociatedMeasurementID { get; set; }

		public string Description { get; set; }

		public int Severity { get; set; }

		public int Operation { get; set; }

		public double SetPoint { get; set; }
		public double Tolerance { get; set; }
		public double Delay { get; set; }
		public double Hysteresis { get; set; }
		public int LoadOrder { get; set; }
		public bool Enabled { get; set; }
		public DateTime CreatedOn { get; set; }
		public string CreatedBy { get; set; }
		public DateTime UpdatedOn { get; set; }
		public string UpdatedBy { get; set; }
	}
}
