package gov.tva.openpdc.ftp.hdfs;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;

import org.apache.ftpserver.ftplet.Authority;
import org.apache.ftpserver.ftplet.AuthorizationRequest;
import org.apache.ftpserver.ftplet.User;

import org.apache.log4j.Logger;

public class Account implements User, Serializable {
	
	private static final long serialVersionUID = -47371353779731294L;
	
	private String _UserName = null;
	private String _Password = null;
	
	private int maxIdleTimeSec = 0; // no limit
	private String homeDir = null;
	private boolean isEnabled = true;
	private Authority[] authorities = new Authority[0];
	private ArrayList<String> groups = new ArrayList<String>();
	private Logger log = Logger.getLogger(Account.class);

	public Account() {
	}

	public Account(User user) {

		this._UserName = user.getName();
		this._Password = user.getPassword();
				
		this.authorities = user.getAuthorities();
		this.maxIdleTimeSec = user.getMaxIdleTime();
		this.homeDir = user.getHomeDirectory();
		this.isEnabled = user.getEnabled();

	}
	
	public ArrayList<String> GetUserGroups() {
		return groups;
	}

	
	public String getMainGroup() {
		if (groups.size() > 0) {
			return groups.get(0);
		} else {
			log.error("User " + this._UserName + " is not a memer of any group");
			return "error";
		}
	}

/**
 * Checks if user is a member of the group
 *
 * @param group to check
 * @return true if the user id a member of the group
 */
	public boolean isGroupMember(String group) {
		for (String userGroup : groups) {
			if (userGroup.equals(group)) {
				return true;
			}
		}
		return false;
	}

/**
 * Set users' groups
 *
 * @param groups to set
 */
	public void setGroups(ArrayList<String> groups) {
		if (groups.size() < 1) {
			log.error("User " + this._UserName + " is not a memer of any group");
		}
		this.groups = groups;
	}

/**
 * Get the user name.
 */
	public String getName() {
		return this._UserName;
	}

/**
 * Set user name.
 */
	public void setName(String user_name) {
		this._UserName = user_name;
	}

/**
 * Get the user password.
 */
	public String getPassword() {
		return this._Password;
	}

/**
 * Set user password.
 */
	public void setPassword(String password) {
		this._Password = password;
	}

	public Authority[] getAuthorities() {
		if (authorities != null) {
			return authorities.clone();
		} else {
			return null;
		}
	}
	
	public void setAuthorities(Authority[] authorities) {
		if (authorities != null) {
			this.authorities = authorities.clone();
		} else {
			this.authorities = null;
		}
	}

	
/**
 * Get the maximum idle time in second.
 */
	public int getMaxIdleTime() {
		return maxIdleTimeSec;
	}

/**
 * Set the maximum idle time in second.
 */
	public void setMaxIdleTime(int idleSec) {
		maxIdleTimeSec = idleSec;
		if (maxIdleTimeSec < 0) {
			maxIdleTimeSec = 0;
		}
	}

/**
 * Get the user enable status.
 */
	public boolean getEnabled() {
		return isEnabled;
	}

/**
 * Set the user enable status.
 */
	public void setEnabled(boolean enb) {
		isEnabled = enb;
	}

/**
 * Get the user home directory.
 */
	public String getHomeDirectory() {
		return homeDir;
	}

/**
 * Set the user home directory.
 */
	public void setHomeDirectory(String home) {
		homeDir = home;
	}

/**
 * String representation.
 */
	public String toString() {
		return this._UserName;
	}
	
	/**
	 * {@inheritDoc}
	 */
	public AuthorizationRequest authorize(AuthorizationRequest request) {
		
		Authority[] authorities = getAuthorities();
		
		    // check for no authorities at all
		if (authorities == null) {
			return null;
		}
		
		boolean someoneCouldAuthorize = false;
		
		for (int i = 0; i < authorities.length; i++) {
			
			Authority authority = authorities[i];
			
			if (authority.canAuthorize(request)) {

				someoneCouldAuthorize = true;
			
				request = authority.authorize(request);
			
			    // authorization failed, return null
				if (request == null) {
					return null;
				}
			}
		
		}
		
		if (someoneCouldAuthorize) {
		return request;
		} else {
		return null;
		}
	}

/**
 * {@inheritDoc}
 */
	public Authority[] getAuthorities(Class<? extends Authority> clazz) {
		
		List<Authority> selected = new ArrayList<Authority>();
		
		for (int i = 0; i < authorities.length; i++) {
			if (authorities[i].getClass().equals(clazz)) {
				selected.add(authorities[i]);
			}
		}
			
		return selected.toArray(new Authority[0]);
	}

}
