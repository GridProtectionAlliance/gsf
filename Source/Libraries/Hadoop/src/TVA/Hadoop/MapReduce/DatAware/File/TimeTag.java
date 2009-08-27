package TVA.Hadoop.MapReduce.DatAware.File;

import java.util.Date;
import java.util.Calendar;

/**
 * The TimeTag class is used in the block map
 * @author jpatter0
 *
 */
public class TimeTag {
	
	public double _dTime;
	
	public Date GetDate( Calendar cBase ) {
			
		long lTest = (long) (1000 * this._dTime);
		
		long lResult = lTest + cBase.getTimeInMillis();
		Calendar cResult = Calendar.getInstance();
		cResult.setTimeInMillis(lResult);
		
		return cResult.getTime();
		
	}
	
	public Calendar GetCalendar( Calendar cBase ) {
			
		long lTest = (long) (1000 * this._dTime);
		
		long lResult = lTest + cBase.getTimeInMillis();
		Calendar cResult = Calendar.getInstance();
		cResult.setTimeInMillis(lResult);

		return cResult;
		
	}	
	
}
