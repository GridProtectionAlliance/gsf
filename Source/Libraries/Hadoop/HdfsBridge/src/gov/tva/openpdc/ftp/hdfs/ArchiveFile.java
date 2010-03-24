package gov.tva.openpdc.ftp.hdfs;

import org.apache.ftpserver.ftplet.FileObject;
import org.apache.ftpserver.ftplet.User;
import org.apache.hadoop.hdfs.DistributedFileSystem;
import org.apache.hadoop.fs.FSDataInputStream;
import org.apache.hadoop.fs.FSDataOutputStream;
import org.apache.hadoop.fs.FileStatus;
import org.apache.hadoop.fs.Path;
import org.apache.hadoop.fs.permission.FsPermission;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

/**
 * This class implements all actions to HDFS
 */
public class ArchiveFile implements FileObject
{

	private final Logger log = LoggerFactory.getLogger(ArchiveFile.class);

	private Path fsPath;
	private Account account;

	/**
	 * Constructs HdfsFileObject from path
	 * 
	 * @param path
	 *            path to represent object
	 * @param user
	 *            accessor of the object
	 */
	public ArchiveFile(String path, User user)
	{
		this.fsPath = new Path(path);
		this.account = (Account) user;

		System.out.println("ArchiveFile > ctor(): this.account = " + this.account.getName());
	}

	/**
	 * Get full name of the object
	 * 
	 * @return full name of the object
	 */
	public String getFullName()
	{
		return fsPath.toString();
	}

	/**
	 * Get short name of the object
	 * 
	 * @return short name of the object
	 */
	public String getShortName()
	{
		String full = getFullName();
		int pos = full.lastIndexOf("/");
		if (pos == 0)
			return "/";
		
		return full.substring(pos + 1);
	}

	/**
	 * HDFS has no hidden objects
	 * 
	 * @return always false
	 */
	public boolean isHidden()
	{
		return false;
	}

	/**
	 * Checks if the object is a directory
	 * 
	 * @return true if the object is a directory
	 */
	public boolean isDirectory()
	{
		try
		{
			log.debug("is directory? : " + fsPath);
			DistributedFileSystem dfs = HdfsBridge.getHdfs();
			FileStatus fs = dfs.getFileStatus(fsPath);
			return fs.isDir();
		}
		catch (IOException e)
		{
			log.debug(fsPath + " is not dir", e);
			return false;
		}
	}

	/**
	 * Get HDFS permissions
	 * 
	 * @return HDFS permissions as a FsPermission instance
	 * @throws IOException
	 *             if path doesn't exist so we get permissions of parent object
	 *             in that case
	 */
	private FsPermission getPermissions() throws IOException
	{
		
			
			
			DistributedFileSystem dfs = HdfsBridge.getHdfs();
/*			
			long cap = dfs.getRawCapacity();
			
			System.out.println( "\n\nDebugBridge > Capacity: " + cap );
			System.out.println( "\n\nDebugBridge > fsPath: " + fsPath );
			
			FsPermission p = dfs.getFileStatus(fsPath).getPermission();
			
			
			
			System.out.println( "\n\nDebugBridge > permissions: " + p.toString() );
			*/
			return dfs.getFileStatus(fsPath).getPermission();

	}

	/**
	 * Checks if the object is a file
	 * 
	 * @return true if the object is a file
	 */
	public boolean isFile()
	{
		try
		{
			DistributedFileSystem dfs = HdfsBridge.getHdfs();
			return dfs.isFile(fsPath);
		}
		catch (IOException e)
		{
			log.debug(fsPath + " is not file", e);
			return false;
		}
	}

	/**
	 * Checks if the object does exist
	 * 
	 * @return true if the object does exist
	 */
	public boolean doesExist()
	{
		try
		{
			DistributedFileSystem dfs = HdfsBridge.getHdfs();
			dfs.getFileStatus(fsPath);
			return true;
		}
		catch (IOException e)
		{
			// log.debug(path + " does not exist", e);
			return false;
		}
	}

	/**
	 * Checks if the user has a read permission on the object
	 * 
	 * @return true if the user can read the object
	 */
	public boolean hasReadPermission()
	{
		try
		{
			FsPermission permissions = getPermissions();
			if (account.getName().equals(getOwnerName()))
			{
				if (permissions.toString().substring(0, 1).equals("r"))
				{
					log.debug("PERMISSIONS: " + fsPath + " -  read allowed for user");
					return true;
				}
			}
			else if (account.isGroupMember(getGroupName()))
			{
				if (permissions.toString().substring(3, 4).equals("r"))
				{
					log.debug("PERMISSIONS: " + fsPath + " -  read allowed for group");
					return true;
				}
			}
			else
			{
				if (permissions.toString().substring(6, 7).equals("r"))
				{
					log.debug("PERMISSIONS: " + fsPath + " -  read allowed for others");
					return true;
				}
			}
			log.debug("PERMISSIONS: " + fsPath + " -  read denied");
			return false;
		}
		catch (IOException e)
		{
			e.printStackTrace(); // To change body of catch statement use File |
			// Settings | File Templates.
			return false;
		}
	}

	private ArchiveFile getParent()
	{
		String pathS = fsPath.toString();
		String parentS = "/";
		int pos = pathS.lastIndexOf("/");
		if (pos > 0)
			parentS = pathS.substring(0, pos);
		
		return new ArchiveFile(parentS, account);
	}

	/**
	 * Checks if the user has a write permission on the object
	 * 
	 * @return true if the user has write permission on the object
	 */
	public boolean hasWritePermission()
	{
		try
		{
			FsPermission permissions = getPermissions();
			if (account.getName().equals(getOwnerName()))
			{
				if (permissions.toString().substring(1, 2).equals("w"))
				{
					log.debug("PERMISSIONS: " + fsPath + " -  write allowed for user");
					return true;
				}
			}
			else if (account.isGroupMember(getGroupName()))
			{
				if (permissions.toString().substring(4, 5).equals("w"))
				{
					log.debug("PERMISSIONS: " + fsPath + " -  write allowed for group");
					return true;
				}
			}
			else
			{
				
				log.debug("\n\nHDFSBRIDGE > PERMISSIONS > len: " + permissions.toString().length() + " > " + permissions.toString() + "\n\n" );
				
				if (permissions.toString().substring(7, 8).equals("w"))
				{
					log.debug("PERMISSIONS: " + fsPath + " -  write allowed for others");
					return true;
				}
			}
			log.debug("PERMISSIONS: " + fsPath + " -  write denied");
			return false;
		}
		catch (IOException e)
		{
			return getParent().hasWritePermission();
		}
	}

	/**
	 * Checks if the user has a delete permission on the object
	 * 
	 * @return true if the user has delete permission on the object
	 */
	public boolean hasDeletePermission()
	{
		return hasWritePermission();
	}

	/**
	 * Get owner of the object
	 * 
	 * @return owner of the object
	 */
	public String getOwnerName()
	{
		try
		{
			DistributedFileSystem dfs = HdfsBridge.getHdfs();
			FileStatus fs = dfs.getFileStatus(fsPath);
			return fs.getOwner();
		}
		catch (IOException e)
		{
			e.printStackTrace();
			return null;
		}
	}

	/**
	 * Get group of the object
	 * 
	 * @return group of the object
	 */
	public String getGroupName()
	{
		try
		{
			DistributedFileSystem dfs = HdfsBridge.getHdfs();
			FileStatus fs = dfs.getFileStatus(fsPath);
			return fs.getGroup();
		}
		catch (IOException e)
		{
			e.printStackTrace();
			return null;
		}
	}

	/**
	 * Get link count
	 * 
	 * @return 3 is for a directory and 1 is for a file
	 */
	public int getLinkCount()
	{
		return isDirectory() ? 3 : 1;
	}

	/**
	 * Get last modification date
	 * 
	 * @return last modification date as a long
	 */
	public long getLastModified()
	{
		try
		{
			DistributedFileSystem dfs = HdfsBridge.getHdfs();
			FileStatus fs = dfs.getFileStatus(fsPath);
			return fs.getModificationTime();
		}
		catch (IOException e)
		{
			e.printStackTrace();
			return 0;
		}
	}

	/**
	 * Get a size of the object
	 * 
	 * @return size of the object in bytes
	 */
	public long getSize()
	{
		try
		{
			DistributedFileSystem dfs = HdfsBridge.getHdfs();
			FileStatus fs = dfs.getFileStatus(fsPath);
			log.info("getSize(): " + fsPath + " : " + fs.getLen());
			return fs.getLen();
		}
		catch (IOException e)
		{
			e.printStackTrace();
			return 0;
		}
	}

	/**
	 * Create a new dir from the object
	 * 
	 * @return true if dir is created
	 */
	public boolean mkdir()
	{

		if (!hasWritePermission())
		{
			log.debug("No write permission : " + fsPath);
			return false;
		}

		try
		{
			DistributedFileSystem dfs = HdfsBridge.getHdfs();
			dfs.mkdirs(fsPath);
			dfs.setOwner(fsPath, account.getName(), account.getMainGroup());
			return true;
		}
		catch (IOException e)
		{
			e.printStackTrace();
			return false;
		}
	}

	/**
	 * Delete object from the HDFS filesystem
	 * 
	 * @return true if the object is deleted
	 */
	public boolean delete()
	{
		try
		{
			DistributedFileSystem dfs = HdfsBridge.getHdfs();
			dfs.delete(fsPath, true);
			return true;
		}
		catch (IOException e)
		{
			e.printStackTrace();
			return false;
		}
	}

	/**
	 * Move the object to another location
	 * 
	 * @param fileObject
	 *            location to move the object
	 * @return true if the object is moved successfully
	 */
	public boolean move(FileObject fileObject)
	{
		try
		{
			DistributedFileSystem dfs = HdfsBridge.getHdfs();
			dfs.rename(fsPath, new Path(fileObject.getFullName()));
			return true;
		}
		catch (IOException e)
		{
			e.printStackTrace();
			return false;
		}
	}

	/**
	 * List files of the directory
	 * 
	 * @return List of files in the directory
	 */
	public FileObject[] listFiles()
	{

		if (!hasReadPermission())
		{
			log.debug("No read permission : " + fsPath);
			return null;
		}

		try
		{
			DistributedFileSystem dfs = HdfsBridge.getHdfs();
			FileStatus fileStats[] = dfs.listStatus(fsPath);

			FileObject fileObjects[] = new FileObject[fileStats.length];
			for (int i = 0; i < fileStats.length; i++)
			{
				fileObjects[i] = new ArchiveFile(fileStats[i].getPath()
						.toString(), account);
			}
			return fileObjects;
		}
		catch (IOException e)
		{
			log.debug("", e);
			return null;
		}
	}

	/**
	 * Creates output stream to write to the object
	 * 
	 * @param l
	 *            is not used here
	 * @return OutputStream
	 * @throws IOException
	 */
	public OutputStream createOutputStream(long l) throws IOException
	{

		// permission check
		if (!hasWritePermission())
			throw new IOException("No write permission : " + fsPath);

		try
		{
			DistributedFileSystem dfs = HdfsBridge.getHdfs();
			FSDataOutputStream out = dfs.create(fsPath);
			//dfs.setOwner(fsPath, account.getName(), account.getMainGroup());
			dfs.setOwner(fsPath, account.getName(), account.getMainGroup());
			return out;
		}
		catch (IOException e)
		{
			e.printStackTrace();
			return null;
		}
	}

	/**
	 * Creates input stream to read from the object
	 * 
	 * @param l
	 *            is not used here
	 * @return OutputStream
	 * @throws IOException
	 */
	public InputStream createInputStream(long l) throws IOException
	{
		// permission check
		if (!hasReadPermission())
			throw new IOException("No read permission : " + fsPath);
		
		try
		{
			DistributedFileSystem dfs = HdfsBridge.getHdfs();
			FSDataInputStream in = dfs.open(fsPath);
			return in;
		}
		catch (IOException e)
		{
			e.printStackTrace();
			return null;
		}
	}
}
