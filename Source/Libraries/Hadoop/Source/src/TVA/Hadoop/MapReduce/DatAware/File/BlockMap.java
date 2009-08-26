package TVA.Hadoop.MapReduce.DatAware.File;

import java.util.ArrayList;

/**
 * Class which represents the DatAware blockmap area which is the variable region of the FAT table for the format.
 * @author jpatter0
 *
 */
public class BlockMap {
	
	public ArrayList<ArchiveDataBlockPointer> _arBlockPointers;
	
	public BlockMap() {
		
		this._arBlockPointers = new ArrayList<ArchiveDataBlockPointer>();
		
	}
	
	/**
	 * Adds a block pointer to our list. Typically used when we are reading the variable area of the DatAware FAT.
	 * @param iPointID		The point ID for the block
	 * @param time			the base timestamp for the block
	 * @return				returns the pointer that was added
	 */
	public ArchiveDataBlockPointer AddBlockPointer(int iPointID, double time) {
		
		ArchiveDataBlockPointer ptr = new ArchiveDataBlockPointer();
		ptr.iPointID = iPointID;
		ptr.Time._dTime = time;
		
		this._arBlockPointers.add( ptr );
		
		return ptr;
	}

	/**
	 * Gets a block pointer by its index
	 * @param x		the index of the block pointer
	 * @return		returns the block pointer
	 */
	public ArchiveDataBlockPointer GetBlockPointerByIndex( int x ) {
		
		return this._arBlockPointers.get(x);
	}
	
	public void Debug() {
		
		System.out.println( "BlockMap Debug" );
		
		for ( int x = 0; x < this._arBlockPointers.size(); x++ ) {
			
			System.out.println( "> Block Ptr: ID [" + this._arBlockPointers.get(x).iPointID + "], Time: " + this._arBlockPointers.get(x).Time._dTime );
			
		}
		
	}
	

}

