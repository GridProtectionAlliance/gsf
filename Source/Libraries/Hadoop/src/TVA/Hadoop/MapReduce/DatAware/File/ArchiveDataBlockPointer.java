package TVA.Hadoop.MapReduce.DatAware.File;


public class ArchiveDataBlockPointer {
	
	public int iIndex;
	public int iPointID;
	public TimeTag Time;
	
	public ArchiveDataBlockPointer() {
		
		this.iIndex = 0; 			// indicates position in block index header
		this.iPointID = 0;			// point id references db, which then tells you more stuff about what type of points reside in this block
		this.Time = new TimeTag();
		this.Time._dTime = 0;
		
	}

}
