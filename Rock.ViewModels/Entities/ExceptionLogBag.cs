//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;
using System.Linq;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Entities
{
    /// <summary>
    /// ExceptionLog View Model
    /// </summary>
    public partial class ExceptionLogBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a table containing the session cookies from the client when the exception occurred.
        /// </summary>
        /// <value>
        /// A System.String containing the session cooks from the client when the exception occurred
        /// </value>
        public string Cookies { get; set; }

        /// <summary>
        /// Gets or sets a message that describes the exception.
        /// </summary>
        /// <value>
        /// A System.String representing the description of the exception.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type (exception class) of the exception that occurred. i.e. System.Data.SqlClient.SqlException
        /// </summary>
        /// <value>
        /// A System.String representing the type name of the exception that occurred. 
        /// </value>
        public string ExceptionType { get; set; }

        /// <summary>
        /// Gets or sets a table containing all the form items from the page request where the exception occurred.
        /// </summary>
        /// <value>
        /// A System.String representing a table containing the value of the form items posted during the page request.
        /// </value>
        public string Form { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this exception has a child/inner exception. 
        /// </summary>
        /// <value>
        /// A System.Boolean value that will be true if the exception has an inner exception otherwise false or null.
        /// </value>
        public bool? HasInnerException { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Rock.Model.Page that the exception occurred on.
        /// </summary>
        /// <value>
        /// A System.Int32 representing the Id of the Rock.Model.Page that the exception occurred on. 
        /// If this exception did not occur on a Rock.Model.Page this value will be null.
        /// </value>
        public int? PageId { get; set; }

        /// <summary>
        /// Gets or sets the relative URL of the page that the exception occurred on.
        /// </summary>
        /// <value>
        /// A System.String representing the URL of the Rock.Model.Page that the exception occurred on. 
        /// </value>
        public string PageUrl { get; set; }

        /// <summary>
        /// Gets or sets the Id of the parent/outer ExceptionLog entity (if it exists). ExceptionLog entities are hierarchical.
        /// </summary>
        /// <value>
        /// An System.Int32 representing the Id of the parent ExceptionId. If this ExceptionLog entity does not have a parent exception,
        /// will be null.
        /// </value>
        public int? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the full query string from the page that the exception occurred on.
        /// </summary>
        /// <value>
        /// A System.String representing the URL Query String from the page that threw the exception.
        /// </value>
        public string QueryString { get; set; }

        /// <summary>
        /// Gets or sets a table of the ServerVariables at the time that the exception occurred.
        /// </summary>
        /// <value>
        /// A System.String containing a table of the ServerVariables at the time the exception occurred.
        /// </value>
        public string ServerVariables { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Rock.Model.Site that the exception occurred on. If this did not occur on a site (i.e. a job) this value will be null.
        /// </summary>
        /// <value>
        /// A System.Int32 representing the Id of Rock.Model.Site that this exception occurred on.
        /// </value>
        public int? SiteId { get; set; }

        /// <summary>
        /// Gets or sets the name of the application or the object that causes the error.
        /// </summary>
        /// <value>
        /// A System.String representing the class type name/application that threw the exception.
        /// </value>
        public string Source { get; set; }

        /// <summary>
        /// Gets a string representation of the immediate frames on the call stack.
        /// </summary>
        /// <value>
        /// A System.String representing the StackTrace of the exception that occurred.
        /// </value>
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the StatusCode that was returned and describes the type of error.  
        /// </summary>
        /// <value>
        /// A System.String value representing the StatusCode that was returned as part of this exception. If a StatusCode was returned
        /// this value will be null.
        /// </value>
        public string StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the created by person alias identifier.
        /// </summary>
        /// <value>
        /// The created by person alias identifier.
        /// </value>
        public int? CreatedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the modified by person alias identifier.
        /// </summary>
        /// <value>
        /// The modified by person alias identifier.
        /// </value>
        public int? ModifiedByPersonAliasId { get; set; }

    }
}
