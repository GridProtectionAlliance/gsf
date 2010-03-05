package TVA.Hadoop.Datamining.Tools;

import org.apache.commons.logging.LogFactory;



/**
 * Main driver for the Archive tools
 * 
 * @author Galen Riley
 * @author Josh Patterson
 * @version 0.1.0
 */
public class Archive {


	public static final org.apache.commons.logging.Log LOG = LogFactory.getLog( Archive.class );
	
	
	public static void PrintUsage() {
		
		System.out.println( "" );
		System.out.println( "Usage: Archive" );
		System.out.println( "\t\t\t	[-copyFileToHdfs <local_src_path> <hdfs_dst_path>]");
		System.out.println( "\t\t\t	[-debug <local_archive_path>]");
		System.out.println( "\t\t\t	[-debugBlock <local_archive_path>]");
		System.out.println( "" );
		
		
	}	
		
	
	
	/**
	 * @param args
	 */
	public static void main(String[] args) {
		// TODO Auto-generated method stub

		PrintUsage();
		
	}

}
