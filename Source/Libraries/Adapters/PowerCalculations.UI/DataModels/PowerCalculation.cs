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

		private int m_id;
		private string m_circuitDescription;
        private Measurement m_activePowerOutputMeasurement;
        private Measurement m_reactivePowerOutputMeasurement;
        private Measurement m_apparentPowerOutputMeasurement;
		private bool m_enabled;
		private Guid m_nodeID;
		private Phasor m_voltagePhasor;
		private Phasor m_currentPhasor;

		#endregion

		#region [ Properties ]

		public int ID
		{
			get { return m_id; }
			set
			{
				if (m_id == value) return;
				m_id = value;
				OnPropertyChanged(nameof(ID));
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
				OnPropertyChanged(nameof(CircuitDescription));
			}
		}

		public Measurement ActivePowerOutputMeasurement
		{
			get { return m_activePowerOutputMeasurement; }
			set
			{
				if (m_activePowerOutputMeasurement == value) return;
				m_activePowerOutputMeasurement = value;
				OnPropertyChanged(nameof(ActivePowerOutputMeasurement));
			}
		}

		public Measurement ReactivePowerOutputMeasurement
		{
			get { return m_reactivePowerOutputMeasurement; }
			set
			{
				if (m_reactivePowerOutputMeasurement == value) return;
				m_reactivePowerOutputMeasurement = value;
				OnPropertyChanged(nameof(ReactivePowerOutputMeasurement));
			}
		}

		public Measurement ApparentPowerOutputMeasurement
		{
			get { return m_apparentPowerOutputMeasurement; }
			set
			{
				if (m_apparentPowerOutputMeasurement == value) return;
				m_apparentPowerOutputMeasurement = value;
				OnPropertyChanged(nameof(ApparentPowerOutputMeasurement));
			}
		}

		[Required(ErrorMessage = "Enabled is a required field. Please enter a value for the enabled flag.")]
		public bool Enabled
		{
			get { return m_enabled; }
			set
			{
				if (m_enabled == value) return;
				m_enabled = value;
				OnPropertyChanged(nameof(Enabled));
			}
		}

		[Required(ErrorMessage = "Node ID is a required field. Please enter a value for the Node ID.")]
		public Guid NodeID
		{
			get { return m_nodeID; }
			set
			{
				if (m_nodeID == value) return;
				m_nodeID = value;
				OnPropertyChanged(nameof(NodeID));
			}
		}

		public Phasor VoltagePhasor
		{
			get { return m_voltagePhasor; }
			set
			{
				if (m_voltagePhasor == value) return;
				m_voltagePhasor = value;
				OnPropertyChanged(nameof(VoltagePhasor));
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
				OnPropertyChanged(nameof(CurrentPhasor));
                UpdateCircuitDescription();
            }
        }

        private void UpdateCircuitDescription()
        {
            if ((object)m_voltagePhasor != null && (object)m_currentPhasor != null)
                CircuitDescription = $"{LookupDeviceName(m_voltagePhasor.DeviceID)}-{(m_currentPhasor.Label ?? "").ToUpperInvariant().RemoveWhiteSpace()}";
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
				var queryFormat = "SELECT ID FROM PowerCalculation WHERE NodeID = {0}";
				var calculationTable = database.RetrieveData(queryFormat, database.CurrentNodeID());

				foreach (DataRow row in calculationTable.Rows)
				{
					calculationList.Add(row.ConvertField<int>("ID"));
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
					var query = string.Format("select pc.ID, pc.CircuitDescription, pc.VoltageAngleSignalID, pc.VoltageMagSignalID, pc.CurrentAngleSignalID, " +
		 									  "       pc.CurrentMagSignalID, pc.ActivePowerOutputSignalID, pc.ReactivePowerOutputSignalID, pc.ApparentPowerOutputSignalID, pc.Enabled, " +
											  "	   pc.nodeid, vp.id voltagePhasorID, cp.id as currentPhasorID " +
											  "from powercalculation pc " +
											  "     left join measurement vm " +
											  "	 on pc.voltagemagsignalid=vm.signalid " +
											  "	 left join measurement cm " +
											  "	 on pc.currentmagsignalid=cm.signalid " +
											  "	 left join phasor vp " +
											  "	 on vm.deviceid=vp.deviceid and vm.phasorsourceindex=vp.sourceindex " +
											  "	 left join phasor cp " +
											  " on cm.deviceid=cp.deviceid and cm.phasorsourceindex=cp.sourceindex WHERE pc.ID IN ({0})", commaSeparatedKeys);
					var calculationTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout);
					calculationList = new PowerCalculation[calculationTable.Rows.Count];

					foreach (DataRow row in calculationTable.Rows)
					{
						var id = row.ConvertField<int>("ID");

						var voltageAngle = database.Guid(row, "VoltageAngleSignalID");
						var voltageMagnitude = database.Guid(row, "VoltageMagSignalID");
						var currentAngle = database.Guid(row, "CurrentAngleSignalID");
						var currentMagnitude = database.Guid(row, "CurrentMagSignalID");

						Guid? activePower = null;
						if (!row.IsNull("ActivePowerOutputSignalID"))
							activePower = database.Guid(row, "ActivePowerOutputSignalID");

						Guid? reactivePower = null;
						if (!row.IsNull("ReactivePowerOutputSignalID"))
							reactivePower = database.Guid(row, "ReactivePowerOutputSignalID");

						Guid? apparentPower = null;
						if (!row.IsNull("ApparentPowerOutputSignalID"))
							apparentPower = database.Guid(row, "ApparentPowerOutputSignalID");

						var currentPhasorID = row.ConvertField<int>("currentPhasorID");
						var voltagePhasorID = row.ConvertField<int>("voltagePhasorID");
						var phasors = Phasor.Load(database, new List<int> {currentPhasorID, voltagePhasorID});

						calculationList[keys.IndexOf(id)] = new PowerCalculation
						{
							ID = row.ConvertField<int>("ID"),
							CircuitDescription = row.Field<string>("CircuitDescription"),
							Enabled = row.ConvertField<bool>("Enabled"),
							NodeID = database.Guid(row, "NodeID")
						};

						foreach (var phasor in phasors)
						{
							if (phasor.ID == currentPhasorID)
							{
								calculationList[keys.IndexOf(id)].CurrentPhasor = phasor;
							}
							else if (phasor.ID == voltagePhasorID)
							{
								calculationList[keys.IndexOf(id)].VoltagePhasor = phasor;
							}
						}

						var mkeys = new List<Guid> { voltageAngle, voltageMagnitude, currentAngle, currentMagnitude };
						if (activePower != null) { mkeys.Add(activePower.Value); }
						if (reactivePower != null) { mkeys.Add(reactivePower.Value); }
						if (apparentPower != null) { mkeys.Add(apparentPower.Value); }
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
							else if (measurement.SignalID == activePower)
							{
								calculationList[keys.IndexOf(id)].ActivePowerOutputMeasurement = measurement;
							}
							else if (measurement.SignalID == reactivePower)
							{
								calculationList[keys.IndexOf(id)].ReactivePowerOutputMeasurement = measurement;
							}
							else if (measurement.SignalID == apparentPower)
							{
								calculationList[keys.IndexOf(id)].ApparentPowerOutputMeasurement = measurement;
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
		/// <param name="id">ID of the record to be deleted.</param>
		/// <returns>String, for display use, indicating success.</returns>
		public static string Delete(AdoDataConnection database, int id)
		{
			bool createdConnection = false;

			try
			{
				createdConnection = CreateConnection(ref database);

				// Setup current user context for any delete triggers
				CommonFunctions.SetCurrentUserContext(database);

				database.Connection.ExecuteNonQuery(database.ParameterizedQueryString("DELETE FROM PowerCalculation WHERE ID = {0}", "companyID"), DefaultTimeout, id);

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

				if (powerCalculation.ID == 0)
				{
					query = database.ParameterizedQueryString("INSERT INTO PowerCalculation (CircuitDescription, VoltageAngleSignalID, VoltageMagSignalID, CurrentAngleSignalID, CurrentMagSignalID, ActivePowerOutputSignalID, ReactivePowerOutputSignalID, ApparentPowerOutputSignalID, Enabled, NodeID) " +
						"VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9})", "circuitDescription", "voltageAngle", "voltageMag", "currentAngle", "currentMag", "activePowerOutput", "reactivePowerOutput", "apparentPowerOutput", "enabled", "nodeID");

					database.Connection.ExecuteNonQuery(query, DefaultTimeout, new[] { powerCalculation.CircuitDescription, database.Guid(powerCalculation.VoltagePhasor.AngleMeasurement.SignalID),
						database.Guid(powerCalculation.VoltagePhasor.MagnitudeMeasurement.SignalID), database.Guid(powerCalculation.CurrentPhasor.AngleMeasurement.SignalID), database.Guid(powerCalculation.CurrentPhasor.MagnitudeMeasurement.SignalID),
						powerCalculation.ActivePowerOutputMeasurement == null ? DBNull.Value : database.Guid(powerCalculation.ActivePowerOutputMeasurement.SignalID), powerCalculation.ReactivePowerOutputMeasurement == null ? DBNull.Value : database.Guid(powerCalculation.ReactivePowerOutputMeasurement.SignalID), powerCalculation.ApparentPowerOutputMeasurement == null ? DBNull.Value : database.Guid(powerCalculation.ApparentPowerOutputMeasurement.SignalID),
						powerCalculation.Enabled, database.CurrentNodeID() });
				}
				else
				{
					query = database.ParameterizedQueryString("UPDATE PowerCalculation SET CircuitDescription = {0}, VoltageAngleSignalID = {1}, VoltageMagSignalID = {2}, CurrentAngleSignalID = {3}, CurrentMagSignalID = {4}, " +
						"activePowerOutputSignalID = {5}, ReactivePowerOutputSignalID = {6}, ApparentPowerOutputSignalID = {7}, Enabled = {8} WHERE ID = {9}", "circuitDescription", "voltageAngle", "voltageMag", "currentAngle", "currentMag", "activePowerOutput", "reactivePowerOutput", "apparentPowerOutput", "enabled", "id");

					database.Connection.ExecuteNonQuery(query, DefaultTimeout, new[] { powerCalculation.CircuitDescription,
						database.Guid(powerCalculation.VoltagePhasor.AngleMeasurement.SignalID), database.Guid(powerCalculation.VoltagePhasor.MagnitudeMeasurement.SignalID), database.Guid(powerCalculation.CurrentPhasor.AngleMeasurement.SignalID), database.Guid(powerCalculation.CurrentPhasor.MagnitudeMeasurement.SignalID),
						powerCalculation.ActivePowerOutputMeasurement == null ? DBNull.Value : database.Guid(powerCalculation.ActivePowerOutputMeasurement.SignalID), powerCalculation.ReactivePowerOutputMeasurement == null ? DBNull.Value : database.Guid(powerCalculation.ReactivePowerOutputMeasurement.SignalID), powerCalculation.ApparentPowerOutputMeasurement == null ? DBNull.Value : database.Guid(powerCalculation.ApparentPowerOutputMeasurement.SignalID), powerCalculation.Enabled, powerCalculation.ID });
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
