using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using GSF;
using GSF.Data;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.DataModels;

namespace PowerCalculations.UI.DataModels
{
	public class PowerCalculation : DataModelBase
	{

		#region [ Members ]

		private int m_powerCalculationId;
		private string m_circuitDescription;
        private Measurement m_realPowerOutputMeasurement;
        private Measurement m_reactivePowerOutputMeasurement;
        private Measurement m_activePowerOutputMeasurement;
		private bool m_powerCalculationEnabled;
		private Guid m_nodeId;
		private Phasor m_voltagePhasor;
		private Phasor m_currentPhasor;

		#endregion

		#region [ Properties ]

		public int PowerCalculationId
		{
			get { return m_powerCalculationId; }
			set
			{
				if (m_powerCalculationId == value) return;
				m_powerCalculationId = value;
				OnPropertyChanged("PowerCalculationId");
			}
		}

		[Required(ErrorMessage = "Power calculation description is a required field. Please provide a value.")]
		[StringLength(4000, ErrorMessage = "Power calculation description cannot exceed 4000 characters")]
		public string CircuitDescription
		{
			get { return m_circuitDescription; }
			set
			{
				if (m_circuitDescription == value) return;
				m_circuitDescription = value;
				OnPropertyChanged("CircuitDescription");
			}
		}

		public Measurement RealPowerOutputMeasurement
		{
			get { return m_realPowerOutputMeasurement; }
			set
			{
				if (m_realPowerOutputMeasurement == value) return;
				m_realPowerOutputMeasurement = value;
				OnPropertyChanged("RealPowerOutputMeasurement");
			}
		}

		public Measurement ReactivePowerOutputMeasurement
		{
			get { return m_reactivePowerOutputMeasurement; }
			set
			{
				if (m_reactivePowerOutputMeasurement == value) return;
				m_reactivePowerOutputMeasurement = value;
				OnPropertyChanged("ReactivePowerOutputMeasurement");
			}
		}

		public Measurement ActivePowerOutputMeasurement
		{
			get { return m_activePowerOutputMeasurement; }
			set
			{
				if (m_activePowerOutputMeasurement == value) return;
				m_activePowerOutputMeasurement = value;
				OnPropertyChanged("ActivePowerOutputMeasurement");
			}
		}

		[Required(ErrorMessage = "Enabled is a required field. Please enter a value for the enabled flag.")]
		public bool PowerCalculationEnabled
		{
			get { return m_powerCalculationEnabled; }
			set
			{
				if (m_powerCalculationEnabled == value) return;
				m_powerCalculationEnabled = value;
				OnPropertyChanged("PowerCalculationEnabled");
			}
		}

		[Required(ErrorMessage = "Node ID is a required field. Please enter a value for the Node ID.")]
		public Guid NodeId
		{
			get { return m_nodeId; }
			set
			{
				if (m_nodeId == value) return;
				m_nodeId = value;
				OnPropertyChanged("NodeId");
			}
		}

		public Phasor VoltagePhasor
		{
			get { return m_voltagePhasor; }
			set
			{
				if (m_voltagePhasor == value) return;
				m_voltagePhasor = value;
				OnPropertyChanged("VoltagePhasor");
                UpdateCircuitDescription();
            }
		}

		public Phasor CurrentPhasor
		{
			get { return m_currentPhasor; }
			set
			{
				if (m_currentPhasor == value) return;
				m_currentPhasor = value;
				OnPropertyChanged("CurrentPhasor");
                UpdateCircuitDescription();
            }
        }

        private void UpdateCircuitDescription()
        {
            if ((object)m_voltagePhasor != null && (object)m_currentPhasor != null)
                CircuitDescription = $"{LookupDeviceName(m_voltagePhasor.DeviceId)}-{(m_currentPhasor.Label ?? "").ToUpperInvariant().RemoveWhiteSpace()}";
        }


        #endregion


        #region [ Statics ]

        /// <summary>
        /// LoadKeys <see cref="PowerCalculation"/> information as an <see cref="ObservableCollection{T}"/> style list.
        /// </summary>
        /// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
        /// <returns>Collection of <see cref="int"/>.</returns>
        public static IList<int> LoadKeys(AdoDataConnection database)
		{
			var createdConnection = false;

			try
			{
				createdConnection = CreateConnection(ref database);

				var calculationList = new List<int>();
				var query = string.Format("SELECT PowerCalculationId FROM PowerCalculation WHERE NodeId='{0}'", database.CurrentNodeID());
				var calculationTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout);

				foreach (DataRow row in calculationTable.Rows)
				{
					calculationList.Add(row.ConvertField<int>("PowerCalculationId"));
				}

				return calculationList;
			}
			finally
			{
				if (createdConnection && database != null)
					database.Dispose();
			}
		}

        private static string LookupDeviceName(int deviceID)
        {
            AdoDataConnection database = null;
            string deviceName = "";

            if (CreateConnection(ref database))
            {
                deviceName = database.ExecuteScalar<string>("SELECT Acronym FROM Device WHERE ID={0}", deviceID) ?? "";
                database.Dispose();
            }

            return deviceName;
        }

		/// <summary>
		/// Loads <see cref="PowerCalculation"/> information as an <see cref="ObservableCollection{T}"/> style list.
		/// </summary>
		/// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
		/// <param name="keys">Keys of the measurement to be loaded from the database</param>
		/// <returns>Collection of <see cref="PowerCalculation"/>.</returns>
		public static ObservableCollection<PowerCalculation> Load(AdoDataConnection database, IList<int> keys)
		{
			var createdConnection = false;

			try
			{
				createdConnection = CreateConnection(ref database);

				PowerCalculation[] calculationList = null;

				if (keys != null && keys.Count > 0)
				{
					var commaSeparatedKeys = keys.Select(key => key.ToString()).Aggregate((str1, str2) => str1 + "," + str2);
					var query = string.Format("select pc.PowerCalculationId, pc.CircuitDescription, pc.VoltageAngleSignalId, pc.VoltageMagSignalId, pc.CurrentAngleSignalId, " +
		 									  "       pc.CurrentMagSignalId, pc.RealPowerOutputSignalId, pc.ReactivePowerOutputSignalId, pc.ActivePowerOutputSignalId, pc.CalculationEnabled, " +
											  "	   pc.nodeid, vp.id voltagePhasorId, cp.id as currentPhasorId " +
											  "from powercalculation pc " +
											  "     left join measurement vm " +
											  "	 on pc.voltagemagsignalid=vm.signalid " +
											  "	 left join measurement cm " +
											  "	 on pc.currentmagsignalid=cm.signalid " +
											  "	 left join phasor vp " +
											  "	 on vm.deviceid=vp.deviceid and vm.phasorsourceindex=vp.sourceindex " +
											  "	 left join phasor cp " +
											  " on cm.deviceid=cp.deviceid and cm.phasorsourceindex=cp.sourceindex WHERE PowerCalculationId IN ({0})", commaSeparatedKeys);
					var calculationTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout);
					calculationList = new PowerCalculation[calculationTable.Rows.Count];

					foreach (DataRow row in calculationTable.Rows)
					{
						var id = row.ConvertField<int>("PowerCalculationId");

						var voltageAngle = database.Guid(row, "VoltageAngleSignalId");
						var voltageMagnitude = database.Guid(row, "VoltageMagSignalId");
						var currentAngle = database.Guid(row, "CurrentAngleSignalId");
						var currentMagnitude = database.Guid(row, "CurrentMagSignalId");

						Guid? realPower = null;
						if (!row.IsNull("RealPowerOutputSignalId"))
							realPower = database.Guid(row, "RealPowerOutputSignalId");

						Guid? reactivePower = null;
						if (!row.IsNull("ReactivePowerOutputSignalId"))
							reactivePower = database.Guid(row, "ReactivePowerOutputSignalId");

						Guid? activePower = null;
						if (!row.IsNull("ActivePowerOutputSignalId"))
							activePower = database.Guid(row, "ActivePowerOutputSignalId");

						var currentPhasorId = row.ConvertField<int>("currentPhasorId");
						var voltagePhasorId = row.ConvertField<int>("voltagePhasorId");
						var phasors = Phasor.Load(database, new List<int> {currentPhasorId, voltagePhasorId});

						calculationList[keys.IndexOf(id)] = new PowerCalculation
						{
							PowerCalculationId = row.Field<int>("PowerCalculationId"),
							CircuitDescription = row.Field<string>("CircuitDescription"),
							PowerCalculationEnabled = row.Field<bool>("CalculationEnabled"),
							NodeId = database.Guid(row, "NodeId")
						};

						foreach (var phasor in phasors)
						{
							if (phasor.Id == currentPhasorId)
							{
								calculationList[keys.IndexOf(id)].CurrentPhasor = phasor;
							}
							else if (phasor.Id == voltagePhasorId)
							{
								calculationList[keys.IndexOf(id)].VoltagePhasor = phasor;
							}
						}

						var mkeys = new List<Guid> { voltageAngle, voltageMagnitude, currentAngle, currentMagnitude };
						if (realPower != null) { mkeys.Add(realPower.Value); }
						if (reactivePower != null) { mkeys.Add(reactivePower.Value); }
						if (activePower != null) { mkeys.Add(activePower.Value); }
						var measurements = Measurement.LoadFromKeys(database, mkeys);
						foreach (var measurement in measurements)
						{
							if (measurement.SignalID == voltageAngle)
							{
								calculationList[keys.IndexOf(id)].VoltagePhasor.AngleMeasurement = measurement;
							}
							else if (measurement.SignalID == voltageMagnitude)
							{
								calculationList[keys.IndexOf(id)].VoltagePhasor.MagnitudeMeasurement = measurement;
							}
							else if (measurement.SignalID == currentAngle)
							{
								calculationList[keys.IndexOf(id)].CurrentPhasor.AngleMeasurement = measurement;
							}
							else if (measurement.SignalID == currentMagnitude)
							{
								calculationList[keys.IndexOf(id)].CurrentPhasor.MagnitudeMeasurement = measurement;
							}
							else if (measurement.SignalID == realPower)
							{
								calculationList[keys.IndexOf(id)].RealPowerOutputMeasurement = measurement;
							}
							else if (measurement.SignalID == reactivePower)
							{
								calculationList[keys.IndexOf(id)].ReactivePowerOutputMeasurement = measurement;
							}
							else if (measurement.SignalID == activePower)
							{
								calculationList[keys.IndexOf(id)].ActivePowerOutputMeasurement = measurement;
							}
						}
					}


				}

				return new ObservableCollection<PowerCalculation>(calculationList ?? new PowerCalculation[0]);
			}
			finally
			{
				if (createdConnection && database != null)
					database.Dispose();
			}
		}

		/// <summary>
		/// Deletes specified <see cref="PowerCalculation"/> record from database.
		/// </summary>
		/// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
		/// <param name="powerCalculationId">ID of the record to be deleted.</param>
		/// <returns>String, for display use, indicating success.</returns>
		public static string Delete(AdoDataConnection database, int powerCalculationId)
		{
			bool createdConnection = false;

			try
			{
				createdConnection = CreateConnection(ref database);

				// Setup current user context for any delete triggers
				CommonFunctions.SetCurrentUserContext(database);

				database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM PowerCalculation WHERE PowerCalculationId = {0}", "companyID"), DefaultTimeout, powerCalculationId);

				return "Power Calculation deleted successfully";
			}
			finally
			{
				if (createdConnection && database != null)
					database.Dispose();
			}
		}

		/// <summary>
		/// Saves <see cref="Company"/> information to database.
		/// </summary>
		/// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
		/// <param name="powerCalculation">Information about <see cref="PowerCalculation"/>.</param>        
		/// <returns>String, for display use, indicating success.</returns>
		public static string Save(AdoDataConnection database, PowerCalculation powerCalculation)
		{
			bool createdConnection = false;
			string query;

			try
			{
				createdConnection = CreateConnection(ref database);

				if (powerCalculation.PowerCalculationId == 0)
				{
					query = database.ParameterizedQueryString("INSERT INTO PowerCalculation (CircuitDescription, VoltageAngleSignalId, VoltageMagSignalId, CurrentAngleSignalId, CurrentMagSignalId, RealPowerOutputSignalId, ReactivePowerOutputSignalId, ActivePowerOutputSignalId, CalculationEnabled, NodeId) " +
						"VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})", "circuitDescription", "voltageAngle", "voltageMag", "currentAngle", "currentMag", "realPowerOutput", "reactivePowerOutput", "activePowerOutput", "calculationEnabled", "nodeId");

					database.Connection.ExecuteNonQuery(query, DefaultTimeout, new[] { powerCalculation.CircuitDescription, database.Guid(powerCalculation.VoltagePhasor.AngleMeasurement.SignalID),
						database.Guid(powerCalculation.VoltagePhasor.MagnitudeMeasurement.SignalID), database.Guid(powerCalculation.CurrentPhasor.AngleMeasurement.SignalID), database.Guid(powerCalculation.CurrentPhasor.MagnitudeMeasurement.SignalID),
						powerCalculation.RealPowerOutputMeasurement == null ? DBNull.Value : database.Guid(powerCalculation.RealPowerOutputMeasurement.SignalID), powerCalculation.ReactivePowerOutputMeasurement == null ? DBNull.Value : database.Guid(powerCalculation.ReactivePowerOutputMeasurement.SignalID), powerCalculation.ActivePowerOutputMeasurement == null ? DBNull.Value : database.Guid(powerCalculation.ActivePowerOutputMeasurement.SignalID),
						powerCalculation.PowerCalculationEnabled, database.CurrentNodeID() });
				}
				else
				{
					query = database.ParameterizedQueryString("UPDATE PowerCalculation SET CircuitDescription = {0}, VoltageAngleSignalId = {1}, VoltageMagSignalId = {2}, CurrentAngleSignalId = {3}, CurrentMagSignalId = {4}, " +
						"RealPowerOutputSignalId = {5}, ReactivePowerOutputSignalId = {6}, ActivePowerOutputSignalId = {7}, CalculationEnabled = {8} WHERE PowerCalculationId = {9}", "circuitDescription", "voltageAngle", "voltageMag", "currentAngle", "currentMag", "realPowerOutput", "reactivePowerOutput", "activePowerOutput", "calculationEnabled", "powerCalculationId");

					database.Connection.ExecuteNonQuery(query, DefaultTimeout, new[] { powerCalculation.CircuitDescription,
						database.Guid(powerCalculation.VoltagePhasor.AngleMeasurement.SignalID), database.Guid(powerCalculation.VoltagePhasor.MagnitudeMeasurement.SignalID), database.Guid(powerCalculation.CurrentPhasor.AngleMeasurement.SignalID), database.Guid(powerCalculation.CurrentPhasor.MagnitudeMeasurement.SignalID),
						powerCalculation.RealPowerOutputMeasurement == null ? DBNull.Value : database.Guid(powerCalculation.RealPowerOutputMeasurement.SignalID), powerCalculation.ReactivePowerOutputMeasurement == null ? DBNull.Value : database.Guid(powerCalculation.ReactivePowerOutputMeasurement.SignalID), powerCalculation.ActivePowerOutputMeasurement == null ? DBNull.Value : database.Guid(powerCalculation.ActivePowerOutputMeasurement.SignalID), powerCalculation.PowerCalculationEnabled, powerCalculation.PowerCalculationId });
				}

				return "Power Calculation information saved successfully";
			}
			finally
			{
				if (createdConnection && database != null)
					database.Dispose();
			}
		}

		#endregion

	}
}
