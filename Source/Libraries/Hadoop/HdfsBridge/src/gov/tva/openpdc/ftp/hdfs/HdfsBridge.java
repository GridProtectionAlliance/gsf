package gov.tva.openpdc.ftp.hdfs;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.HashMap;
import java.util.Map;
import java.util.Properties;

import org.apache.ftpserver.DefaultDataConnectionConfiguration;
import org.apache.ftpserver.FtpServer;
import org.apache.ftpserver.ftplet.FileSystemManager;
import org.apache.ftpserver.ftplet.FileSystemView;
import org.apache.ftpserver.ftplet.FtpException;
import org.apache.ftpserver.ftplet.Ftplet;
import org.apache.ftpserver.ftplet.User;
import org.apache.ftpserver.interfaces.DataConnectionConfiguration;
import org.apache.ftpserver.listener.nio.NioListener;
import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.hdfs.DistributedFileSystem;
import org.apache.log4j.PropertyConfigurator;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class HdfsBridge {

	public class ArchiveFileSystemManager implements FileSystemManager {
		public FileSystemView createFileSystemView(User user) throws FtpException {
			return new ArchiveFileSystemView(user);
		}
	}	
	
	private final static Logger log = LoggerFactory.getLogger( HdfsBridge.class );
	//private static String CONF_FILE = "FtpToHdfsBridge.conf"; 
	
	private static DistributedFileSystem _hdfs = null;
	private static String _superuser = "error";
	private static String _supergroup = "supergroup";
	private static int _port = 0;
	private static String _passivePorts = null;
	private static String _hdfsUri = null;

	
	/*
	 * Config notes
	 * -	need a copy of hdfs-site.xml in the conf directory
	 * 
	 * 
	 */
	

	private static void InitializeHdfsConnection() throws IOException {
		
		_hdfs = new DistributedFileSystem();
		Configuration conf = new Configuration();
		//conf.set("hadoop.job.ugi", superuser + "," + supergroup);
		conf.set("hadoop.job.ugi", "hadoop,hadoop");
		
		// get hdfs uri from conf
		//String HDFS_URI = "";
		
		try {
			
			_hdfs.initialize( new URI( _hdfsUri ), conf );
			
		} catch (URISyntaxException e) {
			
			log.error("HDFS Initialization error", e);
			
		}
		
	}


	public static DistributedFileSystem getHdfs() throws IOException {
		
		if (_hdfs == null) {
			InitializeHdfsConnection();
		}
		
		return _hdfs;
		
	}

	
	
	private static void LoadBridgeConfiguration() throws FileNotFoundException, IOException {
				
		System.out.println( "HdfsBridge > Loading Config" );
		
		Configuration conf = new Configuration(false);
		conf.addResource("HdfsBridge-site.xml");
		
		
		try {
			
			_port = conf.getInt( "openpdc.hdfsbridge.port", 2222);
			
			log.info("port is set. ftp server will be started");
		} catch (Exception e) {
			log.info("port is not set. so ftp server will not be started");
		}
		
		
		if (_port != 0) {
			_passivePorts = conf.get( "openpdc.hdfsbridge.data_ports", "2223-2225" );
			if (_passivePorts == null) {
				log.error("data-ports is not set");
				System.exit(1);
			}
		}
		
		_hdfsUri = conf.get( "hadoop.hdfs.uri", "hdfs://foo" );
		if (_hdfsUri == null) {
			//log.fatal("hdfs-uri is not set");
			log.error("hdfs-uri is not set");
			System.exit(1);
		}
		
		String superuser = conf.get( "hadoop.hdfs.superuser", "hadoop" );
		if (superuser == null) {
			log.error("superuser is not set");
			System.exit(1);
		}
		
		
	}
	
	private void Start() throws Exception {
		
		System.out.println( "HdfsBridge > Starting bridge..." );
		
		
		

	    //log.info("Starting Hdfs-Over-Ftp server. port: " + port + " data-ports: " + passivePorts + " hdfs-uri: " + hdfsUri + ", NERC_MODE: " + NercOperationMode );

	    //HdfsOverFtpSystem.setHDFS_URI(hdfsUri);

	    FtpServer server = new FtpServer();

	    DataConnectionConfiguration dataCon = new DefaultDataConnectionConfiguration();
	    dataCon.setPassivePorts( _passivePorts );
	    server.getListener("default").setDataConnectionConfiguration( dataCon );
	    server.getListener("default").setPort( _port );

	    // this is a bad way to do this, but the main function seems to not be setting it right.
        NioListener listener_hack = (NioListener) server.getListener("default");
        listener_hack.setIdleTimeout( 600 );
        log.info( "HdfsBridge > setting NioListener idle timeout to 600 seconds!" );




			// setup our custom ftplet! ---------------------------------------------------------
	        Map<String, Ftplet> ftpletMap = new HashMap<String, Ftplet>();

	        //gov.tva.ftp.openPDC_Ftplet oHdfsFtplet = new gov.tva.ftp.openPDC_Ftplet();
	        gov.tva.openpdc.ftp.ftplet.HdfsChecksumFtpLet oHdfsFtplet = new gov.tva.openpdc.ftp.ftplet.HdfsChecksumFtpLet(); 
	        oHdfsFtplet.setHDFS_URI( _hdfsUri ); // set the hdfs location uri

	        ftpletMap.put( "gov.tva.openpdc.ftp.ftplet.HdfsChecksumFtpLet", oHdfsFtplet);

	        server.setFtplets( ftpletMap );
	        // setup our custom ftplet! ---------------------------------------------------------

		log.info( "HdfsBridge > starting ftp server with ftplet!" );



	    AccountManager acctManager = new AccountManager();
	    acctManager.setFile( new File( "conf/users.conf" ) );
	    server.setUserManager( acctManager );
	    server.setFileSystem( new ArchiveFileSystemManager() );

	    server.start();		
		
	}
	

	
	public static void DebugSparkyConfig() {
		
		Configuration conf = new Configuration(false);
		conf.addResource("HdfsBridge-site.xml");
/*

	
		String superuser = conf.get( "hadoop.hdfs.superuser", "hadoop" );

 */
		System.out.println( "Debugging HdfsBridge Configuration ------ " );
		System.out.println( "> openpdc.hdfsbridge.port: " + conf.get("openpdc.hdfsbridge.port") );
		System.out.println( "> openpdc.hdfsbridge.data_ports: " + conf.get("openpdc.hdfsbridge.data_ports") );
		System.out.println( "> hadoop.hdfs.uri: " + conf.get("hadoop.hdfs.uri") );
		
	}
	
	
	
	public static void PrintUsage() {
		
		System.out.println( "" );
		System.out.println( "HdfsBridge: usage ---- " );
		System.out.println( "\tStart Hdfs Bridge");
		System.out.println( "\tDebug Hdfs Bridge Confg [-debugconfig]");
		System.out.println( "" );
		
		
	}	
	
	/**
	 * @param args
	 */
	public static void main(String[] args) {

		
		PropertyConfigurator.configure("conf/log4j.properties");

		if ( args.length < 2 ) {
			
			PrintUsage();
			
		} else {
			
			int i = 0;
		    String cmd = args[i++];
		    

		    if ("-debugconfig".equals( cmd )) {

		    	DebugSparkyConfig();
		    	
		    } else { //if ("-start".equals( cmd )) {
				    	
				try {
					
					LoadBridgeConfiguration();
					HdfsBridge bridge = new HdfsBridge();
					bridge.Start();
					
				} catch (FileNotFoundException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				} catch (IOException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				} catch (Exception e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
				
		    } // if
		    
		} // if
		
	} // main(...)

}
