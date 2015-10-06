using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using GSF.Data;
using GSF.TimeSeries.UI;
using GSF.TimeSeries.UI.DataModels;

namespace PowerCalculations.UI.DataModels
{
	public class Phasor : DataModelBase
	{
		#region [ Members ]

		private int m_id;
		private int m_deviceId;
		private string m_label;
		private string m_type;
		private string m_phase;
		private int m_sourceIndex;

		#endregion

		#region [ Properties ]

		public int Id
		{
			get { return m_id; }
			set
			{
				if (m_id == value) return;
				m_id = value;
				OnPropertyChanged("Id");
			}
		}

		public int DeviceId
		{
			get { return m_deviceId; }
			set
			{
				if (m_deviceId == value) return;
				m_deviceId = value;
				OnPropertyChanged("DeviceId");
			}
		}

		public string Label
		{
			get { return m_label; }
			set
			{
				if (m_label == value) return;
				m_label = value;
				OnPropertyChanged("Label");
			}
		}

		public string Type
		{
			get { return m_type; }
			set
			{
				if (m_type == value) return;
				m_type = value;
				OnPropertyChanged("Type");
			}
		}

		public string Phase
		{
			get { return m_phase; }
			set
			{
				if (m_phase == value) return;
				m_phase = value;
				OnPropertyChanged("Phase");
			}
		}

		public int SourceIndex
		{
			get { return m_sourceIndex; }
			set
			{
				if (m_sourceIndex == value) return;
				m_sourceIndex = value;
				OnPropertyChanged("SourceIndex");
			}
		}

		private Measurement m_magnitudeMeasurement;
		public Measurement MagnitudeMeasurement
		{
			get { return m_magnitudeMeasurement; }
			set
			{
				if (m_magnitudeMeasurement == value) return;
				m_magnitudeMeasurement = value;
				OnPropertyChanged("MagnitudeMeasurement");
			}
		}

		private Measurement m_angleMeasurement;
		public Measurement AngleMeasurement
		{
			get { return m_angleMeasurement; }
			set
			{
				if (m_angleMeasurement == value) return;
				m_angleMeasurement = value;
				OnPropertyChanged("AngleMeasurement");
			}
		}

		#endregion


		#region [ Statics ]

		/// <summary>
		/// LoadKeys <see cref="Phasor"/> information as an <see cref="ObservableCollection{T}"/> style list.
		/// </summary>
		/// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
		/// <param name="filter">The type of phasors to load</param>
		/// <returns>Collection of <see cref="int"/>.</returns>
		public static IList<int> LoadKeys(AdoDataConnection database, PhasorType filter = PhasorType.Any)
		{
			var createdConnection = false;

			try
			{
				createdConnection = CreateConnection(ref database);

				var phasorList = new List<int>();
				var query = "SELECT Id FROM Phasor";
				if (filter != PhasorType.Any)
				{
					query += string.Format(" Where Type = '{0}'", filter == PhasorType.Voltage ? "V" : "I");
				}

				var calculationTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout);

				foreach (DataRow row in calculationTable.Rows)
				{
					phasorList.Add(row.ConvertField<int>("Id"));
				}

				return phasorList;
			}
			finally
			{
				if (createdConnection && database != null)
					database.Dispose();
			}
		}

		/// <summary>
		/// Loads <see cref="PowerCalculation"/> information as an <see cref="ObservableCollection{T}"/> style list.
		/// </summary>
		/// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
		/// <param name="keys">Keys of the measurement to be loaded from the database</param>
		/// <returns>Collection of <see cref="PowerCalculation"/>.</returns>
		public static ObservableCollection<Phasor> Load(AdoDataConnection database, IList<int> keys)
		{
			var createdConnection = false;

			try
			{
				createdConnection = CreateConnection(ref database);

				Phasor[] phasorList = null;

				if (keys != null && keys.Count > 0)
				{
					var commaSeparatedKeys = keys.Select(key => key.ToString()).Aggregate((str1, str2) => str1 + "," + str2);
					var query = string.Format("select p.id, p.deviceid, p.label, p.type, p.phase, p.sourceindex, mags.signalid as mag_signalid, angles.signalid as angle_signalid " +
											  "from phasor p left join measurement mags " +
											  "on mags.deviceid = p.deviceid and mags.phasorsourceindex = p.sourceindex and mags.signaltypeid in (1,3) " +
											  "left join measurement angles " +
											  "on angles.deviceid = p.deviceid and angles.phasorsourceindex = p.sourceindex and angles.signaltypeid in (2,4) " +
											  "where p.id in ({0})", commaSeparatedKeys);
					var phasorTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout);
					phasorList = new Phasor[phasorTable.Rows.Count];

					foreach (DataRow row in phasorTable.Rows)
					{
						var id = row.ConvertField<int>("Id");
						var deviceId = row.ConvertField<int>("DeviceId");
						var label = row.ConvertField<string>("Label");
						var type = row.ConvertField<string>("Type");
						var phase = row.ConvertField<string>("Phase");
						var sourceIndex = row.ConvertField<int>("SourceIndex");
						var magnitudeSignalId = database.Guid(row, "mag_signalId");
						var angleSignalId = database.Guid(row, "angle_signalId");

						var measurements = Measurement.LoadFromKeys(database, (new[] {magnitudeSignalId, angleSignalId}).ToList());

						phasorList[keys.IndexOf(id)] = new Phasor
						{
							Id = id,
							DeviceId = deviceId,
							Label = label,
							Type = type,
							Phase = phase,
							SourceIndex = sourceIndex,
							MagnitudeMeasurement = measurements.FirstOrDefault(m => m.SignalID == magnitudeSignalId),
							AngleMeasurement = measurements.FirstOrDefault(m => m.SignalID == angleSignalId)
						};
					}
				}

				return new ObservableCollection<Phasor>(phasorList ?? new Phasor[0]);
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
