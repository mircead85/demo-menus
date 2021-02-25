var userLoggedIn = false;

var userAuthenticated = null;

var userIsAdmin = false;
var userCanCUDUsers = false;
var userCanCUDOthersMenus = false;

var userName = null;
var userPassword = null;

var MenuEntriesRetreived = null;
var menusCurrentPage = 0;
var menusTotalPages = 1;

var usersRetreived = null;
var usersCurrentPage = 0;
var usersTotalPages = 1;

var rolesRetreived = null;
var rolesCurrentPage = 0;
var rolesTotalPages = 1;

function fncInitClientDataModel(bAlsoUser)
{
    userLoggedIn = false;

    if(bAlsoUser == true)
        userAuthenticated = null;

    userIsAdmin = false;
    userCanCUDUsers = false;
    userCanCUDOthersMenus = false;
    userExpectedNumCalories = null;

    if (bAlsoUser == true) {
        userName = null;
        userPassword = null;
    }

    MenuEntriesRetreived = null;
    editedMenu = null;
    menusCurrentPage = 0;
    menusTotalPages = 1;

    usersRetreived = null;
    editedUser = null;
    usersCurrentPage = 0;
    usersTotalPages = 1;

    roleAppliedToUser = null;
    roleIsDisapplied = false;
    rolesRetreived = null;
    editedRole = null;
    rolesCurrentPage = 0;
    rolesTotalPages = 1;
}


function buildUserRoles(user) {
    var rolesTxt = "";
    if (user.UserRoles == null)
        return "--- User has had his Roles edited ---";
    $.each(user.UserRoles, function (idx, item) {
        rolesTxt += item.RoleName + ";";
    });

    return rolesTxt;
}

function retreieveRoles()
{
    var requestUri = serverUri + "/Roles/all/-1/";
    $.ajax({
        url: requestUri,
        type: "GET",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", "Basic " + btoa(userName, userPassword));
        },
        success: function (result) {
            var obj = result;
            if (obj.HasError == true) {
                //rolesRetreived = null;
            }
            else {
                rolesRetreived = obj.Result.ReadObjects;
            }

            statusBarText = "Server said: " + obj.Message;
            document.getElementById('pStatusBarText').innerText = statusBarText;
        },
        error: function (jqXHR, textStatus, errorThrown) {
            displayError(jqXHR, errorThrown);
        }
    });
}
