//******************************************************************************************************
//  GraphLines.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/29/2012 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using UnityEngine;
using Vectrosity;
using System;
using System.Data;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Text;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport;

public class GraphLines : MonoBehaviour
{
    #region [ Members ]

    // Nested Types

	// Creates a dynamically scaled 3D line using Vectrosity asset to draw line for data
	private class DataLine
	{
		public Guid ID;
		
		private GraphLines m_parent;
		private VectorLine m_vector;
		private float[] m_unscaledData;
		private Vector3[] m_linePoints;  
		private float m_min = float.NaN;
		private float m_max = float.NaN;
		
		public DataLine(GraphLines parent, Guid id, int index)
		{			
			m_parent = parent;
			ID = id;

			m_unscaledData = new float[m_parent.m_pointsInLine];
			m_linePoints = new Vector3[m_parent.m_pointsInLine];
			
			m_vector = new VectorLine("DataLine" + index, m_linePoints, m_parent.m_lineMaterial, m_parent.m_lineWidth, LineType.Continuous);
			m_vector.SetColor(m_parent.m_lineColors[index % m_parent.m_lineColors.Length]);
			m_vector.Draw3DAuto(m_parent.m_target);
			
			for (int i = 0; i < m_linePoints.Length; i++)
			{
				m_unscaledData[i] = float.NaN;
				m_linePoints[i] = new Vector3(Mathf.Lerp(-5.0F, 5.0F, i / (float)m_linePoints.Length), -((index + 1) * m_parent.m_lineDepthOffset + 0.05F), 0.0F);
			}
		}
		
		public Color VectorColor
		{
			get
			{
				return m_vector.color;
			}
			set
			{
				m_vector.SetColor(value);
			}
		}
		
		public void Stop()
		{
			if ((object)m_vector != null)
				m_vector.StopDrawing3DAuto();
		}
		
		public void UpdateValue(float newValue)
		{
			bool scaleUpdated = false;
			int i;
			
			if (newValue < m_min || float.IsNaN(m_min))
			{
				m_min = newValue;
				scaleUpdated = true;
			}
			
			if (newValue > m_max || float.IsNaN(m_max))
			{
				m_max = newValue;
				scaleUpdated = true;
			}
			
			// Update line points if scale was updated
			if (scaleUpdated)
			{
				float unscaledValue;
				
				for (i = 0; i < m_linePoints.Length; i++)
				{
					unscaledValue = m_unscaledData[i];
					
					if (float.IsNaN(unscaledValue))
						unscaledValue = MidPoint;
						
					m_linePoints[i].z = ScaleValue(unscaledValue);
				}
			}
			
			// Move y position of all points to the left by one
			for (i = 0; i < m_linePoints.Length - 1; i++)
			{
				m_unscaledData[i] = m_unscaledData[i + 1];
				m_linePoints[i].z = m_linePoints[i + 1].z;
			}
			
			m_unscaledData[i] = newValue;
			m_linePoints[i].z = ScaleValue(newValue);
		}
		
		private float ScaleValue(float newValue)
		{
			return (newValue - m_min) * (m_parent.m_graphScale * 2.0F) / Range - m_parent.m_graphScale;
		}
					
		private float Range
		{
			get
			{
				return m_max - m_min;
			}
		}
		
		private float MidPoint
		{
			get
			{
				return m_min + Range / 2.0F;
			}
		}
	}

	// Creates a fixed 3D line using Vectrosity asset to draw line for legend
	private class LegendLine
	{
		private VectorLine m_vector;
		
		public LegendLine(GraphLines parent, Guid id, int index, Color color)
		{			
			Vector3[] linePoints = new Vector3[2];
			Transform transform = parent.m_legendMesh.transform;
			Vector3 position = transform.position;
			
			m_vector = new VectorLine("LegendLine" + index, linePoints, parent.m_lineMaterial, parent.m_lineWidth, LineType.Discrete);
			m_vector.SetColor(color);
			m_vector.Draw3DAuto(transform);
			
			float spacing = parent.m_legendMesh.characterSize * 1.5F;
			
			// Position legend line relative to text descriptions
			Vector3 point1 = new Vector3(-2.0F, -(spacing / 2.0F + index * spacing), -position.z);
			Vector3 point2 = new Vector3(-0.5F, point1.y, point1.z);
			
			linePoints[0] = point1;
			linePoints[1] = point2;
		}
		
		public void Stop()
		{
			if ((object)m_vector != null)
				m_vector.StopDrawing3DAuto();
		}
	}
	
	// Exposes DataRow field value in a string.Format expression
	private class RowFormatProvider : IFormattable
	{
		private DataRow m_row;
		
		public RowFormatProvider(DataRow row)
		{
			m_row = row;
		}
		
        public string ToString(string format, IFormatProvider provider)
        {
			if (m_row.Table.Columns.Contains(format))
				return m_row[format].ToString();
			
			return "<" + format + ">";
        }
	}

    // Constants
	private const string IniFileName = "GraphLines.ini";

    // Fields	
	private ConcurrentDictionary<Guid, DataLine> m_dataLines;
	private ConcurrentQueue<IMeasurement> m_dataQueue;
	private DataSubscriber m_subscriber;
	private DataTable m_measurementMetadata;
	private List<LegendLine> m_legendLines;
	private WaitHandle m_linesInitializedWaitHandle;
	private bool m_subscribed;
	
	// Public fields are exposed to Unity UI interface
	public string m_title = "GPA Grid Solutions Framework Subscription Demo";
	public string m_connectionString = "server=localhost:6165";
	public string m_filterExpression = "FILTER ActiveMeasurements WHERE SignalType='FREQ' OR SignalType LIKE 'VPH*'";
	public Material m_lineMaterial;
	public int m_lineWidth = 4;
	public float m_lineDepthOffset = 0.75F;
	public int m_pointsInLine = 50;
	public Transform m_target;
	public float m_graphScale = 5.0F;
	public Color[] m_lineColors = new Color[] { Color.blue, Color.yellow, Color.red, Color.white, Color.cyan, Color.magenta, Color.black, Color.gray };
	public TextMesh m_legendMesh;
	public string m_legendFormat = "{0:SignalAcronym}: {0:Description} [{0:PointTag}]";

    #endregion

    #region [ Methods ]
	
	#region [ Unity Event Handlers ]
	
	protected void Start()
	{
		string iniFilePath = Application.dataPath + "/" + IniFileName;
		
		if (Application.platform == RuntimePlatform.Android)
			iniFilePath = Application.persistentDataPath + "/" + IniFileName;
		
		IniFile iniFile = new IniFile(iniFilePath);
		
		m_title = iniFile["Settings", "Title", m_title];
		m_connectionString = iniFile["Settings", "ConnectionString", m_connectionString];
		m_filterExpression = iniFile["Settings", "FilterExpression", m_filterExpression];			
		m_lineWidth = int.Parse(iniFile["Settings", "LineWidth", m_lineWidth.ToString()]);
		m_lineDepthOffset = float.Parse(iniFile["Settings", "LineDepthOffset", m_lineDepthOffset.ToString()]);
		m_pointsInLine = int.Parse(iniFile["Settings", "PointsInLine", m_pointsInLine.ToString()]);
		m_legendFormat = iniFile["Settings", "LegendFormat", m_legendFormat];

		// Attempt to save new INI file if one doesn't exist
		if (!File.Exists(iniFilePath))
		{
			try
			{
				iniFile.Save();
			}
			catch (Exception ex)
			{
				Debug.Log("ERROR: " + ex.Message);
			}
		}
		
		// Attempt to update title
		GameObject titleObject = GameObject.Find("Title");
		
		if ((object)titleObject != null)
		{
			TextMesh titleMesh = titleObject.GetComponent(typeof(TextMesh)) as TextMesh;
			
			if ((object)titleMesh != null)
				titleMesh.text = m_title;
		}
		
		// Create line dictionary and data queue
		m_dataLines = new ConcurrentDictionary<Guid, DataLine>();
		m_dataQueue = new ConcurrentQueue<IMeasurement>();		
		m_legendLines = new List<LegendLine>();
		
		// If 3D text legend mesh property was not defined, attempt to look it up by name
		if ((object)m_legendMesh == null)
		{
			GameObject legendObject = GameObject.Find("Legend");
			
			if ((object)legendObject != null)
				m_legendMesh = legendObject.GetComponent(typeof(TextMesh)) as TextMesh;
		}
		
		// Create a new data subscriber
		m_subscriber = new DataSubscriber();
		
		// Attach to subscriber events
		m_subscriber.MetaDataReceived += subscriber_MetaDataReceived;
        m_subscriber.StatusMessage += subscriber_StatusMessage;
        m_subscriber.ProcessException += subscriber_ProcessException;
        m_subscriber.ConnectionEstablished += subscriber_ConnectionEstablished;
        m_subscriber.ConnectionTerminated += subscriber_ConnectionTerminated;
        m_subscriber.NewMeasurements += subscriber_NewMeasurements;

        // Initialize subscriber
        m_subscriber.ConnectionString = m_connectionString;
        m_subscriber.OperationalModes |= OperationalModes.UseCommonSerializationFormat | OperationalModes.CompressSignalIndexCache | OperationalModes.CompressMetadata;
        m_subscriber.Initialize();

        // Start subscriber connection cycle
        m_subscriber.Start();
	}
	
	protected void OnDestroy()
	{
		// Stop the subscription if connected
		m_subscriber.Stop();
		
		// Dispose of the subscription
		m_subscriber.Dispose();
	}

	protected void Update()
	{
//		// Check for screen resize
//		if (m_lastScreenHeight != Screen.height)
//			OnScreenResize();
		
//		if (m_controlWindowLocation.Contains(Input.mousePosition) && Input.GetMouseButtonUp(0))
//			m_controlWindowVisible = !m_controlWindowVisible;
		
		// Nothing to update if we haven't subscribed yet
		if (!m_subscribed)
			return;
		
		// Make sure lines are initialized before trying to draw them
		if ((object)m_linesInitializedWaitHandle != null)
		{
			// Only wait one millisecond then try again at next update
			if (m_linesInitializedWaitHandle.WaitOne(1))
				m_linesInitializedWaitHandle = null;
			else
				return;
		}
		
		IMeasurement measurement;
		
		// Dequeue all new measurements and apply values to lines
		while (m_dataQueue.TryDequeue(out measurement))
		{
			DataLine line = m_dataLines[measurement.ID];
			line.UpdateValue((float)measurement.Value);
		}		
	}
	
//	private int m_lastScreenHeight = -1;
//	private Rect m_controlWindowLocation;
//	private Rect m_controlWindowContentVisible;
//	private Rect m_controlWindowContentHidden;
//	private bool m_controlWindowVisible = true;
//	
//	private void OnGUI()
//	{
//		Rect controlWindowLocation = m_controlWindowVisible ? m_controlWindowContentVisible : m_controlWindowContentHidden;
//		GUILayout.Window(0, controlWindowLocation, DrawControlsWindow, "Subscription Controls");
//		
//		m_controlWindowLocation = new Rect(0, m_controlWindowVisible ? 50 : 20, Screen.width, 50);
//	}
//	
//	private void OnScreenResize()
//	{
//		m_lastScreenHeight = Screen.height;
//		m_controlWindowContentVisible = new Rect(0, Screen.height - 50, Screen.width, 50);
//		m_controlWindowContentHidden = new Rect(0, Screen.height - 20, Screen.width, 50);
//		
//		Debug.Log(m_controlWindowContentVisible.ToString());
//	}
//	
//	private void DrawControlsWindow(int windowID)
//	{
//		GUILayout.BeginHorizontal();
//		GUILayout.Label("Filter Expression:");
//		GUILayout.TextField(m_filterExpression);
//		
//		if (GUILayout.Button("Hello World", GUILayout.Width(100)))
//			print("Got a click");
//		GUILayout.EndHorizontal();		
//	}
	
    #endregion
	
	#region [ Subscription Event Handlers ]
	
	private void subscriber_ConnectionEstablished(object sender, EventArgs e)
	{
		// Request metadata once connected
		m_subscriber.RefreshMetadata();
	}

	private void subscriber_MetaDataReceived(object sender, EventArgs<DataSet> e)
	{
		m_subscribed = false;
		m_measurementMetadata = null;
		
		if ((object)e != null)
		{
			DataSet metadata = e.Argument;
			
			if ((object)metadata != null)
			{
				// Cache metadata related to measurements
				if (metadata.Tables.Contains("MeasurementDetail"))
				{
					m_measurementMetadata = metadata.Tables["MeasurementDetail"];
					Debug.Log("Received " + metadata.Tables.Count + " metadata tables, "+ m_measurementMetadata.Rows.Count + " measurement records.");
				}				
				else
				{
					Debug.Log("Received metadata does not have a MeasurementDetail table.");
				}
			}
		}
		
		if ((object)m_measurementMetadata == null)
			Debug.Log("No metadata received.");

		// Once metadata has been received, subscribe to desired data
		UnsynchronizedSubscriptionInfo subscriptionInfo;

		subscriptionInfo = new UnsynchronizedSubscriptionInfo(false);
		subscriptionInfo.FilterExpression = m_filterExpression;		
		m_subscriber.UnsynchronizedSubscribe(subscriptionInfo);
	}
	
	// Since new measurements will continue to arrive and be queued even when screen is not visible, it
	// is important that unity application be set to "run in background" to avoid running out of memory
	private void subscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
	{
		// At the moment we first receive data we know that we've sucessfully subscribed,
		// so we go ahead an cache list of measurement signal IDs (we may not know what
		// these are in advance if we used a FILTER expression to subscribe to points)
		if (!m_subscribed)
		{			
			Guid[] subscribedMeasurementIDs = m_subscriber.GetAuthorizedSignalIDs();
			
			// Create a new line for each subscribed measurement, this should be done in
			// advance of updating the legend so the line colors will already be defined
			m_linesInitializedWaitHandle = UIThread.Invoke(CreateDataLines, subscribedMeasurementIDs);
			
			// Flag should be set after wait handle is created for proper synchronization
			m_subscribed = true;
		}
		
		foreach(IMeasurement measurement in e.Argument)
		{
			m_dataQueue.Enqueue(measurement);
		}
	}
		
	private void subscriber_ConnectionTerminated(object sender, EventArgs e)
	{
		if (m_subscribed)
		{
			m_subscribed = false;
			m_linesInitializedWaitHandle = null;
			
			foreach (DataLine dataLine in m_dataLines.Values)
			{
				UIThread.Invoke(StopDataLine, dataLine);
			}
			
			m_dataLines.Clear();
			
			foreach (LegendLine legendLine in m_legendLines)
			{
				UIThread.Invoke(StopLegendLine, legendLine);
			}
			
			m_legendLines.Clear();			
			m_legendMesh.UpdateText("");
		}
		
		// Restart connection cycle when connection is terminated - could be that PDC is being restarted
		m_subscriber.Start();
	}
	
	private void subscriber_StatusMessage(object sender, GSF.EventArgs<string> e)
	{
		Debug.Log(e.Argument);
	}
	
	private void subscriber_ProcessException(object sender, GSF.EventArgs<Exception> e)
	{
		Debug.Log("ERROR: " + e.Argument.Message);
	}

    #endregion
	
	// Creates a new data line for each subscribed measurement
	private void CreateDataLines(object[] args)
	{
		if ((object)args == null || args.Length < 1)
			return;
		
		Guid[] subscribedMeasurementIDs = args[0] as Guid[];
		
		if ((object)subscribedMeasurementIDs == null || (object)m_dataLines == null)
			return;
		
		m_dataLines.Clear();
		
		foreach (Guid measurementID in subscribedMeasurementIDs)
		{
			m_dataLines.TryAdd(measurementID, new DataLine(this, measurementID, m_dataLines.Count));
		}
			
		// Update legend - we do this on a different thread since we've already
		// waited around for initial set of lines to be created on a UI thread,
		// no need to keep UI thread operations pending
		ThreadPool.QueueUserWorkItem(UpdateLegend, subscribedMeasurementIDs);
	}
	
	private void UpdateLegend(object state)
	{
		Guid[] subscribedMeasurementIDs = state as Guid[];
		
		if ((object)subscribedMeasurementIDs == null || (object)m_measurementMetadata == null)
			return;
				
		StringBuilder legendText = new StringBuilder();
		DataRow[] rows;
					
		// Go through each subscribed measurement ID and look up its associated metadata
		foreach (Guid measurementID in subscribedMeasurementIDs)
		{
			// Lookup metadata record where SignalID column is our measurement ID
			rows = m_measurementMetadata.Select(string.Format("SignalID = '{0}'", measurementID));
			
			if (rows.Length > 0)
			{
				// Add formatted metadata label expression to legend text
				legendText.AppendFormat(m_legendFormat, new RowFormatProvider(rows[0]));
				legendText.AppendLine();
			}
		}
		
		// Update text for 3D text labels object with subscribed point tag names
		m_legendMesh.UpdateText(legendText.ToString());
		
		// Create a legend line for subscribed point
		m_legendLines.Clear();
		
		foreach (Guid measurementID in subscribedMeasurementIDs)
		{
			// Lines must be create on the UI thread
			UIThread.Invoke(CreateLegendLine, measurementID);
		}		
	}
	
	// Create a new legend line
	private void CreateLegendLine(object[] args)
	{
		if ((object)args == null || args.Length < 1)
			return;
		
		DataLine dataLine;
		Guid id = (Guid)args[0];
		
		// Attempt to look up associated data line (for line color)
		if (m_dataLines.TryGetValue(id, out dataLine))
			m_legendLines.Add(new LegendLine(this, id, m_legendLines.Count, dataLine.VectorColor));
	}
	
	private void StopDataLine(object[] args)
	{
		if ((object)args == null || args.Length < 1)
			return;
		
		DataLine dataLine = args[0] as DataLine;
		
		if ((object)dataLine != null)
			dataLine.Stop();
	}

	private void StopLegendLine(object[] args)
	{
		if ((object)args == null || args.Length < 1)
			return;
		
		LegendLine legendLine = args[0] as LegendLine;
		
		if ((object)legendLine != null)
			legendLine.Stop();
	}

    #endregion
}