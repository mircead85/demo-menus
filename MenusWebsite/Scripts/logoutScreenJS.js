function initScreenLogout() {
    $('#divLogout').show();
    activeDiv = "divLogout";
    
    bRememberMe = false;

    document.getElementById('txtUsername').value = '';
    document.getElementById('txtPassword').value = '';

    setWebsiteCookie();

    doLogout();
}


function doLogout()
{
    var requestUri = serverUri + "/Logout/";
    $.ajax({
        url: requestUri,
        type: "GET",
        beforeSend: function (xhr) {
            xhr.setRequestHeader("Authorization", "Basic " + btoa(userName + ":" + userPassword));
        },
        success: function (result) {
            var obj = result;
            if (obj.HasError == true) {
                
            }
            else {
                userAuthenticated = null;
                userName = null;
                userPassword = null;
            }

            if (userAuthenticated == null) {
                fncInitClientDataModel(true);
                userLoggedIn = false;
                setWebsiteCookie();

                fncInitClientDataModel(true);
                fncRefreshMenuItems();
            }

            statusBarText = "Server said: " + obj.Message;
            document.getElementById('pStatusBarText').innerText = statusBarText;
        },
        error: function (jqXHR, textStatus, errorThrown) {
            document.getElementById('btnLogin').disabled = false;
            displayError(jqXHR, errorThrown);
        }
    });
}
