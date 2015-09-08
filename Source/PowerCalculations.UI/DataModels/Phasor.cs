using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using GSF.Data;
using GSF.TimeSeries.UI;

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
		private int m_destinationPhasorId;
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

		public int DestinationPhasorId
		{
			get { return m_destinationPhasorId; }
			set
			{
				if (m_destinationPhasorId == value) return;
				m_destinationPhasorId = value;
				OnPropertyChanged("DestinationPhasorId");
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

		#endregion


		#region [ Statics ]

		/// <summary>
		/// LoadKeys <see cref="Phasor"/> information as an <see cref="ObservableCollection{T}"/> style list.
		/// </summary>
		/// <param name="database"><see cref="AdoDataConnection"/> to connection to database.</param>
		/// <returns>Collection of <see cref="int"/>.</returns>
		public static IList<int> LoadKeys(AdoDataConnection database)
		{
			var createdConnection = false;

			try
			{
				createdConnection = CreateConnection(ref database);

				var phasorList = new List<int>();
				const string query = "SELECT Id FROM Phasor";
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
					var query = string.Format("SELECT * FROM Phasor WHERE Id IN ({0})", commaSeparatedKeys);
					var phasorTable = database.Connection.RetrieveData(database.AdapterType, query, DefaultTimeout);
					phasorList = new Phasor[phasorTable.Rows.Count];

					foreach (DataRow row in phasorTable.Rows)
					{
						var id = row.ConvertField<int>("Id");
						var deviceId = row.ConvertField<int>("DeviceId");
						var label = row.ConvertField<string>("Label");
						var type = row.ConvertField<string>("Type");
						var phase = row.ConvertField<string>("Phase");
						var destinationPhasorId = row.ConvertField<int>("DestinationPhasorId");
						var sourceIndex = row.ConvertField<int>("SourceIndex");

						phasorList[keys.IndexOf(id)] = new Phasor
						{
							Id = id,
							DeviceId = deviceId,
							Label = label,
							Type = type,
							Phase = phase,
							DestinationPhasorId = destinationPhasorId,
							SourceIndex = sourceIndex
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
