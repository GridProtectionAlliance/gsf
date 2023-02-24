//******************************************************************************************************
//  Login.js - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/13/2018 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

// The following constants need to be predefined before referencing this script
//   string redirectPage: target page after successful authentication
//   string securePage: secure page to hit to test authentication
//   boolean useAlternateSecurityProvider: indicate whether the alternate securityProvider should be used
//   string loginPage: location of login page
//   string clearCredentialsPage: page used to clear current credentials
//   boolean unsecureConnection: flag that indicates if connection is secure
//   string verificationHeader: HTTP header name used to transmit anti-forgery value
//   string verificationValue: server generated anti-forgery verification token value
//   string useAjaxVerification: HTTP header named use to indicate an AJAX post
//   string redirectPageLabel: user label for redirect location, e.g., main or target
//   string oidcError: error Message from OIDC provider if en error occurred
//   boolean isPOSIX: flag that indicates if host system is POSIX based, e.g., Linux or OSX
//   boolean azureADAuthEnabled: flag that indicates if Azure AD authentication is enabled
//   boolean logoutPageRequested: flag that indicates if logout page was requested
//   boolean msalUseRedirect: flag that indicates if MSAL should use redirect flow
//   json msalConfig: MSAL configuration object

let msalInstance;
let msalLoading = msalUseRedirect;

function loadSettings() {
    $("#username").val(persistentStorage.getItem("username")).trigger("input");
    $("#iwa").prop("checked", !isPOSIX && persistentStorage.getItem("iwa") === "true").change();
    $("#remember").prop("checked", !isPOSIX && persistentStorage.getItem("remember") === "true").change();
    $("#ntlm").prop("checked", !isIE && !isPOSIX && persistentStorage.getItem("ntlm") !== "false").change();

    if (!azureADAuthEnabled)
        return;

    msalInstance = new msal.PublicClientApplication(msalConfig);

    if (!msalUseRedirect)
        return;

    msalInstance.handleRedirectPromise()
        .then(response => {
            msalLoading = false;

            if (response)
                msalAuthResponse(response);
        })
        .catch(error => {
            msalLoading = false;
            loginComplete(false, "Login attempt failed: " + error);
        });
}

function saveSettings() {
    persistentStorage.setItem("username", $("#username").val());
    persistentStorage.setItem("iwa", $("#iwa").prop("checked"));
    persistentStorage.setItem("remember", $("#remember").prop("checked"));
    persistentStorage.setItem("ntlm", $("#ntlm").prop("checked"));
}

function hashPassword(password) {
    return CryptoJS.SHA256(password + "0").toString(CryptoJS.enc.Base64);
}

// Authorize user with basic authentication using provided credentials
function authenticateBasic(username, password, skipHash) {
    $.ajax({
        cache: false,
        url: securePage + (useAlternateSecurityProvider ? "?useAlternate=1": ""),
        method: "post",
        complete: function (xhr) {
            switch (xhr.status) {
                case 200:
                    loginComplete(true);
                    break;
                default:
                    loginFailed(xhr.statusCode());
                    break;
            }
        },
        beforeSend: function (xhr) {            
            if (username.indexOf("\\") <= 0 && !skipHash)
                password = hashPassword(password);

            xhr.setRequestHeader("Authorization", "Basic " + btoa(username + ":" + password));
            xhr.setRequestHeader(verificationHeader, verificationValue);
            xhr.setRequestHeader(useAjaxVerification, "true");
        }
    });
}

// Authorize user with NTLM authentication using provided domain credentials
function authenticateNTLM(username, password) {
    const parts = username.split("\\");

    Ntlm.setCredentials(parts[0], parts[1], password);

    Ntlm.authenticate(securePage + "?scheme=NTLM" + (useAlternateSecurityProvider ? "&useAlternate=1" : ""), function (response) {
        if (response.status === 200)
            loginComplete(true);
        else
            loginFailed(response);
    },
    verificationHeader, verificationValue, useAjaxVerification);
}

// Authorize user with pass-through basic authentication without credentials
// using pre-authorized authentication token cookie with remember me option
function passthroughBasic() {
    $.ajax({
        cache: false,
        url: securePage + (useAlternateSecurityProvider ? "?useAlternate=1" : ""),
        method: "post",
        complete: function (xhr) {
            switch (xhr.status) {
                case 200:
                    loginComplete(true);
                    break;
                default:
                    loginFailed(xhr.statusCode());
                    break;
            }
        },
        beforeSend: function (xhr) {
            xhr.setRequestHeader(verificationHeader, verificationValue);
            xhr.setRequestHeader(useAjaxVerification, "true");
        }
    });
}

// Authorize user with pass-through NTLM (or negotiated) authentication without credentials,
// NTLM implementation on some browsers may prompt for credentials anyway
function passthroughNTLM() {
    $.ajax({
        cache: false,
        url: securePage + "?scheme=NTLM" + (useAlternateSecurityProvider ? "&useAlternate=1" : ""),
        method: "post",
        complete: function (xhr) {
            if (xhr.status === 200) {
                loginComplete(true);
            }
            else {
                const currentIdentity = xhr.getResponseHeader("CurrentIdentity");

                if (currentIdentity)
                    loginComplete(false, "No access available for \"" + currentIdentity + "\" using pass-through authentication.");
                else
                    loginComplete(false, "Current identity unavailable, cannot attempt pass-through authentication - check database configuration.");
            }
        },
        beforeSend: function (xhr) {
            xhr.setRequestHeader(verificationHeader, verificationValue);
            xhr.setRequestHeader(useAjaxVerification, "true");
        }
    });
}

function loginComplete(success, response) {
    if (success) {
        $("#response").text("Authentication succeeded, loading " + redirectPageLabel + " page...");

        if ($("#remember").prop("checked"))
            persistentStorage.setItem("passthrough", "true");

        setTimeout(function () {
            window.location = redirectPage;
        }, 500);
    }
    else {
        if (oidcError.length > 0)
            $("#response").text("Single Sign On Failed");
        else
            $("#response").text("Enter Credentials:");

        $("#workingIcon").hide();
        $("#credentialsForm").show();

        if (response) {
            $("#message").html(response);
            $("#responsePanel").show();
        }

        $("#username").focus();
        $("#username").select();
        $("#login").enable();
        $("#msalAuth").enable();
    }
}

function logoutComplete(success, response) {
    // Allow a moment for screen to complete refresh before displaying logout results
    setTimeout(function () {
        const sessionClearedParameter = getParameterByName("sessionCleared");

        if (sessionClearedParameter && getBool(sessionClearedParameter)) {
            if (success) {
                $("#response").text("Logout Succeeded");
            }
            else {
                $("#response").text("Partial Logout - Session Only");
                $("#message").text("Client session cache cleared but failed to clear browser cached credentials");
                $("#responsePanel").show();
            }
        }
        else if (response) {
            if (success && response === "Logout complete") {
                $("#response").text("Logout Succeeded");
            }
            else if (success) {
                $("#response").text("Partial Logout - Browser Credentials Only");
                $("#message").text("Cleared browser cached credentials but failed to clear client session cache");
                $("#responsePanel").show();
            }
            else {
                $("#response").text("Failed to Logout");
                $("#message").text("Failed to clear client session cache and failed to clear browser cached credentials");
                $("#responsePanel").show();
            }
        }
        else {
            if (success) {
                $("#response").text("Partial Logout - Browser Credentials Only");
                $("#message").text("Cleared browser cached credentials but failed to clear client session cache");
            }
            else {
                $("#response").text("Failed to Logout");
                $("#message").text("Failed to clear client session cache and failed to clear browser cached credentials");
            }

            $("#responsePanel").show();
        }
        
        $("#returnToLogin").enable();
        $("#workingIcon").hide();
    }, 500);
}

function loginFailed(response) {
    let message;

    if (response.status === 0)
        message = "Connection refused - check web server";
    else
        message = response.statusText + " (" + response.status + ")";

    let responseText = response.responseText;
    
    if (responseText && responseText.length) {
        responseText = responseText.split("<html>")[0];        
        const doc = new DOMParser().parseFromString(responseText, "text/html");
        const errorMessage = (doc.body.textContent || "").trim();

        // Do not include InternalServerError response status for authentication errors:
        if (errorMessage.startsWith("User Authentication Exception:"))
            message = errorMessage.substring(30);
        else
            message += "<br/><br/>" + errorMessage;
    }
    
    if (oidcError.length > 0)
        message = oidcError;

    loginComplete(false, "Login attempt failed: " + message);
}

function login(username, password, skipHash) {
    $("#workingIcon").show();

    if (username) {
        // Attempt authentication with specified user name and password
        $("#response").text("Attempting authentication...");

        if (username.indexOf("\\") > 0 && $("#ntlm").prop("checked"))
            authenticateNTLM(username, password);
        else
            authenticateBasic(username, password, skipHash);
    }
    else {
        // When no user name is provided, attempt pass-through authentication
        $("#response").text("Checking authentication...");

        // Test pass-through authentication
        if ($("#iwa").prop("checked"))
            passthroughNTLM();
        else
            passthroughBasic();
    }
}

function logout() {
    persistentStorage.removeItem("passthrough");

    // Attempt to clear any credentials cached by browser
    clearCachedCredentials(logoutPage, function (success, response) {
        logoutComplete(success, response);
    });
}

function getPromptOptions(showPopup) {
    if ($("#iwa").prop("checked") && !showPopup)
        return "none";

    // Any provided password will be ignored, but will be treated
    // as a user indication of desiring to re-authenticate, i.e.,
    // bypassing single-sign on options:
    if (isEmpty($("#password").val()))
        return "select_account";

    return "login";
}

function getLoginRequest(showPopup) {
    const loginRequest = {
        scopes: ["User.Read"],
        prompt: getPromptOptions(showPopup)
    };

    return loginRequest;
}

function msalGetToken(currentAccount) {
    // If Azure AD sign on authentication succeeded, validate token, make
    // sure user is in database and redirect to main page if successful
    return getToken(currentAccount).then(response => {
        login(currentAccount.username, response.accessToken, true);
    });
}

function msalLogin(showPopup) {
    if (!azureADAuthEnabled) {
        loginComplete(false, "Login attempt failed: Azure AD authentication is disabled");
        return;
    }

    $("#workingIcon").show();
    $("#response").text("Logging into Azure AD...");

    if (showPopup === undefined)
        showPopup = !$("#iwa").prop("checked");

    const loginRequest = getLoginRequest(showPopup);
    
    if (showPopup) {
        if (msalUseRedirect)
            msalInstance.loginRedirect(loginRequest);
        else
            msalInstance.loginPopup(loginRequest).then(msalAuthResponse).catch(function (error) {
                loginComplete(false, "Login attempt failed: " + error);
            });
    }
    else {
        msalInstance.ssoSilent(loginRequest).then(() => {
            return msalGetToken(msalInstance.getAllAccounts()[0]);
        }).catch(error => {
            console.error("Silent Error: " + error);
            if (error instanceof msal.InteractionRequiredAuthError)
                msalLogin(true);
            else
                loginComplete(false, "Login attempt failed: " + error);
        });
    }
}

function msalAuthResponse(response) {
    if (window.parent !== window || msalLoading)
        return;

    if (response) {
        msalGetToken(response.account);
    }
    else {
        const currentAccounts = msalInstance.getAllAccounts();

        if (currentAccounts.length === 0)
            msalLogin(true);
        
        msalGetToken(currentAccounts[0]);
    }
}

async function getToken(account) {
    const request = {
        scopes: ["User.Read"],
        account: account
    };

    return await msalInstance.acquireTokenSilent(request).catch(async (error) => {
        if (error instanceof msal.InteractionRequiredAuthError) {
            if (msalUseRedirect)
                return msalInstance.acquireTokenRedirect(request);
            else
                return msalInstance.acquireTokenPopup(request).catch(error => {
                    loginComplete(false, "Login attempt failed: " + error);
                });
        } else {
            loginComplete(false, "Login attempt failed: " + error);
            return Promise.reject(error);
        }
    });
}

function msalAuthMouseEnter() {
    $("#username").disable();
    $("#password").disable();
    $("#ntlmRegion").hide();

    if ($("#iwa").prop("checked") || isEmpty($("#password").val()))
        return;

    $("#azureADPasswordMessage").show();
}

function msalAuthMouseLeave() {
    $("#azureADPasswordMessage").hide();

    if (!$("#iwa").prop("checked")) {
        $("#username").enable();
        $("#password").enable();

        if ($("#username").val().indexOf("\\") > 0)
            $("#ntlmRegion").show();
    }
}

// Select all text when entering input field
$("input").on("click", function () {
    $(this).select();
});

$("input").keypress(function () {
    $("#responsePanel").hide();
});

$("#login").click(function (event) {
    event.preventDefault();

    $("#login").disable();
    $("#msalAuth").disable();
    $("#responsePanel").hide();

    if ($("#iwa").prop("checked"))
        login();
    else
        login($("#username").val(), $("#password").val());

    return false;
});

$("#msalAuth").click(function (event) {
    event.preventDefault();

    msalAuthMouseLeave();
    $("#msalAuth").disable();
    $("#login").disable();
    $("#responsePanel").hide();

    msalLogin();

    return false;
});

$("#msalAuth").mouseenter(msalAuthMouseEnter);

$("#msalAuth").mouseleave(msalAuthMouseLeave);

$("#username").on("input", function () {
    if ($("#username").val().indexOf("\\") > 0)
        $("#ntlmRegion").show();
    else
        $("#ntlmRegion").hide();
});

$("#iwa").change(function () {
    if (this.checked) {
        $("#username").disable();
        $("#password").disable();
        $("#ntlm").prop("checked", !isIE && !isPOSIX).change();
        $("#ntlm").disable();
        $("#ntlmLabel").addClass("disabled");
        $("#ntlmRegion").hide();
    } else {
        $("#username").enable();
        $("#password").enable();

        if (!isIE && !isPOSIX) {
            $("#ntlm").enable();
            $("#ntlmLabel").removeClass("disabled");
        }

        if ($("#username").val().indexOf("\\") > 0)
            $("#ntlmRegion").show();
    }
});

$("#ntlm").change(function () {
    if (unsecureConnection) {
        if (this.checked)
            $("#ntlmMessage").hide();
        else
            $("#ntlmMessage").show();
    }
});

$("#returnToLogin").click(function () {
    window.location = loginPage;
});

// Make enter key auto-click login
$("#username").keyup(function (event) {
    if (event.keyCode === 13)
        $("#login").click();
});

$("#password").keyup(function (event) {
    if (event.keyCode === 13)
        $("#login").click();
});

$("#iwa").keyup(function (event) {
    if (event.keyCode === 13)
        $("#login").click();
});

$("#remember").keyup(function (event) {
    if (event.keyCode === 13)
        $("#login").click();
});

$("#ntlm").keyup(function (event) {
    if (event.keyCode === 13)
        $("#login").click();
});

$(function () {
    if (logoutPageRequested) {
        logout();
        return;
    }

    $(window).on("beforeunload", saveSettings);
    loadSettings();

    if (oidcError.length > 0)
        loginFailed({ status: 0 });

    if (persistentStorage.getItem("passthrough"))
        login();

    $("#username").focus();
    $("#username").select();
});
