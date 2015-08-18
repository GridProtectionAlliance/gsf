package gov.tva.openpdc.ftp.hdfs;

import org.apache.ftpserver.FtpServerConfigurationException;
import org.apache.ftpserver.ftplet.*;
import org.apache.ftpserver.usermanager.*;
import org.apache.ftpserver.util.BaseProperties;
import org.apache.ftpserver.util.IoUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.*;

public class AccountManager extends AbstractUserManager
{
	private final static Logger LOG = LoggerFactory.getLogger(AccountManager.class);
	private final static String DEPRECATED_PREFIX = "FtpServer.user.";
	private final static String CURRENT_PREFIX = "ftpserver.user.";
	
	private BaseProperties user_properties;
	private File user_file = new File("users.conf");
	private boolean configured = false;
	private PasswordEncryptor password_encryptor = new Md5PasswordEncryptor();

	public File getFile()
	{
		return user_file;
	}

	public void setFile(File user_file)
	{
		if (configured)
			throw new IllegalStateException("Must be called before configure()");

		this.user_file = user_file;
	}

	public PasswordEncryptor getPasswordEncryptor()
	{
		return password_encryptor;
	}

	public void setPasswordEncryptor(PasswordEncryptor password_encryptor)
	{
		this.password_encryptor = password_encryptor;
	}

	public void configure()
	{
		configured = true;
		FileInputStream fis = null;
		
		try
		{
			user_properties = new BaseProperties();

			if (user_file != null && user_file.exists())
			{
				try
				{
					fis = new FileInputStream(user_file);
					user_properties.load(fis);
					IoUtils.close(fis);
				}
				
				catch (IOException e)
				{
					throw new IOException("Error opening file");
				}
			}
		}
		
		catch (IOException e)
		{
			throw new FtpServerConfigurationException("Error loading user data file : " + user_file.getAbsolutePath(), e);
		}

		undeprecateProperties();
	}
	
	public synchronized void save(User user) throws FtpException
	{
		init();

		if (user.getName() == null)
			throw new NullPointerException("User name is null.");

		String prefix = CURRENT_PREFIX + user.getName() + '.';

		user_properties.setProperty(prefix + ATTR_PASSWORD, getPassword(user));

		String home = user.getHomeDirectory();
		if (home == null)
			home = "/";

		user_properties.setProperty(prefix + ATTR_HOME, home);
		user_properties.setProperty(prefix + ATTR_ENABLE, user.getEnabled());
		user_properties.setProperty(prefix + ATTR_WRITE_PERM, user.authorize(new WriteRequest()) != null);
		user_properties.setProperty(prefix + ATTR_MAX_IDLE_TIME, user.getMaxIdleTime());

		TransferRateRequest trr = new TransferRateRequest();
		trr = (TransferRateRequest)user.authorize(trr);

		if (trr != null)
		{
			user_properties.setProperty(prefix + ATTR_MAX_UPLOAD_RATE, trr.getMaxUploadRate());
			user_properties.setProperty(prefix + ATTR_MAX_DOWNLOAD_RATE, trr.getMaxDownloadRate());
		}
		
		else
		{
			user_properties.remove(prefix + ATTR_MAX_UPLOAD_RATE);
			user_properties.remove(prefix + ATTR_MAX_DOWNLOAD_RATE);
		}

		ConcurrentLoginRequest clr = new ConcurrentLoginRequest(0, 0);
		clr = (ConcurrentLoginRequest) user.authorize(clr);

		if (clr != null)
		{
			user_properties.setProperty(prefix + ATTR_MAX_LOGIN_NUMBER, clr.getMaxConcurrentLogins());
			user_properties.setProperty(prefix + ATTR_MAX_LOGIN_PER_IP, clr.getMaxConcurrentLoginsPerIP());
		}
		
		else
		{
			user_properties.remove(prefix + ATTR_MAX_LOGIN_NUMBER);
			user_properties.remove(prefix + ATTR_MAX_LOGIN_PER_IP);
		}

		saveData();
	}
	
	public synchronized void delete(String user_name) throws FtpException
	{
		init();

		String prefix = CURRENT_PREFIX + user_name + '.';
		Enumeration<?> properties = user_properties.propertyNames();
		ArrayList<String> keys = new ArrayList<String>();
		String key;
		
		while (properties.hasMoreElements())
		{
			key = properties.nextElement().toString();
			if (key.startsWith(prefix))
				keys.add(key);
		}
		
		Iterator<String> iterator = keys.iterator();
		
		while (iterator.hasNext())
			user_properties.remove(iterator.next());

		saveData();
	}

	public synchronized String[] getAllUserNames()
	{
		init();

		String suffix = '.' + ATTR_HOME;
		String key;
		ArrayList<String> list = new ArrayList<String>();
		Enumeration<?> keys = user_properties.propertyNames();
		
		while (keys.hasMoreElements())
		{
			key = keys.nextElement().toString();
			if (key.endsWith(suffix))
			{
				String name = key.substring(CURRENT_PREFIX.length());
				name = name.substring(0, name.length() - suffix.length());
				list.add(name);
			}
		}

		Collections.sort(list);
		return list.toArray(new String[0]);
	}
	
	public synchronized User getUserByName(String user_name)
	{
		init();

		if (!doesExist(user_name))
			return null;

		String prefix = CURRENT_PREFIX + user_name + '.';
		//HdfsUser user = new HdfsUser();
		Account user = new Account();
		user.setName(user_name);
		user.setEnabled(user_properties.getBoolean(prefix + ATTR_ENABLE, true));
		user.setHomeDirectory(user_properties.getProperty(prefix + ATTR_HOME, "/"));
		user.setGroups(parseGroups(user_properties.getProperty(prefix + "groups"), ","));

		List<Authority> authorities = new ArrayList<Authority>();

		if (user_properties.getBoolean(prefix + ATTR_WRITE_PERM, false))
			authorities.add(new WritePermission());

		authorities.add(new ConcurrentLoginPermission(user_properties.getInteger(prefix + ATTR_MAX_LOGIN_NUMBER, 0), user_properties.getInteger(prefix + ATTR_MAX_LOGIN_PER_IP, 0)));
		authorities.add(new TransferRatePermission(user_properties.getInteger(prefix + ATTR_MAX_DOWNLOAD_RATE, 0), user_properties.getInteger(prefix + ATTR_MAX_UPLOAD_RATE, 0)));

		user.setAuthorities(authorities.toArray(new Authority[0]));
		user.setMaxIdleTime(user_properties.getInteger(prefix + ATTR_MAX_IDLE_TIME, 0));

		return user;
	}
	
	public synchronized boolean doesExist(String name)
	{
		init();
		return user_properties.containsKey(CURRENT_PREFIX + name + '.' + ATTR_HOME);
	}
	
	public synchronized User authenticate(Authentication authentication) throws AuthenticationFailedException
	{
		init();

		if (authentication instanceof UsernamePasswordAuthentication)
		{
			UsernamePasswordAuthentication upa = (UsernamePasswordAuthentication) authentication;

			String user = upa.getUsername();
			String password = upa.getPassword();

			if (user == null)
				throw new AuthenticationFailedException("Authentication failed");

			if (password == null)
				password = "";

			String stored_password = user_properties.getProperty(CURRENT_PREFIX + user + '.' + ATTR_PASSWORD);

			if (stored_password == null)
				throw new AuthenticationFailedException("Authentication failed");

			if (password_encryptor.matches(password, stored_password))
				return getUserByName(user);
			
			else
				throw new AuthenticationFailedException("Authentication failed");

		}
		
		else if (authentication instanceof AnonymousAuthentication)
		{
			if (doesExist("anonymous"))
				return getUserByName("anonymous");
			
			else
				throw new AuthenticationFailedException("Authentication failed");
		}
		
		else
			throw new IllegalArgumentException("Authentication not supported by this user manager");
	}
	
	public synchronized void dispose()
	{
		if (user_properties != null)
		{
			user_properties.clear();
			user_properties = null;
		}
	}
	
	private void init()
	{
		if (!configured)
			configure();
	}

	private void undeprecateProperties()
	{
		Enumeration<?> keys = user_properties.propertyNames();

		boolean save = false;

		while (keys.hasMoreElements())
		{
			String key = keys.nextElement().toString();

			if (key.startsWith(DEPRECATED_PREFIX))
			{
				String newKey = CURRENT_PREFIX + key.substring(DEPRECATED_PREFIX.length());
				user_properties.setProperty(newKey, user_properties.getProperty(key));
				user_properties.remove(key);

				save = true;
			}
		}

		if (save)
		{
			try
			{
				saveData();
			}
			
			catch (FtpException e)
			{
				throw new FtpServerConfigurationException("Failed to save updated user data", e);
			}
		}
	}

	private void saveData() throws FtpException
	{
		File dir = user_file.getAbsoluteFile().getParentFile();
		if (dir != null && !dir.exists() && !dir.mkdirs())
		{
			String dirName = dir.getAbsolutePath();
			throw new FtpServerConfigurationException("Cannot create directory for user data file : " + dirName);
		}
		
		FileOutputStream fos = null;
		try
		{
			fos = new FileOutputStream(user_file);
			user_properties.store(fos, "Generated file - don't edit (please)");
		}
		
		catch (IOException ex)
		{
			LOG.error("Failed saving user data", ex);
			throw new FtpException("Failed saving user data", ex);
		}
		
		finally
		{
			IoUtils.close(fos);
		}
	}

	private String getPassword(User usr)
	{
		String name = usr.getName();
		String password = usr.getPassword();

		if (password != null)
			password = password_encryptor.encrypt(password);
		
		else
		{
			String blank = password_encryptor.encrypt("");

			if (doesExist(name))
			{
				String key = CURRENT_PREFIX + name + '.' + ATTR_PASSWORD;
				password = user_properties.getProperty(key, blank);
			}
			
			else
				password = blank;
		}
		
		return password;
	}

	private ArrayList<String> parseGroups(String groups, String delimiter)
	{
		String group_array[] = groups.split(delimiter);
		return new ArrayList<String>(Arrays.asList(group_array));
	}
}