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
    /// StreakType View Model
    /// </summary>
    public partial class StreakTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a description of the Streak Type.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// This determines whether the streak type will write attendance records when marking someone as present or
        /// if it will just update the enrolled individual’s map.
        /// </summary>
        public bool EnableAttendance { get; set; }

        /// <summary>
        /// Gets or sets the first day of the week for Rock.Model.StreakOccurrenceFrequency.Weekly streak type calculations.
        /// Leave this null to assume the system setting, which is accessed via Rock.RockDateTime.FirstDayOfWeek.
        /// </summary>
        /// <value>
        /// The first day of week.
        /// </value>
        public int? FirstDayOfWeek { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name of the Streak Type. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the timespan that each map bit represents (Rock.Model.StreakOccurrenceFrequency).
        /// </summary>
        public int OccurrenceFrequency { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this streak type requires explicit enrollment. If not set, a person can be
        /// implicitly enrolled through attendance.
        /// </summary>
        public bool RequiresEnrollment { get; set; }

        /// <summary>
        /// Gets or sets the System.DateTime associated with the least significant bit of all maps in this streak type.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Entity associated with attendance for this streak type. If not set, this streak type
        /// will account for any attendance record.
        /// </summary>
        public int? StructureEntityId { get; set; }

        /// <summary>
        /// Gets or sets the structure settings JSON.
        /// </summary>
        /// <value>
        /// The structure settings JSON.
        /// </value>
        public string StructureSettingsJSON { get; set; }

        /// <summary>
        /// Gets or sets the attendance association (Rock.Model.StreakStructureType). If not set, this streak type
        /// will not be associated with attendance.
        /// </summary>
        public int? StructureType { get; set; }

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
