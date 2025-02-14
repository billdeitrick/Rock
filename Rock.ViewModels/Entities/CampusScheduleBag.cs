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
    /// CampusSchedule View Model
    /// </summary>
    public partial class CampusScheduleBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the Id of the Rock.Model.Campus that is associated with this CampusSchedule. This property is required.
        /// </summary>
        /// <value>
        /// An System.Int32 representing the Id of the Rock.Model.Campus that this CampusSchedule is associated with.
        /// </value>
        public int CampusId { get; set; }

        /// <summary>
        /// Gets or sets the display order of the CampusSchedule in the campus schedule list. The lower the number the higher the 
        /// display priority this CampusSchedule has. This property is required.
        /// </summary>
        /// <value>
        /// A System.Int32 representing the display order of the CampusSchedule.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Rock.Model.Schedule that is associated with this CampusSchedule. This property is required.
        /// </summary>
        /// <value>
        /// An System.Int32 referencing the Id of the Rock.Model.Schedule that is associated with this CampusSchedule. 
        /// </value>
        public int ScheduleId { get; set; }

        /// <summary>
        /// The Id of the ScheduleType Rock.Model.DefinedValue that is used to identify the type of Rock.Model.CampusSchedule
        /// that this is. This property is required.
        /// </summary>
        /// <value>
        /// An System.Int32 referencing the Id of the ScheduleType Rock.Model.DefinedValue that identifies the type of schedule that this is.
        /// If a ScheduleType Rock.Model.DefinedValue is not associated with this CampusSchedule this value will be null.
        /// </value>
        public int? ScheduleTypeValueId { get; set; }

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
