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
    /// <summary>
    /// The primary class to use when accessing person preferences. This handles
    /// all the logic of last accessed tracking as well as properly saving and
    /// updating preferences values.
    /// </summary>
    public class PersonPreferenceCollection
    {
        #region Fields

        /// <summary>
        /// The person identifier
        /// </summary>
        private readonly int? _personId;

        /// <summary>
        /// The person alias identifier
        /// </summary>
        private readonly int? _personAliasId;

        /// <summary>
        /// The entity type identifier that we have been scoped to.
        /// </summary>
        private readonly int? _entityTypeId;

        /// <summary>
        /// The entity identifier that we have been scoped to.
        /// </summary>
        private readonly int? _entityId;

        /// <summary>
        /// The prefix that will be prepended to any keys passed to us
        /// by the caller.
        /// </summary>
        private readonly string _prefix;

        /// <summary>
        /// The preferences and values we know about. Dictionary key is the
        /// prefixed preference key.
        /// </summary>
        private readonly ConcurrentDictionary<string, PreferenceValue> _preferences;

        /// <summary>
        /// A list of prefixed preference keys that have had their values updated.
        /// </summary>
        private readonly ConcurrentBag<string> _updatedKeys = new ConcurrentBag<string>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonPreferenceCollection"/> class.
        /// </summary>
        /// <param name="personId">The person identifier owning this collection, should be <c>null</c> for an anonymous visitor.</param>
        /// <param name="personAliasId">The person alias identifier owning this collection.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="prefix">The prefix to apply to user keys when accessing preferences.</param>
        /// <param name="preferences">The preferences to initialize our known preference values with.</param>
        internal PersonPreferenceCollection( int? personId, int? personAliasId, int? entityTypeId, int? entityId, string prefix, IEnumerable<PersonPreferenceCache> preferences )
        {
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

        #endregion

        #region Methods

        /// <summary>
        /// Gets the preference value for the key.
        /// </summary>
        /// <param name="key">The key whose value should be returned.</param>
        /// <returns>A string that represents the value. An empty string is returned if the key was not found.</returns>
        public string GetPreferenceValue( string key )
        {
            var prefixedKey = $"{_prefix}{key}";
            var now = RockDateTime.Now;

            if ( !_preferences.TryGetValue( prefixedKey, out var preference ) )
            {
                return string.Empty;
            }

            if ( _personAliasId.HasValue && preference.LastAccessedDateTime.Date < now.Date && preference.Id != 0 )
            {
                preference.LastAccessedDateTime = now;

                new UpdatePersonPreferenceLastAccessedTransaction( preference.Id ).Enqueue();
            }

            return preference.Value;
        }

        /// <summary>
        /// Sets the preference value for the key.
        /// </summary>
        /// <param name="key">The key whose value should be set.</param>
        /// <param name="value">The new value. An empty string or <c>null</c> will delete the value.</param>
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

        /// <summary>
        /// Saves all the changes that have been made.
        /// </summary>
        public void Save()
        {
            // This is a special case where we exist solely so that code can
            // use preferences without having to do null checking. If we are
            // not actually connected to a person alias then do not save.
            if ( !_personAliasId.HasValue )
            {
                return;
            }

            var updatedKeys = new List<string>();

            // Dequeue all the keys that have been modified.
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

            SaveUpdatedKeys( updatedKeys );
        }

        /// <summary>
        /// Saves the updated keys by adding, updating or deleting any
        /// person preference records that are needed.
        /// </summary>
        /// <param name="updatedKeys">The updated keys.</param>
        private void SaveUpdatedKeys( List<string> updatedKeys )
        {
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

                // Loop through all the keys that were updated and handle any
                // updates, deletes or additions that need to be made.
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

                        continue;
                    }

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
                        // Create it.
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

                // EF will try to detect changes even if passed an empty set.
                if ( preferencesToDelete.Any() )
                {
                    personPreferenceService.DeleteRange( preferencesToDelete );
                }

                // EF will try to detect changes even if passed an empty set.
                if ( preferencesToAdd.Any() )
                {
                    personPreferenceService.AddRange( preferencesToAdd );
                }

                rockContext.SaveChanges();

                // Update our tracked preference identifiers.
                foreach ( var addedPreference in preferencesToAdd )
                {
                    if ( _preferences.TryGetValue( addedPreference.Key, out var preferenceValue ) )
                    {
                        preferenceValue.Id = addedPreference.Id;
                    }
                }
            }
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// A temporary cached value and last accessed date and time for a
        /// preference key.
        /// </summary>
        private class PreferenceValue
        {
            /// <summary>
            /// Gets or sets the identifier. Used to send update requests for
            /// <see cref="PersonPreference.LastAccessedDateTime"/>.
            /// </summary>
            /// <value>The identifier.</value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the current value.
            /// </summary>
            /// <value>The current value.</value>
            public string Value { get; set; }

            /// <summary>
            /// Gets or sets the last accessed date time.
            /// </summary>
            /// <value>The last accessed date time.</value>
            public DateTime LastAccessedDateTime { get; set; }
        }

        #endregion
    }
}
