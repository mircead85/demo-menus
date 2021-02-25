var timeoutDelegateHandle = null;

function initScreenMenus() {
    $('#divMenus').show();
    activeDiv = "divMenus";

    if (!userCanCUDOthersMenus && !userIsAdmin) {
        document.getElementById('ckShowJustOwnMenus').checked = true;
        document.getElementById('ckShowJustOwnMenus').disabled = true;
    }
    else{
        document.getElementById('ckShowJustOwnMenus').disabled = false;
    }

    document.getElementById('tblMenus').innerHTML = "";

    initComboBoxes();
    initColorCodeText();

    doClickFilterMenus();

    loadMenus();

    setWebsiteCookie();
}

function initComboBoxes()
{
    var cbFromHour = document.getElementById('cbFilterFromTimeHour');
    var cbToHour = document.getElementById('cbFilterToTimeHour');
    var cbFromMinute = document.getElementById('cbFilterFromTimeMinute');
    var cbToMinute = document.getElementById('cbFilterToTimeMinute');

    if (cbToMinute.options.length > 0)
        return; //Already inited

    for (i = 0; i < 24; i++)
    {
        var txt = "";
        if (i < 10)
            txt += "0";
        txt += i;

        cbFromHour.options.add(new Option(txt, i));
        cbToHour.options.add(new Option(txt, i));
    }

    for (i = 0; i < 60; i++) {
        var txt = "";
        if (i < 10)
            txt += "0";
        txt += i;

        cbFromMinute.options.add(new Option(txt, i));
        cbToMinute.options.add(new Option(txt, i));
    }
}

function initColorCodeText()
{
    var pColorCodeMenus = document.getElementById('pColorCodeMenus');
    var innerHtmlforP = '';

    var oldChecked = document.getElementById('ckColorCodeMenus').checked;

    if (userAuthenticated.ExpectedNumCalories != null) {
        innerHtmlforP = 'Color code Menus according to user\'s set threshold  (' + userAuthenticated.ExpectedNumCalories + ' calories per day for you)?<input type="checkbox" id="ckColorCodeMenus" size="4" onclick="doClickColorCodeMenus()" />';
        pColorCodeMenus.innerHTML = innerHtmlforP;
        document.getElementById('ckColorCodeMenus').checked = false;
        document.getElementById('ckColorCodeMenus').checked = oldChecked;
    }
    else {
        innerHtmlforP = 'Color code Menus according to user\'s set threshold (--- no limit set --- for you)?<input type="checkbox" id="ckColorCodeMenus" size="4" onclick="doClickColorCodeMenus()" />';
        pColorCodeMenus.innerHTML = innerHtmlforP;
        document.getElementById('ckColorCodeMenus').checked = false;
        //document.getElementById('ckColorCodeMenus').disabled = true;
    }
}

function doClickFilterMenus()
{
    var bFilterMenus = document.getElementById('ckFilterMenus').checked;

    if (bFilterMenus)
        $('#divFilterMenus').show();
    else
        $('#divFilterMenus').hide();
}

function doClickColorCodeMenus()
{

}


function doNavMenus(whereTo) {
    var newPage = 0;
    if (whereTo == -2)
        newPage = 0;
    else if (whereTo == -1)
        newPage = menusCurrentPage - 1;
    else if (whereTo == 0) {
        newPage = parseInt(document.getElementById('txtMenusNavPageNo').value)-1;
    }
    else if (whereTo == 1)
        newPage = menusCurrentPage + 1;
    else if (whereTo == 2)
        newPage = menusTotalPages - 1;

    if (newPage < 0 || newPage >= menusTotalPages) {
        window.alert('Nope, we can\'t navigate to that page! If you think more pages were added, refresh the current page by hitting Go first.');
        return;
    }

    menusNewPage = newPage;

    loadMenus();
}

function loadMenus() {

    var bJustOwnMenus = document.getElementById('ckShowJustOwnMenus').checked;
    
    var bFilterMenus = document.getElementById('ckFilterMenus').checked && !document.getElementById('ckFilterMenus').disabled;
    var filterDateFrom = null;
    var filterDateTo = null;
    var filterHourFrom = null;
    var filterHourTo = null;
    var filterMinuteFrom = null;
    var filterMinuteTo = null;

    if (bFilterMenus)
    {
        filterDateFrom = document.getElementById('dtFilterFromDate').value;
        filterDateTo = document.getElementById('dtFilterToDate').value;

        filterHourFrom = document.getElementById('cbFilterFromTimeHour').selectedIndex;
        if (filterHourFrom >= 0)
            filterHourFrom = document.getElementById('cbFilterFromTimeHour').options[filterHourFrom].value;
        else
            filterHourFrom = null;

        filterHourTo = document.getElementById('cbFilterToTimeHour').selectedIndex;
        if (filterHourTo >= 0)
            filterHourTo = document.getElementById('cbFilterToTimeHour').options[filterHourTo].value;
        else
            filterHourTo = null;


        filterMinuteFrom = document.getElementById('cbFilterFromTimeMinute').selectedIndex;
        if (filterMinuteFrom >= 0)
            filterMinuteFrom = document.getElementById('cbFilterFromTimeMinute').options[filterMinuteFrom].value;
        else
            filterMinuteFrom = null;

        filterMinuteTo = document.getElementById('cbFilterToTimeMinute').selectedIndex;
        if (filterMinuteTo >= 0)
            filterMinuteTo = document.getElementById('cbFilterToTimeMinute').options[filterMinuteTo].value;
        else
            filterMinuteTo = null;


        if (filterDateFrom == null || filterDateTo == null || filterHourFrom == null || filterHourTo == null || filterMinuteFrom == null || filterMinuteTo == null) {
            window.alert('The filter specifications have not been filled out properly!');
            return;
        }

        filterDateFrom = new Date(filterDateFrom);
        filterDateTo = new Date(filterDateTo);
    }

    var ownerId = -1;
    if (bJustOwnMenus)
        ownerId = userAuthenticated.UserID;

    var requestUri = serverUri + "/Menus/all/" +menusNewPage+"/" + ownerId + "/" + bFilterMenus;
    if (bFilterMenus)
        requestUri += "/" + filterDateFrom.getFullYear() + "/" + (filterDateFrom.getMonth()+1) + "/" + filterDateFrom.getDate() + "/" + filterDateTo.getFullYear() + "/" + (filterDateTo.getMonth() + 1) + "/" + filterDateTo.getDate() + "/" + filterHourFrom + "/" + filterMinuteFrom + "/" + filterHourTo + "/" + filterMinuteTo;
    else
        requestUri += "/null/null/null/null/null/null/null/null/null/null";

    document.getElementById('pStatusBarText').innerText = "Loading Menus...";

    $.ajax({
        url: requestUri,
        type: "GET",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", "Basic " + btoa(userName, userPassword));
        },
        success: function (result) {
            var obj = result;
            if (obj.HasError == true) {
                MenuEntriesRetreived = null;
            }
            else {
                MenuEntriesRetreived = obj.Result.ReadObjects;
                if (obj.Result.PagingInfo != null) {
                    menusCurrentPage = obj.Result.PagingInfo.PageNumber;
                    menusTotalPages = Math.ceil(obj.Result.PagingInfo.TotalItems / obj.Result.PagingInfo.ItemsPerPage);
                }
                document.getElementById('txtMenusNavPageNo').value = menusCurrentPage + 1;
                document.getElementById('lblMenusPagesTotal').innerHTML = menusTotalPages;
            }

            statusBarText = "Server said: " + obj.Message;
            document.getElementById('pStatusBarText').innerText = statusBarText;

            var tblHtml = "";
            if (MenuEntriesRetreived != null)
                $.each(MenuEntriesRetreived, function (idx, item) {
                    tblHtml += buildMenuPresentation(idx, item, true, false);
                });

            tblHtml += buildMenuPresentation(-1, null, true, false);
            document.getElementById('tblMenus').innerHTML = tblHtml;
        },
        error: function (jqXHR, textStatus, errorThrown) {
            displayError(jqXHR, errorThrown);

            MenuEntriesRetreived = null;
            document.getElementById('tblMenus').innerHTML = "";
        }
    });
}

function getDateFromString(dateStr)
{
    return new Date(dateStr);
}

function getNetDateStringFromDate(dateObj)
{
    var result = dateObj.getFullYear() + '-' + (dateObj.getMonth() + 1) + "-" + dateObj.getDate() + "T";
    if (dateObj.getHours() < 10)
        result += "0";
    result += dateObj.getHours() + ":";
    if (dateObj.getMinutes() < 10)
        result += "0";
    result +=dateObj.getMinutes() + ":00";
    return result;
}

function formatDate(dateToFormat, bIncludeTime) {
    if (dateToFormat == null)
        return 'null';

    var yr = dateToFormat.getUTCFullYear();
    var mo = dateToFormat.getUTCMonth();
    mo++;
    var day = dateToFormat.getUTCDate();

    var hour = dateToFormat.getUTCHours();
    var min = dateToFormat.getUTCMinutes();

    var result ='' + yr + '-';
    if (mo < 10)
        result += '0';
    result += mo + '-';
    if (day < 10)
        result += '0';
    result += day + ' ';

    if (bIncludeTime) {
        if (hour < 10)
            result += '0';
        result += hour + ':';
        if (min < 10)
            result += '0';
        result += min;
    }

    return result;
}

function buildMenuPresentation(MenuIdx, MenuEntry, bBuildTrTag, bEditMenuSection) {
    var trHtml = "";

    if (MenuIdx!=-1 && MenuEntry == null)
        return "--- Menu at this row has been DELETED ---";

    if (bBuildTrTag) {
        trHtml += '<tr id="trMenus_Idx_' + MenuIdx + '" tag="' + MenuIdx + '" class="clsTrEntry" >';
    }

    var bDoColorCode = false;

    if (document.getElementById('ckColorCodeMenus').checked == true)
        bDoColorCode = true;

    if (MenuEntry == null)
        bDoColorCode = false;
    else if (MenuEntry.ColorCodingWithRegardToOwnerSettings == null)
          bDoColorCode = false;

    if (bDoColorCode && MenuEntry != null) {
        trHtml += '<td class="clsTdEntry" ';

        if (MenuEntry.ColorCodingWithRegardToOwnerSettings >= 0)
            trHtml += 'bgcolor = "green"';
        else
            trHtml += 'bgcolor = "red"';

        trHtml += '>';
    }
    else
        trHtml += '<td class="clsTdEntry">';

    trHtml += ' <p id="txtMenu_Idx_'+MenuIdx+'_Text">';

    if(MenuIdx!=-1)
    {
        trHtml += 'Text: '+MenuEntry.Text+'; Moment: '+formatDate(getDateFromString(MenuEntry.Moment), true) + ';<br/><b>Number of calories: '+MenuEntry.NumCalories+'</b><br/>';
        if(MenuEntry.Owner != null)
            trHtml+=' Owner: '+MenuEntry.Owner.DisplayName+'.';

        trHtml += '</p>';
    }
    else
    {
        trHtml += "--- NEW Menu ---";
        MenuEntry = {Text: 'Enter', Moment: getNetDateStringFromDate(new Date()), NumCalories:0, Owner:null};
    }
    
    trHtml += '<p><input type="button" id="btnStartEditMenu_Idx_' + MenuIdx + '" value="Edit Menu" onclick="doStartEditMenu(' + MenuIdx + ');" class="clsBtnTableButton"/></p>';
    trHtml += '</td>';

    if (bEditMenuSection) {
        trHtml += '<td id="txtMenu_Idx_' + MenuIdx + '_Edit" tag="' + MenuIdx + '" class="clsTdEntry">';
        trHtml += '<p>Menu Text: <input type="text" id="txtMenuText_Idx_' + MenuIdx + '" value=' + MenuEntry.Text + ' size="15" /></p>';
        trHtml += '<p>Menu Date: <input type="date" id="dtMenuMomentDate_Idx_' + MenuIdx + '" value=' + formatDate(getDateFromString(MenuEntry.Moment),false) + ' size="15" /></p>';
        trHtml += '<p>Menu Time - Hour: <input type="text" id="txtMenuMomentHour_Idx_' + MenuIdx + '" value=' + getDateFromString(MenuEntry.Moment).getUTCHours() + ' size="15" /></p>';
        trHtml += '<p>Menu Time - Minute: <input type="text" id="txtMenuMomentMinute_Idx_' + MenuIdx + '" value=' + getDateFromString(MenuEntry.Moment).getUTCMinutes() + ' size="15" /></p>';
        trHtml += '<p><b>Number of calories: <input type="text" id="txtMenuCalories_Idx_' + MenuIdx + '" value=' + MenuEntry.NumCalories + ' size="15" /></b></p>';

        if (MenuEntry.Owner != null)
            trHtml += '<p>Owner: '+ MenuEntry.Owner.DisplayName+'. (you cannot edit owner)</p>';
        else
            trHtml += '<p>Owner: ' + userAuthenticated.DisplayName + '. (you cannot edit owner)</p>';

        trHtml += '<input type="button" id="btnEditMenu_Idx_' + MenuIdx + '" value="Confirm" onclick="doEditMenu(' + MenuIdx + ', false);" class="clsBtnTableButton"/>';
        trHtml += '<input type="button" id="btnDeleteMenu_Idx_' + MenuIdx + '" value="Delete Menu" onclick="doEditMenu(' + MenuIdx + ', true);" class="clsBtnTableButton"/>';
        trHtml += '<input type="button" id="btnCancelEditMenu_Idx_' + MenuIdx + '" value="Cancel" onclick="cancelEditMenu(' + MenuIdx + ');" class="clsBtnTableButton"/>';

        trHtml += '</td>';
    }

    if (bBuildTrTag)
        trHtml += "</tr>";

    return trHtml;
}

function doStartEditMenu(MenuIdx) {
    if (MenuIdx != -1)
        document.getElementById('trMenus_Idx_' + MenuIdx).innerHTML = buildMenuPresentation(MenuIdx, MenuEntriesRetreived[MenuIdx], false, true);
    else
        document.getElementById('trMenus_Idx_' + MenuIdx).innerHTML = buildMenuPresentation(MenuIdx, null, false, true);
}

function cancelEditMenu(MenuIdx) {
    if (MenuIdx != -1)
        document.getElementById('trMenus_Idx_' + MenuIdx).innerHTML = buildMenuPresentation(MenuIdx, MenuEntriesRetreived[MenuIdx], false, false);
    else
        document.getElementById('trMenus_Idx_' + MenuIdx).innerHTML = buildMenuPresentation(MenuIdx, null, false, false);
}

function doEditMenu(MenuIdx, bDoDelete) {
    var MenuID = -1;
    var ownerID = -1;
    var muEntry = null;

    if (MenuIdx != -1) {
        var muEntry = MenuEntriesRetreived[MenuIdx]; 
        MenuID = muEntry.EntryID;
        if (muEntry.Owner != null) //Should always be true
            ownerID = muEntry.Owner.UserID;
    }
    else {
        ownerID = userAuthenticated.UserID;
    }

    
    var reqText = null;
    var reqMomentDate = null;
    var reqMomentHour = null;
    var reqMomentMinute = null;
    var reqCalories = null;

    try{
        reqText = document.getElementById('txtMenuText_Idx_' + MenuIdx).value;
        reqMomentDate = new Date(document.getElementById('dtMenuMomentDate_Idx_' + MenuIdx).value);
        reqMomentHour = parseInt(document.getElementById('txtMenuMomentHour_Idx_' + MenuIdx).value);
        if (reqMomentHour < 0 || reqMomentHour > 23)
            throw new Exception();
        reqMomentMinute = parseInt(document.getElementById('txtMenuMomentMinute_Idx_' + MenuIdx).value);
        if (reqMomentMinute < 0 || reqMomentMinute > 59)
            throw new Exception();
        reqCalories = parseInt(document.getElementById('txtMenuCalories_Idx_' + MenuIdx).value);
        if (reqCalories < 0)
            throw new Exception();

        if (!(reqCalories <= reqCalories) || reqMomentDate == null || !(reqMomentHour <= reqMomentHour) || !(reqMomentMinute <= reqMomentMinute))
            throw new Exception();
    } catch(ex)
    {
        window.alert('Menu details have not been filled out properly!');
        return;
    }

    var editCtrl = document.getElementById('btnEditMenu_Idx_' + MenuIdx);
    var deleteCtrl = document.getElementById('btnDeleteMenu_Idx_' + MenuIdx);

    editCtrl.disabled = true;
    deleteCtrl.disabled = true;

    var requestUri = serverUri + "/Menus/" + MenuID + "/" + ownerID + "/" + reqText + "/" +reqMomentDate.getFullYear() + "/" + (reqMomentDate.getMonth()+1) + "/" + reqMomentDate.getDate() + "/" + reqMomentHour + "/" + reqMomentMinute + "/" + reqCalories;
    var requestType = "PUT";
    if (bDoDelete)
    {
        requestUri = serverUri + "/Menus/" + MenuID;
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
                        if (MenuID == -1) {
                            MenuEntriesRetreived.push(obj.Result.NewlyCreatedObjectsWithIds[0]);
                            MenuIdx = MenuEntriesRetreived.length - 1;
                            muEntry = MenuEntriesRetreived[MenuIdx];
                            var MenusTable = document.getElementById('tblMenus');
                            var row = MenusTable.insertRow(MenusTable.rows.length - 1);
                            row.id = 'trMenus_Idx_' + MenuIdx;
                            row.tag = MenuIdx;
                        }
                        else {
                            muEntry = MenuEntriesRetreived[MenuIdx];
                            muEntry.Text = reqText;
                            muEntry.Moment = getNetDateStringFromDate(new Date(reqMomentDate.getFullYear(), reqMomentDate.getMonth(), reqMomentDate.getDate(), reqMomentHour, reqMomentMinute));
                            muEntry.NumCalories = reqCalories;
                        }
                    }
                    else {
                        if (MenuIdx != -1) {
                            MenuEntriesRetreived[MenuIdx] = null;
                        }
                    }
                    document.getElementById('trMenus_Idx_' + MenuIdx).innerHTML = buildMenuPresentation(MenuIdx, MenuEntriesRetreived[MenuIdx], false, false);
                }

                loadMenus(); //Just refresh ALL the menus because a single edit can generate color code changes to many entries.
            }
            editCtrl.disabled = false;
            deleteCtrl.disabled = false;

            statusBarText = "Server said: " + obj.Message;
            document.getElementById('pStatusBarText').innerText = statusBarText;
        },
        error: function (jqXHR, textStatus, errorThrown) {
            editCtrl.disabled = false;
            deleteCtrl.disabled = false;

            displayError(jqXHR, errorThrown);
        }
    });
}