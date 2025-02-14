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
    /// InteractionChannel View Model
    /// </summary>
    public partial class InteractionChannelBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the channel data.
        /// </summary>
        /// <value>
        /// The channel data.
        /// </value>
        public string ChannelData { get; set; }

        /// <summary>
        /// Gets or sets the channel detail template.
        /// </summary>
        /// <value>
        /// The channel detail template.
        /// </value>
        public string ChannelDetailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the channel entity identifier.
        /// Note, the ChannelEntityType is inferred based on what the ChannelTypeMediumValue is:
        /// 
        /// <item>
        ///     <term>Page Views (<see cref="F:Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE" />)</term>
        ///     <description><see cref="T:Rock.Model.Site" /> Id</description></item>
        /// <item>
        ///     <term>Communication Recipient Activity (<see cref="F:Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_COMMUNICATION" />)</term>
        ///     <description><see cref="T:Rock.Model.Communication" /> Id</description></item>
        /// <item>
        ///     <term>Content Channel Activity (<see cref="F:Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_CONTENTCHANNEL" />)</term>
        ///     <description><see cref="T:Rock.Model.ContentChannel" /> Id</description></item>
        /// <item>
        ///     <term>System Events, like Workflow Form Entry (<see cref="F:Rock.SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_SYSTEM_EVENTS" />)</term>
        ///     <description>null, only one Channel</description></item>
        /// </summary>
        /// <value>
        /// The channel entity identifier.
        /// </value>
        public int? ChannelEntityId { get; set; }

        /// <summary>
        /// Gets or sets the channel list template.
        /// </summary>
        /// <value>
        /// The channel list template.
        /// </value>
        public string ChannelListTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Channel Type Rock.Model.DefinedValue representing what type of Interaction Channel this is.
        /// This helps determine the Rock.Model.InteractionChannel.ChannelEntityId
        /// </summary>
        /// <value>
        /// A System.Int32 representing the Id of the Rock.Model.DefinedValue identifying the interaction channel type. If no value is selected this can be null.
        /// </value>
        public int? ChannelTypeMediumValueId { get; set; }

        /// <summary>
        /// Gets or sets the length of time (in minutes) that components of this channel should be cached
        /// </summary>
        /// <value>
        /// The duration (in minutes) of the component cache.
        /// </value>
        public int? ComponentCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the component custom 1 label.
        /// </summary>
        /// <value>
        /// The component custom 1 label.
        /// </value>
        public string ComponentCustom1Label { get; set; }

        /// <summary>
        /// Gets or sets the component custom 2 label.
        /// </summary>
        /// <value>
        /// The component custom 2 label.
        /// </value>
        public string ComponentCustom2Label { get; set; }

        /// <summary>
        /// Gets or sets the component custom indexed 1 label.
        /// </summary>
        /// <value>
        /// The component custom indexed 1 label.
        /// </value>
        public string ComponentCustomIndexed1Label { get; set; }

        /// <summary>
        /// Gets or sets the component detail template.
        /// </summary>
        /// <value>
        /// The component detail template.
        /// </value>
        public string ComponentDetailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.EntityType for each of this channel's components.
        /// The Id of the Rock.Model.InteractionChannel.ComponentEntityTypeId is stored in down in Rock.Model.InteractionComponent.EntityId.
        /// For example:
        /// 
        /// <item>
        ///     <term>PageView</term>
        ///     <description>EntityType is <see cref="T:Rock.Model.Page" />. Page.Id is stored down in <see cref="P:Rock.Model.InteractionComponent.EntityId" /></description></item>
        /// <item>
        ///     <term>Communication Recipient Activity</term>
        ///     <description>EntityType is <see cref="T:Rock.Model.Communication" />. Communication.Id is stored down in <see cref="P:Rock.Model.InteractionComponent.EntityId" /> </description></item>
        /// <item>
        ///     <term>Workflow Entry Form</term>
        ///     <description>EntityType is <see cref="T:Rock.Model.WorkflowType" />. WorkflowType.Id is stored down in <see cref="P:Rock.Model.InteractionComponent.EntityId" /></description></item>
        /// </summary>
        /// <value>
        /// A System.Int32 representing the EntityTypeId for the Rock.Model.EntityType
        /// </value>
        public int? ComponentEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the component list template.
        /// </summary>
        /// <value>
        /// The component list template.
        /// </value>
        public string ComponentListTemplate { get; set; }

        /// <summary>
        /// Gets or sets the engagement strength.
        /// </summary>
        /// <value>
        /// The engagement strength.
        /// </value>
        public int? EngagementStrength { get; set; }

        /// <summary>
        /// Gets or sets the interaction custom 1 label.
        /// </summary>
        /// <value>
        /// The interaction custom 1 label.
        /// </value>
        public string InteractionCustom1Label { get; set; }

        /// <summary>
        /// Gets or sets the interaction custom 2 label.
        /// </summary>
        /// <value>
        /// The interaction custom 2 label.
        /// </value>
        public string InteractionCustom2Label { get; set; }

        /// <summary>
        /// Gets or sets the interaction custom indexed 1 label.
        /// </summary>
        /// <value>
        /// The interaction custom indexed 1 label.
        /// </value>
        public string InteractionCustomIndexed1Label { get; set; }

        /// <summary>
        /// Gets or sets the interaction detail template.
        /// </summary>
        /// <value>
        /// The interaction detail template.
        /// </value>
        public string InteractionDetailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId for the Rock.Model.EntityType of entity that was modified. For example:
        /// 
        /// <item>
        ///     <term>PageView</term>
        ///     <description>null</description></item>
        /// <item>
        ///     <term>Communication Recipient Activity</term>
        ///     <description><see cref="T:Rock.Model.CommunicationRecipient" /></description></item>
        /// <item>
        ///     <term>Workflow Entry Form</term>
        ///     <description><see cref="T:Rock.Model.Workflow" /></description></item>
        /// </summary>
        /// <value>
        /// A System.Int32 representing the EntityTypeId for the Rock.Model.EntityType of the entity that was modified.
        /// </value>
        public int? InteractionEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the interaction list template.
        /// </summary>
        /// <value>
        /// The interaction list template.
        /// </value>
        public string InteractionListTemplate { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an active group. This value is required.
        /// </summary>
        /// <value>
        /// A System.Boolean value that is true if this group is active, otherwise false.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the interaction channel name.
        /// </summary>
        /// <value>
        /// The interaction channel name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the retention days.
        /// </summary>
        /// <value>
        /// The retention days.
        /// </value>
        public int? RetentionDuration { get; set; }

        /// <summary>
        /// Gets or sets the session detail template.
        /// </summary>
        /// <value>
        /// The session detail template.
        /// </value>
        public string SessionDetailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the session list template.
        /// </summary>
        /// <value>
        /// The session list template.
        /// </value>
        public string SessionListTemplate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [uses session].
        /// Set to true if interactions in this channel from a web browser session (for example: PageViews).
        /// Set to false if interactions in this channel are not associated with a web browser session (for example: communication clicks and opens from an email client or sms device).
        /// </summary>
        /// <value>
        ///   true if [uses session]; otherwise, false.
        /// </value>
        public bool UsesSession { get; set; }

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
