using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Lekha.Utilities.Tests
{
    public class TimeCalculatorTests
    {
        [Theory]
        [InlineData(1, 2022, 1, 1, 2021, 1, 1, 2022, 12, 31, true)]
        [InlineData(2, 2022, 12, 31, 2021, 1, 1, 2022, 12, 31, true)]
        [InlineData(3, 2021, 3, 1, 2021, 1, 1, 2022, 12, 31, true)]
        [InlineData(4, 2022, 3, 1, 2021, 1, 1, 2022, 12, 31, true)]
        [InlineData(5, 2019, 1, 1, 2021, 1, 1, 2022, 12, 31, false)]
        [InlineData(6, 2023, 1, 1, 2021, 1, 1, 2022, 12, 31, false)]
        [InlineData(7, 2020, 12, 31, 2021, 1, 1, 2022, 12, 31, false)]
        public void TestWithNoHhMmSs(int id, int momentYear, int momentMonth, int momentDay,
            int beginYear, int beginMonth, int beginDay,
            int endYear, int endMonth, int endDay,
            bool shouldMatch)
        {
            var moment = new DateTimeOffset(momentYear, momentMonth, momentDay, 0, 0, 0, TimeSpan.FromSeconds(0));
            var begin = new DateTimeOffset(beginYear, beginMonth, beginDay, 0, 0, 0, TimeSpan.FromSeconds(0));
            var end = new DateTimeOffset(endYear, endMonth, endDay, 0, 0, 0, TimeSpan.FromSeconds(0));
            var calculator = new TimeCalculator();
            string slotId = null;
            var matchingTime = calculator.IsMatchingTime(moment, begin, end, null, out slotId);

            matchingTime.Should().Be(shouldMatch);
        }

        [Theory]
        [InlineData(1, 2022, 1, 1, 10, 30, 10, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0, true)]
        [InlineData(2, 2022, 12, 30, 18, 0, 0, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0, true)]
        [InlineData(3, 2021, 3, 1, 10, 30, 10, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0, true)]
        [InlineData(4, 2022, 3, 1, 10, 30, 10, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0, true)]
        [InlineData(5, 2019, 1, 1, 10, 30, 10, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0, false)]
        [InlineData(6, 2023, 1, 1, 10, 30, 10, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0, false)]
        [InlineData(7, 2020, 12, 31, 10, 30, 10, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0, false)]
        public void TestWithHhMmSs(int id, int momentYear, int momentMonth, int momentDay, int momentHour, int momentMinute, int momentSecond,
            int beginYear, int beginMonth, int beginDay, int beginHour, int beginMinute, int beginSecond,
            int endYear, int endMonth, int endDay, int endHour, int endMinute, int endSecond,
            bool shouldMatch)
        {
            var moment = new DateTimeOffset(momentYear, momentMonth, momentDay, momentHour, momentMinute, momentSecond, TimeSpan.FromSeconds(0));
            var begin = new DateTimeOffset(beginYear, beginMonth, beginDay, beginHour, beginMinute, beginSecond, TimeSpan.FromSeconds(0));
            var end = new DateTimeOffset(endYear, endMonth, endDay, endHour, endMinute, endSecond, TimeSpan.FromSeconds(0));
            var calculator = new TimeCalculator();
            string slotId = null;
            var matchingTime = calculator.IsMatchingTime(moment, begin, end, null, out slotId);

            matchingTime.Should().Be(shouldMatch);
        }

        [Theory]
        [InlineData(1, 2022, 1, 1, 10, 30, 10, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0,
            "1", 10, 30, 10, 11, 20, 10,
            "2", 10, 30, 10, 11, 20, 10,
            "3", 10, 30, 10, 11, 20, 10,
            "4", 10, 30, 10, 11, 20, 10,
            "5", 10, 30, 10, 11, 20, 10,
            true, "1")]
        [InlineData(2, 2022, 12, 30, 17, 31, 0, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0,
            "1", 17, 30, 10, 17, 35, 10,
            "2", 10, 30, 10, 11, 20, 10,
            "3", 10, 30, 10, 11, 20, 10,
            "4", 10, 30, 10, 11, 20, 10,
            "5", 10, 30, 10, 11, 20, 10,
            true, "1")]
        [InlineData(22, 2022, 12, 30, 17, 30, 10, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0,
            "1", 17, 30, 10, 17, 35, 10,
            "2", 10, 30, 10, 11, 20, 10,
            "3", 10, 30, 10, 11, 20, 10,
            "4", 10, 30, 10, 11, 20, 10,
            "5", 10, 30, 10, 11, 20, 10,
            true, "1")]
        [InlineData(23, 2022, 12, 30, 17, 35, 10, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0,
            "1", 17, 30, 10, 17, 35, 10,
            "2", 10, 30, 10, 11, 20, 10,
            "3", 10, 30, 10, 11, 20, 10,
            "4", 10, 30, 10, 11, 20, 10,
            "5", 10, 30, 10, 11, 20, 10,
            true, "1")]
        [InlineData(3, 2021, 3, 1, 10, 30, 10, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0,
            "1", 10, 30, 10, 11, 20, 10,
            "2", 10, 30, 10, 11, 20, 10,
            "3", 10, 30, 10, 11, 20, 10,
            "4", 10, 30, 10, 11, 20, 10,
            "5", 10, 30, 10, 11, 20, 10,
            true, "1")]
        [InlineData(4, 2022, 3, 1, 10, 30, 10, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0,
            "1", 10, 30, 10, 11, 20, 10,
            "2", 10, 30, 10, 11, 20, 10,
            "3", 10, 30, 10, 11, 20, 10,
            "4", 10, 30, 10, 11, 20, 10,
            "5", 10, 30, 10, 11, 20, 10,
            true, "1")]
        [InlineData(5, 2019, 1, 1, 10, 30, 10, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0,
            "1", 10, 30, 10, 11, 20, 10,
            "2", 10, 30, 10, 11, 20, 10,
            "3", 10, 30, 10, 11, 20, 10,
            "4", 10, 30, 10, 11, 20, 10,
            "5", 10, 30, 10, 11, 20, 10,
            false, null)]
        [InlineData(6, 2023, 1, 1, 10, 30, 10, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0,
            "1", 10, 30, 10, 11, 20, 10,
            "2", 10, 30, 10, 11, 20, 10,
            "3", 10, 30, 10, 11, 20, 10,
            "4", 10, 30, 10, 11, 20, 10,
            "5", 10, 30, 10, 11, 20, 10,
            false, null)]
        [InlineData(7, 2020, 12, 31, 10, 30, 10, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0,
            "1", 10, 30, 10, 11, 20, 10,
            "2", 10, 30, 10, 11, 20, 10,
            "3", 10, 30, 10, 11, 20, 10,
            "4", 10, 30, 10, 11, 20, 10,
            "5", 10, 30, 10, 11, 20, 10,
            false, null)]
        [InlineData(8, 2022, 12, 30, 17, 30, 9, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0,
            "1", 17, 30, 10, 17, 35, 10,
            "2", 10, 30, 10, 11, 20, 10,
            "3", 10, 30, 10, 11, 20, 10,
            "4", 10, 30, 10, 11, 20, 10,
            "5", 10, 30, 10, 11, 20, 10,
            false, null)]
        [InlineData(8, 2022, 12, 30, 17, 35, 11, 2021, 1, 1, 8, 0, 0, 2022, 12, 31, 18, 0, 0,
            "1", 17, 30, 10, 17, 35, 10,
            "2", 10, 30, 10, 11, 20, 10,
            "3", 10, 30, 10, 11, 20, 10,
            "4", 10, 30, 10, 11, 20, 10,
            "5", 10, 30, 10, 11, 20, 10,
            false, null)]
        public void TestWithHhMmSsAndSlots(int id, int momentYear, int momentMonth, int momentDay, int momentHour, int momentMinute, int momentSecond,
            int beginYear, int beginMonth, int beginDay, int beginHour, int beginMinute, int beginSecond,
            int endYear, int endMonth, int endDay, int endHour, int endMinute, int endSecond,
            string slotId1, int slotBeginHour1, int slotBeginMinute1, int slotBeginSecond1, int slotEndHour1, int slotEndMinute1, int slotEndSecond1,
            string slotId2, int slotBeginHour2, int slotBeginMinute2, int slotBeginSecond2, int slotEndHour2, int slotEndMinute2, int slotEndSecond2,
            string slotId3, int slotBeginHour3, int slotBeginMinute3, int slotBeginSecond3, int slotEndHour3, int slotEndMinute3, int slotEndSecond3,
            string slotId4, int slotBeginHour4, int slotBeginMinute4, int slotBeginSecond4, int slotEndHour4, int slotEndMinute4, int slotEndSecond4,
            string slotId5, int slotBeginHour5, int slotBeginMinute5, int slotBeginSecond5, int slotEndHour5, int slotEndMinute5, int slotEndSecond5,
            bool shouldMatch,
            string expectedSlotId)
        {
            var moment = new DateTimeOffset(momentYear, momentMonth, momentDay, momentHour, momentMinute, momentSecond, TimeSpan.FromSeconds(0));
            var begin = new DateTimeOffset(beginYear, beginMonth, beginDay, beginHour, beginMinute, beginSecond, TimeSpan.FromSeconds(0));
            var end = new DateTimeOffset(endYear, endMonth, endDay, endHour, endMinute, endSecond, TimeSpan.FromSeconds(0));

            var slots = new List<TimeCalculator.TimeSlot>();
            slots.Add(new TimeCalculator.TimeSlot
            {
                Id = slotId1,
                Begin = new TimeSpan(slotBeginHour1, slotBeginMinute1, slotBeginSecond1),
                End = new TimeSpan(slotEndHour1, slotEndMinute1, slotEndSecond1)
            });
            slots.Add(new TimeCalculator.TimeSlot
            {
                Id = slotId2,
                Begin = new TimeSpan(slotBeginHour2, slotBeginMinute2, slotBeginSecond2),
                End = new TimeSpan(slotEndHour2, slotEndMinute2, slotEndSecond2)
            });
            slots.Add(new TimeCalculator.TimeSlot
            {
                Id = slotId3,
                Begin = new TimeSpan(slotBeginHour3, slotBeginMinute3, slotBeginSecond3),
                End = new TimeSpan(slotEndHour3, slotEndMinute3, slotEndSecond3)
            });
            slots.Add(new TimeCalculator.TimeSlot
            {
                Id = slotId4,
                Begin = new TimeSpan(slotBeginHour4, slotBeginMinute4, slotBeginSecond4),
                End = new TimeSpan(slotEndHour4, slotEndMinute4, slotEndSecond4)
            });
            slots.Add(new TimeCalculator.TimeSlot
            {
                Id = slotId5,
                Begin = new TimeSpan(slotBeginHour5, slotBeginMinute5, slotBeginSecond5),
                End = new TimeSpan(slotEndHour5, slotEndMinute5, slotEndSecond5)
            });

            var calculator = new TimeCalculator();
            string slotId = null;
            var matchingTime = calculator.IsMatchingTime(moment, begin, end, slots, out slotId);

            matchingTime.Should().Be(shouldMatch);
            slotId.Should().Be(expectedSlotId);
        }
    }
}
