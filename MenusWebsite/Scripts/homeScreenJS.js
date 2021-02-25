

function initScreenHome()
{
    if (userAuthenticated == null)
    {
        userWelcomeMessage = userWelcomeMessageDEFAULT;
    }
    else
    {
        userWelcomeMessage = "Hi there, ";
        userWelcomeMessage += userAuthenticated.DisplayName;
        userWelcomeMessage += "! <br/>";
        userWelcomeMessage += "Welcome back! You enjoy the following roles: ";
        var userRoles = userAuthenticated.UserRoles;
        userWelcomeMessage += buildUserRoles(userAuthenticated);

        userWelcomeMessage += "<br/>Your roles enable you to:<br/>"
        if (userIsAdmin)
            userWelcomeMessage += "You are Admin: yes; You can edit Roles: yes;<br/>";
        else
            userWelcomeMessage += "You are Admin: no; You can edit Roles: no;<br/>";

        if (userCanCUDUsers)
            userWelcomeMessage += "You can edit other users: yes;<br/>";
        else
            userWelcomeMessage += "You can edit other users: no;<br/>";

        if (userCanCUDOthersMenus)
            userWelcomeMessage += "You can edit other users' Menu entries: yes;<br/>";
        else
            userWelcomeMessage += "You can edit other users' Menu entries: no;<br/>";

        userWelcomeMessage += "You can edit your own Menus: yes.<br/>";


        userWelcomeMessage += "<br/>";

        userWelcomeMessage += "<b>Expected Number of Calories per day for you: ";
        if (userAuthenticated.ExpectedNumCalories == null)
            userWelcomeMessage += " --- not set ---.";
        else
            userWelcomeMessage += userAuthenticated.ExpectedNumCalories;

        userWelcomeMessage += "</b><br/>";
    }

    document.getElementById('pWelcomeUserMsg').innerHTML = userWelcomeMessage;
    $('#divHome').show();
    activeDiv = "divHome";

    if(userAuthenticated != null)
        setWebsiteCookie();
}