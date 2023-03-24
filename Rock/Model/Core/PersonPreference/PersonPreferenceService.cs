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

namespace Rock.Model
{
    public partial class PersonPreferenceService
    {
        public List<PersonPreferenceCache> GetPersonPreferences( int personId )
        {
            return Queryable()
                .Where( pp => pp.PersonAlias.PersonId == personId )
                .Select( pp => new PersonPreferenceCache
                {
                    Id = pp.Id,
                    PersonId = pp.PersonAlias.PersonId,
                    PersonAliasId = pp.PersonAliasId,
                    EntityTypeId = pp.EntityTypeId,
                    EntityId = pp.EntityId,
                    Key = pp.Key,
                    Value = pp.Value,
                    LastAccessedDateTime = pp.LastAccessedDateTime
                } )
                .ToList();
        }

        public List<PersonPreferenceCache> GetPersonAliasPreferences( int personAliasId )
        {
            return Queryable()
                .Where( pp => pp.PersonAliasId == personAliasId )
                .Select( pp => new PersonPreferenceCache
                {
                    Id = pp.Id,
                    PersonAliasId = pp.PersonAliasId,
                    EntityTypeId = pp.EntityTypeId,
                    EntityId = pp.EntityId,
                    Key = pp.Key,
                    Value = pp.Value,
                    LastAccessedDateTime = pp.LastAccessedDateTime
                } )
                .ToList();
        }

        public class PersonPreferenceCache
        {
            public int Id { get; set; }

            public int? PersonId { get; set; }

            public int PersonAliasId { get; set; }

            public int? EntityTypeId { get; set; }

            public int? EntityId { get; set; }

            public string Key { get; set; }

            public string Value { get; set; }

            public DateTime LastAccessedDateTime { get; set; }
        }

        private class HasDynamicPersonPreferencesAttribute : System.Attribute
        {

        }
    }
}
