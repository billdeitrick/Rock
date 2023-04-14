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
using System.Collections.Generic;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.FamilyPreRegistration
{
    /// <summary>
    /// The box that contains all the initialization information for the Family Pre-Registration block.
    /// </summary>
    public class FamilyPreRegistrationInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the adult 1 information .
        /// </summary>
        public FamilyPreRegistrationPersonBag Adult1 { get; set; }

        /// <summary>
        /// Gets or sets the adult 2 information.
        /// </summary>
        public FamilyPreRegistrationPersonBag Adult2 { get; set; }

        /// <summary>
        /// Filters the campus field by campus types.
        /// </summary>
        public List<Guid> CampusTypesFilter { get; set; }

        /// <summary>
        /// Filters the campus field by campus statuses.
        /// </summary>
        public List<Guid> CampusStatusesFilter { get; set; }

        /// <summary>
        /// Gets or sets the attribute unique identifier used to select campus schedules.
        /// </summary>
        public Guid? CampusSchedulesAttributeGuid { get; set; }

        /// <summary>
        /// The number of columns used to display the form.
        /// </summary>
        public int Columns { get; set; }

        /// <summary>
        /// The campus unique identifier to use by default when adding a new family.
        /// </summary>
        public Guid? DefaultCampusGuid { get; set; }

        /// <summary>
        /// Indicates whether the campus field is hidden.
        /// <para>If there is only one active campus then the campus field will not show.</para>
        /// </summary>
        public bool IsCampusHidden { get; set; }

        /// <summary>
        /// Indicates whether the campus field is optional.
        /// <para>If the campus field is required and there is only one active campus, then the campus field will not show and the single campus will be selected.</para>
        /// </summary>
        public bool IsCampusOptional { get; set; }

        public bool IsPlannedSchedulePanelHidden { get; set; }
        public bool IsPlannedVisitDatePanelHidden { get; set; }
        public bool IsPlannedVisitDateOptional { get; set; }
        public bool IsAdultMobilePhoneOptional { get; set; }
        public bool IsAdultMobilePhoneHidden { get; set; }
        public bool IsAdultProfilePhotoOptional { get; set; }
        public bool IsAdultProfilePhotoHidden { get; set; }
        public string CreateAccountTitle { get; set; }
        public string CreateAccountDescription { get; set; }
        public bool IsCreateAccountOptional { get; set; }
        public bool IsCreateAccountHidden { get; set; }
        public bool IsAddressOptional { get; set; }
        public bool IsAddressHidden { get; set; }
        public Dictionary<string, PublicAttributeBag> FamilyAttributes { get; set; }
        public Dictionary<string, string> FamilyAttributeValues { get; set; }
        public bool IsAdultGenderOptional { get; set; }
        public bool IsAdultGenderHidden { get; set; }
        public bool IsAdultSuffixOptional { get; set; }
        public bool IsAdultSuffixHidden { get; set; }
        public bool IsAdultBirthdayOptional { get; set; }
        public bool IsAdultBirthdayHidden { get; set; }
        public bool IsAdultEmailOptional { get; set; }
        public bool IsAdultEmailHidden { get; set; }
        public bool IsAdultMaritalStatusOptional { get; set; }
        public bool IsAdultDisplayCommunicationPreferenceHidden { get; set; }
        public bool IsAdultDisplayCommunicationPreferenceOptional { get; set; }
        public bool IsAdultMaritalStatusHidden { get; set; }
        public bool IsRaceOptionOptional { get; set; }
        public bool IsRaceOptionHidden { get; set; }
        public bool IsEthnicityOptionOptional { get; set; }
        public bool IsEthnicityOptionHidden { get; set; }
        public List<ListItemBag> ChildRelationshipTypes { get; set; }
        public bool IsChildSuffixHidden { get; set; }
        public bool IsChildGenderOptional { get; set; }
        public bool IsChildGenderHidden { get; set; }
        public bool IsChildBirthDateHidden { get; set; }
        public bool IsChildBirthDateOptional { get; set; }
        public bool IsChildGradeOptional { get; set; }
        public bool IsChildGradeHidden { get; set; }
        public bool IsChildMobilePhoneHidden { get; set; }
        public bool IsChildEmailHidden { get; set; }
        public bool IsChildDisplayCommunicationPreferenceHidden { get; set; }
        public bool IsChildProfilePhotoHidden { get; set; }
        public bool IsChildRaceHidden { get; set; }
        public bool IsChildEthnicityHidden { get; set; }
        public bool IsChildMobilePhoneOptional { get; set; }
        public bool IsChildEmailOptional { get; set; }
        public bool IsChildProfilePhotoOptional { get; set; }
        public bool IsChildRaceOptional { get; set; }
        public bool IsChildEthnicityOptional { get; set; }
        public Dictionary<string, PublicAttributeBag> ChildAttributes { get; set; }
        public Dictionary<string, string> ChildAttributeValuesTemplate { get; set; }
        public List<FamilyPreRegistrationPersonBag> Children { get; set; }
        public Guid FamilyGuid { get; set; }
        public AddressControlBag Address { get; set; }
    }
}
