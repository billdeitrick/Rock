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
using Rock.Enums.Blocks.Group.Scheduling;
using Rock.Enums.Controls;
using Rock.Field.Types;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduler;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Group.Scheduling
{
    /// <summary>
    /// Allows group schedules for groups and locations to be managed by a scheduler.
    /// </summary>

    [DisplayName( "Group Scheduler" )]
    [Category( "Group Scheduling" )]
    [Description( "Allows group schedules for groups and locations to be managed by a scheduler." )]
    [IconCssClass( "fa fa-calendar-alt" )]

    #region Block Attributes

    [BooleanField( "Enable Alternate Group Individual Selection",
        Key = AttributeKey.EnableAlternateGroupIndividualSelection,
        Description = "Determines if individuals may be selected from alternate groups.",
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        Order = 0,
        IsRequired = false )]

    [BooleanField( "Enable Parent Group Individual Selection",
        Key = AttributeKey.EnableParentGroupIndividualSelection,
        Description = "Determines if individuals may be selected from parent groups.",
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        Order = 1,
        IsRequired = false )]

    [BooleanField( "Enable Data View Individual Selection",
        Key = AttributeKey.EnableDataViewIndividualSelection,
        Description = "Determines if individuals may be selected from data views.",
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = false,
        Order = 2,
        IsRequired = false )]

    [LinkedPage( "Roster Page",
        Key = AttributeKey.RosterPage,
        Description = "Page used for viewing the group schedule roster.",
        Order = 3,
        IsRequired = true )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "7ADCE833-A785-4A54-9805-7335809C5367" )]
    [Rock.SystemGuid.BlockTypeGuid( "511D8E2E-4AF3-48D8-88EF-2AB311CD47E0" )]
    public class GroupScheduler : RockObsidianBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string EnableAlternateGroupIndividualSelection = "EnableAlternateGroupIndividualSelection";
            public const string EnableParentGroupIndividualSelection = "EnableParentGroupIndividualSelection";
            public const string EnableDataViewIndividualSelection = "EnableDataViewIndividualSelection";
            public const string RosterPage = "RosterPage";
        }

        private static class NavigationUrlKey
        {
            public const string RosterPage = "RosterPage";
        }

        private static class PageParameterKey
        {
            public const string GroupIds = "GroupIds";
            public const string LocationIds = "LocationIds";
            public const string ScheduleIds = "ScheduleIds";
            public const string OccurrenceDate = "OccurrenceDate";
        }

        #endregion

        #region Fields

        private List<int> _groupIds;
        private List<int> _locationIds;
        private List<int> _scheduleIds;

        private List<DateTime> _occurrenceDates;
        private List<string> _occurrenceDateStrings;

        private List<GroupLocationSchedule> _unfilteredGroupLocationSchedules;
        private List<GroupLocationSchedule> _filteredGroupLocationSchedules;

        #endregion

        #region Properties

        public override string BlockFileUrl => $"{base.BlockFileUrl}.obs";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new GroupSchedulerInitializationBox();

                rockContext.SqlLogging( true );

                SetBoxInitialState( box, rockContext );

                rockContext.SqlLogging( false );

                return box;
            }
        }

        /// <summary>
        /// Sets the initial state of the box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialState( GroupSchedulerInitializationBox box, RockContext rockContext )
        {
            var block = new BlockService( rockContext ).Get( this.BlockId );
            block.LoadAttributes( rockContext );

            box.AppliedFilters = new GroupSchedulerAppliedFiltersBag
            {
                Filters = GetDefaultOrUserPreferenceFilters( rockContext ),
                ScheduleOccurrences = GetScheduleOccurrences( rockContext ),
                NavigationUrls = GetNavigationUrls()
            };
            box.SecurityGrantToken = GetSecurityGrantToken();
        }

        /// <summary>
        /// Gets the filters, overriding any defaults with user preferences.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The filters.</returns>
        private GroupSchedulerFiltersBag GetDefaultOrUserPreferenceFilters( RockContext rockContext )
        {
            var filters = new GroupSchedulerFiltersBag();

            // TODO (JPH): Override defaults with user preferences, once supported in Obsidian blocks.

            // Dev-time defaults:
            filters.Groups = new List<ListItemBag>
            {
                new ListItemBag { Value = "e82bd99e-7dc1-483c-a4f2-09017088c55e", Text = "Children's" },
                new ListItemBag { Value = "0ba93d66-21b1-4229-979d-f76ceb57666d", Text = "A/V Team" }
            };

            RefineFilters( rockContext, filters );

            return filters;
        }

        /// <summary>
        /// Refines the filters, overriding any selections if necessary, as some filter values are dependent on the values of other filters
        /// and current user authorization.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters that should be refined.</param>
        private void RefineFilters( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            ValidateDateRange( filters );
            GetAuthorizedGroups( rockContext, filters );
            GetLocationsAndSchedules( rockContext, filters );
        }

        /// <summary>
        /// Validates the date range and attempts to set the first and last "end of week" dates (as well as the friendly date range) on the provided filters object.
        /// </summary>
        /// <param name="filters">The filters whose date range should be validated.</param>
        private void ValidateDateRange( GroupSchedulerFiltersBag filters )
        {
            /*
             * Date range validation approach: Since we're likely scheduling for recurring schedules that have no end date, we must
             * have defined start and end date filters so we don't try to show occurrences that span into eternity. We'll allow the
             * selection of past dates, so the individual may choose a past week as the source when cloning schedules. The UI will be
             * responsible for preventing the scheduling/manipulation of past schedules (and we'll also double-check & prevent doing
             * so within this block's action methods).
             * 
             *  1) If no date range selected, default to the next 6 weeks.
             *  2) If only a start date is selected, set end date = start date.
             *  3) If only an end date is selected, set start date = end date.
             *  4) If end date >= start date, set end date = start date.
             *  5) Allow any other valid selections (knowing they might be selecting an excessively-large range).
             * 
             * Our goal is to "translate" the sliding date range selection to weeks, as the Group Scheduler is designed to work
             * against a week at a time. More specifically, we need to determine the "end of week dates" we're working against,
             * and from those values, we can then determine the "start of week dates" to have blocks of 7-day ranges.
             */

            var defaultDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Next,
                TimeUnit = TimeUnitType.Week,
                TimeValue = 6
            };

            var defaultStartDate = RockDateTime.Today;
            // Add 35 days to today's "end of week" date, so we'll include this current week + the following 5 weeks.
            var defaultEndDate = RockDateTime.Today.EndOfWeek( RockDateTime.FirstDayOfWeek ).AddDays( 35 );

            if ( filters.DateRange == null )
            {
                filters.DateRange = defaultDateRange;
            }

            if ( filters.DateRange.RangeType == SlidingDateRangeType.DateRange )
            {
                var lowerDate = filters.DateRange.LowerDate;
                var upperDate = filters.DateRange.UpperDate;

                if ( lowerDate.HasValue && upperDate.HasValue )
                {
                    if ( upperDate < lowerDate )
                    {
                        filters.DateRange.UpperDate = lowerDate;
                    }
                }
                else if ( lowerDate.HasValue )
                {
                    filters.DateRange.UpperDate = lowerDate;
                }
                else if ( upperDate.HasValue )
                {
                    filters.DateRange.LowerDate = upperDate;
                }
                else
                {
                    filters.DateRange.LowerDate = defaultStartDate;
                    filters.DateRange.UpperDate = defaultEndDate;
                }
            }

            /*
             * Use the non-Obsidian Sliding Date Range Picker control (for now) to calculate the selected start and end dates,
             * as it has logic built into its property getters and setters. This way, we'll keep our behavior consistent with
             * Web Forms usages of this picker.
             */
            SlidingDateRangePicker NewPicker( SlidingDateRangeBag slidingDateRange )
            {
                return new SlidingDateRangePicker
                {
                    SlidingDateRangeMode = ( SlidingDateRangePicker.SlidingDateRangeType ) ( int ) slidingDateRange.RangeType,
                    TimeUnit = ( SlidingDateRangePicker.TimeUnitType ) ( int ) ( slidingDateRange.TimeUnit ?? 0 ),
                    NumberOfTimeUnits = slidingDateRange.TimeValue ?? 1,
                    DateRangeModeStart = slidingDateRange.LowerDate?.DateTime,
                    DateRangeModeEnd = slidingDateRange.UpperDate?.DateTime
                };
            }

            var picker = NewPicker( filters.DateRange );
            var dateRange = picker.SelectedDateRange;

            // At this point, we should have validated start and end dates, but if for some reason we don't, default to the next 6 weeks.
            if ( dateRange?.Start == null || dateRange?.End == null )
            {
                picker = NewPicker( defaultDateRange );
                dateRange = picker.SelectedDateRange;
            }

            // Reset the Obsidian picker control to match any changes made above.
            filters.DateRange = new SlidingDateRangeBag
            {
                RangeType = ( SlidingDateRangeType ) ( int ) picker.SlidingDateRangeMode,
                TimeUnit = ( TimeUnitType ) ( int ) picker.TimeUnit,
                TimeValue = picker.NumberOfTimeUnits,
                LowerDate = picker.DateRangeModeStart,
                UpperDate = picker.DateRangeModeEnd
            };

            var firstEndOfWeekDate = ( dateRange?.Start ?? defaultStartDate ).EndOfWeek( RockDateTime.FirstDayOfWeek );
            var lastEndOfWeekDate = ( dateRange?.End ?? defaultEndDate ).EndOfWeek( RockDateTime.FirstDayOfWeek );

            string friendlyDateRange;

            // This doesn't need to be precise; just need to determine if we should try to list all "end of week" dates or just a range.
            var numberOfWeeks = ( lastEndOfWeekDate - firstEndOfWeekDate ).TotalDays / 7;
            if ( numberOfWeeks > 4 )
            {
                friendlyDateRange = $"{FormatDate( firstEndOfWeekDate )} - {FormatDate( lastEndOfWeekDate )}";
            }
            else
            {
                var endOfWeekDates = new List<DateTime>();
                var endOfWeekDate = firstEndOfWeekDate;
                while ( endOfWeekDate <= lastEndOfWeekDate )
                {
                    endOfWeekDates.Add( endOfWeekDate );
                    endOfWeekDate = endOfWeekDate.AddDays( 7 );
                }

                friendlyDateRange = string.Join( ", ", endOfWeekDates.Select( d => FormatDate( d ) ) );
            }

            filters.FirstEndOfWeekDate = firstEndOfWeekDate;
            filters.LastEndOfWeekDate = lastEndOfWeekDate;
            filters.FriendlyDateRange = friendlyDateRange;
        }

        /// <summary>
        /// Formats the provided date.
        /// </summary>
        /// <param name="date">The date to format.</param>
        /// <returns>A formatted date string.</returns>
        private string FormatDate( DateTime date )
        {
            var currentYear = RockDateTime.Now.Year;
            var format = date.Year == currentYear ? "M/d" : "M/d/yyyy";

            return date.ToString( format );
        }

        /// <summary>
        /// Gets the authorized groups from those selected within the filters, ensuring the current person has EDIT or SCHEDULE permission.
        /// <para>
        /// The groups will be updated on the filters object to include only those that are authorized.
        /// </para>
        /// <para>
        /// Private _groupIds are set as a result of calling this method.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters whose groups should be loaded and validated.</param>
        private void GetAuthorizedGroups( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            if ( filters.Groups?.Any() != true )
            {
                return;
            }

            var groupGuids = filters.Groups
                .Select( g => g.Value.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();

            if ( !groupGuids.Any() )
            {
                filters.Groups = null;
                return;
            }

            // Get the selected groups and preload ParentGroup, as it's needed for a proper Authorization check.
            var groups = new GroupService( rockContext )
                .GetByGuids( groupGuids )
                .Include( g => g.ParentGroup )
                .AsNoTracking()
                .Where( g =>
                    g.IsActive
                    && !g.IsArchived
                    && g.GroupType.IsSchedulingEnabled
                    && !g.DisableScheduling
                )
                .ToList();

            // Ensure the current user has the correct permission(s) to schedule the selected groups and update the filters if necessary.
            groups = groups
                .Where( g =>
                    g.IsAuthorized( Authorization.EDIT, this.RequestContext.CurrentPerson )
                    || g.IsAuthorized( Authorization.SCHEDULE, this.RequestContext.CurrentPerson )
                )
                .OrderBy( g => g.Order )
                .ThenBy( g => g.Name )
                .ToList();

            filters.Groups = groups
                .Select( g => new ListItemBag
                {
                    Value = g.Guid.ToString(),
                    Text = g.Name
                } )
                .ToList();

            // Set aside the final list of group IDs for later use.
            _groupIds = groups
                .Select( g => g.Id )
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Gets the available and selected locations and schedules, based on the combined, currently-applied filters.
        /// <para>
        /// The locations and schedules will be updated on the filters object.
        /// </para>
        /// <para>
        /// Private _unfilteredGroupLocationSchedules and _filteredGroupLocationSchedules are set as a result of calling this method.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters whose locations and schedules should be loaded.</param>
        private void GetLocationsAndSchedules( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            if ( _groupIds?.Any() != true )
            {
                _unfilteredGroupLocationSchedules = null;
                _filteredGroupLocationSchedules = null;
                filters.Locations = null;
                filters.Schedules = null;
                return;
            }

            /*
             * Get any already-existing attendance occurrence records tied to the [group, location, schedule, occurrence date] occurrences
             * we're retrieving. We'll need these IDs to facilitate scheduling within the Obsidian JavaScript block. Note that we'll create
             * any missing attendance occurrence records below, before sending the final, filtered collection of occurrences back to the client.
             */
            var attendanceOccurrencesQry = new AttendanceOccurrenceService( rockContext )
                .Queryable()
                .AsNoTracking();

            /*
             * Determine the actual start and end dates, based on the date range validation that has already taken place.
             * For the end date, add a day so we can follow Rock's rule: let your start be "inclusive" and your end be "exclusive".
             */
            var actualStartDate = filters.FirstEndOfWeekDate.StartOfWeek( RockDateTime.FirstDayOfWeek );
            var actualEndDate = filters.LastEndOfWeekDate.AddDays( 1 );

            /*
             * Get all locations and schedules tied to the selected group(s) initially, so we can properly load the "available" lists.
             * 
             * Go ahead and materialize the list so we can:
             *  1) Perform additional, in-memory filtering.
             *  2) Set the scheduled start date/time(s) on the returned instances; these represent the "occurrences" that may be scheduled.
             */
            var groupLocationSchedules = new GroupLocationService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( gl =>
                    _groupIds.Contains( gl.GroupId )
                    && gl.Location.IsActive
                )
                .SelectMany( gl => gl.Schedules, ( gl, s ) => new
                {
                    gl.Group,
                    gl.Group.ParentGroup,
                    GroupLocation = gl,
                    gl.Location,
                    Schedule = s,
                    Config = gl.GroupLocationScheduleConfigs.FirstOrDefault( c => c.ScheduleId == s.Id )
                } )
                .Where( gls =>
                    gls.Schedule.IsActive
                    /*
                     * Limit to those schedules that fall within the specified date range. Due to the design of recurring schedules,
                     * we can only do loose date comparisons at the query level. We'll potentially pull back more records than we'll
                     * actually display (for now), and further refine once the schedule objects are materialized below.
                     * 
                     * Get schedules whose:
                     * 
                     *  1) EffectiveStartDate < end date (so we don't get any Schedules that start AFTER the specified date range), AND
                     *  2) EffectiveEndDate is null OR >= start date (so we don't get any Schedules that have already ended BEFORE the specified date range).
                     * 
                     * Keep in mind that schedules with a null EffectiveEndDate represent recurring schedules that have no end date.
                     */
                    && gls.Schedule.EffectiveStartDate.HasValue
                    && gls.Schedule.EffectiveStartDate.Value < actualEndDate
                    && (
                        !gls.Schedule.EffectiveEndDate.HasValue
                        || gls.Schedule.EffectiveEndDate.Value >= actualStartDate
                    )
                )
                .Select( gls => new GroupLocationSchedule
                {
                    Group = gls.Group,
                    ParentGroup = gls.ParentGroup,
                    GroupLocation = gls.GroupLocation,
                    Location = gls.Location,
                    Schedule = gls.Schedule,
                    Config = gls.Config,
                    AttendanceOccurrences = attendanceOccurrencesQry.Where( ao =>
                        ao.GroupId == gls.Group.Id
                        && ao.LocationId == gls.Location.Id
                        && ao.ScheduleId == gls.Schedule.Id
                    )
                    .ToList()
                } )
                .ToList();

            // Remove any schedules that don't actually have start any date/time(s) within the specified date range, and set the start date/time(s) on those that remain.
            if ( groupLocationSchedules.Any() )
            {
                for ( int i = groupLocationSchedules.Count - 1; i >= 0; i-- )
                {
                    var groupLocationSchedule = groupLocationSchedules.ElementAt( i );

                    var startDateTimes = groupLocationSchedule.Schedule.GetScheduledStartTimes( actualStartDate, actualEndDate );
                    if ( startDateTimes?.Any() != true )
                    {
                        groupLocationSchedules.Remove( groupLocationSchedule );
                        continue;
                    }

                    groupLocationSchedule.StartDateTimes.AddRange( startDateTimes );
                }
            }

            _unfilteredGroupLocationSchedules = groupLocationSchedules;

            // Refine the complete list of GroupLocationSchedules by the selected locations.
            var selectedLocationGuids = ( filters.Locations?.SelectedLocations ?? new List<ListItemBag>() )
                .Select( l => l.Value?.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();

            var glsMatchingLocations = groupLocationSchedules
                .Where( gls => !selectedLocationGuids.Any() || selectedLocationGuids.Contains( gls.Location.Guid ) )
                .ToList();

            // Refine the complete list of GroupLocationSchedules by the selected schedules.
            var selectedScheduleGuids = ( filters.Schedules?.SelectedSchedules ?? new List<ListItemBag>() )
                .Select( s => s.Value?.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();

            var glsMatchingSchedules = groupLocationSchedules
                .Where( gls => !selectedScheduleGuids.Any() || selectedScheduleGuids.Contains( gls.Schedule.Guid ) )
                .ToList();

            /*
             * Refine down to the intersection of the above two collections.
             * This is the list of GroupLocationSchedules that match all currently-applied filters.
             */
            _filteredGroupLocationSchedules = glsMatchingLocations
                .Intersect( glsMatchingSchedules )
                .ToList();

            // Determine the new list of available (and selected) locations based on the currently-selected schedules.
            var availableLocations = GetAvailableLocations( glsMatchingSchedules );
            var selectedLocations = availableLocations
                .Where( l => selectedLocationGuids.Any( selected => selected.ToString() == l.Value ) )
                .ToList();

            // Determine the new list of available (and selected) schedules based on the currently-selected locations.
            var availableSchedules = GetAvailableSchedules( glsMatchingLocations );
            var selectedSchedules = availableSchedules
                .Where( s => selectedScheduleGuids.Any( selected => selected.ToString() == s.Value ) )
                .ToList();

            // Update the filters object to reflect the results.
            filters.Locations = new GroupSchedulerLocationsBag
            {
                AvailableLocations = availableLocations,
                SelectedLocations = selectedLocations
            };

            filters.Schedules = new GroupSchedulerSchedulesBag
            {
                AvailableSchedules = availableSchedules,
                SelectedSchedules = selectedSchedules
            };
        }

        /// <summary>
        /// Gets the available locations as list item bags, from the provided group, location, schedules collection.
        /// </summary>
        /// <param name="groupLocationSchedules">The group, location, schedules collection from which to get the available locations.</param>
        /// <returns>A sorted list of list item bags representing the available locations.</returns>
        private List<ListItemBag> GetAvailableLocations( List<GroupLocationSchedule> groupLocationSchedules )
        {
            return groupLocationSchedules
                .GroupBy( gls => gls.Location.Id )
                .Select( grouping => new
                {
                    grouping.FirstOrDefault()?.GroupLocation,
                    grouping.FirstOrDefault()?.Location
                } )
                .Where( l => l.GroupLocation != null && l.Location != null )
                .Select( l => new
                {
                    l.GroupLocation.Order,
                    Value = l.Location.Guid.ToString(),
                    Text = l.Location.ToString( true )
                } )
                .OrderBy( l => l.Order )
                .ThenBy( l => l.Text )
                .Select( l => new ListItemBag
                {
                    Value = l.Value,
                    Text = l.Text
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the available schedules as list item bags, from the provided group, location, schedules collection.
        /// </summary>
        /// <param name="groupLocationSchedules">The group, location, schedules collection from which to get the available schedules.</param>
        /// <returns>A sorted list of list item bags representing the available schedules.</returns>
        private List<ListItemBag> GetAvailableSchedules( List<GroupLocationSchedule> groupLocationSchedules )
        {
            return groupLocationSchedules
                .GroupBy( gls => gls.Schedule.Id )
                .Select( grouping => grouping.FirstOrDefault()?.Schedule )
                .Where( schedule => schedule != null )
                .ToList()
                .OrderByOrderAndNextScheduledDateTime()
                .Select( schedule => new ListItemBag
                {
                    Value = schedule.Guid.ToString(),
                    Text = schedule.ToString()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the list of [group, location, schedule, occurrence date] occurrences, based on the currently-applied filters.
        /// <para>
        /// Private _locationIds, _scheduleIds, _occurrenceDates and _occurrenceDateStrings are set as a result of calling this method.
        /// </para>
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The list of [group, location, schedule, occurrence date] occurrences.</returns>
        private List<GroupSchedulerOccurrenceBag> GetScheduleOccurrences( RockContext rockContext )
        {
            if ( _filteredGroupLocationSchedules?.Any() != true )
            {
                _locationIds = null;
                _scheduleIds = null;
                _occurrenceDates = null;
                _occurrenceDateStrings = null;
                return null;
            }

            _occurrenceDates = new List<DateTime>();

            EnsureAttendanceOccurrencesExist( rockContext );

            var occurrences = _filteredGroupLocationSchedules
                .SelectMany( gls => gls.StartDateTimes, ( gls, startDateTime ) =>
                {
                    var attendanceOccurrenceId = gls.AttendanceOccurrences
                        .FirstOrDefault( ao =>
                            ao.GroupId == gls.Group.Id
                            && ao.LocationId == gls.Location.Id
                            && ao.ScheduleId == gls.Schedule.Id
                            && ao.OccurrenceDate == startDateTime.Date
                        )?.Id ?? 0;

                    if ( attendanceOccurrenceId > 0 && !_occurrenceDates.Contains( startDateTime.Date ) )
                    {
                        _occurrenceDates.Add( startDateTime.Date );
                    }

                    return new GroupSchedulerOccurrenceBag
                    {
                        AttendanceOccurrenceId = attendanceOccurrenceId,
                        GroupId = gls.Group.Id,
                        GroupName = gls.Group.Name,
                        ParentGroupId = gls.Group.ParentGroupId,
                        ParentGroupName = gls.Group.ParentGroup?.Name,
                        LocationId = gls.Location.Id,
                        LocationName = gls.Location.ToString( true ),
                        ScheduleId = gls.Schedule.Id,
                        ScheduleName = gls.Schedule.ToString(),
                        ScheduleOrder = gls.Schedule.Order,
                        OccurrenceDate = startDateTime.Date.ToISO8601DateString(),
                        FriendlyOccurrenceDate = startDateTime.Date.ToString( "dddd, MMM d" ),
                        OccurrenceDateTime = startDateTime,
                        SundayDate = RockDateTime.GetSundayDate( startDateTime ).ToISO8601DateString(),
                        MinimumCapacity = gls.Config?.MinimumCapacity,
                        DesiredCapacity = gls.Config?.DesiredCapacity,
                        MaximumCapacity = gls.Config?.MaximumCapacity,
                        IsSchedulingEnabled = startDateTime.Date >= RockDateTime.Today
                    };
                } )
                .Where( o => o.AttendanceOccurrenceId > 0 )
                .OrderBy( o => o.OccurrenceDate )
                .ThenBy( o => o.ScheduleOrder )
                .ThenBy( o => o.OccurrenceDateTime )
                .ToList();

            _locationIds = occurrences.Select( o => o.LocationId ).Distinct().ToList();
            _scheduleIds = occurrences.Select( o => o.ScheduleId ).Distinct().ToList();
            _occurrenceDateStrings = occurrences.Select( o => o.OccurrenceDate ).Distinct().ToList();

            return occurrences;
        }

        /// <summary>
        /// Ensures attendance occurrence records exists for all [group, location, schedule, occurrence date] occurrences, based on the currently-applied filters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private void EnsureAttendanceOccurrencesExist( RockContext rockContext )
        {
            if ( _filteredGroupLocationSchedules?.Any() != true )
            {
                return;
            }

            var newAttendanceOccurrences = new List<AttendanceOccurrence>();

            foreach ( var gls in _filteredGroupLocationSchedules )
            {
                foreach ( var startDateTime in gls.StartDateTimes )
                {
                    var occurrenceDate = startDateTime.Date;
                    if ( gls.AttendanceOccurrences.Any( ao => ao.OccurrenceDate == occurrenceDate ) )
                    {
                        continue;
                    }

                    var attendanceOccurrence = new AttendanceOccurrence
                    {
                        GroupId = gls.Group.Id,
                        LocationId = gls.Location.Id,
                        ScheduleId = gls.Schedule.Id,
                        OccurrenceDate = occurrenceDate
                    };

                    gls.AttendanceOccurrences.Add( attendanceOccurrence );
                    newAttendanceOccurrences.Add( attendanceOccurrence );
                }
            }

            if ( !newAttendanceOccurrences.Any() )
            {
                return;
            }

            var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );
            attendanceOccurrenceService.AddRange( newAttendanceOccurrences );
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetNavigationUrls()
        {
            var queryParams = new Dictionary<string, string>();
            if ( _groupIds?.Any() == true )
            {
                queryParams.Add( PageParameterKey.GroupIds, _groupIds.AsDelimited( "," ) );
            }

            if ( _locationIds?.Any() == true )
            {
                queryParams.Add( PageParameterKey.LocationIds, _locationIds.AsDelimited( "," ) );
            }

            if ( _scheduleIds?.Any() == true )
            {
                queryParams.Add( PageParameterKey.ScheduleIds, _scheduleIds.AsDelimited( "," ) );
            }

            if ( _occurrenceDateStrings?.Count == 1 )
            {
                queryParams.Add( PageParameterKey.OccurrenceDate, _occurrenceDateStrings.First() );
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.RosterPage] = this.GetLinkedPageUrl( AttributeKey.RosterPage, queryParams )
            };
        }

        /// <summary>
        /// Validates and applies the provided filters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters to apply.</param>
        /// <returns>An object containing the validated filters and new list of filtered [group, location, schedule, occurrence date] occurrences.</returns>
        private GroupSchedulerAppliedFiltersBag ApplyFilters( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            RefineFilters( rockContext, filters );

            // TODO (JPH): Save selected filters to user preferences, once supported in Obsidian blocks.

            var appliedFilters = new GroupSchedulerAppliedFiltersBag
            {
                Filters = filters,
                ScheduleOccurrences = GetScheduleOccurrences( rockContext ),
                NavigationUrls = GetNavigationUrls()
            };

            return appliedFilters;
        }

        /// <summary>
        /// Gets the resource settings, overriding defaults with any previously-saved user preferences.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="groupId">The group ID for this group scheduler occurrence.</param>
        /// <returns>An object containing the available and applied resource settings.</returns>
        private GroupSchedulerResourceSettingsBag GetDefaultOrUserPreferenceResourceSettings( RockContext rockContext, int groupId )
        {
            var enabledResourceListSourceTypes = GetEnabledResourceListSourceTypes( rockContext, groupId );

            var resourceSettings = new GroupSchedulerResourceSettingsBag
            {
                EnabledResourceListSourceTypes = enabledResourceListSourceTypes,
                ResourceListSourceType = enabledResourceListSourceTypes.FirstOrDefault()
            };

            // TODO (JPH): Override defaults with user preferences, once supported in Obsidian blocks.

            SetGroupMemberFilterType( resourceSettings );

            return resourceSettings;
        }

        /// <summary>
        /// Gets the enabled resource list source types from which individuals may be scheduled.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="groupId">The group ID for this group scheduler occurrence.</param>
        /// <returns>The enabled resource list source types.</returns>
        private List<ResourceListSourceType> GetEnabledResourceListSourceTypes( RockContext rockContext, int groupId )
        {
            var group = new GroupService( rockContext ).GetNoTracking( groupId );
            if ( group == null )
            {
                return new List<ResourceListSourceType>();
            }

            var enabledTypes = new List<ResourceListSourceType> {
                ResourceListSourceType.GroupMembers,
                ResourceListSourceType.GroupMatchingPreference,
                ResourceListSourceType.GroupMatchingAssignment
            };

            if ( !group.SchedulingMustMeetRequirements )
            {
                // Only allow these alternate source types if enabled by block settings AND the group.
                if ( GetAttributeValue( AttributeKey.EnableAlternateGroupIndividualSelection ).AsBoolean() )
                {
                    enabledTypes.Add( ResourceListSourceType.AlternateGroup );
                }

                if ( group.ParentGroupId.HasValue && GetAttributeValue( AttributeKey.EnableParentGroupIndividualSelection ).AsBoolean() )
                {
                    enabledTypes.Add( ResourceListSourceType.ParentGroup );
                }

                if ( GetAttributeValue( AttributeKey.EnableDataViewIndividualSelection ).AsBoolean() )
                {
                    enabledTypes.Add( ResourceListSourceType.DataView );
                }
            }

            return enabledTypes;
        }

        /// <summary>
        /// Sets the group member filter type based on the currently-selected resource list source type.
        /// </summary>
        /// <param name="settings">The resource settings.</param>
        private void SetGroupMemberFilterType( GroupSchedulerResourceSettingsBag settings )
        {
            settings.ResourceGroupMemberFilterType = settings.ResourceListSourceType == ResourceListSourceType.GroupMatchingPreference
                ? SchedulerResourceGroupMemberFilterType.ShowMatchingPreference
                : SchedulerResourceGroupMemberFilterType.ShowAllGroupMembers;
        }

        /// <summary>
        /// Validates and applies the provided resource settings to user preferences.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="settingsToApply">The resource settings to apply.</param>
        /// <returns>An object containing the validated and applied + available resource settings.</returns>
        private GroupSchedulerResourceSettingsBag ApplyResourceSettings( RockContext rockContext, GroupSchedulerApplyResourceSettingsBag settingsToApply )
        {
            var resourceSettings = GetDefaultOrUserPreferenceResourceSettings( rockContext, settingsToApply.GroupId );

            if ( resourceSettings.EnabledResourceListSourceTypes.Contains( settingsToApply.ResourceListSourceType ) )
            {
                resourceSettings.ResourceListSourceType = settingsToApply.ResourceListSourceType;

                // TODO (JPH): Save selected resource list source type to user preferences, once supported in Obsidian blocks.

                SetGroupMemberFilterType( resourceSettings );
            }

            return resourceSettings;
        }

        /// <summary>
        /// Validates the provided filters and gets the clone settings, overriding any defaults with user preferences.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters containing the groups, locations and schedules currently available.</param>
        /// <returns>An object containing the available and applied clone settings.</returns>
        private GroupSchedulerCloneSettingsBag GetDefaultOrUserPreferenceCloneSettings( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            RefineFilters( rockContext, filters );

            // Populate private _occurrenceDates collection, Etc.
            GetScheduleOccurrences( rockContext );

            DateTime endOfWeekDate;

            var sourceEndOfWeekDates = new List<DateTime>();
            foreach ( var occurrenceDate in _occurrenceDates )
            {
                endOfWeekDate = occurrenceDate.EndOfWeek( RockDateTime.FirstDayOfWeek );
                if ( !sourceEndOfWeekDates.Contains( endOfWeekDate ) )
                {
                    sourceEndOfWeekDates.Add( endOfWeekDate );
                }
            }

            var destinationEndOfWeekDates = new List<DateTime>();
            endOfWeekDate = RockDateTime.Now.EndOfWeek( RockDateTime.FirstDayOfWeek );
            for ( int i = 0; i < 12; i++ )
            {
                destinationEndOfWeekDates.Add( endOfWeekDate );
                endOfWeekDate = endOfWeekDate.AddDays( 7 );
            }

            var cloneSettings = new GroupSchedulerCloneSettingsBag
            {
                AvailableSourceDates = GetAvailableCloneDates( sourceEndOfWeekDates ),
                AvailableDestinationDates = GetAvailableCloneDates( destinationEndOfWeekDates ),
                AvailableGroups = filters.Groups,
                AvailableLocations = GetAvailableLocations( _unfilteredGroupLocationSchedules ),
                AvailableSchedules = GetAvailableSchedules( _unfilteredGroupLocationSchedules )
            };

            // TODO (JPH): Override defaults with user preferences, once supported in Obsidian blocks.

            return cloneSettings;
        }

        /// <summary>
        /// Gets the available clone dates as list item bags, from the provided "end of week" dates.
        /// </summary>
        /// <param name="endOfWeekDates">The "end of week" dates from which to get the available clone dates.</param>
        /// <returns>A sorted list of list item bags representing the available clone dates.</returns>
        private List<ListItemBag> GetAvailableCloneDates( List<DateTime> endOfWeekDates )
        {
            var dateFormat = "M/d/yyyy";

            return endOfWeekDates
                .OrderBy( d => d )
                .Select( d => new ListItemBag
                {
                    Value = d.ToISO8601DateString(),
                    Text = $"{d.StartOfWeek( RockDateTime.FirstDayOfWeek ).ToString( dateFormat )} to {d.ToString( dateFormat )}"
                } )
                .ToList();
        }

        /// <summary>
        /// Validates the provided clone settings, saves them to user preferences and clones the schedules specified within the settings.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="cloneSettings">The clone settings dictating which schedules should be cloned.</param>
        /// <returns>An object containing the outcome of the clone schedules attempt.</returns>
        private GroupSchedulerCloneSchedulesResponseBag CloneSchedules( RockContext rockContext, GroupSchedulerCloneSettingsBag cloneSettings )
        {
            /*
             * We'll transpose the provided clone settings to filters objects in order to leverage the same private helpers that are used when
             * populating the Group Scheduler for the UI. This way, we can make use of permissions checks, Etc., that are already performed
             * in those scenarios.
             *  1) Create a filters object to represent the source week's date range + selected groups, locations and schedules; run this
             *     object through the private helpers to strip out any unauthorized groups + any group, location, schedule combos that don't
             *     actually exist within the source week.
             *  2) Modify the filters object to represent the destination week's date range + source set of group, location, schedule combos;
             *     run this object through the private helpers to further strip out any group, location, schedule combos that might not be
             *     relevant for the destination week, then create any missing AttendanceOccurrence records for the remaining occurrences.
             *  3) Clone the relevant Attendance records for each source-to-destination occurrence.
             */
            var response = new GroupSchedulerCloneSchedulesResponseBag();

            if ( cloneSettings.SelectedGroups?.Any() != true || cloneSettings.SelectedDestinationDate == cloneSettings.SelectedSourceDate )
            {
                return response;
            }

            var groups = cloneSettings.SelectedGroups
                .Select( g => new ListItemBag { Value = g } )
                .ToList();

            var locations = ( cloneSettings.SelectedLocations ?? new List<string>() )
                .Select( l => new ListItemBag { Value = l } )
                .ToList();

            var schedules = ( cloneSettings.SelectedSchedules ?? new List<string>() )
                .Select( s => new ListItemBag { Value = s } )
                .ToList();

            var filters = new GroupSchedulerFiltersBag
            {
                Groups = groups,
                Locations = new GroupSchedulerLocationsBag { SelectedLocations = locations },
                Schedules = new GroupSchedulerSchedulesBag { SelectedSchedules = schedules },
                DateRange = new SlidingDateRangeBag
                {
                    RangeType = SlidingDateRangeType.DateRange,
                    LowerDate = cloneSettings.SelectedSourceDate.StartOfWeek( RockDateTime.FirstDayOfWeek ),
                    UpperDate = cloneSettings.SelectedSourceDate.EndOfWeek( RockDateTime.FirstDayOfWeek )
                }
            };

            RefineFilters( rockContext, filters );
            if ( filters.Groups?.Any() != true )
            {
                // The individual is not authorized to schedule any of the groups that were provided.
                return response;
            }

            var sourceOccurrences = GetScheduleOccurrences( rockContext );
            if ( sourceOccurrences?.Any() != true )
            {
                // There weren't any occurrences that match the source filters.
                return response;
            }

            /*
             * We have at least one source schedule occurrence to clone. Move on to step 2 in order to strip out any group, location,
             * schedule combos that aren't relevant for the destination week + create any missing AttendanceOccurrence records for the
             * occurrences that remain.
             */
            filters.DateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.DateRange,
                LowerDate = cloneSettings.SelectedDestinationDate.StartOfWeek( RockDateTime.FirstDayOfWeek ),
                UpperDate = cloneSettings.SelectedDestinationDate.EndOfWeek( RockDateTime.FirstDayOfWeek )
            };

            RefineFilters( rockContext, filters );

            var destinationOccurrences = GetScheduleOccurrences( rockContext );
            if ( destinationOccurrences?.Any() != true )
            {
                // There weren't any occurrences that match the destination filters.
                return response;
            }

            // We now have our source and destination occurrences; for each source with a matching destination, attempt to clone the scheduled resources.
            var anyOccurrencesToClone = false;
            var blackoutSkippedCount = 0;
            var conflictSkippedCount = 0;
            var overCapacitySkippedCount = 0;

            var attendanceService = new AttendanceService( rockContext );
            var daysDifference = ( cloneSettings.SelectedDestinationDate - cloneSettings.SelectedSourceDate ).Days;

            foreach ( var source in sourceOccurrences )
            {
                var destination = destinationOccurrences.FirstOrDefault( d =>
                    d.GroupId == source.GroupId
                    && d.LocationId == source.LocationId
                    && d.ScheduleId == source.ScheduleId
                    && d.OccurrenceDateTime == source.OccurrenceDateTime.AddDays( daysDifference )
                );

                if ( destination == null )
                {
                    // There is no matching destination occurrence for this source occurrence.
                    continue;
                }

                anyOccurrencesToClone = true;

                var result = attendanceService.CloneScheduledPeople(
                    source.AttendanceOccurrenceId,
                    destination.AttendanceOccurrenceId,
                    this.RequestContext.CurrentPerson.PrimaryAlias
                );

                if ( result.ClonedCount > 0 )
                {
                    response.OccurrencesClonedCount++;
                }

                blackoutSkippedCount += result.BlackoutSkippedCount;
                conflictSkippedCount += result.ConflictSkippedCount;
                overCapacitySkippedCount += result.OverCapacitySkippedCount;
            }

            response.AnyOccurrencesToClone = anyOccurrencesToClone;

            return response;
        }

        /// <summary>
        /// Validates the provided filters and auto-schedules occurrences specified within the filters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters containing the occurrences to auto-schedule.</param>
        /// <returns>An object containing the validated filters and new list of filtered [group, location, schedule, occurrence date] occurrences.</returns>
        private GroupSchedulerAppliedFiltersBag AutoSchedule( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            RefineFilters( rockContext, filters );

            var scheduleOccurrences = GetScheduleOccurrences( rockContext );
            if ( scheduleOccurrences.Any() )
            {
                var attendanceOccurrenceIds = scheduleOccurrences
                    .Where( s => s.OccurrenceDateTime > RockDateTime.Now )
                    .Select( s => s.AttendanceOccurrenceId )
                    .ToList();

                var attendanceService = new AttendanceService( rockContext );

                attendanceService.SchedulePersonsAutomaticallyForAttendanceOccurrences( attendanceOccurrenceIds, this.RequestContext.CurrentPerson.PrimaryAlias );
                rockContext.SaveChanges();
            }

            var appliedFilters = new GroupSchedulerAppliedFiltersBag
            {
                Filters = filters,
                ScheduleOccurrences = scheduleOccurrences,
                NavigationUrls = GetNavigationUrls()
            };

            return appliedFilters;
        }

        /// <summary>
        /// Validates the provided filters and sends confirmations to individuals scheduled for occurrences specified within the filters.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="filters">The filters containing the groups with individuals who should receive confirmations.</param>
        /// <returns>An object containing the outcome of the send communications attempt.</returns>
        private GroupSchedulerSendNowResponseBag SendNow( RockContext rockContext, GroupSchedulerFiltersBag filters )
        {
            var response = new GroupSchedulerSendNowResponseBag();

            RefineFilters( rockContext, filters );

            var attendanceOccurrenceIds = GetScheduleOccurrences( rockContext )
                .Where( s => s.OccurrenceDateTime > RockDateTime.Now )
                .Select( s => s.AttendanceOccurrenceId )
                .ToList();

            if ( attendanceOccurrenceIds.Any() )
            {
                var attendanceService = new AttendanceService( rockContext );
                var sendConfirmationAttendancesQuery = attendanceService.GetPendingAndAutoAcceptScheduledConfirmations()
                    .Where( a => attendanceOccurrenceIds.Contains( a.OccurrenceId ) )
                    .Where( a => a.ScheduleConfirmationSent != true );

                var sendMessageResult = attendanceService.SendScheduleConfirmationCommunication( sendConfirmationAttendancesQuery );
                response.AnyCommunicationsToSend = sendConfirmationAttendancesQuery.Any();
                rockContext.SaveChanges();

                response.Errors = sendMessageResult.Errors;
                response.Warnings = sendMessageResult.Warnings;
                response.CommunicationsSentCount = sendMessageResult.MessagesSent;
            }

            return response;
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</returns>
        private string GetSecurityGrantToken()
        {
            return new Rock.Security.SecurityGrant().ToToken();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Applies the provided filters.
        /// </summary>
        /// <param name="bag">The filters to apply.</param>
        /// <returns>An object containing the validated filters and new list of filtered [group, location, schedule, occurrence date] occurrences.</returns>
        [BlockAction]
        public BlockActionResult ApplyFilters( GroupSchedulerFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var appliedFilters = ApplyFilters( rockContext, bag );

                return ActionOk( appliedFilters );
            }
        }

        /// <summary>
        /// Gets the resource settings.
        /// </summary>
        /// <param name="groupId">The group ID for this group scheduler occurrence.</param>
        /// <returns>An object containing the available and applied resource settings.</returns>
        [BlockAction]
        public BlockActionResult GetResourceSettings( int groupId )
        {
            using ( var rockContext = new RockContext() )
            {
                var resourceSettings = GetDefaultOrUserPreferenceResourceSettings( rockContext, groupId );

                return ActionOk( resourceSettings );
            }
        }

        /// <summary>
        /// Applies the provided resource settings.
        /// </summary>
        /// <param name="bag">The resource settings to apply.</param>
        /// <returns>An object containing the validated and applied + available resource settings.</returns>
        [BlockAction]
        public BlockActionResult ApplyResourceSettings( GroupSchedulerApplyResourceSettingsBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var resourceSettings = ApplyResourceSettings( rockContext, bag );

                return ActionOk( resourceSettings );
            }
        }

        /// <summary>
        /// Gets the clone settings.
        /// </summary>
        /// <param name="bag">The filters containing the groups currently visible.</param>
        /// <returns>An object containing the available and applied clone settings.</returns>
        [BlockAction]
        public BlockActionResult GetCloneSettings( GroupSchedulerFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var cloneSettings = GetDefaultOrUserPreferenceCloneSettings( rockContext, bag );

                return ActionOk( cloneSettings );
            }
        }

        /// <summary>
        /// Clones the schedules specified within the provided settings.
        /// </summary>
        /// <param name="bag">The clone settings dictating which schedules should be cloned.</param>
        /// <returns>An object containing the outcome of the clone schedules attempt.</returns>
        [BlockAction]
        public BlockActionResult CloneSchedules( GroupSchedulerCloneSettingsBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var response = CloneSchedules( rockContext, bag );

                return ActionOk( response );
            }
        }

        /// <summary>
        /// Auto-schedules occurrences specified within the provided filters.
        /// </summary>
        /// <param name="bag">The filters containing the occurrences to auto-schedule.</param>
        /// <returns>An object containing the validated filters and new list of filtered [group, location, schedule, occurrence date] occurrences.</returns>
        [BlockAction]
        public BlockActionResult AutoSchedule( GroupSchedulerFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var appliedFilters = AutoSchedule( rockContext, bag );

                return ActionOk( appliedFilters );
            }
        }

        /// <summary>
        /// Sends confirmations to individuals scheduled for occurrences within the provided filters.
        /// </summary>
        /// <param name="bag">The filters containing the groups with individuals who should receive confirmations.</param>
        /// <returns>An object containing the outcome of the send communications attempt.</returns>
        [BlockAction]
        public BlockActionResult SendNow( GroupSchedulerFiltersBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var response = SendNow( rockContext, bag );

                return ActionOk( response );
            }
        }

        #endregion

        #region Supporting Classes

        private class GroupLocationSchedule
        {
            private readonly List<DateTime> _startDateTimes = new List<DateTime>();

            public Rock.Model.Group Group { get; set; }

            public Rock.Model.Group ParentGroup { get; set; }

            public GroupLocation GroupLocation { get; set; }

            public Location Location { get; set; }

            public Schedule Schedule { get; set; }

            public GroupLocationScheduleConfig Config { get; set; }

            public List<AttendanceOccurrence> AttendanceOccurrences { get; set; }

            public List<DateTime> StartDateTimes => _startDateTimes;
        }

        #endregion
    }
}
