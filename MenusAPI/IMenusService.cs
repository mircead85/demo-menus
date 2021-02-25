/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MenusAPI
{
    [ServiceContract]
    public interface IMenusService
    {
        /// <summary>
        /// Creates the database structure (tables, etc.) required for operation of the service.
        /// If the structure already exists, it is first deleted, resulting in a clean new database.
        /// </summary>
        [OperationContract]
        APIReply ResetDatabase(ResetDatabaseRequest request);

        /// <summary>
        /// Checks wheather the database is accesible and has required elements for operation.
        /// </summary>
        /// <returns>True if the database is accesible and appears to have required elements for operation.</returns>
        [OperationContract]
        APIReply IsDatabaseUp(IsDatabaseUpRequest request);

        /// <summary>
        /// Tries to authenticate the user with the specified credentials and returns the User business object if succesful.
        /// </summary>
        /// <returns>The User business object if authentication with the specified credentials was successful is returned as the sole object in the ReadObjectsReply.</returns>
        [OperationContract]
        AuthenticateUserReply AuthenticateUser(AuthenticateUserRequest request);

        /// <summary>
        /// Logs out the current user. It might log out from all tokens.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [OperationContract]
        APIReply LogoutUser(LogoutUserRequest request);

        [OperationContract]
        APIReply CreateNewAccount(CreateNewAccountRequest request);

        /// <summary>
        /// Performs as need creation, updating or removal of specified users, along with all their associated data, such as Roles and Menus, if this data is specified (except for removal, where all associated data is removed). 
        /// </summary>
        /// <param name="affectedUsers">The collection of users which are to be subject to creation/updating/removal.</param>
        [OperationContract]
        CUDOperationsReply CUDUsers(CUDUsersRequest request);

        [OperationContract]
        CUDOperationsReply CUDMenus(CUDMenusRequest request);

        [OperationContract]
        CUDOperationsReply CUDRoles(CUDRolesRequest request);

        [OperationContract]
        ReadObjectsReply ReadLog(ReadLogRequest request);

        [OperationContract]
        ReadObjectsReply ReadMenus(ReadMenusRequest request);

        [OperationContract]
        ReadObjectsReply ReadUsers(ReadUsersRequest request);

        [OperationContract]
        ReadObjectsReply ReadRoles(ReadRolesRequest request);
    }
    
}
