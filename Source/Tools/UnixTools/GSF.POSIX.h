//******************************************************************************************************
//  GSF.POSIX.h - Gbtc
//
//  Copyright © 2014, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  09/14/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

// Structure used to return key spwd information
struct UserPasswordInformation
{
	// Do not include any pointer types in this structure
	// to keep marshaling into .NET simple
	long lastChangeDate;
	long minDaysForChange;
	long maxDaysForChange;
	long warningDays;
	long inactivityDays;
	long accountExpirationDate;
};

int AuthenticateUser(const char* userName, const char* password);

int ChangeUserPassword(const char* userName, const char* oldPassword, const char* newPassword);

int GetLocalUserID(const char* userName, /*out*/ unsigned int* uid);

int GetLocalUserPrimaryGroupID(const char* userName, /*out*/ unsigned int* gid);

// Preallocate outbound userName to 256 characters
int GetLocalUserName(unsigned int uid, /*out*/ char* userName);

char* GetLocalUserHomeDirectory(const char* userName);

char* GetLocalUserGecos(const char* userName);

#ifdef __APPLE__

int GetLocalUserPasswordInformation(const char* userName, /*out*/ int* lastChangeDate, /*out*/ int* maxDaysForChange, /*out*/ int* accountExpirationDate);

#else

// Values for status parameter:
//      1 -- "*"        Account is disabled
//      2 -- "!<...>"   The account locked out (e.g., with passwd -l <username>)
//      2 -- "!"        The password expired / never set - effectively locked out
//      2 -- "!!"       The password expired / never set - effectively locked out
//      3 -- ""         No password defined
//      0 -- "<else>"   Account assumed normal (encrypted password)
int GetLocalUserPasswordInformation(const char* userName, struct UserPasswordInformation* userPasswordInfo, /*out*/ int* status);

#endif

int SetLocalUserPassword(const char* userName, const char* password, const char* salt);

char* GetPasswordHash(const char* password, const char* salt);

int GetLocalUserGroupCount(const char* userName);

// Preallocate outbound groupIDs as an unsigned integer array sized from GetLocalUserGroupCount
int GetLocalUserGroupIDs(const char* userName, int groupCount, /*out*/ gid_t** groupIDs);

int GetLocalGroupID(const char* groupName, /*out*/ unsigned int* gid);

// Preallocate outbound groupName to 256 characters
int GetLocalGroupName(unsigned int gid, /*out*/ char* groupName);

int GetLocalGroupMembers(const char* groupName, /*out*/ char*** groupMembers);

void FreeLocalGroupMembers(char** groupMembers);