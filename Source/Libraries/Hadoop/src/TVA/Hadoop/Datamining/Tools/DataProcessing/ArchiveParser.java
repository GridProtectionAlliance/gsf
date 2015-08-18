package TVA.Hadoop.Datamining.Tools.DataProcessing;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileReader;
import java.io.FileWriter;
import java.util.Calendar;

import TVA.Hadoop.Datamining.File.ArchiveFileReader;
import TVA.Hadoop.Datamining.File.DataBlockDescription;

public class ArchiveParser
{
	private ArchiveFileReader m_fileHandler;
	
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
	
	public int GetBlockPointTypeID( int blockIndex ) throws Exception
	{
		return this.m_fileHandler.getBlockMap().getBlockDescriptions().get(blockIndex).getPointId();
	}
	
	private int ParseBlock( int blockIndex ) throws Exception
	{
		this.m_fileHandler.parseBlock(blockIndex);
		
		// return block point id
		return GetBlockPointTypeID(blockIndex);
	}
	
	public void ParsePointId(File inFile, int pointId, File outFile) throws Exception
	{
		// parse all blocks with a given point id in the open file/folder, write to fileName
		if( inFile.isFile() )
		{
			m_fileHandler = new ArchiveFileReader(inFile);
			
			for( int blockIndex = 0; blockIndex < m_fileHandler.getFAT().getDataBlockCount(); blockIndex++ )
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
	public void ParseAllBlocks(File inFile, File outFolder) throws Exception
	{
		// parse all blocks in the open file/folder, write to fileName
		if( inFile.isFile() )
		{
			m_fileHandler = new ArchiveFileReader(inFile);
			
			for( int blockIndex = 0; blockIndex < m_fileHandler.getFAT().getDataBlockCount(); blockIndex++ )
				InsertBlockInCsv(blockIndex, new File(outFolder.getAbsoluteFile() + "/" + GetBlockPointTypeID(blockIndex) + ".csv"));
		}
		else
		{
			for (File file : inFile.listFiles())
				ParseAllBlocks(file, outFolder);
		}
	}
	public void ParseInterestingBlocks(File inFile, File outFolder) throws Exception
	{
		// parse all "interesting" blocks in the open file/folder, write to fileName
		if( inFile.isFile() )
		{
			m_fileHandler = new ArchiveFileReader(inFile);
			
			for( int blockIndex = 0; blockIndex < m_fileHandler.getFAT().getDataBlockCount(); blockIndex++ )
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
	
	public void WriteFile( String fileName ) throws Exception
	{
		BufferedWriter writer = new BufferedWriter(new FileWriter(fileName, true));
		
		DataBlockDescription blockDescrip = m_fileHandler.getCurrentBlock().getBlockDescription();
		
		for( int i = 0; i < m_fileHandler.getCurrentBlock().getPointCount(); i++ )
			writer.write(m_fileHandler.getCurrentBlock().getPoints().get(i).getRealTimeInMillis(blockDescrip) + ",");
		
		writer.write(System.getProperty("line.separator"));
		
		for( int i = 0; i < m_fileHandler.getCurrentBlock().getPointCount(); i++ )
			writer.write(m_fileHandler.getCurrentBlock().getPoints().get(i).getValue() + ",");
		
		writer.write(System.getProperty("line.separator"));
		
		writer.write((m_fileHandler.getCurrentBlock().getPoints().get(m_fileHandler.getCurrentBlock().getPointCount()-1).getRealTimeInMillis(blockDescrip) - m_fileHandler.getCurrentBlock().getPoints().get(0).getRealTimeInMillis(blockDescrip))/(double) 1000.0 + " seconds");
		
		writer.write(System.getProperty("line.separator") + System.getProperty("line.separator"));
		
		writer.close();
	}
	
	// TODO: Optimize this method to touch disk less, very slow now
	private int InsertBlockInCsv( int blockIndex, File file ) throws Exception
	{
		file.createNewFile();
		File tempFile = new File(file.getPath() + "_temp");
		tempFile.createNewFile();
		
		// from currently open file, parse the block at blockIndex
		int pointId = ParseBlock(blockIndex);
		
		// grab the points in this block, they are sorted coming out of the file
		DataBlockDescription blockDescrip = m_fileHandler.getCurrentBlock().getBlockDescription();
		int pointIndex = 0;
		
		// open the file
		BufferedReader fileReader = new BufferedReader(new FileReader(file), 1024*1024*64);
		String line = null;
		boolean fileLineSaved = false;
		
		// create a temp file with a similar name
		BufferedWriter tempWriter = new BufferedWriter(new FileWriter(tempFile, true), 1024*1024*32);
		
		// read file
		while(pointIndex < m_fileHandler.getCurrentBlock().getPointCount()-1 && (fileLineSaved && line != null) || ((line = fileReader.readLine()) != null))
		{
			fileLineSaved = false;
			
			long lineTimestamp = Long.parseLong(line.split(",")[0]);
			float lineValue = Float.parseFloat(line.split(",")[1]);
			
			if( lineTimestamp < m_fileHandler.getCurrentBlock().getPoints().get(pointIndex).getRealTimeInMillis(blockDescrip) )
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
				tempWriter.write(Long.toString(m_fileHandler.getCurrentBlock().getPoints().get(pointIndex).getRealTimeInMillis(blockDescrip)));
				tempWriter.write(",");
				tempWriter.write(Float.toString(m_fileHandler.getCurrentBlock().getPoints().get(pointIndex).getValue()));
				tempWriter.write(System.getProperty("line.separator"));
				
				pointIndex++;
			}
		}
		
		// one of the sets is complete, write the other one
		if( pointIndex >= m_fileHandler.getCurrentBlock().getPointCount() )
		{
			// we're out of points, copy the rest of the file over
			while((line = fileReader.readLine()) != null)
				tempWriter.write(fileReader.readLine());
		}
		else
		{
			// we've read the whole file, write the rest of the points
			while( pointIndex < m_fileHandler.getCurrentBlock().getPointCount() )
			{
				// write the point we just read from the block first
				tempWriter.write(Long.toString(m_fileHandler.getCurrentBlock().getPoints().get(pointIndex).getRealTimeInMillis(blockDescrip)));
				tempWriter.write(",");
				tempWriter.write(Float.toString(m_fileHandler.getCurrentBlock().getPoints().get(pointIndex).getValue()));
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
	
	private boolean IsBlockInteresting(int blockIndex) throws Exception
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
	 * @throws Exception 
	 */
	public static void main( String[] args ) throws Exception
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
