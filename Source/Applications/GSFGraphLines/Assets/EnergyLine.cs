//******************************************************************************************************
//  EnergyLine.cs - Gbtc
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
using System.Collections.Generic;
using System.Collections.Concurrent;
using GSF;
using GSF.TimeSeries;
using GSF.TimeSeries.Transport;

public class EnergyLine : MonoBehaviour
{
	// Creates a dynamically scaled 3D line using Vectrosity asset to draw line
	private class Line
	{
		public Guid ID;
		
		private EnergyLine parent;
		private VectorLine vector;
		private float[] unscaledData;
		private Vector3[] linePoints;  
		private float min = float.NaN;
		private float max = float.NaN;
		
		public Line(EnergyLine source, Guid id, int index)
		{			
			parent = source;
			ID = id;

			unscaledData = new float[parent.pointsInLine];
			linePoints = new Vector3[parent.pointsInLine];
			
			vector = new VectorLine("Line" + index, linePoints, parent.lineMaterial, parent.lineWidth, LineType.Continuous);
			vector.SetColor(parent.lineColors[index % parent.lineColors.Length]);
			vector.Draw3DAuto(parent.target);
			
			for (int i = 0; i < linePoints.Length; i++)
			{
				unscaledData[i] = float.NaN;
				linePoints[i] = new Vector3(Mathf.Lerp(-5.0F, 5.0F, i / (float)linePoints.Length), -((index + 1) * parent.lineDepthOffset + 0.05F), 0.0F);
			}
		}
		
		public void UpdateValue(float newValue)
		{
			bool scaleUpdated = false;
			int i;
			
			if (newValue < min || float.IsNaN(min))
			{
				min = newValue;
				scaleUpdated = true;
			}
			
			if (newValue > max || float.IsNaN(max))
			{
				max = newValue;
				scaleUpdated = true;
			}
			
			// Update line points if scale was updated
			if (scaleUpdated)
			{
				float unscaledValue;
				
				for (i = 0; i < linePoints.Length; i++)
				{
					unscaledValue = unscaledData[i];
					
					if (float.IsNaN(unscaledValue))
						unscaledValue = MidPoint;
						
					linePoints[i].z = ScaleValue(unscaledValue);
				}
			}
			
			// Move y position of all points to the left by one
			for (i = 0; i < linePoints.Length - 1; i++)
			{
				unscaledData[i] = unscaledData[i + 1];
				linePoints[i].z = linePoints[i + 1].z;
			}
			
			unscaledData[i] = newValue;
			linePoints[i].z = ScaleValue(newValue);
		}
		
		private float ScaleValue(float newValue)
		{
			return (newValue - min) * (parent.graphScale * 2.0F) / Range - parent.graphScale;
		}
					
		private float Range
		{
			get
			{
				return max - min;
			}
		}
		
		private float MidPoint
		{
			get
			{
				return min + Range / 2.0F;
			}
		}
	}
	
	public string connectionString = "server=localhost:6165";
	public string filterExpression = "PPA:2;PPA:5;PPA:6;PPA:9;PPA:10";
	public Material lineMaterial;
	public int lineWidth = 4;
	public float lineDepthOffset = 0.25F;
	public int pointsInLine = 100;
	public Transform target;
	public float graphScale = 5.0F;
	public Color[] lineColors = new Color[] { Color.blue, Color.yellow, Color.red, Color.white, Color.cyan, Color.magenta, Color.black, Color.gray };
	
	private ConcurrentDictionary<Guid, Line> lines;
	private ConcurrentQueue<IMeasurement> dataQueue;
	private DataSubscriber subscriber;	

	protected void Start()
	{
		// Create line dictionary and data queue
		lines = new ConcurrentDictionary<Guid, Line>();
		dataQueue = new ConcurrentQueue<IMeasurement>();		
		
		// Create a new data subscriber
		subscriber = new DataSubscriber();
		
		// Attach to subscriber events
        subscriber.StatusMessage += subscriber_StatusMessage;
        subscriber.ProcessException += subscriber_ProcessException;
        subscriber.ConnectionEstablished += subscriber_ConnectionEstablished;
        subscriber.ConnectionTerminated += subscriber_ConnectionTerminated;
        subscriber.NewMeasurements += subscriber_NewMeasurements;

        // Initialize subscriber
        subscriber.ConnectionString = connectionString;
        subscriber.OperationalModes |= OperationalModes.UseCommonSerializationFormat | OperationalModes.CompressSignalIndexCache;
        subscriber.Initialize();

        // Start subscriber connection cycle
        subscriber.Start();
	}
	
	protected void OnDestroy()
	{
		// Stop the subscription if connected
		subscriber.Stop();
		
		// Dispose of the subscription
		subscriber.Dispose();
	}

	protected void Update()
	{
		IMeasurement measurement;
		
		while (dataQueue.TryDequeue(out measurement))
		{
			Line line = lines.GetOrAdd(measurement.ID, CreateNewLine);
			line.UpdateValue((float)measurement.AdjustedValue);
		}
	}
	
	private Line CreateNewLine(Guid id)
	{
		return new Line(this, id, lines.Count);
	}
	
	// Since new measurements will continue to arrive and be queued even when screen is not visible, it
	// is important that unity application be set to "run in background" to avoid running out of memory
	private void subscriber_NewMeasurements(object sender, EventArgs<ICollection<IMeasurement>> e)
	{
		foreach(IMeasurement measurement in e.Argument)
		{
			dataQueue.Enqueue(measurement);
		}
	}
	
	private void subscriber_ConnectionEstablished(object sender, EventArgs e)
	{
		UnsynchronizedSubscriptionInfo subscriptionInfo;

		// Once connected, subscribe to desired data
		subscriptionInfo = new UnsynchronizedSubscriptionInfo(false);
		subscriptionInfo.FilterExpression = filterExpression;
		
		subscriber.UnsynchronizedSubscribe(subscriptionInfo);
	}
	
	private void subscriber_ConnectionTerminated(object sender, EventArgs e)
	{
		// Restart connection cycle when connection is terminated - could be that PDC is being restarted
		subscriber.Start();
	}
	
	private void subscriber_StatusMessage(object sender, GSF.EventArgs<string> e)
	{
		Debug.Log(e.Argument);
	}
	
	private void subscriber_ProcessException(object sender, GSF.EventArgs<Exception> e)
	{
		Debug.LogException(e.Argument);
	}
}
