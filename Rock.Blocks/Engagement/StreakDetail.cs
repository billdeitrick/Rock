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
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.StreakDetail;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays the details of a particular streak.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianDetailBlockType" />

    [DisplayName( "Streak Detail" )]
    [Category( "Engagement" )]
    [Description( "Displays the details of a particular streak." )]
    [IconCssClass( "fa fa-question" )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "867abce8-47a9-46fa-8a35-47ebbc60c4fe" )]
    [Rock.SystemGuid.BlockTypeGuid( "1c98107f-dfbf-44bd-a860-0c9df2e6c495" )]
    public class StreakDetail : RockObsidianDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string StreakId = "StreakId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        public override string BlockFileUrl => $"{base.BlockFileUrl}.obs";

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<StreakBag, StreakDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = GetAttributeQualifiedColumns<Streak>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private StreakDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new StreakDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the Streak for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="streak">The Streak to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Streak is valid, <c>false</c> otherwise.</returns>
        private bool ValidateStreak( Streak streak, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<StreakBag, StreakDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Streak.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Streak.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Streak.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="StreakBag"/> that represents the entity.</returns>
        private StreakBag GetCommonEntityBag( Streak entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new StreakBag
            {
                IdKey = entity.IdKey,
                CreatedByPersonAlias = entity.CreatedByPersonAlias.ToListItemBag(),
                CreatedByPersonAliasId = entity.CreatedByPersonAliasId,
                CreatedDateTime = entity.CreatedDateTime,
                CurrentStreakCount = entity.CurrentStreakCount,
                CurrentStreakStartDate = entity.CurrentStreakStartDate,
                EngagementCount = entity.EngagementCount,
                EnrollmentDate = entity.EnrollmentDate,
                ForeignGuid = entity.ForeignGuid,
                ForeignId = entity.ForeignId,
                ForeignKey = entity.ForeignKey,
                InactiveDateTime = entity.InactiveDateTime,
                IsActive = entity.IsActive,
                Location = entity.Location.ToListItemBag(),
                LocationId = entity.LocationId,
                LongestStreakCount = entity.LongestStreakCount,
                LongestStreakEndDate = entity.LongestStreakEndDate,
                LongestStreakStartDate = entity.LongestStreakStartDate,
                ModifiedByPersonAlias = entity.ModifiedByPersonAlias.ToListItemBag(),
                ModifiedByPersonAliasId = entity.ModifiedByPersonAliasId,
                ModifiedDateTime = entity.ModifiedDateTime,
                PersonAlias = entity.PersonAlias.ToListItemBag(),
                PersonAliasId = entity.PersonAliasId,
                StreakType = entity.StreakType.ToListItemBag(),
                StreakTypeId = entity.StreakTypeId
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="StreakBag"/> that represents the entity.</returns>
        private StreakBag GetEntityBagForView( Streak entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="StreakBag"/> that represents the entity.</returns>
        private StreakBag GetEntityBagForEdit( Streak entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( Streak entity, DetailBlockBox<StreakBag, StreakDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.CreatedByPersonAlias ),
                () => entity.CreatedByPersonAliasId = box.Entity.CreatedByPersonAlias.GetEntityId<PersonAlias>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.CreatedByPersonAliasId ),
                () => entity.CreatedByPersonAliasId = box.Entity.CreatedByPersonAliasId );

            box.IfValidProperty( nameof( box.Entity.CreatedDateTime ),
                () => entity.CreatedDateTime = box.Entity.CreatedDateTime );

            box.IfValidProperty( nameof( box.Entity.CurrentStreakCount ),
                () => entity.CurrentStreakCount = box.Entity.CurrentStreakCount );

            box.IfValidProperty( nameof( box.Entity.CurrentStreakStartDate ),
                () => entity.CurrentStreakStartDate = box.Entity.CurrentStreakStartDate );

            box.IfValidProperty( nameof( box.Entity.EngagementCount ),
                () => entity.EngagementCount = box.Entity.EngagementCount );

            box.IfValidProperty( nameof( box.Entity.EnrollmentDate ),
                () => entity.EnrollmentDate = box.Entity.EnrollmentDate );

            box.IfValidProperty( nameof( box.Entity.ForeignGuid ),
                () => entity.ForeignGuid = box.Entity.ForeignGuid );

            box.IfValidProperty( nameof( box.Entity.ForeignId ),
                () => entity.ForeignId = box.Entity.ForeignId );

            box.IfValidProperty( nameof( box.Entity.ForeignKey ),
                () => entity.ForeignKey = box.Entity.ForeignKey );

            box.IfValidProperty( nameof( box.Entity.InactiveDateTime ),
                () => entity.InactiveDateTime = box.Entity.InactiveDateTime );

            /*box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );*/

            box.IfValidProperty( nameof( box.Entity.Location ),
                () => entity.LocationId = box.Entity.Location.GetEntityId<Location>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.LocationId ),
                () => entity.LocationId = box.Entity.LocationId );

            box.IfValidProperty( nameof( box.Entity.LongestStreakCount ),
                () => entity.LongestStreakCount = box.Entity.LongestStreakCount );

            box.IfValidProperty( nameof( box.Entity.LongestStreakEndDate ),
                () => entity.LongestStreakEndDate = box.Entity.LongestStreakEndDate );

            box.IfValidProperty( nameof( box.Entity.LongestStreakStartDate ),
                () => entity.LongestStreakStartDate = box.Entity.LongestStreakStartDate );

            box.IfValidProperty( nameof( box.Entity.ModifiedByPersonAlias ),
                () => entity.ModifiedByPersonAliasId = box.Entity.ModifiedByPersonAlias.GetEntityId<PersonAlias>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.ModifiedByPersonAliasId ),
                () => entity.ModifiedByPersonAliasId = box.Entity.ModifiedByPersonAliasId );

            box.IfValidProperty( nameof( box.Entity.ModifiedDateTime ),
                () => entity.ModifiedDateTime = box.Entity.ModifiedDateTime );

            box.IfValidProperty( nameof( box.Entity.PersonAlias ),
                () => entity.PersonAliasId = box.Entity.PersonAlias.GetEntityId<PersonAlias>( rockContext ).Value );

            box.IfValidProperty( nameof( box.Entity.PersonAliasId ),
                () => entity.PersonAliasId = box.Entity.PersonAliasId );

            box.IfValidProperty( nameof( box.Entity.StreakType ),
                () => entity.StreakTypeId = box.Entity.StreakType.GetEntityId<StreakType>( rockContext ).Value );

            box.IfValidProperty( nameof( box.Entity.StreakTypeId ),
                () => entity.StreakTypeId = box.Entity.StreakTypeId );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="Streak"/> to be viewed or edited on the page.</returns>
        private Streak GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<Streak, StreakService>( rockContext, PageParameterKey.StreakId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( Streak entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out Streak entity, out BlockActionResult error )
        {
            var entityService = new StreakService( rockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new Streak();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Streak.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Streak.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<StreakBag, StreakDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<StreakBag, StreakDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new StreakService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateStreak( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.StreakId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( rockContext );

                return ActionOk( GetEntityBagForView( entity ) );
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new StreakService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<StreakBag, StreakDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<StreakBag, StreakDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        #endregion
    }
}
