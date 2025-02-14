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
    /// Interaction View Model
    /// </summary>
    public partial class InteractionBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the campaign name
        /// </summary>
        /// <value>
        /// The campaign.
        /// </value>
        public string Campaign { get; set; }

        /// <summary>
        /// Gets or sets the channel custom 1.
        /// </summary>
        /// <value>
        /// The channel custom 1.
        /// </value>
        public string ChannelCustom1 { get; set; }

        /// <summary>
        /// Gets or sets the channel custom 2.
        /// </summary>
        /// <value>
        /// The channel custom 2.
        /// </value>
        public string ChannelCustom2 { get; set; }

        /// <summary>
        /// Gets or sets the channel custom indexed 1.
        /// </summary>
        /// <value>
        /// The channel custom indexed 1.
        /// </value>
        public string ChannelCustomIndexed1 { get; set; }

        /// <summary>
        /// Gets or sets the campaign content.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity that this interaction component is tracking activity for.
        /// 
        /// <item>
        ///     <term>Page Views</term>
        ///     <description>EntityId is left null, Page is the Component, Site is the Channel</description></item>
        /// <item>
        ///     <term>Communication Recipient Activity</term>
        ///     <description>EntityId is the <see cref="T:Rock.Model.CommunicationRecipient" /> Id. Communication is the Component, single Channel</description></item>
        /// <item>
        ///     <term>Content Channel Activity</term>
        ///     <description>EntityId is left null, ContentChannel is the Component, single Channel</description></item>
        /// <item>
        ///     <term>Workflow Form Entry</term>
        ///     <description>EntityId is the <see cref="T:Rock.Model.Workflow" /> Id, WorkflowType is the Component, single Channel </description></item>
        /// </summary>
        /// <value>
        /// A System.Int32 representing the Id of the entity (object) that this interaction component is related to.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Rock.Model.InteractionComponent Component that is associated with this Interaction.
        /// </summary>
        /// <value>
        /// An System.Int32 representing the Id of the Rock.Model.InteractionComponent component that this Interaction is associated with.
        /// </value>
        public int InteractionComponentId { get; set; }

        /// <summary>
        /// Gets or sets the interaction data.
        /// </summary>
        /// <value>
        /// The interaction data.
        /// </value>
        public string InteractionData { get; set; }

        /// <summary>
        /// Gets or sets the interaction datetime.
        /// </summary>
        /// <value>
        /// The interaction datetime.
        /// </value>
        public DateTime InteractionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the interaction end date time.
        /// </summary>
        /// <value>
        /// The interaction end date time.
        /// </value>
        public DateTime? InteractionEndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the length of time (or percent of time) of the interaction.
        /// The units on this depend on the InteractionChannel, which might have this be a Percent, Days, Seconds, Minutes, etc
        /// For example, if this interaction type is watching a video, this might be what percent of the video they watched
        /// </summary>
        /// <value>
        /// The length of the interaction.
        /// </value>
        public double? InteractionLength { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Rock.Model.InteractionSession Session that that is associated with this Interaction.
        /// </summary>
        /// <value>
        /// An System.Int32 representing the Id of the Rock.Model.InteractionSession session that this Interaction is associated with.
        /// </value>
        public int? InteractionSessionId { get; set; }

        /// <summary>
        /// Gets or sets the interaction summary.
        /// </summary>
        /// <value>
        /// The interaction summary.
        /// </value>
        public string InteractionSummary { get; set; }

        /// <summary>
        /// Gets or sets the interaction time to serve.
        /// The units on this depend on the InteractionChannel, which might have this be a Percent, Days, Seconds, Minutes, etc.
        /// For example, if this is a page view, this would be how long (in seconds) it took for Rock to generate a response.
        /// </summary>
        /// <value>
        /// The interaction time to serve.
        /// </value>
        public double? InteractionTimeToServe { get; set; }

        /// <summary>
        /// Gets or sets the campaign medium.
        /// </summary>
        /// <value>
        /// The medium.
        /// </value>
        public string Medium { get; set; }

        /// <summary>
        /// Gets or sets the operation. For example: 'View', 'Opened', 'Click', 'Prayed', 'Form Viewed', 'Form Completed', 'Complete', 'Incomplete', 'Watch', 'Present'.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the personal device identifier.
        /// </summary>
        /// <value>
        /// The personal device identifier.
        /// </value>
        public int? PersonalDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the related entity identifier.
        /// </summary>
        /// <value>
        /// The related entity identifier.
        /// </value>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Gets or sets the related entity type identifier.
        /// </summary>
        /// <value>
        /// The related entity type identifier.
        /// </value>
        public int? RelatedEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the campaign source.
        /// </summary>
        /// <value>
        /// The source.
        /// </value>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the term(s).
        /// </summary>
        /// <value>
        /// The term.
        /// </value>
        public string Term { get; set; }

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
