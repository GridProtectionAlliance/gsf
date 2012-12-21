using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using UnityEngine;

public class IniFile
{
	private string m_fileName;
	private ConcurrentDictionary<string, ConcurrentDictionary<string, string>> m_iniData;
	
	public IniFile(string fileName)
	{
		if ((object)fileName == null)
			throw new ArgumentNullException("fileName");

		m_fileName = fileName;
		m_iniData = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>(StringComparer.CurrentCultureIgnoreCase);
		
		Load();
	}

    public string this[string sectionName, string entryName]
    {
        get
        {
			ConcurrentDictionary<string, string> section = m_iniData.GetOrAdd(sectionName, CreateNewSection);			
            return section.GetOrAdd(entryName, (string)null);
        }
        set
        {
            ConcurrentDictionary<string, string> section = m_iniData.GetOrAdd(sectionName, CreateNewSection);
			section[entryName] = value;
        }
    }

	public string this[string sectionName, string entryName, string defaultValue]
    {
        get
        {
			ConcurrentDictionary<string, string> section = m_iniData.GetOrAdd(sectionName, CreateNewSection);			
            return section.GetOrAdd(entryName, defaultValue);
        }
    }
	
	public void Load()
	{	
		if (File.Exists(m_fileName))
		{
			using (StreamReader reader = new StreamReader(m_fileName))
			{
				string line = reader.ReadLine ();
				ConcurrentDictionary<string, string> section = null;
			
				while ((object)line != null)
				{
					line = RemoveComments (line);
				
					if (line.Length > 0)
					{
						// Check for new section				
						int startBracketIndex = line.IndexOf('[');
					
						if (startBracketIndex == 0)
						{
							int endBracketIndex = line.IndexOf(']');
						
							if (endBracketIndex > 1) 
							{
								string sectionName = line.Substring(startBracketIndex + 1, endBracketIndex - 1);
							
								if (!string.IsNullOrEmpty(sectionName))
									section = m_iniData.GetOrAdd(sectionName, CreateNewSection);
							}					
						}
					
						if ((object)section == null)
							throw new InvalidOperationException("INI file did not begin with a [section]");
					
						// Check for key/value pair
						int equalsIndex = line.IndexOf("=");
					
						if (equalsIndex > 0)
						{
							string key = line.Substring(0, equalsIndex).Trim();
						
							if (!string.IsNullOrEmpty(key))
								section[key] = line.Substring(equalsIndex + 1).Trim();
						}
					}
							
					line = reader.ReadLine();
				}
			}
		}
	}
	
	// Saving INI file will strip comments - sorry :-(
	public void Save()
	{
		Save(m_fileName);
	}
	
	public void Save(string fileName)
	{
		if ((object)fileName == null)
			throw new ArgumentNullException("fileName");

		using (StreamWriter writer = new StreamWriter(fileName))
		{
			foreach (KeyValuePair<string, ConcurrentDictionary<string, string>> section in m_iniData)
			{
				writer.WriteLine("[{0}]", section.Key);
				
				foreach (KeyValuePair<string, string> entry in section.Value)
				{
					writer.WriteLine("{0} = {1}", entry.Key, entry.Value);
				}
				
				writer.WriteLine();
			}
		}
	}
	
	private ConcurrentDictionary<string, string> CreateNewSection(string sectionName)
	{
		return new ConcurrentDictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
	}
	
	// Remove any comments from key value string
	private static string RemoveComments(string keyValue)
	{
		// Remove any trailing comments from key value
		keyValue = keyValue.Trim();

		int commentIndex = keyValue.IndexOf(';');

		if (commentIndex > -1)
			keyValue = keyValue.Substring(0, commentIndex).Trim();

		return keyValue;
	}
}
