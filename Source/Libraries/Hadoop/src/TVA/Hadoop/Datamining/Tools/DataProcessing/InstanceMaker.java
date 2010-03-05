package TVA.Hadoop.Datamining.Tools.DataProcessing;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.security.InvalidParameterException;

import TVA.Hadoop.Datamining.Tools.DataProcessing.ValueObjects.FrequencyClassification;

public class InstanceMaker
{
	public InstanceMaker()
	{
	}
	
	private void ParseFile(File inFile, int pointsPer, int deltaWindow, File outFile) throws IOException
	{
		if( 0 >= deltaWindow )
			throw new InvalidParameterException("deltaWindow must be positive.");
		
		int windowsComplete = 0;
		
		// this strategy is slow, but is easiest to program, and performance in sample generation is not important
		while(true)
		{
			try
			{
				// reset to the start of the window
				BufferedReader reader = new BufferedReader( new FileReader(inFile) );
				reader.mark(100 * pointsPer);
				
				// skip until we get to the next window
				// why can't i set position manually?
				int linesRead = 0;
				while( linesRead < deltaWindow * windowsComplete )
				{
					reader.readLine();
					linesRead++;
				}
				
				String[] timestamps = new String[pointsPer];
				String[] values = new String[pointsPer];
				
				int linesReadInThisWindow = 0;
				while( linesReadInThisWindow < pointsPer )
				{
					String pointString = reader.readLine();					
					String[] chunks = pointString.split(",");
					
					timestamps[linesReadInThisWindow] = chunks[0];
					values[linesReadInThisWindow] = chunks[1];
					
					linesReadInThisWindow++;
				}
				
				reader.close();
				
				BufferedWriter writer = new BufferedWriter( new FileWriter(outFile, true) );
				
				// now write file
				//for( int i = 0; i < pointsPer; i++ )
					//writer.write(timestamps[i] + ",");
				//writer.write("\r\n");
				for( int i = 0; i < pointsPer; i++ )
				{
					writer.write(values[i]);
					if( i < pointsPer - 1 )
						writer.write(",");
				}
				
				writer.write(FrequencyClassification.UNCLASSIFIED.toString());
				
				writer.write(System.getProperty("line.separator"));
				//writer.write("\r\n");
				
				writer.close();
				
				windowsComplete++;
			}
			catch(NullPointerException e)
			{
				//e.printStackTrace();
				System.out.println("done reading " + inFile.getName());
				break;
			}
		}
	}
	
	public void ParseWindows(File inFile, int pointsPer, int deltaWindow, File outFile) throws IOException
	{
		// parse all blocks with a given point id in the open file/folder, write to fileName
		if( inFile.isFile() )
		{
			ParseFile(inFile, pointsPer, deltaWindow, outFile);
		}
		else
		{
			for (File file : inFile.listFiles())
				ParseWindows(file, pointsPer, deltaWindow, outFile);
		}
	}
}