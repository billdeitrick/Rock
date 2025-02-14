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
    /// PageRoute View Model
    /// </summary>
    public partial class PageRouteBag : EntityBagBase
    {
        /// <summary>
        /// If true then the route will be accessible from all sites regardless if EnableExclusiveRoutes is set on the site
        /// </summary>
        /// <value>
        ///   true if this instance is global; otherwise, false.
        /// </value>
        public bool IsGlobal { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the PageRoute is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A System.Boolean value that is true if the PageRoute is part of the Rock core system/framework, otherwise false.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Rock.Model.Page that the PageRoute is linked to. This property is required.
        /// </summary>
        /// <value>
        /// A System.Int32 containing the Id of the Rock.Model.Page that the PageRoute is linked to.
        /// </value>
        public int PageId { get; set; }

        /// <summary>
        /// Gets or sets the format of the route path. Route examples include: Page NewAccount or Checkin/Welcome. 
        /// A specific group Group/{GroupId} (i.e. Group/16). A person's history Person/{PersonId}/History (i.e. Person/12/History).
        /// This property is required.
        /// </summary>
        /// <value>
        /// A System.String containing the format of the RoutePath.
        /// </value>
        public string Route { get; set; }

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
