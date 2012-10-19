//******************************************************************************************************
//  SimpleSubscribe.java - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
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
//  04/19/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

package org.gpa.gsf.timeseries.sample;

import java.io.IOException;
import java.util.Scanner;

import org.gpa.gsf.timeseries.transport.DataSubscriber;
import org.gpa.gsf.timeseries.transport.SubscriptionInfo;

/**
 * Provides an example of how to use the Subscriber API.
 * 
 * @see DataSubscriber
 */
public class SimpleSubscribe
{
	private static DataSubscriber s_subscriber;
	
	/**
	 * Main method. Entry point into the program.
	 * 
	 * @param args command line arguments
	 */
	public static void main(String[] args)
	{
		String hostname;
		int port;
		
		// Ensure that the necessary
		// command line arguments are given
		if (args.length < 2)
		{
			System.err.println("Error: requires two command line arguments");
			System.err.println("       1. hostname of publisher");
			System.err.println("       2. port used to initiate connection");
			return;
		}
		
		// Get hostname and port
		hostname = args[0];
		port = Integer.parseInt(args[1]);
		
		// Run the subscriber
		runSubscriber(hostname, port);
		
		// Wait until user presses enter before quitting
		waitForUserInput();
		
		// Disconnect the subscriber to stop background threads
		if (s_subscriber != null)
			s_subscriber.disconnect();
	}
	
	// The proper procedure when creating and running a subscriber is:
	//   - Create subscriber
	//   - Add listeners
	//   - Connect to publisher
	//   - Subscribe
	private static void runSubscriber(String hostname, int port)
	{
		try
		{
			s_subscriber = new DataSubscriber();
			s_subscriber.addSubscriberListener(new SampleSubscriberListener());
			s_subscriber.connect(hostname, port);
			s_subscriber.subscribe(createSubscriptionInfo());
		}
		catch (IOException ex)
		{
			ex.printStackTrace();
		}
	}

	// SubscriptionInfo is a helper object which allows the user
	// to set up their subscription and reuse subscription settings
	private static SubscriptionInfo createSubscriptionInfo()
	{
		SubscriptionInfo info = new SubscriptionInfo();
		info.setFilterExpression("PPA:1;PPA:2;PPA:3;PPA:4;PPA:5;PPA:6;PPA:7;PPA:8;PPA:9;PPA:10;PPA:11;PPA:12;PPA:13;PPA:14");
		return info;
	}
	
	// Waits for the user to press Enter before proceeding
	private static void waitForUserInput()
	{
		Scanner scan = new Scanner(System.in);
		scan.nextLine();
	}
}
