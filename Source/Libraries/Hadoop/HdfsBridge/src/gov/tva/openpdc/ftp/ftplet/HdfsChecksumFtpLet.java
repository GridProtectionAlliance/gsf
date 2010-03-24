package gov.tva.openpdc.ftp.ftplet;

import java.io.IOException;
import java.net.URI;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.Date;

import org.apache.ftpserver.DefaultFtpHandler;
import org.apache.ftpserver.ftplet.DefaultFtpReply;
import org.apache.ftpserver.ftplet.DefaultFtplet;
import org.apache.ftpserver.ftplet.FileObject;
import org.apache.ftpserver.ftplet.FileSystemView;
import org.apache.ftpserver.ftplet.FtpException;
import org.apache.ftpserver.ftplet.FtpReply;
import org.apache.ftpserver.ftplet.FtpRequest;
import org.apache.ftpserver.ftplet.FtpSession;
import org.apache.ftpserver.ftplet.FtpletContext;
import org.apache.ftpserver.ftplet.FtpletResult;
import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.fs.FileChecksum;
import org.apache.hadoop.fs.LocalFileSystem;
import org.apache.hadoop.fs.MD5MD5CRC32FileChecksum;
import org.apache.hadoop.fs.Path;
import org.apache.hadoop.hdfs.DistributedFileSystem;
import org.apache.hadoop.hdfs.DistributedFileSystem.DiskStatus;
import org.apache.log4j.Level;
import org.apache.log4j.Logger;

public class HdfsChecksumFtpLet extends DefaultFtplet {

    private static DistributedFileSystem dfs = null;
    //private static enum OPERATION_MODE { NERC, ADMIN };
    public static String HDFS_URI = "hdfs://socdvmhdfs1:9000";
    //public static OPERATION_MODE NercOperationMode = OPERATION_MODE.NERC; 
    private static final String DFS_MD5_NOT_DONE = "dfs_not_done";

    private final static Logger log = Logger.getLogger( HdfsChecksumFtpLet.class );    
	
    private String strLastFileUploaded = "";
    private String strLastFileUploadPath = "";

    private static void hdfsInit() {
    	
    	System.out.println( "HdfsBridge_FtpLet > hdfsinit: " + HDFS_URI );
    	log.info( "HdfsBridge_FtpLet > hdfsinit: " + HDFS_URI );
    	
        dfs = new DistributedFileSystem();
        Configuration conf = new Configuration();
        //conf.set("hadoop.job.ugi", superuser + "," + supergroup);
        conf.set("hadoop.job.ugi", "hadoop,supergroup" );
        try {
            dfs.initialize(new URI(HDFS_URI), conf);
        } catch (Exception e) {
            log.error("DFS Initialization error", e);
            System.out.println( "HdfsBridge_FtpLet > DFS Initialization error! " + e.toString() );
        }
    }

    
    public static void setHDFS_URI(String HDFS_URI) {
    	System.out.println( "HdfsBridge_FtpLet > Setting HDFS_URI: " + HDFS_URI );
    	HdfsChecksumFtpLet.HDFS_URI = HDFS_URI;
    }

    /**
     * Get dfs
     *
     * @return dfs
     * @throws IOException
     */
    public static DistributedFileSystem getDfs() {
    	
    	//System.out.println( "HdfsBridge_FtpLet > getDFS: " + HDFS_URI );
    	
        if (dfs == null) {
            hdfsInit();
        }
        return dfs;
    } 


    
    
    
    
    
    

	public static String getHexString(byte[] b) throws Exception {
		  String result = "";
		  for (int i=0; i < b.length; i++) {
		    result +=
		          Integer.toString( ( b[i] & 0xff ) + 0x100, 16).substring( 1 );
		  }
		  return result;
		}
   
    
	private void TurnOnHDFSDebugLogging() {
		
    	Logger l = Logger.getLogger(org.apache.hadoop.hdfs.DFSClient.class);
    	l.setLevel(Level.DEBUG);		
		
	}
    
	private void TurnOffHDFSDebugLogging() {
		
    	Logger l = Logger.getLogger(org.apache.hadoop.hdfs.DFSClient.class);
    	l.setLevel(Level.ERROR);		
		
	}
    
    
    private String GetHDFSChecksumForFile( String strHDFSPath ) {
 
    	String strPath = strHDFSPath; // "/tmp/dummyfile.d";
    	String strReturnValue = "";
    	String strNotReadyCause = "java.io.IOException: Fail to get block MD5";
	
    	try {
    	
    		System.out.println( this.GetNow() + " > HdfsBridge_FtpLet > Checking Checksum: '" + strPath + "'" );
    		
    		
    		long start_ts = System.currentTimeMillis();
    		
    		MD5MD5CRC32FileChecksum md5 = this.dfs.getFileChecksum( new Path( strPath ) );
  
       		
    		long end_ts = System.currentTimeMillis();
    		
    		System.out.println( this.GetNow() + " > HdfsBridge_FtpLet > getFileChecksum exe Time: " + (end_ts - start_ts) + " ms" );
   		
    		
    		
    		if ( null != md5 ) {
    		
    			String hashWithAlgo = md5.toString();
    			strReturnValue = hashWithAlgo.substring( hashWithAlgo.indexOf(":") );
    			
    			System.out.println( "HdfsBridge_FtpLet > algo:md5: " + md5.toString() );
    			System.out.println( "HdfsBridge_FtpLet > md5: " + strReturnValue );
    			//System.out.println( "openPDC > md5.getBytes().length: " + md5.getBytes().length );

    			return strReturnValue;
    			
    		} else {
    			
    			System.out.println( "HdfsBridge_FtpLet > md5 was null!" );
    			
    		}
    		
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			
			String strCause = e.toString().substring(0, strNotReadyCause.length() );
			if ( strCause.compareTo(strNotReadyCause) == 0 ) {
				System.out.println( "HdfsBridge_FtpLet > The Exception was caused by hadoop not being ready to calc the blocks" );
				strReturnValue = DFS_MD5_NOT_DONE;
			} else {
				System.out.println( "HdfsBridge_FtpLet > The Exception was caused by a bad hdfs path!" );
				strReturnValue = "";
			}
			
		} catch (java.lang.NullPointerException e) {		

			System.out.println( "HdfsBridge_FtpLet > The Exception was caused by a bad hdfs path!" );
			strReturnValue = "";

		} catch (Exception e) {
			
			//System.out.println(e);
			e.printStackTrace();
			
		}
    		
		return strReturnValue;
    	
    }    
    

    
    
    
	@Override
	public void init(FtpletContext ftpletContext) throws FtpException {
		
		// setup the DFS connection
		getDfs();

		System.out.println("HdfsBridge_FtpLet > openPDC_Ftplet::init()");

		Logger l = Logger.getLogger(DefaultFtpHandler.class);
    	l.setLevel(Level.ALL);		
		
		
	}
	
    @Override
    public FtpletResult onConnect(FtpSession session) throws FtpException,
            IOException {
    	
        System.out.println("HdfsBridge_FtpLet > User connected to FtpServer!");
        
        return super.onConnect(session);
    }

    @Override
    public FtpletResult onDisconnect(FtpSession session) throws FtpException,
            IOException {
    	
        System.out.println("HdfsBridge_FtpLet > User Disconnected from FtpServer");
        
        return super.onDisconnect(session);
    }
    
    private String GetNow() {
    	
		DateFormat dfm = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
		Date a = new Date();
	
		return dfm.format(a);
    	
    	
    }

    @Override
    public FtpletResult beforeCommand(FtpSession session, FtpRequest request) throws FtpException, IOException {
    	
    	FtpletResult retVal = FtpletResult.DEFAULT;
    	String strRetMD5 = "";
    	long start_ts = 0;
    	long end_ts = 0;
    //	System.out.println("openPDC > beforeCommand: " + request.getCommand() );
    	
    	if ( request.getCommand().compareTo( "HDFSCHKSM" ) == 0 ) {
    		
    		retVal = FtpletResult.SKIP;
    		
    		System.out.println( this.GetNow() + " > HdfsBridge_FtpLet > RECD Custom Command - " + request.getCommand() + ", Argument: " + request.getArgument() );
    		
    		start_ts = System.currentTimeMillis();

    		try {
    			strRetMD5 = this.GetHDFSChecksumForFile( request.getArgument() );
    		} catch (Exception specialEx) {
    			System.out.println( "HDFSCHKSM Exception (what?): " + specialEx );
    		}
    		
    		end_ts = System.currentTimeMillis();
    		
    		System.out.println( this.GetNow() + " > HdfsBridge_FtpLet > HDFS Checksum Execution Time: " + (end_ts - start_ts) + " ms" );
	    		 
    		
			if ( strRetMD5.compareTo( DFS_MD5_NOT_DONE ) == 0 ) {
				
    			System.out.println( this.GetNow() + " > HdfsBridge_FtpLet > ERROR > Could not calculate checksum! Sending CHKSM Error Response Code Of " + FtpReply.REPLY_452_REQUESTED_ACTION_NOT_TAKEN );
    			
    			session.write( new DefaultFtpReply( FtpReply.REPLY_452_REQUESTED_ACTION_NOT_TAKEN, "HDFS Says 'Its Not Ready': " + request.getArgument() + " (Weaksauce)" ) );
				
				
			} else if ( strRetMD5.compareTo( "" ) == 0 ) {
    		
    			System.out.println( this.GetNow() + " > HdfsBridge_FtpLet > Sending CHKSM Error Response Code Of " + FtpReply.REPLY_450_REQUESTED_FILE_ACTION_NOT_TAKEN );
    			    			
    			session.write( new DefaultFtpReply( FtpReply.REPLY_450_REQUESTED_FILE_ACTION_NOT_TAKEN, "Bad Path Parameter: " + request.getArgument() + " (Weaksauce)" ) );
    			
    		} else {
    			
    			
    		
    		
    			System.out.println( this.GetNow() + " > HdfsBridge_FtpLet > Sending CHKSM Response Code Of " + FtpReply.REPLY_200_COMMAND_OKAY );

    			session.write( new DefaultFtpReply( FtpReply.REPLY_200_COMMAND_OKAY, strRetMD5 ) );
    			
    		}

			
    	} else if ( request.getCommand().compareTo( "HDFS_DEBUG" ) == 0 ) {
    		
    		String strReturnMsg = "(HdfsBridge_FtpLet > Testing Debug Stuff)";
    		retVal = FtpletResult.SKIP;
    		long lSleep = 100;
    		
    		try {
    			
    			lSleep = Long.parseLong( request.getArgument().trim() );

				Thread.sleep(lSleep);
				
				strReturnMsg = "HdfsBridge_FtpLet > slept " + lSleep + "ms";
				
			} catch (InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			} catch (Exception e) {
				e.printStackTrace();
				strReturnMsg = e.getMessage().substring(0, 100);
			}
    		
			System.out.println("HdfsBridge_FtpLet > Testing Debug Stuff: " + FtpReply.REPLY_200_COMMAND_OKAY );

			session.write( new DefaultFtpReply( FtpReply.REPLY_200_COMMAND_OKAY, strReturnMsg ) );
    		
			
			
    	} else if ( request.getCommand().compareTo( "HDFS_DEBUG_ON" ) == 0 ) {
    		
    		TurnOnHDFSDebugLogging();
    		retVal = FtpletResult.SKIP;
    		
			System.out.println("HdfsBridge_FtpLet > Turning On DFSClient Logging: " + FtpReply.REPLY_200_COMMAND_OKAY );
			session.write( new DefaultFtpReply( FtpReply.REPLY_200_COMMAND_OKAY, "(openPDC > Turning On DFSClient Logging)" ) );
    		
    		
    	} else if ( request.getCommand().compareTo( "HDFS_DEBUG_OFF" ) == 0 ) {
    		
    		TurnOffHDFSDebugLogging();
    		retVal = FtpletResult.SKIP;
    		
			System.out.println("HdfsBridge_FtpLet > Turning Off DFSClient Logging: " + FtpReply.REPLY_200_COMMAND_OKAY );
			session.write( new DefaultFtpReply( FtpReply.REPLY_200_COMMAND_OKAY, "(openPDC > Turning Off DFSClient Logging)" ) );
 

    	} else if ( request.getCommand().compareTo( "LAST_HDFS_UPLD" ) == 0 ) {
    		
    		retVal = FtpletResult.SKIP;
    		
			System.out.println("HdfsBridge_FtpLet > Get last hdfs upload: " + this.strLastFileUploadPath + "/" + this.strLastFileUploaded );
			session.write( new DefaultFtpReply( FtpReply.REPLY_200_COMMAND_OKAY, this.strLastFileUploadPath + "/" + this.strLastFileUploaded ) );
    		
    		
       	} else if ( request.getCommand().toLowerCase().compareTo("stor") == 0) {
       	 
       		// upload file
       		this.strLastFileUploaded = request.getArgument();
       		try {
       			FileSystemView fsv = session.getFileSystemView();
       			if ( null != fsv ) {
       				
       				//FtpFile fFile = session.getFileSystemView().getWorkingDirectory();
       				FileObject fFile = session.getFileSystemView().getCurrentDirectory();
       				
       				if ( null != fFile ) {
       				
       					//this.strLastFileUploadPath = session.getFileSystemView().getWorkingDirectory().getAbsolutePath();
       					this.strLastFileUploadPath = fFile.getFullName();
       					
       				} else {
       					
       					System.out.println( "HdfsBridge_FtpLet > [ERR] could not get session.getFileSystemView().getWorkingDirectory()!");
       				
       				}
       				
       			} else {
       				
       				System.out.println( "openPDC > [ERR] could not get session.getFileSystemView()!");
       				
       			}
       		} catch (Exception e) {
       			e.printStackTrace();
       		}
       		System.out.println("HdfsBridge_FtpLet >" + request.getCommand() );
       		System.out.println("arguments for STOR: " + request.getArgument() );
 
			
    	} else if ( request.getCommand().toLowerCase().compareTo("cwd") == 0) {
    		
    		// attempt to change working directory
    		System.out.println("HdfsBridge_FtpLet >" + request.getCommand() );

    	} else if ( request.getCommand().toLowerCase().compareTo("cdup") == 0) {
    		
    		// attempt to change to the parent directory
    		System.out.println("HdfsBridge_FtpLet >" + request.getCommand() );

    	} else if ( request.getCommand().toLowerCase().compareTo("nlst") == 0) {
    		
    		// get a directory listing
    		System.out.println("HdfsBridge_FtpLet >" + request.getCommand() );
						
    	} else {
    		
    		System.out.println("HdfsBridge_FtpLet > Command Other: '" + request.getCommand() + "'" );
    		
    	}
    	
    	
    	
    	return retVal;
    }
   

    
    
}
