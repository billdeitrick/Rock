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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Example
{
    /// <summary>
    /// Allows testing the new Grid component.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Grid Test" )]
    [Category( "Example" )]
    [Description( "Allows testing the new Grid component." )]
    [IconCssClass( "fa fa-list" )]

    [Rock.SystemGuid.EntityTypeGuid( "1934a378-57d6-44d0-b7cd-4443f347a1ee" )]
    public class GridTest : RockObsidianBlockType
    {
        public override string BlockFileUrl => $"{base.BlockFileUrl}.obs";

        public override object GetObsidianBlockInitialization()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var builder = GetGridBuilder( GetGridAttributes() );
            sw.Stop();

            sw.Restart();
            builder.BuildDefinition();
            sw.Stop();

            return new
            {
                GridDefinition = GetGridBuilder( GetGridAttributes() ).BuildDefinition()
            };
        }

        [BlockAction]
        public BlockActionResult GetGridData()
        {
            using ( var rockContext = new RockContext() )
            {
                var count = RequestContext.GetPageParameter( "count" )?.AsIntegerOrNull() ?? 10_000;

                var gridAttributes = GetGridAttributes();
                var gridAttributeIds = gridAttributes.Select( a => a.Id ).ToList();

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var prayerRequests = new PrayerRequestService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Take( count )
                    .ToList();
                sw.Stop();
                System.Diagnostics.Debug.WriteLine( $"Entity load took {sw.Elapsed.TotalMilliseconds}ms." );

                sw.Restart();
                Helper.LoadFilteredAttributes( prayerRequests, rockContext, a => gridAttributeIds.Contains( a.Id ) );
                sw.Stop();
                System.Diagnostics.Debug.WriteLine( $"Attribute load took {sw.Elapsed.TotalMilliseconds}ms." );

                sw.Restart();
                var data = GetGridBuilder( gridAttributes ).Build( prayerRequests );
                sw.Stop();
                System.Diagnostics.Debug.WriteLine( $"Row translation took {sw.Elapsed.TotalMilliseconds}ms." );

                return ActionOk( data );
            }
        }

        [BlockAction]
        public BlockActionResult CreateEntitySet( GridEntitySetBag entitySet )
        {
            if ( entitySet == null )
            {
                return ActionBadRequest( "No entity set data was provided." );
            }

            var entityType = entitySet.EntityTypeKey.IsNotNullOrWhiteSpace()
                ? EntityTypeCache.Get( entitySet.EntityTypeKey, false )
                : null;

            var rockEntitySet = new EntitySet()
            {
                EntityTypeId = entityType?.Id,
                ExpireDateTime = RockDateTime.Now.AddMinutes( 5 )
            };

            var entitySetItems = new List<EntitySetItem>();

            using ( var rockContext = new RockContext() )
            {
                if ( entityType != null )
                {
                    var entityKeys = entitySet.Items.Select( i => i.EntityKey ).ToList();
                    var entityIdLookup = Rock.Reflection.GetEntityIdsForEntityType( entityType, entityKeys, true, rockContext );

                    foreach ( var item in entitySet.Items )
                    {
                        if ( !entityIdLookup.TryGetValue( item.EntityKey, out var entityId ) )
                        {
                            continue;
                        }

                        entitySetItems.Add( new EntitySetItem
                        {
                            EntityId = entityId,
                            AdditionalMergeValues = item.AdditionalMergeValues
                        } );
                    }
                }
                else
                {
                    entitySetItems.AddRange( entitySet.Items.Select( i => new EntitySetItem
                    {
                        EntityId = 0,
                        AdditionalMergeValues = i.AdditionalMergeValues
                    } ) );
                }

                if ( !entitySetItems.Any() )
                {
                    return ActionBadRequest( "No entities were found to create the set." );
                }

                var entitySetService = new EntitySetService( rockContext );
                entitySetService.Add( rockEntitySet );
                rockContext.SaveChanges();

                entitySetItems.ForEach( a =>
                {
                    a.EntitySetId = rockEntitySet.Id;
                } );

                rockContext.BulkInsert( entitySetItems );

                // Todo: Change to IdKey.
                return ActionOk( rockEntitySet.Id.ToString() );
            }
        }

        public class GridEntitySetBag
        {
            public string EntityTypeKey { get; set; }

            public List<GridEntitySetItemBag> Items { get; set; }
        }

        public class GridEntitySetItemBag
        {
            public string EntityKey { get; set; }

            public int Order { get; set; }

            public Dictionary<string, object> AdditionalMergeValues { get; set; }
        }

        private List<AttributeCache> GetGridAttributes()
        {
            var entityTypeId = EntityTypeCache.GetId<PrayerRequest>().Value;

            return AttributeCache.GetByEntityTypeQualifier( entityTypeId, string.Empty, string.Empty, false )
                .Where( a => a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        private GridBuilder<PrayerRequest> GetGridBuilder( List<AttributeCache> gridAttributes )
        {
            return new GridBuilder<PrayerRequest>()
                .UseWithBlock( this )
                .AddField( "guid", pr => pr.Guid.ToString() )
                .AddField( "personId", pr => pr.RequestedByPersonAlias?.PersonId )
                .AddField( "name", pr => new { pr.FirstName, pr.LastName } )
                .AddTextField( "email", pr => pr.Email )
                .AddDateTimeField( "enteredDateTime", pr => pr.EnteredDateTime )
                .AddDateTimeField( "expirationDateTime", pr => pr.ExpirationDate )
                .AddField( "isUrgent", pr => pr.IsUrgent )
                .AddField( "isPublic", pr => pr.IsPublic )
                .AddField( "id", pr => pr.Id )
                .AddField( "mode", pr => new ListItemBag
                {
                    Value = pr.IsUrgent == true ? "#900000" : "#009000",
                    Text = pr.IsUrgent != true ? "Closed" : "Open"
                } )
                .AddAttributeFields( gridAttributes );
        }
    }
}
