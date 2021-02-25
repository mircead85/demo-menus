﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MenusWebsite.ServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference.IMenusService")]
    public interface IMenusService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/ResetDatabase", ReplyAction="http://tempuri.org/IMenusService/ResetDatabaseResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.ReadObjectsReply))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.AuthenticateUserReply))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.CUDOperationsReply))]
        MenusAPI.APIReply ResetDatabase(MenusAPI.ResetDatabaseRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/ResetDatabase", ReplyAction="http://tempuri.org/IMenusService/ResetDatabaseResponse")]
        System.Threading.Tasks.Task<MenusAPI.APIReply> ResetDatabaseAsync(MenusAPI.ResetDatabaseRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/IsDatabaseUp", ReplyAction="http://tempuri.org/IMenusService/IsDatabaseUpResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.ReadObjectsReply))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.AuthenticateUserReply))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.CUDOperationsReply))]
        MenusAPI.APIReply IsDatabaseUp(MenusAPI.IsDatabaseUpRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/IsDatabaseUp", ReplyAction="http://tempuri.org/IMenusService/IsDatabaseUpResponse")]
        System.Threading.Tasks.Task<MenusAPI.APIReply> IsDatabaseUpAsync(MenusAPI.IsDatabaseUpRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/AuthenticateUser", ReplyAction="http://tempuri.org/IMenusService/AuthenticateUserResponse")]
        MenusAPI.AuthenticateUserReply AuthenticateUser(MenusAPI.AuthenticateUserRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/AuthenticateUser", ReplyAction="http://tempuri.org/IMenusService/AuthenticateUserResponse")]
        System.Threading.Tasks.Task<MenusAPI.AuthenticateUserReply> AuthenticateUserAsync(MenusAPI.AuthenticateUserRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/LogoutUser", ReplyAction="http://tempuri.org/IMenusService/LogoutUserResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.ReadObjectsReply))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.AuthenticateUserReply))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.CUDOperationsReply))]
        MenusAPI.APIReply LogoutUser(MenusAPI.LogoutUserRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/LogoutUser", ReplyAction="http://tempuri.org/IMenusService/LogoutUserResponse")]
        System.Threading.Tasks.Task<MenusAPI.APIReply> LogoutUserAsync(MenusAPI.LogoutUserRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/CreateNewAccount", ReplyAction="http://tempuri.org/IMenusService/CreateNewAccountResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.ReadObjectsReply))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.AuthenticateUserReply))]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.CUDOperationsReply))]
        MenusAPI.APIReply CreateNewAccount(MenusAPI.CreateNewAccountRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/CreateNewAccount", ReplyAction="http://tempuri.org/IMenusService/CreateNewAccountResponse")]
        System.Threading.Tasks.Task<MenusAPI.APIReply> CreateNewAccountAsync(MenusAPI.CreateNewAccountRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/CUDUsers", ReplyAction="http://tempuri.org/IMenusService/CUDUsersResponse")]
        MenusAPI.CUDOperationsReply CUDUsers(MenusAPI.CUDUsersRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/CUDUsers", ReplyAction="http://tempuri.org/IMenusService/CUDUsersResponse")]
        System.Threading.Tasks.Task<MenusAPI.CUDOperationsReply> CUDUsersAsync(MenusAPI.CUDUsersRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/CUDMenus", ReplyAction="http://tempuri.org/IMenusService/CUDMenusResponse")]
        MenusAPI.CUDOperationsReply CUDMenus(MenusAPI.CUDMenusRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/CUDMenus", ReplyAction="http://tempuri.org/IMenusService/CUDMenusResponse")]
        System.Threading.Tasks.Task<MenusAPI.CUDOperationsReply> CUDMenusAsync(MenusAPI.CUDMenusRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/CUDRoles", ReplyAction="http://tempuri.org/IMenusService/CUDRolesResponse")]
        MenusAPI.CUDOperationsReply CUDRoles(MenusAPI.CUDRolesRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/CUDRoles", ReplyAction="http://tempuri.org/IMenusService/CUDRolesResponse")]
        System.Threading.Tasks.Task<MenusAPI.CUDOperationsReply> CUDRolesAsync(MenusAPI.CUDRolesRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/ReadLog", ReplyAction="http://tempuri.org/IMenusService/ReadLogResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.AuthenticateUserReply))]
        MenusAPI.ReadObjectsReply ReadLog(MenusAPI.ReadLogRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/ReadLog", ReplyAction="http://tempuri.org/IMenusService/ReadLogResponse")]
        System.Threading.Tasks.Task<MenusAPI.ReadObjectsReply> ReadLogAsync(MenusAPI.ReadLogRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/ReadMenus", ReplyAction="http://tempuri.org/IMenusService/ReadMenusResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.AuthenticateUserReply))]
        MenusAPI.ReadObjectsReply ReadMenus(MenusAPI.ReadMenusRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/ReadMenus", ReplyAction="http://tempuri.org/IMenusService/ReadMenusResponse")]
        System.Threading.Tasks.Task<MenusAPI.ReadObjectsReply> ReadMenusAsync(MenusAPI.ReadMenusRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/ReadUsers", ReplyAction="http://tempuri.org/IMenusService/ReadUsersResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.AuthenticateUserReply))]
        MenusAPI.ReadObjectsReply ReadUsers(MenusAPI.ReadUsersRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/ReadUsers", ReplyAction="http://tempuri.org/IMenusService/ReadUsersResponse")]
        System.Threading.Tasks.Task<MenusAPI.ReadObjectsReply> ReadUsersAsync(MenusAPI.ReadUsersRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/ReadRoles", ReplyAction="http://tempuri.org/IMenusService/ReadRolesResponse")]
        [System.ServiceModel.ServiceKnownTypeAttribute(typeof(MenusAPI.AuthenticateUserReply))]
        MenusAPI.ReadObjectsReply ReadRoles(MenusAPI.ReadRolesRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMenusService/ReadRoles", ReplyAction="http://tempuri.org/IMenusService/ReadRolesResponse")]
        System.Threading.Tasks.Task<MenusAPI.ReadObjectsReply> ReadRolesAsync(MenusAPI.ReadRolesRequest request);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IMenusServiceChannel : MenusWebsite.ServiceReference.IMenusService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class MenusServiceClient : System.ServiceModel.ClientBase<MenusWebsite.ServiceReference.IMenusService>, MenusWebsite.ServiceReference.IMenusService {
        
        public MenusServiceClient() {
        }
        
        public MenusServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public MenusServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MenusServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MenusServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public MenusAPI.APIReply ResetDatabase(MenusAPI.ResetDatabaseRequest request) {
            return base.Channel.ResetDatabase(request);
        }
        
        public System.Threading.Tasks.Task<MenusAPI.APIReply> ResetDatabaseAsync(MenusAPI.ResetDatabaseRequest request) {
            return base.Channel.ResetDatabaseAsync(request);
        }
        
        public MenusAPI.APIReply IsDatabaseUp(MenusAPI.IsDatabaseUpRequest request) {
            return base.Channel.IsDatabaseUp(request);
        }
        
        public System.Threading.Tasks.Task<MenusAPI.APIReply> IsDatabaseUpAsync(MenusAPI.IsDatabaseUpRequest request) {
            return base.Channel.IsDatabaseUpAsync(request);
        }
        
        public MenusAPI.AuthenticateUserReply AuthenticateUser(MenusAPI.AuthenticateUserRequest request) {
            return base.Channel.AuthenticateUser(request);
        }
        
        public System.Threading.Tasks.Task<MenusAPI.AuthenticateUserReply> AuthenticateUserAsync(MenusAPI.AuthenticateUserRequest request) {
            return base.Channel.AuthenticateUserAsync(request);
        }
        
        public MenusAPI.APIReply LogoutUser(MenusAPI.LogoutUserRequest request) {
            return base.Channel.LogoutUser(request);
        }
        
        public System.Threading.Tasks.Task<MenusAPI.APIReply> LogoutUserAsync(MenusAPI.LogoutUserRequest request) {
            return base.Channel.LogoutUserAsync(request);
        }
        
        public MenusAPI.APIReply CreateNewAccount(MenusAPI.CreateNewAccountRequest request) {
            return base.Channel.CreateNewAccount(request);
        }
        
        public System.Threading.Tasks.Task<MenusAPI.APIReply> CreateNewAccountAsync(MenusAPI.CreateNewAccountRequest request) {
            return base.Channel.CreateNewAccountAsync(request);
        }
        
        public MenusAPI.CUDOperationsReply CUDUsers(MenusAPI.CUDUsersRequest request) {
            return base.Channel.CUDUsers(request);
        }
        
        public System.Threading.Tasks.Task<MenusAPI.CUDOperationsReply> CUDUsersAsync(MenusAPI.CUDUsersRequest request) {
            return base.Channel.CUDUsersAsync(request);
        }
        
        public MenusAPI.CUDOperationsReply CUDMenus(MenusAPI.CUDMenusRequest request) {
            return base.Channel.CUDMenus(request);
        }
        
        public System.Threading.Tasks.Task<MenusAPI.CUDOperationsReply> CUDMenusAsync(MenusAPI.CUDMenusRequest request) {
            return base.Channel.CUDMenusAsync(request);
        }
        
        public MenusAPI.CUDOperationsReply CUDRoles(MenusAPI.CUDRolesRequest request) {
            return base.Channel.CUDRoles(request);
        }
        
        public System.Threading.Tasks.Task<MenusAPI.CUDOperationsReply> CUDRolesAsync(MenusAPI.CUDRolesRequest request) {
            return base.Channel.CUDRolesAsync(request);
        }
        
        public MenusAPI.ReadObjectsReply ReadLog(MenusAPI.ReadLogRequest request) {
            return base.Channel.ReadLog(request);
        }
        
        public System.Threading.Tasks.Task<MenusAPI.ReadObjectsReply> ReadLogAsync(MenusAPI.ReadLogRequest request) {
            return base.Channel.ReadLogAsync(request);
        }
        
        public MenusAPI.ReadObjectsReply ReadMenus(MenusAPI.ReadMenusRequest request) {
            return base.Channel.ReadMenus(request);
        }
        
        public System.Threading.Tasks.Task<MenusAPI.ReadObjectsReply> ReadMenusAsync(MenusAPI.ReadMenusRequest request) {
            return base.Channel.ReadMenusAsync(request);
        }
        
        public MenusAPI.ReadObjectsReply ReadUsers(MenusAPI.ReadUsersRequest request) {
            return base.Channel.ReadUsers(request);
        }
        
        public System.Threading.Tasks.Task<MenusAPI.ReadObjectsReply> ReadUsersAsync(MenusAPI.ReadUsersRequest request) {
            return base.Channel.ReadUsersAsync(request);
        }
        
        public MenusAPI.ReadObjectsReply ReadRoles(MenusAPI.ReadRolesRequest request) {
            return base.Channel.ReadRoles(request);
        }
        
        public System.Threading.Tasks.Task<MenusAPI.ReadObjectsReply> ReadRolesAsync(MenusAPI.ReadRolesRequest request) {
            return base.Channel.ReadRolesAsync(request);
        }
    }
}
