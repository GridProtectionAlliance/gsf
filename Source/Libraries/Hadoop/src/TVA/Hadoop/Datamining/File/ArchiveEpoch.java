package TVA.Hadoop.Datamining.File;

import java.util.Calendar;
import java.util.TimeZone;

public class ArchiveEpoch
{
	static public Calendar base;	
	static
	{		 
		base.setTimeZone(TimeZone.getTimeZone("UTC"));
		base.set(1995, 0, 1, 0, 0, 0);
		base.set(Calendar.MILLISECOND, 0);
	}
}
