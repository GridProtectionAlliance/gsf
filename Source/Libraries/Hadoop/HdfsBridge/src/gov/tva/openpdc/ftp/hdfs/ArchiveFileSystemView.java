package gov.tva.openpdc.ftp.hdfs;

import org.apache.ftpserver.ftplet.FileObject;
import org.apache.ftpserver.ftplet.FileSystemView;
import org.apache.ftpserver.ftplet.FtpException;
import org.apache.ftpserver.ftplet.User;

/**
 * Implemented FileSystemView to use HdfsFileObject
 */
public class ArchiveFileSystemView implements FileSystemView
{
	// the root directory will always end with '/'.
	private String rootDir = "/";

	// the first and the last character will always be '/'
	// It is always with respect to the root directory.
	private String currDir = "/";

	private User account;

	/**
	 * Constructor - set the user object.
	 */
	protected ArchiveFileSystemView(User user) throws FtpException
	{
		if (user == null)
			throw new IllegalArgumentException("user can not be null");
		if (user.getHomeDirectory() == null)
			throw new IllegalArgumentException("User home directory can not be null");

		// add last '/' if necessary
		String rootDir = user.getHomeDirectory();

		if (!rootDir.endsWith("/"))
			rootDir += '/';
		this.rootDir = rootDir;

		this.account = user;
	}

	/**
	 * Get the user home directory. It would be the file system root for the
	 * user.
	 */
	public FileObject getHomeDirectory()
	{
		return new ArchiveFile("/", account);
	}

	/**
	 * Get the current directory.
	 */
	public FileObject getCurrentDirectory()
	{
		return new ArchiveFile(currDir, account);
	}

	/**
	 * Get file object.
	 */
	public FileObject getFileObject(String file)
	{
		String path;
		if (file.startsWith("/"))
			path = file;
		else if (currDir.length() > 1)
			path = currDir + "/" + file;
		else
			path = "/" + file;
		
		return new ArchiveFile(path, account);
	}

	/**
	 * Change directory.
	 */
	public boolean changeDirectory(String dir)
	{
		String path;
		if (dir.startsWith("/"))
		{
			path = dir;
		} else if (currDir.length() > 1)
		{
			path = currDir + "/" + dir;
		} else
		{
			path = "/" + dir;
		}
		ArchiveFile file = new ArchiveFile(path, account);
		if (file.isDirectory() && file.hasReadPermission())
		{
			currDir = path;
			return true;
		} else
		{
			return false;
		}
	}

	/**
	 * Is the file content random accessible?
	 */
	public boolean isRandomAccessible()
	{
		return true;
	}

	/**
	 * Dispose file system view - does nothing.
	 */
	public void dispose()
	{
	}
}
