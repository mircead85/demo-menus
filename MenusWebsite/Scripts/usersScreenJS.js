function initScreenUsers() {
    $('#divUsers').show();
    activeDiv = "divUsers";

    welcomeTextUsers = welcomeTextUsersDEFAULT;
    if (!userIsAdmin)
        welcomeTextUsers = "Welcome! Since you are not privileged, you can only see your own user!";

    document.getElementById('h2WelcomeTextUsers').innerText = welcomeTextUsers;

    document.getElementById('tblUsers').innerHTML = "";

    if(userIsAdmin)
        retreieveRoles();

    loadUsers();

    setWebsiteCookie();
}

function doNavUsers(whereTo)
{
    var newPage = 0;
    if (whereTo == -2)
        newPage = 0;
    else if (whereTo == -1)
        newPage = usersCurrentPage - 1;
    else if (whereTo == 0) {
        newPage = parseInt(document.getElementById('txtUsersNavPageNo').value)-1;
    }
    else if (whereTo == 1)
        newPage = usersCurrentPage + 1;
    else if (whereTo == 2)
        newPage = usersTotalPages - 1;

    if (newPage < 0 || newPage >= usersTotalPages) {
        window.alert('Nope, we can\'t navigate to that page! If you think more pages were added, refresh the current page by hitting Go first.');
        return;
    }
    usersNewPage = newPage;

    loadUsers();
}

function loadUsers()
{
    if (userCanCUDUsers)
    {
        var requestUri = serverUri + "/Users/all/" + usersNewPage;
        $.ajax({
            url: requestUri,
            type: "GET",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Basic " + btoa(userName, userPassword));
            },
            success: function (result) {
                var obj = result;
                if (obj.HasError == true) {
                    usersRetreived = null;
                }
                else {
                    usersRetreived = obj.Result.ReadObjects;
                    if(obj.Result.PagingInfo != null)
                    {
                        usersCurrentPage = obj.Result.PagingInfo.PageNumber;
                        usersTotalPages = Math.ceil(obj.Result.PagingInfo.TotalItems / obj.Result.PagingInfo.ItemsPerPage);
                    }
                    document.getElementById('txtUsersNavPageNo').value = usersCurrentPage+1;
                    document.getElementById('lblUsersPagesTotal').innerHTML = usersTotalPages;
                }

                statusBarText = "Server said: " + obj.Message;
                document.getElementById('pStatusBarText').innerText = statusBarText;

                var tblHtml = buildUserPresentation(-1, userAuthenticated, true, false);
                $.each(usersRetreived, function (idx, item) {
                    if (item.UserID == userAuthenticated.UserID)
                        return;
                    tblHtml += buildUserPresentation(idx, item, true, false);
                });

                document.getElementById('tblUsers').innerHTML = tblHtml;
            },
            error: function (jqXHR, textStatus, errorThrown) {
                displayError(jqXHR, errorThrown);

                var tblHtml = buildUserPresentation(-1, userAuthenticated, true, false);
                document.getElementById('tblUsers').innerHTML = tblHtml;
            }
        });
    }
    else
    {
        var tblHtml = buildUserPresentation(-1, userAuthenticated, true, false);
        document.getElementById('tblUsers').innerHTML = tblHtml;
    }
}

function buildUserPresentation(idUser, user, bBuildTrTag, bEditUserSection) {

    var trHtml = '';

    if (idUser!= -1 && user == null)
        return "--- User at this row has been DELETED ---";

    if (bBuildTrTag)
        trHtml += '<tr id="trUsers_Idx_' + idUser + '" tag="' + idUser + '" class="clsTrEntry">';

    trHtml += '<td class="clsTdEntry">';
    trHtml += '<p>Username: ' + user.UserCredentials.UserName + '; Password: --omitted--; DisplayName: ' + user.DisplayName + '.</p>';
    if (user.ExpectedNumCalories != null)
        trHtml += '<p><b>Expected Number of Calories per day: ' + user.ExpectedNumCalories + ' .</b></p>';
    else
        trHtml += '<p><b>Expected Number of Calories per day: --- not set --- .</b></p>';
    if (idUser < 0)
        trHtml += '<p><b>This is your user!!</b></p>';

    trHtml += '<p>Roles: ' + buildUserRoles(user) + '</p>'
    trHtml += '<p>';
    trHtml += '<input type="button" id="btnStartEditUser_Idx_' + idUser + '" value="Edit User" onclick="doStartEditUser(' + idUser + ');" class="clsBtnTableButton"/>';
    trHtml += '</p>';
    trHtml += '</td>';

    if (bEditUserSection) {
        trHtml += '<td id="tdUsers_Idx_' + idUser + '_Edit" tag="' + idUser + '" class="clsTdEntry">';
        trHtml += '<p>Choose Username: <input type="text" id="txtUserUsername_Idx_' + idUser + '" value='+user.UserCredentials.UserName+' size="15" /></p>';
        trHtml += '<p>Choose Password (leave blank for no change): <input type="password" id="txtUserPassword_Idx_' + idUser + '" value ="" size="15" /></p>';
        trHtml += '<p>Choose a Display Name:<input type="text" id="txtUserDisplayName_Idx_' + idUser + '" value='+user.DisplayName+' size="15" /></p>';
        trHtml += '<p>Expected number of calories per day:<input type="text" id="txtUserExpectedCalories_Idx_' + idUser + '" value=' + (user.ExpectedNumCalories != null ? user.ExpectedNumCalories : '" "') + ' size="15" /></p>';
        trHtml += '<p>';
        trHtml += '<input type="button" id="btnEditUser_Idx_' + idUser + '" value="Confirm" onclick="doEditUser(' + idUser + ', false, false);" class="clsBtnTableButton"/>';
        trHtml += '<input type="button" id="btnDeleteUser_Idx_' + idUser + '" value="Delete User" onclick="doEditUser(' + idUser + ', true, false);" class="clsBtnTableButton"/>';
        trHtml += '<input type="button" id="btnCancelEditUser_Idx_' + idUser + '" value="Cancel" onclick="cancelEditUser(' + idUser + ');" class="clsBtnTableButton"/>';
        trHtml += '</p>';
        trHtml += '</td>';

        if(userCanCUDUsers)
        {
            trHtml += '<td id="tdUsers_Idx_' + idUser + '_EditRoles" tag="1" class="clsTdEntry">';


            if (user.UserRoles != null) {
                $.each(user.UserRoles, function (idx, item) {
                    trHtml += '<p>' + item.RoleName + ': <input type="checkbox" checked=true id="ckEditUserRoles_Idx_' + idUser + '_' + item.RoleID + '" size="4" /></p>';
                });

                if (rolesRetreived != null) {
                    $.each(rolesRetreived, function (idx, item) {
                        var bRoleExisted = false;
                        $.each(user.UserRoles, function (idx2, usrRole) {
                            if (usrRole.RoleID == item.RoleID)
                            { bRoleExisted = true; return; }
                        });
                        if (bRoleExisted)
                            return;
                        trHtml += '<p>' + item.RoleName + ': <input type="checkbox" id="ckEditUserRoles_Idx_' + idUser + '_' + item.RoleID.toString() + '" size="4" class="clsBtnTableButton"/></p>';
                    });
                }
                trHtml += '<p>';
                trHtml += '<input type="button" id="btnUpdateUserRoles_Idx_' + idUser + '" value="Update" onclick="doEditUser(' + idUser + ', false, true);" class="clsBtnTableButton"/>';
                trHtml += '</p>';
                trHtml += '<p>';
            }
            else
                trHtml += "--- Users has had his Roles edited ---";
            
            trHtml += '</td>';
        }
    }

    if(bBuildTrTag)
        trHtml += '</tr>';

    return trHtml;
}

function doStartEditUser(userIdx) {
    if(userIdx == -1)
        document.getElementById('trUsers_Idx_'+userIdx).innerHTML = buildUserPresentation(userIdx, userAuthenticated, false, true);
    else
        document.getElementById('trUsers_Idx_'+userIdx).innerHTML = buildUserPresentation(userIdx, usersRetreived[userIdx], false, true);
}

function cancelEditUser(userIdx) {
    if (userIdx == -1)
        document.getElementById('trUsers_Idx_' + userIdx).innerHTML = buildUserPresentation(userIdx, userAuthenticated, false, false);
    else
        document.getElementById('trUsers_Idx_' + userIdx).innerHTML = buildUserPresentation(userIdx, usersRetreived[userIdx], false, false);
}

function doEditUser(userIdx, bDoDelete, bEditRoles)
{
    var user = userAuthenticated;
    if (userIdx != -1)
        user = usersRetreived[userIdx];

    var reqUserId = user.UserID;
    var reqUsername = document.getElementById('txtUserUsername_Idx_'+userIdx).value;
    var reqPassword = document.getElementById('txtUserPassword_Idx_' + userIdx).value;
    if(reqPassword == "")
        reqPassword = null;
    var reqDisplayname = document.getElementById('txtUserDisplayName_Idx_' + userIdx).value;
    var reqCalories = -1;
    if (parseInt(document.getElementById('txtUserExpectedCalories_Idx_' + userIdx).value, 10) != NaN)
        reqCalories = parseInt(document.getElementById('txtUserExpectedCalories_Idx_' + userIdx).value, 10);
    if (!(reqCalories <= reqCalories)) //NaN
        reqCalories = -1;

    var reqRoles = null;
    if (bEditRoles)
        reqRoles = getEditRolesString(userIdx);

    var editCtrl = document.getElementById('btnEditUser_Idx_' + userIdx);
    var deleteCtrl = document.getElementById('btnDeleteUser_Idx_' + userIdx);

    var updateCtrl = document.getElementById('btnUpdateUserRoles_Idx_' + userIdx);

    editCtrl.disabled = true;
    deleteCtrl.disabled = true;
    if (updateCtrl != null && updateCtrl != undefined)
        updateCtrl.disabled = true;

    var requestUri = serverUri + "/Users/" + reqUserId + "/" + "/" + reqUsername + "/" + reqPassword + "/" + reqDisplayname + "/" + reqCalories + "/" + reqRoles;
    var requestType = "PUT";
    if (bDoDelete)
    {
        requestUri = serverUri + "/Users/" + reqUserId;
        requestType = "DELETE";
    }

    $.ajax({
        url: requestUri,
        type: requestType,
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", "Basic " + btoa(userName, userPassword));
        },
        success: function (result) {
            var obj = result;
            if (obj.HasError == true) {
                editCtrl.disabled = false;
                deleteCtrl.disabled = false;
                if (updateCtrl != null && updateCtrl != undefined)
                    updateCtrl.disabled = false;
            }
            else {
                //window.alert("Number of elements which generated changes: " + obj.Result.NumberOfEntriesWhichGeneratedChanges);
                if(obj.Result.NumberOfEntriesWhichGeneratedChanges == 1)
                {
                    user.UserCredentials.UserName = reqUsername;
                    if (reqPassword != null)
                        user.UserCredentials.Password = reqPassword;
                    user.DisplayName = reqDisplayname;
                    user.ExpectedNumCalories = reqCalories < 0 ? null : reqCalories;

                    if (!bDoDelete) {
                        if(bEditRoles)
                        {
                            if (userIdx == -1)
                                fncMMLogout();
                            else {
                                user.UserRoles = null;
                            }
                        }
                    }
                    else {
                        if (userIdx != -1) {
                            usersRetreived[userIdx] = null;
                            user = null;
                        }
                        else
                            fncMMLogout();
                    }
                    document.getElementById('trUsers_Idx_' + userIdx).innerHTML = buildUserPresentation(userIdx, user, false, false);
                }

                loadUsers();
            }

            editCtrl.disabled = false;
            deleteCtrl.disabled = false;
            if (updateCtrl != null && updateCtrl != undefined)
                updateCtrl.disabled = false;

            statusBarText = "Server said: " + obj.Message;
            document.getElementById('pStatusBarText').innerText = statusBarText;
        },
        error: function (jqXHR, textStatus, errorThrown) {
            editCtrl.disabled = false;
            deleteCtrl.disabled = false;
            if (updateCtrl != null && updateCtrl != undefined)
                updateCtrl.disabled = false;
            displayError(jqXHR, errorThrown);
        }
    });
}

function getEditRolesString(userIdx)
{
    var user = userAuthenticated;
    if (userIdx != -1)
        user = usersRetreived[userIdx];

    var rolesToIterate = rolesRetreived;

    var reqUserId = user.UserID;

    var reqRoles = "";

    $.each(rolesToIterate, function (idx, role) {
        var ckValue = false;
        var ckCtrl = document.getElementById('ckEditUserRoles_Idx_' + userIdx + '_' + role.RoleID);
        if(ckCtrl != null && ckCtrl != undefined)
        {
            var ckVal = document.getElementById('ckEditUserRoles_Idx_' + userIdx + '_' + role.RoleID).checked;
            if (ckVal)
                reqRoles+= role.RoleID + "_true;";
            else
                reqRoles+= role.RoleID + "_false;";
        }
    });

    return reqRoles;
}