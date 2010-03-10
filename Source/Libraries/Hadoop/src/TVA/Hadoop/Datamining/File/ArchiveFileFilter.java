package TVA.Hadoop.Datamining.File;

import java.io.File;
import java.util.ArrayList;

public class ArchiveFileFilter
{
	private File _inFile;
	private File _outFile;
	private ArrayList<Integer> points;
	
	public ArchiveFileFilter(File inFile, File outFile, int[] pointIds) throws Exception
	{
		_inFile = inFile;
		_outFile = outFile;
		
		points = new ArrayList<Integer>();
		for(int i = 0; i < pointIds.length; i++)
			points.add(pointIds[i]);
		
		ArchiveFileReader reader = new ArchiveFileReader(_inFile);
		ArchiveFileWriter writer = new ArchiveFileWriter(_outFile, reader.getFAT().getStartTime());
		writer.initializeBlockMap(reader.getBlockMap().getHeader());
		
		for(int i = 0; i < reader.getFAT().getDataBlockCount(); i++)
		{
			DataBlockDescription blockDescrip = reader.getBlockMap().getBlockDescriptions().get(i);
			if(points.contains(blockDescrip.getPointId()))
			{
				DataBlock block = reader.parseBlock(i);
				
				for(int j = 0; j < block.getPointCount(); j++)
				{
					DataPoint point = DataPoint.convert(blockDescrip, block.getPoints().get(j));
					writer.addPoint(point);
				}
			}
		}
		
		writer.close();
	}
	
	
	public static void main( String[] args ) throws Exception
	{		
		/*
		ArchiveFileReader reader = new ArchiveFileReader(new File("input/archive.d"));
		System.out.println(reader.getBlockMap().getBlockDescriptions().size() * DataBlockDescription.length + 10);
		reader.parseBlock(0);
		System.out.println(reader.getCurrentBlock().getBlockDescription().getPointId());
		System.out.println(reader.getCurrentBlock().getBlockCapacity());
		System.out.println(reader.getCurrentBlock().getBlockSize());
		System.out.println(reader.getCurrentBlock().isFull());
		System.out.println(reader.getCurrentBlock().getPoints().get(0).getValue());
		*/
		
		//ArchiveFileFilter filter = new ArchiveFileFilter(new File("input/archive.d"), new File("output/1603.d"), new int[] { 2433 } );
		
		ArchiveFileReader reader = new ArchiveFileReader(new File("output/1603.d"));
		reader.getBlockMap().printPointIds();
	}
}
