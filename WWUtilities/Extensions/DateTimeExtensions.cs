using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ww.Utilities.Extensions
{
    [DebuggerStepThrough]
    public static class DateTimeExtensions
    {
        public static DateTime AddWorkingDays(this DateTime specificDate, int workingDaysToAdd, IEnumerable<DateTime> skipDays = null)
        {
            int completeWeeks = workingDaysToAdd / 5;
            DateTime date = specificDate.AddDays(completeWeeks * 7);
            workingDaysToAdd = workingDaysToAdd % 5;
            for (int i = 0; i < workingDaysToAdd; i++)
            {
                date = date.AddDays(1);
                if (skipDays != null)
                {
                    while (!IsWeekDay(date) || IsHoliday(date) || skipDays.Contains(date))
                    {
                        date = date.AddDays(1);
                    }
                }
                else
                {
                    while (!IsWeekDay(date) || IsHoliday(date))
                    {
                        date = date.AddDays(1);
                    }
                }
            }
            return date;
        }

        public static int CountWorkingDaysSince(this DateTime currentDate, DateTime sinceDate)
        {
            return Enumerable.Range(0, (currentDate - sinceDate).Days)
                .Select(offset => sinceDate.AddDays(offset))
                .Count(day => day.IsWeekDay());
        }

        public static string EmptyStringIfDefault(this DateTime dt)
        {
            return dt != default(DateTime) ? dt.ToString() : "";
        }

        public static DateTime GetEndOfMonth(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, 1).AddMonths(1).AddTicks(-1);
        }

        public static DateTime GetNextWeekday(this DateTime startDay, DayOfWeek dayToFind, bool returnTodayIfMatchesDayToFind = false)
        {
            // Returns a DateTime of the next weekday (specified by dayToFind) that occurs after the startDay.
            // Alternatively this will return a DateTime of today's date if today is the same day as specified by dayToFind and returnTodayIfMatchesDayToFind is true.
            int daysToAdd = ((int)dayToFind - (int)startDay.DayOfWeek + 7) % 7;
            if (!returnTodayIfMatchesDayToFind && startDay.Day == DateTime.Today.Day) daysToAdd += 7;
            return startDay.AddDays(daysToAdd);
        }

        public static DateTime GetStartOfMonth(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, 1);
        }

        public static bool IsHoliday(this DateTime date)
        {
            var fncAdjustHoliday = new Func<DateTime, DateTime>((datActualHoliday) =>
            {
                if (datActualHoliday.IsWeekDay())
                    return datActualHoliday;

                if (datActualHoliday.DayOfWeek == DayOfWeek.Saturday)
                    return datActualHoliday.AddDays(-1);
                else
                    return datActualHoliday.AddDays(1);
            });

            if (date == fncAdjustHoliday(new DateTime(2018, 12, 24))) return true;
            if (date == fncAdjustHoliday(new DateTime(date.Year, 1, 1))) return true;
            if (date == fncAdjustHoliday(new DateTime(date.Year, 7, 4))) return true;
            if (date == fncAdjustHoliday(new DateTime(date.Year, 12, 25))) return true;
            else if (date.Month == 5)
            {
                DateTime memorialDay = new DateTime(date.Year, 5, 31);
                DayOfWeek dayOfWeek = memorialDay.DayOfWeek;
                while (dayOfWeek != DayOfWeek.Monday)
                {
                    memorialDay = memorialDay.AddDays(-1);
                    dayOfWeek = memorialDay.DayOfWeek;
                }
                if (date == memorialDay) return true;
            }
            else if (date.Month == 9)
            {
                DateTime laborDay = new DateTime(date.Year, 9, 1);
                DayOfWeek dayOfWeek = laborDay.DayOfWeek;
                while (dayOfWeek != DayOfWeek.Monday)
                {
                    laborDay = laborDay.AddDays(1);
                    dayOfWeek = laborDay.DayOfWeek;
                }
                if (date == laborDay) return true;
            }
            else if (date.Month == 11)
            {
                if (date == GetThanksgivingDay(date.Year)) return true;
            }
            return false;
        }

        public static DateTime GetThanksgivingDay(int year)
        {
            return new DateTime(year,
                       11,
                       Enumerable.Range(1, 30).Where(day => new DateTime(year, 11, day).DayOfWeek == DayOfWeek.Thursday).Select(x => x).ElementAt(3));
        }

        public static bool IsWeekDay(this DateTime date)
        {
            DayOfWeek day = date.DayOfWeek;
            return day != DayOfWeek.Saturday && day != DayOfWeek.Sunday;
        }

        public static string replaceDateTimeString(this DateTime rundate, string datetimeString)
        {
            //last month
            if (datetimeString.Contains("[prior month]"))
                datetimeString = datetimeString.Replace("[prior month]", rundate.AddMonths(-1).ToString("MMMM").ToLower());

            if (datetimeString.Contains("[prior day]"))
                datetimeString = datetimeString.Replace("[prior day]", rundate.AddDays(-1).ToString("yyyy-MM-dd"));

            //last month and year
            if (datetimeString.Contains("[yyyy-LM]"))
                datetimeString = datetimeString.Replace("[yyyy-LM]", rundate.AddMonths(-1).ToString("yyyy-M"));

            //report year
            if (datetimeString.Contains("[report year]"))
                datetimeString = datetimeString.Replace("[report year]", rundate.AddMonths(-1).ToString("yyyy"));

            //replace all others with a regular expression
            Regex rx = new Regex(@"\[([\w-]+)\]");
            datetimeString = rx.Replace(datetimeString, m => rundate.ToString(m.Groups[1].Value));

            return datetimeString;
        }

        public static string ToIsoDateString(this DateTime dt)
        {
            return dt.ToString("u");
        }

        public static string ToMilitaryDateTimeString(this DateTime time)
        {
            return (time.ToShortDateString() + " " + time.ToMilitaryTimeString());
        }

        public static string ToMilitaryTimeString(this DateTime time)
        {
            return time.ToString("HH:mm:ss");
        }

        public static string ToSortableDateString(this DateTime time)
        {
            return time.ToString("yyyy/MM/dd");
        }

        public static string ToSortableDateTimeString(this DateTime time)
        {
            return time.ToString("yyyy/MM/dd HH:mm:ss");
        }

        public static string ToShortDateTimeString(this DateTime time)
        {
            return time.ToShortDateString() + " " + time.ToShortTimeString();
        }

        public static string ToShortDateTimeLeadingZerosString(this DateTime time)
        {
            return time.ToString("MM/dd/yyyy hh:mm tt");
        }

        public static string ToShortDateLeadingZerosString(this DateTime date)
        {
            return date.ToString("MM/dd/yyyy");
        }

        public static string ToShortTimeLeadingZerosString(this DateTime date)
        {
            return date.ToString("hh:mm tt");
        }

        public static string ToXMLDateString(this DateTime time)
        {
            return (time.ToString("yyyy-MM-dd"));
        }

        public static string ToXMLDateTimeString(this DateTime time)
        {
            return (time.ToString("s"));
        }

        public static string ToFileDate(this DateTime date)
        {
            return date.ToString("yyyyMMdd");
        }

        public static string ToFileDateTimeString(this DateTime time)
        {
            return $"{time.ToString("yyyyMMdd")}_{time.ToMilitaryTimeString().Replace(":",string.Empty)}";
        }

        public static string ToDayOfWeekMonthYearString(this DateTime time, bool includeYear = true, bool firstThreeCharactersOnly = false)
        {
            return (firstThreeCharactersOnly ? time.DayOfWeek.ToString().Substring(0 , 3) : time.DayOfWeek.ToString()) + ", " + (firstThreeCharactersOnly ? CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(time.Month).Substring(0, 3) : CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(time.Month)) + " " + time.Day + (includeYear ? ", " + time.Year : "");
        }

        /// <summary>
        /// Returns true when the specified DateTime variables are within the specified milliseconds of each other.
        /// </summary>
        public static bool WithinMillieconds(this DateTime thisDateTime, DateTime compareToDateTime, int milliseconds)
        {
            return Math.Abs((thisDateTime - compareToDateTime).TotalMilliseconds) <= Math.Abs(milliseconds);
        }

        public static int MonthDifference(this DateTime startDate, DateTime endDate)
        {
            return Math.Abs(12 * (startDate.Year - endDate.Year) + startDate.Month - endDate.Month);
        }

        public static DateTime StartOfWeek(this DateTime currentDay, DayOfWeek setStartDay = DayOfWeek.Sunday)
        {
            return currentDay.AddDays((int)setStartDay - (int)currentDay.DayOfWeek);
        }

        public static DateTime EndOfWeek(this DateTime currentDay, DayOfWeek setEndDay = DayOfWeek.Saturday)
        {
            return currentDay.AddDays(setEndDay - currentDay.DayOfWeek);
        }

        public static bool IsValidMinMax(this DateTime dateTime)
        {
            if(dateTime == null) { return false; }
            return dateTime > DateTime.MinValue && dateTime < DateTime.MaxValue;
        }

        public static DateTime GetNextShippingDay(this DateTime specificDate, IEnumerable<DayOfWeek> shippingDays)
        {
            var lstShippingDays = shippingDays.ToList();
            DateTime date = specificDate.AddDays(1);
            while (!lstShippingDays.Contains(date.DayOfWeek) || date.IsHoliday())
            {
                date = date.AddDays(1);
            }
            return date;
        }
    }
}