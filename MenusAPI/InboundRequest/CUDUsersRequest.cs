/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{
    /// <summary>
    /// Requests that the specified users are created/updated/deleted as needed, *along with all associated data*, if such data is specified.
    /// </summary>
    [DataContract]
    public class CUDUsersRequest:CUDRequest
    {
        public override UserRequestTypes TypeOfThisRequest
        {
            get
            {
                return UserRequestTypes.CUDUsers;
            }
        }

        /// <summary>
        /// The users which are to be subject to operations.
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<UserBO> UsersAndDataToCUD
        {
            get
            {
                return ObjectsToCUD.OfType<UserBO>();
            }
        }

        [DataMember]
        public bool JustUpdateAssociationForReferencedRoles { get; set; }
    }
}
