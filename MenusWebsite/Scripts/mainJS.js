var uri = 'api/test';

var serverUri = 'api/MenusServiceBiding'

$(document).ready(function () {
    fncInitClientDataModel(true);
    
    fncHideAllDivs();
    fncRefreshMenuItems();
    if (getCookie("bRememberMe") == "true" && getCookie("bAutologin") == "true")
        fncMMLogin();
    else
        fncMMHome();
});
