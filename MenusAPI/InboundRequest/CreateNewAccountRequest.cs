/*Copyright (c) Mircea Digulescu, 2016. All rights reseverd*/ using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MenusAPI
{ 
    [DataContract]
    public class CreateNewAccountRequest : APIInboundRequest
    {
        /// <summary>
        /// A value used in conjuction with the current date to allow a fresh user to create an account.
        /// </summary>
        [DataMember]
        public int NumericalProof { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public int? ExpectedNumCalories;

        public override UserRequestTypes TypeOfThisRequest
        {
            get
            {
                return UserRequestTypes.CreateNewAccount;
            }
        }
    }
}
