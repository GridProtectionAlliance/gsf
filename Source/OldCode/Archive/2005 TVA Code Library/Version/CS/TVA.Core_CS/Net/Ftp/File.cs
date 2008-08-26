using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net.Sockets;

// James Ritchie Carroll - 2003


namespace TVA
{
	namespace Net
	{
		namespace Ftp
		{
			
			
			public class File : IFile, IComparable
			{
				
				
				
				private Directory m_parent;
				private string m_name;
				private long m_size;
				private string m_permission;
				private DateTime m_timestamp;
				
				internal File(Directory parent, ItemInfo info)
				{
					
					m_parent = parent;
					m_name = info.Name;
					m_size = info.Size;
					m_permission = info.Permission;
					m_timestamp = info.TimeStamp.Value;
					
				}
				
				internal File(Directory parent, string name)
				{
					
					m_parent = parent;
					m_name = name;
					
				}
				
				public string Name
				{
					get
					{
						return m_name;
					}
				}
				
				public string FullPath
				{
					get
					{
						return m_parent.FullPath + m_name;
					}
				}
				
				public bool IsFile
				{
					get
					{
						return true;
					}
				}
				
				public bool IsDirectory
				{
					get
					{
						return false;
					}
				}
				
				public long Size
				{
					get
					{
						return m_size;
					}
					set
					{
						m_size = value;
					}
				}
				
				public string Permission
				{
					get
					{
						return m_permission;
					}
					set
					{
						m_permission = value;
					}
				}
				
				public DateTime TimeStamp
				{
					get
					{
						return m_timestamp;
					}
					set
					{
						m_timestamp = value;
					}
				}
				
				public Directory Parent
				{
					get
					{
						m_parent.CheckSessionCurrentDirectory();
						return m_parent;
					}
				}
				
				public InputDataStream GetInputStream()
				{
					
					return ((InputDataStream) (GetStream(0, TransferDirection.Download)));
					
				}
				
				public OutputDataStream GetOutputStream()
				{
					
					return ((OutputDataStream) (GetStream(0, TransferDirection.Upload)));
					
				}
				
				public InputDataStream GetInputStream(long offset)
				{
					
					return ((InputDataStream) (GetStream(offset, TransferDirection.Download)));
					
				}
				
				public OutputDataStream GetOutputStream(long offset)
				{
					
					return ((OutputDataStream) (GetStream(offset, TransferDirection.Upload)));
					
				}
				
				private DataStream GetStream(long offset, TransferDirection dir)
				{
					
					m_parent.CheckSessionCurrentDirectory();
					
					SessionConnected Session = m_parent.Session;
					
					if (offset != 0)
					{
						Session.ControlChannel.REST(offset);
					}
					
					DataStream stream = Session.ControlChannel.GetPassiveDataStream(dir);
					
					try
					{
						if (dir == TransferDirection.Download)
						{
							Session.ControlChannel.RETR(m_name);
						}
						else
						{
							Session.ControlChannel.STOR(m_name);
						}
					}
					catch
					{
						stream.Close();
						throw;
					}
					
					return stream;
					
				}
				
				public int CompareTo(object obj)
				{
					
					// Files are sorted by name
					if (obj is IFile)
					{
						return Strings.StrComp(m_name, ((IFile) obj).Name, (m_parent.CaseInsensitive ? CompareMethod.Text : CompareMethod.Binary));
					}
					else
					{
						throw (new ArgumentException("File can only be compared to other Files or Directories"));
					}
					
				}
				
			}
			
		}
	}
}
