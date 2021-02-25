var validChallengResp = -1;
var challengeText = null;

function initScreenLogin()
{
    $('#divLogin').show();
    activeDiv = "divLogin";

    document.getElementById('ckFilterMenus').checked = false;

    var cookieUsername = getCookie("userName");
    var cookiePassword = getCookie("userPassword");
    var cookiebRememberMe = getCookie("bRememberMe");
    var cookiebAutologin = getCookie("bAutologin");

    if (cookiebRememberMe == "true")
        document.getElementById('ckLoginRememberMe').checked = true;
    else
        document.getElementById('ckLoginRememberMe').checked = false;

    if (cookiebRememberMe == "true")
    {
        document.getElementById('txtUsername').value = cookieUsername;
        document.getElementById('txtPassword').value = cookiePassword;
    }

    var a = Math.floor((Math.random() * 10) + 1);
    var b = Math.floor((Math.random() * 10) + 1);

    challengeText = "How much is " + a + " plus " + b + "?";
    document.getElementById('pChallengeText').innerText = challengeText;
    validChallengResp = a + b;

    txtCreateAccountStatusReply = txtCreateAccountStatusReplyDEFAULT;
    document.getElementById('pCreateAccountText').innerText = txtCreateAccountStatusReply;

    if (cookiebAutologin == "true" && cookiePassword != null && cookiePassword != undefined && cookiePassword != "null")
        doLogin();
}

function doLogin()
{
    userName = document.getElementById('txtUsername').value;
    userPassword = document.getElementById('txtPassword').value;
    bRememberMe = document.getElementById('ckLoginRememberMe').checked;
    bAutoLogin = bRememberMe;
    try {
        setWebsiteCookie();
    }
    catch (e) { } //In case setting cookies fails
    var requestUri = serverUri + "/Authenticate/";
    document.getElementById('btnLogin').disabled = true;
    $.ajax({
        url: requestUri,
        type: "GET",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", "Basic " + btoa(userName+ ":" + userPassword));
        },
        success: function (result) {
            document.getElementById('btnLogin').disabled = false;
            var obj = result;
            if(obj.HasError == true)
            {
                userAuthenticated = null;
            }
            else
            {
                userAuthenticated = obj.Result.ReadObjects[0];
                userName = ':' + obj.Result.SecurityToken.TokenId;
                userPassword = ':ignored';
            }

            if (userAuthenticated != null) {
                fncInitClientDataModel(false);
                $.each(userAuthenticated.UserRoles, function (idx, userRole) {
                    if (userRole.IsAdmin)
                        userIsAdmin = true;
                    if (userRole.CanCRUDUsers)
                        userCanCUDUsers = true;
                    if (userRole.CanCRUDEntries)
                        userCanCUDOthersMenus = true;
                });
                userLoggedIn = true;
            }
            else {
                //Failure to login.
                userLoggedIn = false;
            }

            statusBarText = "Server said: " + obj.Message;
            document.getElementById('pStatusBarText').innerText = statusBarText;

            if (userLoggedIn) {
                fncRefreshMenuItems();
                fncMMGoToCookieDiv();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            document.getElementById('btnLogin').disabled = false;
            displayError(jqXHR, errorThrown);
        }
    });

    document.getElementById('txtPassword').value = "";
}

function doCreateAccount()
{
    txtCreateAccountStatusReply = "Creating account...";
    
    var newUserName = document.getElementById('txtnewUsername').value;
    var newuserPassword = document.getElementById('txtnewPassword').value;
    var reenterPassword = document.getElementById('txtrenterPassword').value;
    var displayName = document.getElementById('txtDisplayName').value;
    var chlgResponse = document.getElementById('txtchallengeResponse').value;

    if (newuserPassword != reenterPassword || newuserPassword == "" || newuserPassword==null)
    {
        txtCreateAccountStatusReply = "Password reentered incorrectly!";
    }

    if (chlgResponse != validChallengResp.toString()) {
        txtCreateAccountStatusReply = "You did not answer the challenge correctly!";
    }
    
    document.getElementById('pCreateAccountText').innerText = txtCreateAccountStatusReply;

    if (txtCreateAccountStatusReply != "Creating account...")
        return;

    var requestUri = serverUri + "/Users/new/" + newUserName + "/" + newuserPassword + "/" + displayName + "/" + "null"+ "/" + "11431";
    document.getElementById('btnCreateAccount').disabled = true;
    $.ajax({
        url: requestUri,
        type: "POST",
        success: function (result) {
            document.getElementById('btnCreateAccount').disabled = false;
            var obj = result;
            if (obj.HasError == true) {
                txtCreateAccountStatusReply = "Nope, it didn't work. See status message for details.";
            }
            else {
                txtCreateAccountStatusReply = "Hurray!! You created a new account. You can now log in with those credentials.";
            }
            statusBarText = "Server said: " + obj.Message;
            document.getElementById('pStatusBarText').innerText = statusBarText;

            document.getElementById('pCreateAccountText').innerText = txtCreateAccountStatusReply;
        },
        error: function (jqXHR, textStatus, errorThrown) {
            document.getElementById('btnCreateAccount').disabled = false;
            displayError(jqXHR, errorThrown);

            txtCreateAccountStatusReply = "Nope, it didn't work!";
            document.getElementById('pCreateAccountText').innerText = txtCreateAccountStatusReply;
        }
    });

    document.getElementById('txtnewPassword').value = "";
    document.getElementById('txtrenterPassword').value = "";
}