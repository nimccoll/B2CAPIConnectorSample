//===============================================================================
// Microsoft FastTrack for Azure
// Azure AD B2C API Connector Sample
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================
using System;

namespace B2CAPIConnectorFunction
{
    public class Employee
    {
        public string EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string MiddleInitial { get; set; }
        public string LastName { get; set; }
        public int JobID { get; set; }
        public string JobTitle { get; set; }
        public int JobLevel { get; set; }
        public string PublisherID { get; set; }
        public string PublisherName { get; set; }
        public DateTime HireDate { get; set; }
        public string UserRole { get; set; }
    }
}
