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
    /// DefinedValue View Model
    /// </summary>
    public partial class DefinedValueBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the category identifier. This property is ignored if DefinedType.CategorizedValuesEnabled is disabled.
        /// </summary>
        /// <value>
        /// A System.Int32 representing the identifier of the Rock.Model.Category that this Defined Value belongs to.
        /// </value>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the DefinedTypeId of the Rock.Model.DefinedType that this DefinedValue belongs to. This property is required.
        /// </summary>
        /// <value>
        /// A System.Int32 representing the DefinedTypeId of the Rock.Model.DefinedType that this DefinedValue belongs to.
        /// </value>
        public int DefinedTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Description of the DefinedValue.
        /// </summary>
        /// <value>
        /// A System.String representing the description of the DefinedValue.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this DefinedValue is active.
        /// </summary>
        /// <value>
        ///   true if this instance is active; otherwise, false.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this DefinedValue is part of the Rock core system/framework. this property is required.
        /// </summary>
        /// <value>
        /// A System.Boolean that is true if it is part of the Rock core system/framework; otherwise false.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the sort and display order of the DefinedValue.  This is an ascending order, so the lower the value the higher the sort priority.
        /// </summary>
        /// <value>
        /// A System.Int32 representing the sort order of the DefinedValue.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the Value of the DefinedValue. This property is required.
        /// </summary>
        /// <value>
        /// A System.String that represents the Value of the DefinedValue.
        /// </value>
        public string Value { get; set; }

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
