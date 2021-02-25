function fncMMGoToCookieDiv()
{
    var cookieDiv = getCookie("activeDiv");
    if (cookieDiv == null || cookieDiv == undefined || cookieDiv == "null")
        cookieDiv = "divHome";
    
    if(cookieDiv == "divLogin" || cookieDiv == "divLogout")
        cookieDiv = "divHome";

    if (cookieDiv == "divRoles" && !userIsAdmin)
        cookieDiv = "divHome";
    
    if (cookieDiv == "divHome")
        fncMMHome();
    else if (cookieDiv == "divUsers")
        fncMMUsers();
    else if (cookieDiv == "divRoles")
        fncMMRoles();
    else if (cookieDiv == "divMenus")
        fncMMMenus();
    else
        fncMMHome(); //If no valid prior active div, go Home.
}

function fncMMLogin() {
    fncHideAllDivs();
    initScreenLogin();
}

function fncMMHome()
{
    fncHideAllDivs();
    initScreenHome();
}

function fncMMUsers()
{
    fncHideAllDivs();
    initScreenUsers();
}

function fncMMRoles() {
    fncHideAllDivs();
    initScreenRoles();
}

function fncMMMenus() {
    fncHideAllDivs();
    initScreenMenus();
}

function fncMMLogout() {
    fncHideAllDivs();
    initScreenLogout();
}