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
using System.Text.RegularExpressions;

using Rock.Data;

namespace Rock.Model
{
    public partial class PersonPreferenceService
    {
        public IQueryable<PersonPreference> GetPersonPreferencesQueryable( int personId )
        {
            return Queryable()
                .Where( pp => pp.PersonAlias.PersonId == personId );
        }

        public IQueryable<PersonPreference> GetVisitorPreferencesQueryable( int personAliasId )
        {
            return Queryable()
                .Where( pp => pp.PersonAliasId == personAliasId );
        }

        public static string GetPreferencePrefix( IEntity entity )
        {
            if ( entity == null )
            {
                throw new ArgumentNullException( nameof( entity ) );
            }

            return GetPreferencePrefix( entity.GetType(), entity.Id );
        }

        public static string GetPreferencePrefix( Type type, int id )
        {
            if ( type.IsDynamicProxyType() )
            {
                type = type.BaseType;
            }

            var prefix = type.Name.ToLower()
                .SplitCase()
                .Trim()
                .Replace( " ", "-" );

            prefix = Regex.Replace( prefix, "[^a-zA-Z0-9-]", string.Empty );

            return $"{prefix}-{id}-";
        }

        private class HasDynamicPersonPreferencesAttribute : System.Attribute
        {

        }
    }
}
