using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Web.Cache.NonEntities;

namespace Rock.Utility
{
    public class PersonPreferenceCollection
    {
        private readonly int? _personId;

        private readonly int? _personAliasId;

        private readonly int? _entityTypeId;

        private readonly int? _entityId;

        private readonly string _prefix;

        private readonly ConcurrentDictionary<string, PreferenceValue> _preferences;

        private readonly ConcurrentBag<string> _updatedKeys = new ConcurrentBag<string>();

        internal PersonPreferenceCollection( int? personId, int? personAliasId, int? entityTypeId, int? entityId, string prefix, IEnumerable<PersonPreferenceCache> preferences )
        {
            if ( !personAliasId.HasValue )
            {
                throw new ArgumentNullException( nameof( personAliasId ) );
            }

            if ( entityTypeId.HasValue && !entityId.HasValue )
            {
                throw new ArgumentNullException( nameof( entityId ), "Parameter required when 'entityTypeId' is not null." );
            }

            _personId = personId;
            _personAliasId = personAliasId;
            _entityTypeId = entityTypeId;
            _entityId = entityId;
            _prefix = prefix;
            _preferences = new ConcurrentDictionary<string, PreferenceValue>();

            // Order by Id so that preferences created earlier have priority.
            foreach ( var preference in preferences.OrderBy( p => p.Id ) )
            {
                _preferences.AddOrIgnore( preference.Key, new PreferenceValue
                {
                    Id = preference.Id,
                    Value = preference.Value ?? string.Empty,
                    LastAccessedDateTime = preference.LastAccessedDateTime
                } );
            }
        }

        public string GetPreferenceValue( string key )
        {
            var prefixedKey = $"{_prefix}{key}";
            var now = RockDateTime.Now;

            if ( !_preferences.TryGetValue( prefixedKey, out var preference ) )
            {
                return string.Empty;
            }

            if ( preference.LastAccessedDateTime.Date < now.Date )
            {
                preference.LastAccessedDateTime = now;

                new UpdatePersonPreferenceLastAccessedTransaction( preference.Id ).Enqueue();
            }

            return preference.Value;
        }

        public void SetPreferenceValue( string key, string value )
        {
            var prefixedKey = $"{_prefix}{key}";

            _preferences.AddOrUpdate( prefixedKey,
                k => new PreferenceValue
                {
                    Value = value ?? string.Empty,
                    LastAccessedDateTime = RockDateTime.Now
                },
                ( k, p ) => new PreferenceValue
                {
                    Value = value ?? string.Empty,
                    LastAccessedDateTime = RockDateTime.Now
                }
            );

            _updatedKeys.Add( prefixedKey );
        }

        public void Save()
        {
            var updatedKeys = new List<string>();

            while ( _updatedKeys.TryTake( out var key ) )
            {
                if ( !updatedKeys.Contains( key ) )
                {
                    updatedKeys.Add( key );
                }
            }

            if ( !updatedKeys.Any() )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var personPreferenceService = new PersonPreferenceService( rockContext );
                var qry = _personId.HasValue
                    ? personPreferenceService.GetPersonPreferencesQueryable( _personId.Value )
                    : personPreferenceService.GetVisitorPreferencesQueryable( _personAliasId.Value );

                // Speed up the save operation when changing existing values.
                qry = qry.Include( pp => pp.PersonAlias );

                // Filter the query to the correct entity or to the global set.
                if ( _entityTypeId.HasValue )
                {
                    qry = qry.Where( pp => pp.EntityTypeId == _entityTypeId.Value
                        && pp.EntityId == _entityId.Value );
                }
                else
                {
                    qry = qry.Where( pp => !pp.EntityTypeId.HasValue
                        && !pp.EntityId.HasValue );
                }

                // Filter to just the keys that were modified. Tested this on
                // up to 1,000 fake keys and SQL doesn't blow up.
                qry = qry.Where( pp => updatedKeys.Contains( pp.Key ) );

                // Order by Id so when we update below we always update the
                // earliest value if there are duplicates.
                var preferences = qry.OrderBy( pp => pp.Id ).ToList();

                var preferencesToDelete = new List<PersonPreference>();
                var preferencesToAdd = new List<PersonPreference>();

                foreach ( var key in updatedKeys )
                {
                    string value;

                    if ( _preferences.TryGetValue( key, out var preferenceValue ) )
                    {
                        value = preferenceValue.Value;
                    }
                    else
                    {
                        value = null;
                    }

                    var preference = preferences.Where( pp => pp.Key == key ).FirstOrDefault();

                    // If the value is empty, then we need to delete the preference.
                    if ( value.IsNullOrWhiteSpace() )
                    {
                        if ( preference != null )
                        {
                            preferencesToDelete.Add( preference );
                        }
                    }
                    else
                    {
                        // Preference value needs to be created or updated.
                        if ( preference != null )
                        {
                            // Only update if it actually changed, or if they haven't
                            // accessed it yet today.
                            if ( preference.Value != value || preference.LastAccessedDateTime.Date != RockDateTime.Now.Date )
                            {
                                preference.Value = value;
                                preference.LastAccessedDateTime = RockDateTime.Now;
                            }
                        }
                        else
                        {
                            preference = new PersonPreference
                            {
                                PersonAliasId = _personAliasId.Value,
                                Key = key,
                                EntityTypeId = _entityTypeId,
                                EntityId = _entityId,
                                Value = value,
                                LastAccessedDateTime = RockDateTime.Now,
                            };

                            preferencesToAdd.Add( preference );
                        }
                    }
                }

                if ( preferencesToDelete.Any() )
                {
                    personPreferenceService.DeleteRange( preferencesToDelete );
                }

                if ( preferencesToAdd.Any() )
                {
                    personPreferenceService.AddRange( preferencesToAdd );
                }

                rockContext.SaveChanges();
            }
        }

        private class PreferenceValue
        {
            public int Id { get; set; }

            public string Value { get; set; }

            public DateTime LastAccessedDateTime { get; set; }
        }
    }
}
