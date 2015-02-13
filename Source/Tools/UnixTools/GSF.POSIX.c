//******************************************************************************************************
//  GSF.POSIX.c - Gbtc
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
//  08/29/2014 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

// This will create a library and shared object for needed GSF functions

//  Compiling for Linux:
//
//	Get PAM development libraries, e.g., for Ubuntu: apt-get install libpam0g-dev
//
//	To compile to an object library
//      gcc -c -Wall -Werror -fpic GSF.POSIX.c
//
//	To compile as a shared object
//      gcc -shared -o GSF.POSIX.so GSF.POSIX.o -lpam -lpam_misc -lcrypt
//
//	To deploy shared object, e.g., copy to user libraries folder
//		sudo cp GSF.POSIX.so /usr/lib/
//

#include <stdlib.h>
#include <security/pam_appl.h>
#include <sys/types.h>
#include <stdio.h>
#include <string.h>
#include <paths.h>
#include <pwd.h>
#include <grp.h>

#ifdef __APPLE__
    #include <security/openpam.h>
    #include <uuid/uuid.h>
    #include <unistd.h>
#else
    #include <security/pam_misc.h>
    #include <shadow.h>
    #include <crypt.h>
#endif

#include "GSF.POSIX.h"

// Define custom PAM conversation function for authentication
int AuthenticateConversation(int num_msg, const struct pam_message** msg, struct pam_response** resp, void* appdata_ptr)
{
    const struct pam_message* message = *msg;

    if (num_msg == 1 && message->msg != NULL && message->msg_style == PAM_PROMPT_ECHO_OFF)
    {
        // Provide password for the PAM conversation response that was passed into appdata_ptr
        struct pam_response* reply = (struct pam_response*)malloc(sizeof(struct pam_response));
        reply[0].resp = (char*)appdata_ptr;
        reply[0].resp_retcode = 0;

        *resp = reply;

        return PAM_SUCCESS;
    }

    return PAM_CONV_ERR;
}

struct ChangePasswordInformation
{
    char* userName;         // User name
    char* oldPassword;      // Old password
    char* newPassword1;     // Initial request for new password
    char* newPassword2;     // Confirmation request for new password
    int requestCount;       // Always initialize to zero
};

// Define custom PAM conversation function for changing a password
int ChangePasswordConversation(int num_msg, const struct pam_message** msg, struct pam_response** resp, void* appdata_ptr)
{
    const struct pam_message* message = *msg;

    if (num_msg == 1 && message->msg != NULL && (message->msg_style == PAM_PROMPT_ECHO_ON || message->msg_style == PAM_PROMPT_ECHO_OFF))
    {
        // Initialize the PAM conversation response
        struct pam_response* reply = (struct pam_response*)malloc(sizeof(struct pam_response));
        reply[0].resp_retcode = 0;

        // Dereference change password information structure
        struct ChangePasswordInformation* info = (struct ChangePasswordInformation*)appdata_ptr;

        if (message->msg_style == PAM_PROMPT_ECHO_ON)
        {
            // PAM_PROMPT_ECHO_ON request is for user name
            reply[0].resp = info->userName;
        }
        else
        {
            // PAM_PROMPT_ECHO_OFF requests are for passwords, starting with old password
            switch (info->requestCount)
            {
                case 0:
                    reply[0].resp = info->oldPassword;
                    break;
                case 1:
                    reply[0].resp = info->newPassword1;
                    break;
                case 2:
                    reply[0].resp = info->newPassword2;
                    break;
                default:
                    reply[0].resp = NULL;
                    break;
            }

            info->requestCount++;
        }

        *resp = reply;

        return PAM_SUCCESS;
    }

    return PAM_CONV_ERR;
}

int AuthenticateUser(const char* userName, const char* password) 
{
    // Set up a custom PAM conversation passing in authentication password
    char* appdata = strdup(password);
    struct pam_conv pamc = { AuthenticateConversation, appdata };
    pam_handle_t* pamh; 
    int retval;

    // Start PAM
    retval = pam_start("login", userName, &pamc, &pamh);

    if (retval == PAM_SUCCESS)
    {
        // Authenticate the user
        retval = pam_authenticate(pamh, 0);

        // All done
        pam_end(pamh, 0); 
    }
    else
    {
        free(appdata);
    }

    return retval;
}

int ChangeUserPassword(const char* userName, const char* oldPassword, const char* newPassword)
{
    // Set up a custom PAM conversation passing in needed information to change the user's password
    struct ChangePasswordInformation appdata;

    // Have to create copies of these strings as PAM frees them after every conversation request
    appdata.userName = strdup(userName);
    appdata.oldPassword = strdup(oldPassword);
    appdata.newPassword1 = strdup(newPassword);
    appdata.newPassword2 = strdup(newPassword);
    appdata.requestCount = 0;

    struct pam_conv pamc = { ChangePasswordConversation, &appdata };
    pam_handle_t* pamh; 
    int retval;

    // Start PAM
    retval = pam_start("login", userName, &pamc, &pamh);

    if (retval == PAM_SUCCESS)
    {
        // Begin PAM change password conversation
        if (retval == PAM_SUCCESS)
            retval = pam_chauthtok(pamh, PAM_SILENT);

        // All done
        pam_end(pamh, 0); 
    }
    else
    {
        free(appdata.userName);
    }

    if (appdata.requestCount < 3)
        free(appdata.newPassword2);

    if (appdata.requestCount < 2)
        free(appdata.newPassword1);

    if (appdata.requestCount < 1)
        free(appdata.oldPassword);

    return retval;
}

int GetLocalUserID(const char* userName, /*out*/ unsigned int* uid)
{
    struct passwd* pwd = getpwnam(userName);

    if (pwd == NULL)
    {
        *uid = 0;
        return 1;
    }

    *uid = pwd->pw_uid;
    return 0;
}

int GetLocalUserPrimaryGroupID(const char* userName, /*out*/ unsigned int* gid)
{
    struct passwd* pwd = getpwnam(userName);

    if (pwd == NULL)
    {
        *gid = 0;
        return 1;
    }

    *gid = pwd->pw_gid;
    return 0;
}

// Preallocate outbound userName to 256 characters
int GetLocalUserName(unsigned int uid, /*out*/ char* userName)
{
    struct passwd* pwd = getpwuid((uid_t)uid);

    if (pwd == NULL)
        return 1;

    strcpy(userName, pwd->pw_name);

    return 0;
}

char* GetLocalUserHomeDirectory(const char* userName)
{
    struct passwd* pwd = getpwnam(userName);

    if (pwd == NULL)
        return NULL;

    return pwd->pw_dir;
}

char* GetLocalUserGecos(const char* userName)
{
    struct passwd* pwd = getpwnam(userName);

    if (pwd == NULL)
        return NULL;

    return pwd->pw_gecos;
}

#ifdef __APPLE__

int GetLocalUserPasswordInformation(const char* userName, /*out*/ int* lastChangeDate, /*out*/ int* maxDaysForChange, /*out*/ int* accountExpirationDate)
{
    struct passwd* pwd = getpwnam(userName);

    if (pwd == NULL)
        return 1;

    *lastChangeDate = pwd->pw_change;
    *maxDaysForChange = 0;
    *accountExpirationDate = pwd->pw_expire;

    if (*lastChangeDate == 0 && *accountExpirationDate == 0)
    {
        // Apple account has password aging disabled, apply Linux style defaults
        *maxDaysForChange = 99999;
        *accountExpirationDate = -1;
    }

    return 0;
}

#else

// Values for status parameter:
//      1 -- "*"        Account is disabled
//      2 -- "!<...>"   The account locked out (e.g., with passwd -l <username>)
//      2 -- "!"        The password expired / never set - effectively locked out
//      2 -- "!!"       The password expired / never set - effectively locked out
//      3 -- ""         No password defined
//      0 -- "<else>"   Account assumed normal (encrypted password)
int GetLocalUserPasswordInformation(const char* userName, struct UserPasswordInformation* userPasswordInfo, /*out*/ int* status)
{
    struct spwd* sp = getspnam(userName);

    if (sp == NULL)
        return 1;

    userPasswordInfo->lastChangeDate = sp->sp_lstchg;
    userPasswordInfo->minDaysForChange = sp->sp_min;
    userPasswordInfo->maxDaysForChange = sp->sp_max;
    userPasswordInfo->warningDays = sp->sp_warn;
    userPasswordInfo->inactivityDays = sp->sp_inact;
    userPasswordInfo->accountExpirationDate = sp->sp_expire;

    if (sp->sp_pwdp == NULL)
    {
        *status = 3;
    }
    else
    {
        // Determine account password status
        int passwordLen = strlen(sp->sp_pwdp);

        if (passwordLen > 0)
        {
            char firstChar = sp->sp_pwdp[0];

            if (passwordLen == 1 && firstChar == '*')
                *status = 1;
            else if (firstChar == '!')
                *status = 2;
            else
                *status = 0;
        }
        else
        {
            *status = 3;
        }
    }

    return 0;
}

#endif

int SetLocalUserPassword(const char* userName, const char* password, const char* salt)
{
#ifdef __APPLE__
    return 1;
#else
    struct passwd* pwd = getpwnam(userName);

    if (pwd == NULL)
        return 1;

    // Not allowing password to be set on root
    if (pwd->pw_uid == 0)
        return 1;

    int retval = 1;

    struct spwd* sp = getspnam(userName);

    if (sp != NULL)
    {
        if (lckpwdf() == 0)
        {
            FILE* shadow = fopen(_PATH_SHADOW, "w");

            if (shadow != NULL)
            {
                sp->sp_pwdp = crypt(password, salt);
                retval = putspent(sp, shadow);
                fclose(shadow);
            }

            ulckpwdf();
        }
    }

    return retval;
#endif
}

char* GetPasswordHash(const char* password, const char* salt)
{
    return crypt(password, salt);
}

int GetLocalUserGroupCount(const char* userName)
{
    struct passwd* pwd = getpwnam(userName);

    if (pwd == NULL)
        return -1;

    gid_t groupIDs[1];
    int groupCount = 0;

#ifdef __APPLE__
    if (getgrouplist(userName, pwd->pw_gid, (int*)groupIDs, &groupCount) == -1)
        return groupCount;
#else
    if (getgrouplist(userName, pwd->pw_gid, groupIDs, &groupCount) == -1)
        return groupCount;
#endif

    return -1;
}

// Preallocate outbound groupIDs as an unsigned integer array sized from GetLocalUserGroupCount
int GetLocalUserGroupIDs(const char* userName, int groupCount, /*out*/ gid_t** groupIDs)
{
    struct passwd* pwd = getpwnam(userName);

    if (pwd == NULL)
        return 1;

#ifdef __APPLE__
    getgrouplist(userName, pwd->pw_gid, (int*)*groupIDs, &groupCount);
#else
    getgrouplist(userName, pwd->pw_gid, *groupIDs, &groupCount);
#endif

    return 0;
}

int GetLocalGroupID(const char* groupName, /*out*/ unsigned int* gid)
{
    struct group* grp = getgrnam(groupName);

    if (grp == NULL)
    {
        *gid = 0;
        return 1;
    }

    *gid = grp->gr_gid;
    return 0;
}

// Preallocate outbound groupName to 256 characters
int GetLocalGroupName(unsigned int gid, /*out*/ char* groupName)
{
    struct group* grp = getgrgid((gid_t)gid);

    if (grp == NULL)
        return 1;

    strcpy(groupName, grp->gr_name);

    return 0;
}

int GetLocalGroupMembers(const char* groupName, /*out*/ char*** groupMembers)
{
    struct group* grp;
    struct passwd* pwd;
    int i, grpCount = 0, pwdCount = 0;

    // Get secondary group members
    grp = getgrnam(groupName);

    if (grp == NULL)
        return 1;

    // Count number of secondary group members
    for (i = 0; grp->gr_mem[i] != NULL; i++)
        grpCount++;

    // Count number of primary group members
    while ((pwd = getpwent()) != NULL)
    {
        if (pwd->pw_gid == grp->gr_gid)
            pwdCount++;
    }

    *groupMembers = (char**)malloc(sizeof(char*) * (grpCount + pwdCount + 1));
    char** memberList = *groupMembers;

    if (memberList != NULL)
    {
        // Add secondary group members
        for (i = 0; i < grpCount; i++)
            memberList[i] = strdup(grp->gr_mem[i]);

        if (pwdCount > 0)
        {
            // Move back to beginning of passwd file
            setpwent();

            // Add primary group members
            while ((pwd = getpwent()) != NULL)
            {
                if (pwd->pw_gid == grp->gr_gid)
                    memberList[i++] = strdup(pwd->pw_name);
            }
        }

        memberList[i] = NULL;
    }

    endpwent();
    return 0;
}

void FreeLocalGroupMembers(char** groupMembers)
{
    if (groupMembers != NULL)
    {
        int i;

        for (i = 0; groupMembers[i] != NULL; i++)
            free(groupMembers[i]);

        free(groupMembers);  
    }
}
