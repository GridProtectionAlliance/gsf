package TVA.Hadoop.MapReduce.DatAware.File;

import org.apache.hadoop.io.*;

import java.io.IOException;
import java.io.DataInput;
import java.io.DataOutput;
import java.util.Calendar;
import java.util.TimeZone;

import org.apache.commons.logging.Log;
import org.apache.commons.logging.LogFactory;

/**
 * 
 * @author Josh Patterson
 * @version 0.0.1
 * 
 * The StandardPointFile is the type used by hadoop to interpret key/value pairs in the DatAware file format.
 * It is fed to the custom Map Reduce program that is used to analyze the DatAware files on the hadoop cluster.
 *
 */
public class StandardPointFile implements Writable, Comparable {
	
	public int iTimeTag = 0;	// value/time from disk 
	public short Flags = 0;		// value from disk
	public float Value = 0;		// value from disk
	public int iPointID = 0;	// value from block pointer, allows us to figure out at the MapReduce level what this point belonged to.
	//public int Index = 0;		// index is a debug counter which is used to track which points get pulled from disk during development
	static private Calendar cBaseDateTime = Calendar.getInstance(); 
	
	private static final Log LOG = LogFactory.getLog( StandardPointFile.class );
	  
	  
	/**
	 * A handy static init area to get our cBaseDateTime init'd. 
	 * cBaseDateTime is set to January 1st 1995 as this is the base timestamp all DatAware file data points
	 * are based from.
	 */
	static {
			 
		StandardPointFile.cBaseDateTime.setTimeZone(TimeZone.getTimeZone("UTC"));
		StandardPointFile.cBaseDateTime.set(1995, 0, 1, 0, 0, 0);
		StandardPointFile.cBaseDateTime.set(Calendar.MILLISECOND, 0);
	
	}
  
	  
	/**
	 * Serializes the point to disk.
	 * 
	 * @param 	out			A DataOutput object to write data to.
	 * @see 		DataOutput	
	 * @see org.apache.hadoop.io.Writable#write(java.io.DataOutput)
	 * 
	 */
	public void write(DataOutput out) throws IOException {
	
		out.writeInt(iTimeTag);
		out.writeShort(this.Flags);
		out.writeFloat(this.Value);
		//out.writeInt(this.Index);
		out.writeInt(this.iPointID);
		
	}


  	/**
  	 * Deserializes the point from the underlying data. 
  	 * 
  	 * @param	in		A DataInput object to read the point from.
  	 * @see java.io.DataInput
  	 * @see org.apache.hadoop.io.Writable#readFields(java.io.DataInput)
  	 * 
  	 */
	public void readFields(DataInput in) throws IOException {
	
		this.iTimeTag = in.readInt();
		this.Flags = in.readShort();
		this.Value = in.readFloat();
		//this.Index = in.readInt();
		this.iPointID = in.readInt();
		 
	}
   
	/**
  	 * This is a static method that deserializes a point from the underlying binary representation.
  	 * 
  	 * @param in A DataInput object that represents the underlying stream to read from.
  	 * @return A StandardPointFile which is the basic point structure for the DatAware Historian.
  	 * @throws IOException
  	 */
	public static StandardPointFile read(DataInput in) throws IOException {
		
		StandardPointFile p = new StandardPointFile();
		p.readFields(in);
		return p;
		
	}
	

	/**
	 * A comparison method used by the Comparable interface to order the points in a list. 
	 */
   	@Override
   	public int compareTo(Object arg0) {
   		
   		StandardPointFile oOther = (StandardPointFile)arg0; 

   		if ( this.iTimeTag < oOther.iTimeTag ) {
   			return -1;
   		} else if ( this.iTimeTag > oOther.iTimeTag ) {
   			return 1;
   		}

   		// ok, its similar down to the seconds. now check milliseconds
   		int iThisFlags = this.Flags;
   		int iThisMS = (iThisFlags >> 5);
   		
   		int iOtherFlags = oOther.Flags;
   		int iOtherMS = (iOtherFlags >> 5);

   		if ( iThisMS < iOtherMS ) {
   			return -1;
   		} else if ( iThisMS > iOtherMS ) {
   			return 1;
   		}
   		
   		// default -- they are equal
   		return 0;
   	}	
 	
	   	

	/**
   	 * Used to quickly get the difference between this point and another point in milliseconds.
   	 * @param altPoint the other point to measure against.
   	 * @return a Long value in milliseconds.
   	 */
	public long CalcDeltaInMS( StandardPointFile altPoint ) {
		
		return altPoint.GetCalendar().getTimeInMillis() - this.GetCalendar().getTimeInMillis();
		
	}
	
	public void Debug() {
		
		System.out.println( StandardPointFile.cBaseDateTime.getTime() );
		
	}

	/**
	 * Get this point's Date in milliseconds based off of the standard DatAware reference Date.
	 * @return A Calendar object representing this point's Date/Time.
	 */
	public Calendar GetCalendar() {
	
		long time = this.iTimeTag;
		long lTimeTag = (long) (1000 * time);
				
		// now get ms from flag
		int iFlag = this.Flags;
		int iMS = (iFlag >> 5);
		long lFlagMS = (long)(iMS);
		
		Calendar cResult = Calendar.getInstance();
		cResult.setTimeZone(TimeZone.getTimeZone("UTC"));
				
		cResult.setTimeInMillis( StandardPointFile.cBaseDateTime.getTimeInMillis() + lTimeTag + lFlagMS);
				
		return cResult;
		   
	}
	       
	/**
	 * Used to get this point's Date/Time based off a specific reference base date.
	 * @param cBase The reference base date to use.
	 * @return A Calendar object representing this point's Date/Time.
	 */
	public Calendar GetCalendar( Calendar cBase ) {
	
		long time = this.iTimeTag;
		long lTimeTag = (long) (1000 * time);
		
		int iFlag = this.Flags;
		int iMS = (iFlag >> 5);
		long lFlagMS = (long)(iMS);
		
		Calendar cResult = Calendar.getInstance();
		
		cResult.setTimeInMillis( cBase.getTimeInMillis() + lTimeTag + lFlagMS);
		
		return cResult;
		
	}
	       
}

