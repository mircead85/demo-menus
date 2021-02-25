var lstSectionDivs = [$('#divHome'), $('#divUsers'), $('#divRoles'), $('#divMenus'), $('#divLogin'), $('#divLogout'), $('#divAllProducts')];
var lstMenuItemsTds = [$('#tdMMHome'), $('#tdMMUsers'), $('#tdMMRoles'), $('#tdMMMenus'), $('#tdMMLogin'), $('#tdMMLogout') ];

var txtCreateAccountStatusReply = null;
var txtCreateAccountStatusReplyDEFAULT = "Fill in the data above and click Create Account to create a new account.";

var activeDiv = "Home";

var statusBarText = null;
var statusBarTextDEFAULT = "None";

var userWelcomeMessageDEFAULT = "Please Login or Create a new account from the Login menu!";
var userWelcomeMessage = null;

var welcomeTextUsers = null;
var welcomeTextUsersDEFAULT = "Hello privileged user! Here you can administer Users.";

var welcomeTextRolesDEFAULT = "Hello privileged user! Here you can administer Roles.";

var bRememberMe = false;
var bAutoLogin = false;

var usersPage = 0;
var rolesPage = 0;
var menusPage = 0;

var usersNewPage = 0;
var rolesNewPage = 0;
var menusNewPage = 0;

function displayError(jqXHR, errorThrown)
{
    statusBarText = "Server said ERROR last time: " + errorThrown.toString() + ". ";

    if (jqXHR.responseJSON.HasError == true
        && jqXHR.responseJSON.Result != null)
    {
        statusBarText += jqXHR.responseJSON.Result.Error.detail.ExceptionType + " (" + jqXHR.responseJSON.Result.Error.detail.ExceptionMessage + ").";
    }

    document.getElementById('pStatusBarText').innerText = statusBarText;
}

function getCookie(cname) {
    var name = cname + "=";
    if (document.cookie == null)
        return null;
    var ca = document.cookie.split('$');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return null;
}

function setWebsiteCookie()
{
    var cookieStr = "";
    var expiry = new Date();
    expiry.setTime(expiry.getTime() + 1000 * 60 * 60 * 24 * 7);
    if(bRememberMe)
    {
        bAutoLogin = true;
        cookieStr += "bRememberMe=true$ bAutologin=" + bAutoLogin + "$";
        cookieStr += "userName=" + userName + "$userPassword=" + userPassword + "$";
        if (activeDiv != "divLogin" && activeDiv != "divLogout")
            cookieStr += "activeDiv=" + activeDiv + "$";
        else
            cookieStr += "activeDiv=" + getCookie("activeDiv")+"$";
    }
    else
    {
        bAutologin = false;
        cookieStr += "bRememberMe=false$ bAutologin=false$ userName=null$ userPassword=null$activeDiv=divHome$";
    }
    cookieStr += ";expires=" + expiry.toUTCString();
    document.cookie = cookieStr;
}

function fncHideAllDivs() {
    $.each(lstSectionDivs, function (key, item) {
        item.hide();
    }
    );
}

function fncRefreshMenuItems() {
    $.each(lstMenuItemsTds, function (key, item) {
        item.hide();
    });
    $('#tdMMHome').show();
    if (userAuthenticated != null) {
        $('#tdMMLogout').show();
        $('#tdMMMenus').show();
        $('#tdMMUsers').show();
        if (userIsAdmin)
            $('#tdMMRoles').show();
    }
    else
        $('#tdMMLogin').show();
}
