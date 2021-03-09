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
//   string loginPage: location of login page
//   string clearCredentialsPage: page used to clear current credentials
//   boolean unsecureConnection: flag that indicates if connection is secure
//   string verificationHeader: HTTP header name used to transmit anti-forgery value
//   string verificationValue: server generated anti-forgery verification token value
//   string useAjaxVerfication: HTTP header named use to indicate an AJAX post
//   string redirectPageLabel: user label for redirect location, e.g., main or target

function loadSettings() {
    $("#username").val(persistentStorage.getItem("username")).trigger("input");
    $("#iwa").prop("checked", persistentStorage.getItem("iwa") === "true").change();
    $("#remember").prop("checked", persistentStorage.getItem("remember") === "true").change();
    $("#ntlm").prop("checked", !isIE && persistentStorage.getItem("ntlm") !== "false").change();
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
function authenticateBasic(username, password) {
    $.ajax({
        cache: false,
        url: securePage,
        type: "post",
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
            if (username.indexOf("\\") <= 0)
                password = hashPassword(password);

            xhr.setRequestHeader("Authorization", "Basic " + btoa(username + ":" + password));
            xhr.setRequestHeader(verificationHeader, verificationValue);
            xhr.setRequestHeader(useAjaxVerfication, "true");
        }
    });
}

// Authorize user with NTLM authentication using provided domain credentials
function authenticateNTLM(username, password) {
    const parts = username.split("\\");

    Ntlm.setCredentials(parts[0], parts[1], password);

    Ntlm.authenticate(securePage + "?scheme=NTLM", function (response) {
        if (response.status === 200)
            loginComplete(true);
        else
            loginFailed(response);
    },
    verificationHeader, verificationValue, useAjaxVerfication);
}

// Authorize user with pass-through basic authentication without credentials
// using pre-authorized authentication token cookie with remember me option
function passthroughBasic() {
    $.ajax({
        cache: false,
        url: securePage,
        type: "post",
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
            xhr.setRequestHeader(useAjaxVerfication, "true");
        }
    });
}

// Authorize user with pass-through NTLM (or negotiated) authentication without credentials,
// NTLM implementation on some browsers may prompt for credentials anyway
function passthroughNTLM() {
    $.ajax({
        cache: false,
        url: securePage + "?scheme=NTLM",
        type: "post",
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
            xhr.setRequestHeader(useAjaxVerfication, "true");
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
        $("#response").text("Enter Credentials:");
        $("#workingIcon").hide();
        $("#credentialsForm").show();

        if (response) {
            $("#message").text(response);
            $("#responsePanel").show();
        }

        $("#username").focus();
        $("#username").select();
        $("#login").enable();
    }
}

function logoutComplete(success) {
    $("#workingIcon").hide();
    $("#reloginForm").show();

    if (getBool(getParameterByName("sessionCleared"))) {
        if (success) {
            $("#response").text("Logout Succeeded");
        }
        else {
            $("#response").text("Partial Logout - Session Only");
            $("#message").text("Client session cache cleared but failed to clear browser cached credentials");
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
}

function loginFailed(response) {
    let message;

    if (response.status === 0)
        message = "Connection refused - check web server";
    else
        message = response.statusText + " (" + response.status + ")";

    loginComplete(false, "Login attempt failed: " + message);
}

function login(username, password) {
    $("#workingIcon").show();

    if (username) {
        // Attempt authentication with specified user name and password
        $("#response").text("Attempting authentication...");

        if (username.indexOf("\\") > 0 && $("#ntlm").prop("checked"))
            authenticateNTLM(username, password);
        else
            authenticateBasic(username, password);
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
    $("#response").text("Logging out...");
    $("#workingIcon").show();
    $("#credentialsForm").hide();

    persistentStorage.removeItem("passthrough");

    // Attempt to clear any credentials cached by browser
    clearCachedCredentials(clearCredentialsPage, function (success) {
        logoutComplete(success);
    });
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
    $("#responsePanel").hide();

    if ($("#iwa").prop("checked"))
        login();
    else
        login($("#username").val(), $("#password").val());
});

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
        $("#ntlm").prop("checked", !isIE).change();
        $("#ntlm").disable();
        $("#ntlmLabel").addClass("disabled");
        $("#ntlmRegion").hide();
    } else {
        $("#username").enable();
        $("#password").enable();

        if (!isIE) {
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
    if (getParameterByName("logout") != null) {
        logout();
        return;
    }

    $(window).on("beforeunload", saveSettings);
    loadSettings();

    if (persistentStorage.getItem("passthrough"))
        login();

    $("#username").focus();
    $("#username").select();
});
