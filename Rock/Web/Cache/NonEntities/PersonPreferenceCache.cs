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
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Utility;

namespace Rock.Web.Cache.NonEntities
{
    public class PersonPreferenceCache
    {
        #region Properties

        public int Id { get; }

        public int PersonId { get; }

        public int PersonAliasId { get; }

        public int? EntityTypeId { get; }

        public int? EntityId { get; }

        public string Key { get; }

        public string Value { get; private set; }

        public DateTime LastAccessedDateTime { get; private set; }

        #endregion

        #region Constructors

        internal PersonPreferenceCache( PersonPreference personPreference )
        {
            Id = personPreference.Id;
            PersonId = personPreference.PersonAlias.PersonId;
            PersonAliasId = personPreference.PersonAliasId;
            EntityTypeId = personPreference.EntityTypeId;
            EntityId = personPreference.EntityId;
            Key = personPreference.Key;
            Value = personPreference.Value;
            LastAccessedDateTime = personPreference.LastAccessedDateTime;
        }

        #endregion

        #region Private Static Methods

        private static List<PersonPreferenceCache> GetPersonPreferences( int personId )
        {
            return PersonOrVisitorCache.GetAllForPerson( personId )
                .Where( pp => !pp.EntityTypeId.HasValue )
                .ToList();
        }

        private static List<PersonPreferenceCache> GetPersonPreferences( int personId, EntityTypeCache entityTypeCache, int entityId )
        {
            return PersonOrVisitorCache.GetAllForPerson( personId )
                .Where( pp => pp.EntityTypeId == entityTypeCache.Id
                    && pp.EntityId == entityId )
                .ToList();
        }

        private static List<PersonPreferenceCache> GetVisitorPreferences( int personAliasId )
        {
            return PersonOrVisitorCache.GetAllForVisitor( personAliasId )
                .Where( pp => !pp.EntityTypeId.HasValue )
                .ToList();
        }

        private static List<PersonPreferenceCache> GetVisitorPreferences( int personAliasId, EntityTypeCache entityTypeCache, int entityId )
        {
            return PersonOrVisitorCache.GetAllForVisitor( personAliasId )
                .Where( pp => pp.EntityTypeId == entityTypeCache.Id
                    && pp.EntityId == entityId )
                .ToList();
        }

        #endregion

        #region Public Static Methods

        public static PersonPreferenceCollection GetPersonPreferenceCollection( Person person )
        {
            var preferences = GetPersonPreferences( person.Id );
            var prefix = string.Empty;

            return new PersonPreferenceCollection( person.Id, person.PrimaryAliasId, null, null, prefix, preferences );
        }

        public static PersonPreferenceCollection GetPersonPreferenceCollection( Person person, IEntity entity )
        {
            if ( entity == null )
            {
                throw new ArgumentNullException( nameof( entity ) );
            }

            var entityTypeCache = EntityTypeCache.Get( entity.GetType() );
            var prefix = PersonPreferenceService.GetPreferencePrefix( entity );
            var preferences = GetPersonPreferences( person.Id, entityTypeCache, entity.Id );

            return new PersonPreferenceCollection( person.Id, person.PrimaryAliasId, entityTypeCache.Id, entity.Id, prefix, preferences );
        }

        public static PersonPreferenceCollection GetPersonPreferenceCollection( Person person, IEntityCache entity )
        {
            if ( entity == null )
            {
                throw new ArgumentNullException( nameof( entity ) );
            }

            var entityTypeCache = EntityTypeCache.Get( entity.CachedEntityTypeId );
            var prefix = PersonPreferenceService.GetPreferencePrefix( entityTypeCache.GetEntityType(), entity.Id );
            var preferences = GetPersonPreferences( person.Id, entityTypeCache, entity.Id );

            return new PersonPreferenceCollection( person.Id, person.PrimaryAliasId, entityTypeCache.Id, entity.Id, prefix, preferences );
        }

        public static PersonPreferenceCollection GetVisitorPreferenceCollection( int personAliasId )
        {
            var preferences = GetVisitorPreferences( personAliasId );
            var prefix = string.Empty;

            return new PersonPreferenceCollection( null, personAliasId, null, null, prefix, preferences );
        }

        public static PersonPreferenceCollection GetVisitorPreferenceCollection( int personAliasId, IEntity entity )
        {
            if ( entity == null )
            {
                throw new ArgumentNullException( nameof( entity ) );
            }

            var entityTypeCache = EntityTypeCache.Get( entity.GetType() );
            var prefix = PersonPreferenceService.GetPreferencePrefix( entity );
            var preferences = GetVisitorPreferences( personAliasId, entityTypeCache, entity.Id );

            return new PersonPreferenceCollection( null, personAliasId, entityTypeCache.Id, entity.Id, prefix, preferences );
        }

        public static PersonPreferenceCollection GetVisitorPreferenceCollection( int personAliasId, IEntityCache entity )
        {
            if ( entity == null )
            {
                throw new ArgumentNullException( nameof( entity ) );
            }

            var entityTypeCache = EntityTypeCache.Get( entity.GetType() );
            var prefix = PersonPreferenceService.GetPreferencePrefix( entityTypeCache.GetEntityType(), entity.Id );
            var preferences = GetVisitorPreferences( personAliasId, entityTypeCache, entity.Id );

            return new PersonPreferenceCollection( null, personAliasId, entityTypeCache.Id, entity.Id, prefix, preferences );
        }

        public static void FlushPerson( int personId )
        {
            PersonOrVisitorCache.FlushPerson( personId );
        }

        public static void FlushVisitor( int personAliasId )
        {
            PersonOrVisitorCache.FlushVisitor( personAliasId );
        }

        #endregion

        #region Private Static Methods

        #endregion
    }
}
