function initScreenRoles() {
    $('#divRoles').show();
    activeDiv = "divRoles";

    document.getElementById('h2WelcomeTextRoles').innerText = welcomeTextRolesDEFAULT;

    document.getElementById('tblRoles').innerHTML = "";

    loadRoles();

    setWebsiteCookie();
}

function doNavRoles(whereTo) {
    var newPage = 0;
    if (whereTo == -2)
        newPage = 0;
    else if (whereTo == -1)
        newPage = rolesCurrentPage - 1;
    else if (whereTo == 0) {
        newPage = parseInt(document.getElementById('txtRolesNavPageNo').value)-1;
    }
    else if (whereTo == 1)
        newPage = rolesCurrentPage + 1;
    else if (whereTo == 2)
        newPage = rolesTotalPages - 1;

    if (newPage < 0 || newPage >= rolesTotalPages) {
        window.alert('Nope, we can\'t navigate to that page! If you think more pages were added, refresh the current page by hitting Go first.');
        return;
    }

    rolesNewPage = newPage;

    loadRoles();
}

function loadRoles() {
    if (userIsAdmin) {
        var requestUri = serverUri + "/Roles/all/" + rolesNewPage;
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
                    if (obj.Result.PagingInfo != null) {
                        rolesCurrentPage = obj.Result.PagingInfo.PageNumber;
                        rolesTotalPages = Math.ceil(obj.Result.PagingInfo.TotalItems / obj.Result.PagingInfo.ItemsPerPage);
                    }
                    document.getElementById('txtRolesNavPageNo').value = rolesCurrentPage + 1;
                    document.getElementById('lblRolesPagesTotal').innerHTML = rolesTotalPages;
                }

                statusBarText = "Server said: " + obj.Message;
                document.getElementById('pStatusBarText').innerText = statusBarText;

                var tblHtml = "";
                if(rolesRetreived != null)
                $.each(rolesRetreived, function (idx, item) {
                    tblHtml += buildRolePresentation(idx, item, true, false);
                });

                tblHtml += buildRolePresentation(-1, null, true, false);
                document.getElementById('tblRoles').innerHTML = tblHtml;
            },
            error: function (jqXHR, textStatus, errorThrown) {
                displayError(jqXHR, errorThrown);

                document.getElementById('tblRoles').innerHTML = "";
            }
        });
    }
    else
    {
        document.getElementById('tblRoles').innerHTML = "";
    }
}



function buildRolePresentation(roleIdx, role, bBuildTrTag, bEditRoleSection)
{
    var trHtml = "";

    if (roleIdx != -1 && role == null)
        return "--- Role at this row has been DELETED ---";

    if (bBuildTrTag)
        trHtml += '<tr id="trRoles_Idx_' + roleIdx + '" tag="' + roleIdx + '" class="clsTrEntry">';

    trHtml += '<td class="clsTdEntry">';

    if (roleIdx != -1) {
        trHtml += '<p>Role Name: ' + role.RoleName + '; ';
        if (role.IsAdmin)
            trHtml += 'Is Admin: yes; ';
        else
            trHtml += 'Is Admin: no; ';
        if (role.CanCRUDUsers)
            trHtml += 'Can edit User accounts: yes; ';
        else
            trHtml += 'Can edit User accounts: no; ';
        if (role.CanCRUDEntries)
            trHtml += 'Can edit others\' Menu entries: yes; ';
        else
            trHtml += 'Can edit others\' Menu entries: no; ';
        trHtml += '</p>';
    }
    else
    {
        trHtml += "--- NEW ROLE ---"
        role = { RoleName: 'Enter', IsAdmin: false, CanCRUDUsers: false, CanCRUDEntries: false };
    }

    trHtml += '<p><input type="button" id="btnStartEditRole_Idx_' + roleIdx + '" value="Edit Role" onclick="doStartEditRole(' + roleIdx + ');" class="clsBtnTableButton"/></p>';
    trHtml += '</td>';

    if (bEditRoleSection)
    {
        trHtml += '<td id="tdRoles_Idx_' + roleIdx + '_Edit" tag="' + roleIdx + '" class="clsTdEntry">';
        trHtml += '<p>Choose Role Name: <input type="text" id="txtRoleName_Idx_'+roleIdx+'" value='+role.RoleName+' size="15" /></p>';

        if(role.IsAdmin)
            trHtml += '<p>Is Admin: <input type="checkbox" checked=true id="ckRoleIsAdmin_Idx_'+roleIdx+'" size="4" /></p>';
        else
            trHtml += '<p>Is Admin: <input type="checkbox" id="ckRoleIsAdmin_Idx_'+roleIdx+'" size="4" /></p>';

        if(role.CanCRUDUsers)
            trHtml += '<p>Can CUD Users: <input type="checkbox" checked=true id="ckRoleCanCUDUsers_Idx_'+roleIdx+'" size="4" /></p>';
        else
            trHtml += '<p>Can CUD Users: <input type="checkbox" id="ckRoleCanCUDUsers_Idx_'+roleIdx+'" size="4" /></p>';

        if(role.CanCRUDEntries)
            trHtml += '<p>Can CUD others\' Menus: <input type="checkbox" checked=true id="ckRoleCanCUDOthersMenus_Idx_' + roleIdx + '" size="4" /></p>';
        else
            trHtml += '<p>Can CUD others\' Menus: <input type="checkbox" id="ckRoleCanCUDOthersMenus_Idx_'+roleIdx+'" size="4" /></p>';

        trHtml += '<input type="button" id="btnEditRole_Idx_' + roleIdx + '" value="Confirm" onclick="doEditRole(' + roleIdx + ', false);" class="clsBtnTableButton"/>';
        trHtml += '<input type="button" id="btnDeleteRole_Idx_' + roleIdx + '" value="Delete Role" onclick="doEditRole(' + roleIdx + ', true);" class="clsBtnTableButton"/>';
        trHtml += '<input type="button" id="btnCancelEditRole_Idx_' + roleIdx + '" value="Cancel" onclick="cancelEditRole(' + roleIdx + ');" class="clsBtnTableButton"/>';

        trHtml += '</td>';
    }

    if (bBuildTrTag)
        trHtml += "</tr>";

    return trHtml;
}


function doStartEditRole(roleIdx) {
    if(roleIdx != -1)
        document.getElementById('trRoles_Idx_' + roleIdx).innerHTML = buildRolePresentation(roleIdx, rolesRetreived[roleIdx], false, true);
    else
        document.getElementById('trRoles_Idx_' + roleIdx).innerHTML = buildRolePresentation(roleIdx, null, false, true);
}

function cancelEditRole(roleIdx) {
    if (roleIdx != -1)
        document.getElementById('trRoles_Idx_' + roleIdx).innerHTML = buildRolePresentation(roleIdx, rolesRetreived[roleIdx], false, false);
    else
        document.getElementById('trRoles_Idx_' + roleIdx).innerHTML = buildRolePresentation(roleIdx, null, false, false);
}

function doEditRole(roleIdx, bDoDelete)
{
    var roleID = -1;
    if (roleIdx != -1)
        roleID = rolesRetreived[roleIdx].RoleID;

    var roleName = document.getElementById('txtRoleName_Idx_' + roleIdx).value;
    var roleIsAdmin = document.getElementById('ckRoleIsAdmin_Idx_' + roleIdx).checked;
    var roleCanCRUDUsers = document.getElementById('ckRoleCanCUDUsers_Idx_' + roleIdx).checked;
    var roleCanCRUDEntries = document.getElementById('ckRoleCanCUDOthersMenus_Idx_' + roleIdx).checked;

    var editCtrl = document.getElementById('btnEditRole_Idx_' + roleIdx);
    var deleteCtrl = document.getElementById('btnDeleteRole_Idx_' + roleIdx);

    editCtrl.disabled = true;
    deleteCtrl.disabled = true;

    var requestUri = serverUri + "/Roles/" + roleID + "/" + roleName + "/" + roleIsAdmin + "/" + roleCanCRUDUsers + "/" + roleCanCRUDEntries;
    var requestType = "PUT";
    if (bDoDelete) {
        requestUri = serverUri + "/Roles/" + roleID;
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
            }
            else {
                //window.alert("Number of elements which generated changes: " + obj.Result.NumberOfEntriesWhichGeneratedChanges);
                if (obj.Result.NumberOfEntriesWhichGeneratedChanges == 1) {
                    if (!bDoDelete) {
                        if (roleID == -1) {
                            rolesRetreived.push(obj.Result.NewlyCreatedObjectsWithIds[0]);
                            roleIdx = rolesRetreived.length - 1;
                            role = rolesRetreived[roleIdx];
                            var rolesTable = document.getElementById('tblRoles');
                            var row = rolesTable.insertRow(rolesTable.rows.length-1);
                            row.id = 'trRoles_Idx_' + roleIdx;
                            row.tag = roleIdx;
                        }
                        else
                        {
                            role = rolesRetreived[roleIdx];
                            role.RoleName = roleName;
                            role.IsAdmin = roleIsAdmin;
                            role.CanCRUDEntries = roleCanCRUDEntries;
                            role.CanCRUDUsers = roleCanCRUDUsers;
                        }

                    }
                    else {
                        if (roleIdx != -1) {
                            rolesRetreived[roleIdx] = null;
                        }
                    }
                    document.getElementById('trRoles_Idx_' + roleIdx).innerHTML = buildRolePresentation(roleIdx, rolesRetreived[roleIdx], false, false);
                }
            }
            editCtrl.disabled = false;
            deleteCtrl.disabled = false;

            statusBarText = "Server said: " + obj.Message;
            document.getElementById('pStatusBarText').innerText = statusBarText;

            loadRoles();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            editCtrl.disabled = false;
            deleteCtrl.disabled = false;

            displayError(jqXHR, errorThrown);
        }
    });
}