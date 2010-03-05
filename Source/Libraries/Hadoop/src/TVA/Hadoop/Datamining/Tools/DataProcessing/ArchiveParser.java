package TVA.Hadoop.Datamining.Tools.DataProcessing;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Calendar;

import TVA.Hadoop.MapReduce.Historian.File.StandardPointFile;

public class ArchiveParser
{
	private LocalArchiveFile m_fileHandler;
	
	// These point IDs correspond to frequency measurements
	private int[] m_pointIds = {
			// P1 Historian
			13, 37, 55, 75, 1285, 1302, 1851, 2171, 2191, 2431, 3063, 3070, 3077, 3084, 3091, 
			3098, 3105, 3112, 3119, 3126, 3133, 3140, 3290, 3306, 3470, 3481, 3502, 3521, 3542, 
			3565, 3576, 3583, 3590, 3599, 3750, 3883, 3911, 3918, 4016, 4131, 4148, 4383, 4398, 
			4401, 4428,
			// P2 Historian
			4059, 4067, 3892, 1649, 1603, 1924, 4091, 1952, 1885, 1863, 3385, 3633, 3726, 3661, 
			3628, 4099, 3029, 1668, 4443, 3863, 1624, 2136, 3173, 3970, 4039, 2025, 3638, 3040, 
			4390, 1586, 3771, 3731, 4155, 4182, 3925, 4083, 3606, 3948, 3213, 3993, 4075,
			// P3 Historian
			125, 1687, 1766, 1821, 1838, 2054, 2581, 2622, 2663, 2713, 2718, 2723, 3261, 3324, 
			3325, 3326, 3390, 3410, 3430, 3450, 3666, 3676, 3686, 3696, 3706, 3716, 3783, 3803, 
			3823, 3843, 4209, 4238, 4267, 4296, 4325, 4354
	};
	
	public ArchiveParser()
	{
	}
	
	public ArchiveParser( int[] interestingPointIds )
	{
		m_pointIds = interestingPointIds;
	}
	
	public int GetBlockPointTypeID( int blockIndex )
	{
		return this.m_fileHandler.FAT._BlockMap.GetBlockPointerByIndex(blockIndex).iPointID;
	}
	
	private int ParseBlock( int blockIndex )
	{
		this.m_fileHandler.ParseBlock(blockIndex);
		
		// return block point id
		return GetBlockPointTypeID(blockIndex);
	}
	
	public void ParsePointId(File inFile, int pointId, File outFile) throws IOException
	{
		// parse all blocks with a given point id in the open file/folder, write to fileName
		if( inFile.isFile() )
		{
			m_fileHandler = new LocalArchiveFile(inFile.getPath());
			m_fileHandler.ReadFAT();
			
			for( int blockIndex = 0; blockIndex < m_fileHandler.FAT._EventBlockCount; blockIndex++ )
			{
				if( pointId == this.GetBlockPointTypeID(blockIndex) )
					InsertBlockInCsv(blockIndex, outFile);
			}
		}
		else
		{
			for (File file : inFile.listFiles())
				ParsePointId(file, pointId, outFile);
		}
	}
	public void ParseAllBlocks(File inFile, File outFolder) throws IOException
	{
		// parse all blocks in the open file/folder, write to fileName
		if( inFile.isFile() )
		{
			m_fileHandler = new LocalArchiveFile(inFile.getPath());
			m_fileHandler.ReadFAT();
			
			for( int blockIndex = 0; blockIndex < m_fileHandler.FAT._EventBlockCount; blockIndex++ )
				InsertBlockInCsv(blockIndex, new File(outFolder.getAbsoluteFile() + "/" + GetBlockPointTypeID(blockIndex) + ".csv"));
		}
		else
		{
			for (File file : inFile.listFiles())
				ParseAllBlocks(file, outFolder);
		}
	}
	public void ParseInterestingBlocks(File inFile, File outFolder) throws IOException
	{
		// parse all "interesting" blocks in the open file/folder, write to fileName
		if( inFile.isFile() )
		{
			m_fileHandler = new LocalArchiveFile(inFile.getPath());
			m_fileHandler.ReadFAT();
			
			for( int blockIndex = 0; blockIndex < m_fileHandler.FAT._EventBlockCount; blockIndex++ )
			{
				if( IsBlockInteresting(blockIndex) )
					InsertBlockInCsv(blockIndex, new File(outFolder.getAbsoluteFile() + "/" + GetBlockPointTypeID(blockIndex) + ".csv"));
			}
		}
		else
		{
			for (File file : inFile.listFiles())
				ParseInterestingBlocks(file, outFolder);
		}
	}
	
	public void WriteFile( String fileName ) throws IOException
	{
		BufferedWriter writer = new BufferedWriter(new FileWriter(fileName, true));
		
		for( int i = 0; i < m_fileHandler.arPoints.size(); i++ )
			writer.write(m_fileHandler.arPoints.get(i).GetCalendar().getTimeInMillis() + ",");
		
		writer.write(System.getProperty("line.separator"));
		
		for( int i = 0; i < m_fileHandler.arPoints.size(); i++ )
			writer.write(m_fileHandler.arPoints.get(i).Value + ",");
		
		writer.write(System.getProperty("line.separator"));
		
		writer.write((m_fileHandler.arPoints.get(m_fileHandler.arPoints.size()-1).GetCalendar().getTimeInMillis() - m_fileHandler.arPoints.get(0).GetCalendar().getTimeInMillis())/(double) 1000.0 + " seconds");
		
		writer.write(System.getProperty("line.separator") + System.getProperty("line.separator"));
		
		writer.close();
	}
	
	// TODO: Optimize this method to touch disk less, very slow now
	private int InsertBlockInCsv( int blockIndex, File file ) throws IOException
	{
		file.createNewFile();
		File tempFile = new File(file.getPath() + "_temp");
		tempFile.createNewFile();
		
		// from currently open file, parse the block at blockIndex
		int pointId = ParseBlock(blockIndex);
		
		// grab the points in this block, they are sorted coming out of the file
		ArrayList<StandardPointFile> points = m_fileHandler.arPoints;
		int pointIndex = 0;
		
		// open the file
		BufferedReader fileReader = new BufferedReader(new FileReader(file), 1024*1024*64);
		String line = null;
		boolean fileLineSaved = false;
		
		// create a temp file with a similar name
		BufferedWriter tempWriter = new BufferedWriter(new FileWriter(tempFile, true), 1024*1024*32);
		
		// read file
		while(pointIndex < points.size()-1 && (fileLineSaved && line != null) || ((line = fileReader.readLine()) != null))
		{
			fileLineSaved = false;
			
			long lineTimestamp = Long.parseLong(line.split(",")[0]);
			float lineValue = Float.parseFloat(line.split(",")[1]);
			
			if( lineTimestamp < points.get(pointIndex).GetCalendar().getTimeInMillis() )
			{
				// write the point from the existing file first
				tempWriter.write(Long.toString(lineTimestamp));
				tempWriter.write(",");
				tempWriter.write(Float.toString(lineValue));
				tempWriter.write(System.getProperty("line.separator"));

				fileLineSaved = true;
			}
			else
			{
				// write the point we just read from the block first
				tempWriter.write(Long.toString(points.get(pointIndex).GetCalendar().getTimeInMillis()));
				tempWriter.write(",");
				tempWriter.write(Float.toString(points.get(pointIndex).Value));
				tempWriter.write(System.getProperty("line.separator"));
				
				pointIndex++;
			}
		}
		
		// one of the sets is complete, write the other one
		if( pointIndex >= points.size() )
		{
			// we're out of points, copy the rest of the file over
			while((line = fileReader.readLine()) != null)
				tempWriter.write(fileReader.readLine());
		}
		else
		{
			// we've read the whole file, write the rest of the points
			while( pointIndex < points.size() )
			{
				// write the point we just read from the block first
				tempWriter.write(Long.toString(points.get(pointIndex).GetCalendar().getTimeInMillis()));
				tempWriter.write(",");
				tempWriter.write(Float.toString(points.get(pointIndex).Value));
				tempWriter.write(System.getProperty("line.separator"));
				
				pointIndex++;
			}
		}
		
		fileReader.close();
		tempWriter.close();
		
		// replace the old file with the temp one
		file.delete();
		tempFile.renameTo(file);
		//tempFile.delete();
		
		return pointId;
	}
	
	private boolean IsBlockInteresting(int blockIndex)
	{
		for( int i = 0; i < m_pointIds.length; i++ )
		{
			if( m_pointIds[i] == GetBlockPointTypeID(blockIndex) )
				return true;
		}
		
		return false;
	}
	
	/**
	 * Usage examples.
	 * 
	 * @param args
	 * @throws IOException 
	 */
	public static void main( String[] args ) throws IOException
	{
		long start = Calendar.getInstance().getTimeInMillis();
		ArchiveParser parser = new ArchiveParser();
		//parser.ParseInterestingBlocks(new File("input/archive.d"), new File("output"));
		parser.ParsePointId(new File("input/archive.d"), 3098, new File("output/3098.csv"));
		long end = Calendar.getInstance().getTimeInMillis();
		
		long expired = end-start;
		
		System.out.println(expired);
	}
}
