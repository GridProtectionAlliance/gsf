using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Net.Sockets;

// 06-16-06


// JRC: Class properties converted to public members for optimization...
namespace TVA.Communication
{
	public class StateInfo<T>
	{
		
		
		public T Client;
		public Guid ID;
		public string Passphrase;
		public byte[] DataBuffer;
		public DateTime LastSendTimestamp;
		public DateTime LastReceiveTimestamp;
		
		public StateInfo<T> This
		{
			get
			{
				return this;
			}
		}
		
		#region " Old Code "
		
		//Public DataBuffer As Byte()
		//Public BytesReceived As Integer
		
		//Public Property ID() As Guid
		//    Get
		//        Return m_id
		//    End Get
		//    Set(ByVal value As Guid)
		//        m_id = value
		//    End Set
		//End Property
		
		//Public Property Client() As T
		//    Get
		//        Return m_client
		//    End Get
		//    Set(ByVal value As T)
		//        m_client = value
		//    End Set
		//End Property
		
		//Public Property DataBuffer() As Byte()
		//    Get
		//        Return m_dataBuffer
		//    End Get
		//    Set(ByVal value As Byte())
		//        m_dataBuffer = value
		//        m_bytesReceived = 0
		//    End Set
		//End Property
		
		//Public Property Passphrase() As String
		//    Get
		//        Return m_passphrase
		//    End Get
		//    Set(ByVal value As String)
		//        m_passphrase = value
		//    End Set
		//End Property
		
		//Public Property BytesReceived() As Integer
		//    Get
		//        Return m_bytesReceived
		//    End Get
		//    Set(ByVal value As Integer)
		//        m_bytesReceived = value
		//    End Set
		//End Property
		
		//Public Property PacketSize() As Integer
		//    Get
		//        Return m_packetSize
		//    End Get
		//    Set(ByVal value As Integer)
		//        m_packetSize = value
		//    End Set
		//End Property
		
		#endregion
		
	}
}
