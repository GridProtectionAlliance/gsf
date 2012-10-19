//******************************************************************************************************
//  AdvancedSubscribe.java - Gbtc
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
//  04/23/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

package org.gpa.gsf.timeseries.sample;

import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.io.IOException;
import java.io.OutputStream;
import java.io.PrintStream;
import java.net.SocketException;
import java.nio.ByteBuffer;
import java.nio.charset.Charset;

import javax.swing.Box;
import javax.swing.BoxLayout;
import javax.swing.JApplet;
import javax.swing.JButton;
import javax.swing.JLabel;
import javax.swing.JScrollPane;
import javax.swing.JTextArea;
import javax.swing.JTextField;
import javax.swing.SwingUtilities;

import org.gpa.gsf.timeseries.transport.DataSubscriber;
import org.gpa.gsf.timeseries.transport.SubscriberConnector;
import org.gpa.gsf.timeseries.transport.SubscriptionInfo;
import org.gpa.gsf.timeseries.transport.event.SubscriberConnectorListener;

/**
 * Sample applet to demonstrate the more advanced use of the subscriber API.
 *
 * This application accepts the hostname and port of the publisher via text
 * fields on the applet, connects to the publisher, subscribes, and displays
 * information about the measurements it receives. It assumes that the
 * publisher is providing fourteen measurements (PPA:1 through PPA:14) and
 * will retry infinitely if it has any trouble connecting. It will also
 * automatically reconnect if the connection is terminated.
 *
 * Measurements are transmitted via a separate UDP data channel.
 */
public class AdvancedSubscribe extends JApplet
{
	private Object m_connectLock;
	private ActionListener m_connectListener;
	private ActionListener m_disconnectListener;
	
	private JScrollPane m_outputScrollPane;
	private JTextArea m_outputTextArea;
	private JLabel m_hostnameLabel;
	private JLabel m_portLabel;
	private JTextField m_hostnameTextField;
	private JTextField m_portTextField;
	private JButton m_connectButton;
	private JButton m_disconnectButton;
	private Box m_buttonBox;
	
	private PrintStream m_outputStream;
	private SubscriptionInfo m_subscriptionInfo;
	private SubscriberConnector m_connector;
	private DataSubscriber m_subscriber;
	
	@Override
	public void init()
	{
		GridBagConstraints constraints;
		
		super.init();
		
		// Initialize members
		m_connectLock = new Object();
		m_connectListener = createConnectListener();
		m_disconnectListener = createDisconnectListener();
		
		m_outputTextArea = createOutputTextArea();
		m_outputScrollPane = new JScrollPane(m_outputTextArea, JScrollPane.VERTICAL_SCROLLBAR_ALWAYS, JScrollPane.HORIZONTAL_SCROLLBAR_AS_NEEDED);
		m_hostnameLabel = new JLabel("Hostname");
		m_portLabel = new JLabel("Port");
		m_hostnameTextField = createHostnameTextField();
		m_portTextField = createPortTextField();
		m_connectButton = createConnectButton();
		m_disconnectButton = createDisconnectButton();
		m_buttonBox = createButtonBox();
		
		m_outputStream = createOutputStream();
		m_subscriptionInfo = createSubscriptionInfo();
		m_connector = createSubscriberConnector(m_subscriptionInfo, m_outputStream);
		m_subscriber = createSubscriber();
		
		// Component layout
		getContentPane().setLayout(new GridBagLayout());

		constraints = new GridBagConstraints();
		constraints.fill = GridBagConstraints.BOTH;
		constraints.gridwidth = 2;
		constraints.weighty = 1.0;
		getContentPane().add(m_outputScrollPane, constraints);

		constraints = new GridBagConstraints();
		constraints.gridy = 1;
		constraints.weightx = 0.5;
		getContentPane().add(m_hostnameLabel, constraints);
		
		constraints = new GridBagConstraints();
		constraints.gridx = 1;
		constraints.gridy = 1;
		constraints.weightx = 0.5;
		getContentPane().add(m_portLabel, constraints);

		constraints = new GridBagConstraints();
		constraints.fill = GridBagConstraints.HORIZONTAL;
		constraints.gridy = 2;
		constraints.weightx = 0.5;
		getContentPane().add(m_hostnameTextField, constraints);

		constraints = new GridBagConstraints();
		constraints.fill = GridBagConstraints.HORIZONTAL;
		constraints.gridx = 1;
		constraints.gridy = 2;
		constraints.weightx = 0.5;
		getContentPane().add(m_portTextField, constraints);

		constraints = new GridBagConstraints();
		constraints.anchor = GridBagConstraints.CENTER;
		constraints.gridy = 3;
		constraints.gridwidth = 2;
		getContentPane().add(m_buttonBox, constraints);
	}
	
	// Creates the action listener used to connect to the publisher.
	private ActionListener createConnectListener()
	{
		return new ActionListener()
		{
			@Override
			public void actionPerformed(ActionEvent arg0)
			{
				// Set the hostname and port before attempting to connect
				m_connector.setHostname(m_hostnameTextField.getText());
				m_connector.setPort(Integer.parseInt(m_portTextField.getText()));
				
				// Connect asynchronously so we don't hog the UI thread
				connectAsync();
			}
		};
	}
	
	// Connects and subscribes to the publisher asynchronously.
	private void connectAsync()
	{
		new Thread(new Runnable()
		{
			@Override
			public void run()
			{
				// Lock the connect lock so we don't encounter
				// synchronization issues between calls to connectAsync
				// and disconnectAsync
				synchronized (m_connectLock)
				{
					try
					{
						if (m_connector.connect(m_subscriber))
							m_subscriber.subscribe(m_subscriptionInfo);
					}
					catch (SocketException ex)
					{
						ex.printStackTrace(m_outputStream);
					}
				}
			}
		}).start();
	}
	
	// Creates the disconnect listener used to disconnect from the publisher.
	private ActionListener createDisconnectListener()
	{
		return new ActionListener()
		{
			@Override
			public void actionPerformed(ActionEvent arg0)
			{
				// Disconnect asynchronously so we don't hog the UI thread
				disconnectAsync();
			}
		};
	}
	
	// Disconnects from the publisher asynchronously.
	private void disconnectAsync()
	{
		new Thread(new Runnable()
		{
			@Override
			public void run()
			{
				// Cancel the connector first so it won't try
				// to automatically reconnect for any reason
				m_connector.cancel();
				
				// Lock the connect lock so we don't encounter
				// synchronization issues between calls to connectAsync
				// and disconnectAsync
				synchronized (m_connectLock)
				{
					// Disconnect from the publisher
					m_subscriber.disconnect();
					
					// Create a new subscriber connector since we cancelled the old one
					m_connector = createSubscriberConnector(m_subscriptionInfo, m_outputStream);
				}
			}
		}).start();
	}
	
	// Creates the text area to which we send our program output.
	private JTextArea createOutputTextArea()
	{
		JTextArea outputTextArea = new JTextArea();
		outputTextArea.setEditable(false);
		outputTextArea.setLineWrap(true);
		outputTextArea.setWrapStyleWord(true);
		return outputTextArea;
	}
	
	// Creates the text field where the user enters the hostname.
	private JTextField createHostnameTextField()
	{
		JTextField hostnameTextField = new JTextField("localhost");
		hostnameTextField.addActionListener(m_connectListener);
		return hostnameTextField;
	}
	
	// Creates the text field where the user enters the port number.
	private JTextField createPortTextField()
	{
		JTextField portTextField = new JTextField("6170");
		portTextField.addActionListener(m_connectListener);
		return portTextField;
	}
	
	// Creates the button used to connect to the publisher.
	private JButton createConnectButton()
	{
		JButton connectButton = new JButton("Connect");
		connectButton.addActionListener(m_connectListener);
		return connectButton;
	}
	
	// Creates the button used to disconnect from the publisher.
	private JButton createDisconnectButton()
	{
		JButton disconnectButton = new JButton("Disconnect");
		disconnectButton.addActionListener(m_disconnectListener);
		return disconnectButton;
	}
	
	// Creates the box that contains the connect and disconnect buttons.
	private Box createButtonBox()
	{
		Box buttonBox = new Box(BoxLayout.X_AXIS);
		buttonBox.add(m_connectButton);
		buttonBox.add(m_disconnectButton);
		return buttonBox;
	}
	
	// Creates the output stream that writes to the text area.
	private PrintStream createOutputStream()
	{
		return new PrintStream(new TextAreaOutputStream(m_outputTextArea));
	}
	
	// SubscriptionInfo is a helper object which allows the user
	// to set up their subscription and resuse subscription settings.
	private SubscriptionInfo createSubscriptionInfo()
	{
		SubscriptionInfo info = new SubscriptionInfo();

		// The following filter expression formats are also available:
		//
		// - Signal ID list -
		//info.setFilterExpression("7aaf0a8f-3a4f-4c43-ab43-ed9d1e64a255;" +
		//						 "93673c68-d59d-4926-b7e9-e7678f9f66b4;" +
		//						 "65ac9cf6-ae33-4ece-91b6-bb79343855d5;" +
		//						 "3647f729-d0ed-4f79-85ad-dae2149cd432;" +
		//						 "069c5e29-f78a-46f6-9dff-c92cb4f69371;" +
		//						 "25355a7b-2a9d-4ef2-99ba-4dd791461379");
		//
		// - Filter pattern -
		//info.setFilterExpression("FILTER ActiveMeasurements WHERE ID LIKE 'PPA:*'");
		//info.setFilterExpression("FILTER ActiveMeasurements WHERE Device = 'SHELBY' AND SignalType = 'FREQ'");
		
		info.setFilterExpression("PPA:1;PPA:2;PPA:3;PPA:4;PPA:5;PPA:6;PPA:7;PPA:8;PPA:9;PPA:10;PPA:11;PPA:12;PPA:13;PPA:14");

		// To set up a remotely synchronized subscription, set this flag
		// to true and add the framesPerSecond parameter to the
		// ExtraConnectionStringParameters. Additionally, the following
		// example demonstrates the use of some other useful parameters
		// when setting up remotely synchronized subscriptions.
		//
		//info.setRemotelySynchronized(true);
		//info.setExtraConnectionStringParameters("framesPerSecond=30;timeResolution=10000;downsamplingMethod=Closest");
		
		info.setRemotelySynchronized(false);
		info.setThrottled(false);
		
		info.setUdpDataChannel(true);
		info.setDataChannelLocalPort(9600);
		
		info.setTimeIncluded(true);
		info.setLagTime(3.0);
		info.setLeadTime(1.0);
		info.setLocalClockAsRealTime(false);
		info.setMillisecondResolution(true);
		
		return info;
	}
	
	// SubscriberConnector is another helper object which allows the
	// user to modify settings for auto-reconnects and retry cycles.
	private SubscriberConnector createSubscriberConnector(SubscriptionInfo info, PrintStream outputStream)
	{
		SubscriberConnector connector = new SubscriberConnector();
		SubscriberConnectorListener listener = new SampleSubscriberConnectorListener(info, outputStream);
		
		connector.setMaxRetries(-1);
		connector.setRetryInterval(5000);
		connector.setAutoReconnect(true);
		connector.addSubscriberConnectorListener(listener);
		
		return connector;
	}
	
	// Creates the data subscriber.
	private DataSubscriber createSubscriber()
	{
		DataSubscriber subscriber = new DataSubscriber();
		subscriber.addSubscriberListener(new SampleSubscriberListener(m_outputStream, m_outputStream));
		return subscriber;
	}
	
	private static final long serialVersionUID = -787591661184215315L;
	
	// OutputStream that, when wrapped by a
	// PrintStream, allows for output to a JTextArea.
	private class TextAreaOutputStream extends OutputStream
	{
		private ByteBuffer m_buffer;
		private JTextArea m_outputArea;
		
		public TextAreaOutputStream(JTextArea outputTextArea)
		{
			if (outputTextArea == null)
				throw new IllegalArgumentException("outputTextArea must not be null");
			
			m_buffer = ByteBuffer.allocate(2000);
			m_outputArea = outputTextArea;
		}
		
		@Override
		public void flush() throws IOException
		{
			m_buffer.flip();
			append(Charset.defaultCharset().decode(m_buffer).toString());
			m_buffer = m_buffer.compact();
		}
		
		@Override
		public void write(byte[] b) throws IOException
		{
			write(b, 0, b.length);
		}
		
		@Override
		public void write(byte[] b, int off, int len) throws IOException
		{
			int rem = m_buffer.remaining();
			
			if (len <= rem)
			{
				m_buffer.put(b, off, len);
				
				if (m_buffer.position() >= DumpThreshold)
					flush();
			}
			else
			{
				m_buffer.put(b, off, rem);
				flush();
				
				for (int i = rem; i < len; i += m_buffer.capacity())
				{
					rem = Math.min(m_buffer.remaining(), len - i);
					m_buffer.put(b, off + i, rem);
					flush();
				}
			}
		}
		
		@Override
		public void write(int b) throws IOException
		{
			m_buffer.put((byte)b);
			
			if (m_buffer.position() >= DumpThreshold)
				flush();
		}
		
		private void append(final String text)
		{
			SwingUtilities.invokeLater(new Runnable()
			{
				@Override
				public void run()
				{
					StringBuilder textBuilder = new StringBuilder(m_outputArea.getText());
					
					textBuilder.append(text);
					
					while (textBuilder.length() > CharacterLimit)
						textBuilder.delete(0, textBuilder.indexOf(System.lineSeparator()) + 1);
					
					m_outputArea.setText(textBuilder.toString());
				}
			});
		}
		
		private static final int DumpThreshold = 50;
		private static final int CharacterLimit = 8192;
	}
}
