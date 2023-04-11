using System;
using System.Collections.Generic;

namespace Lekha.Utilities
{
    /// <summary>
    /// Provides time calcuation utilities
    /// </summary>
    public class TimeCalculator
    {
        /// <summary>
        /// Represents a time slot for a given day
        /// </summary>
        public class TimeSlot
        {
            /// <summary>
            /// ID of the slot
            /// </summary>
            public string Id { get; set; }
            /// <summary>
            /// Time slot - begin time
            /// </summary>
            public TimeSpan Begin { get; set; }
            /// <summary>
            /// Time slot - end time
            /// </summary>
            public TimeSpan End { get; set; }
        }
        /// <summary>
        /// Given a moment in DateTimeOffset, determines if it falls within the specified DateTimeOffset range.
        /// Optionally, checks if the hour:min:sec of the moment falls within any of the specified time slots of the day.
        /// </summary>
        /// <param name="moment">Moment to check</param>
        /// <param name="beginDate">Date Range - begin</param>
        /// <param name="endDate">Date Range - end</param>
        /// <param name="timeSlots">Time slots of the date to check against</param>
        /// <param name="timeSlotId">Matching timeslot ID</param>
        /// <returns>true, if the moment matches with the date range and time slots</returns>
        public bool IsMatchingTime(DateTimeOffset moment, DateTimeOffset beginDate, DateTimeOffset endDate, IEnumerable<TimeSlot> timeSlots, out string timeSlotId)
        {
            timeSlotId = null;
            bool retVal = false;
            if (moment >= beginDate && moment <= endDate)
            {
                if (timeSlots == null)
                {
                    return true;
                }

                var momentOnDayInSeconds = new TimeSpan(moment.Hour, moment.Minute, moment.Second);
                foreach (var timeSlot in timeSlots)
                {
                    if (momentOnDayInSeconds >= timeSlot.Begin && momentOnDayInSeconds <= timeSlot.End)
                    {
                        timeSlotId = timeSlot.Id;
                        retVal = true;
                        break;
                    }
                }
            }
            return retVal;
        }
    }
}
