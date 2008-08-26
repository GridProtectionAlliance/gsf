using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

// 06-01-06

namespace TVA.Communication
{
	public enum TransportProtocol
	{
		Tcp,
		Udp,
		Serial,
		File
	}
	
	public enum CRCCheckType
	{
		None,
		CRC16,
		CRC32,
		CRC_CCITT
	}
}
