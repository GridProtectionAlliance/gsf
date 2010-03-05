package TVA.Hadoop.Datamining.Tools;

import java.io.File;
import java.io.IOException;

import org.apache.commons.logging.LogFactory;
import org.apache.log4j.PropertyConfigurator;

import TVA.Hadoop.Datamining.Tools.DataProcessing.ArchiveParser;
import TVA.Hadoop.Datamining.Tools.DataProcessing.InstanceMaker;


/**
 * Main driver for the Instance tools
 * 
 * @author Galen Riley, 
 * @author Josh Patterson
 * @version 0.1.0
 */
public class Instances {

	public static final org.apache.commons.logging.Log LOG = LogFactory.getLog( Instances.class );
	
	
	
	public static void GenerateInstances( String strLocalSrcArchivePath, String strTargetTrainingInstancesPath, int pointId, int pointsPerWindow, int deltaWindow ) throws IOException {
		
		ArchiveParser parser = new ArchiveParser();
		File tempFile = new File(strTargetTrainingInstancesPath + "_temp");
		parser.ParsePointId(new File(strLocalSrcArchivePath), pointId, tempFile);
		
		InstanceMaker maker = new InstanceMaker();
		maker.ParseWindows(new File(strLocalSrcArchivePath), pointsPerWindow, deltaWindow, new File(strTargetTrainingInstancesPath));
		
		tempFile.delete();
	}
	
	
	
	public static void PrintUsage() {
		
		System.out.println( "" );
		System.out.println( "Usage: Instances" );
		System.out.println( "\t\t\t	[-copyFileToHdfs <local_src_path> <hdfs_dst_path>]");
		System.out.println( "\t\t\t	[-generateInstances <local_src_path> <dst_path> <format>]");
		System.out.println( "" );
		
		
	}	
	
	
	
	
	
	public static void main(String[] args) {
	
		

    	PropertyConfigurator.configure( "conf/log4j.props");

		if ( args.length < 2 ) {
			
			PrintUsage();
			
		} else {
			
			int i = 0;
		    String cmd = args[i++];

		    if ("-generateInstances".equals( cmd )) {

				if ( args.length < 7 ) {
					PrintUsage();
					return;
				}
		    	
		    	
		    	String strLocalArchivePath = args[ i++ ];
		    	String strDestInstancesPath = args[ i++ ];
		    	int iPointID = Integer.parseInt( args[ i++ ] );
		    	int pointsPerWindow = Integer.parseInt( args[ i++ ] );
		    	int deltaWindow = Integer.parseInt( args[ i++ ] );
		    	
		    	try {
					GenerateInstances( strLocalArchivePath, strDestInstancesPath, iPointID, pointsPerWindow, deltaWindow );
				} catch (IOException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}

		    	
		    } else {
		    	
		    	PrintUsage();
		    	
		    }
		    
		}		
		
	}
	
}
