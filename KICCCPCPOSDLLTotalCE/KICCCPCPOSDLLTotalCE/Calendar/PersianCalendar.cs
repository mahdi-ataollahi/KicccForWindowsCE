﻿// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
//
// ==--==
namespace System.Globalization
{
    using System;

    ////////////////////////////////////////////////////////////////////////////
    //
    //  Notes about PersianCalendar
    //
    ////////////////////////////////////////////////////////////////////////////
    // Modern Persian calendar is a solar observation based calendar. Each new year begins on the day when the vernal equinox occurs before noon.
    // The epoch is the date of the vernal equinox prior to the epoch of the Islamic calendar (Microsoft 19, 622 Julian or Microsoft 22, 622 Gregorian)

    // There is no Persian year 0. Ordinary years have 365 days. Leap years have 366 days with the last month (Esfand) gaining the extra day.
    /*
     **  Calendar support range:
     **      Calendar    Minimum     Maximum
     **      ==========  ==========  ==========
     **      Gregorian   0622/03/22   9999/12/31
     **      Persian     0001/01/01   9378/10/13
     */

    [Serializable]
    public class PersianCalendar : Calendar
    {
        // Number of 100ns (10E-7 second) ticks per time unit
        internal const long TicksPerMillisecond = 10000;
        internal const long TicksPerSecond = TicksPerMillisecond * 1000;
        internal const long TicksPerMinute = TicksPerSecond * 60;
        internal const long TicksPerHour = TicksPerMinute * 60;
        internal const long TicksPerDay = TicksPerHour * 24;

        // Number of milliseconds per time unit
        internal const int MillisPerSecond = 1000;
        internal const int MillisPerMinute = MillisPerSecond * 60;
        internal const int MillisPerHour = MillisPerMinute * 60;
        internal const int MillisPerDay = MillisPerHour * 24;

        // Return the tick count corresponding to the given hour, minute, second.
        // Will check the if the parameters are valid.
        internal static long TimeToTicks(int hour, int minute, int second, int millisecond)
        {
            if (hour >= 0 && hour < 24 && minute >= 0 && minute < 60 && second >= 0 && second < 60)
            {
                if (millisecond < 0 || millisecond >= MillisPerSecond)
                {
                    throw new ArgumentOutOfRangeException(
                                "millisecond",
                                String.Format(
                                    CultureInfo.InvariantCulture,
                                    "ArgumentOutOfRange_Range", 0, MillisPerSecond - 1));
                }
                return TimeToTicks(hour, minute, second) + millisecond * TicksPerMillisecond;
            }
            throw new ArgumentOutOfRangeException(null, "ArgumentOutOfRange_BadHourMinuteSecond");
        }

        internal static long TimeToTicks(int hour, int minute, int second)
        {
            long num = (long)hour * 3600L + (long)minute * 60L + second;
            if (num > 922337203685L || num < -922337203685L)
            {
                throw new ArgumentOutOfRangeException(null, "Overflow_TimeSpanTooLong");
            }

            return num * 10000000;
        }

        public static readonly int PersianEra = 1;

        internal static long PersianEpoch = new DateTime(622, 3, 22).Ticks / TicksPerDay;
        const int ApproximateHalfYear = 180;

        internal const int DatePartYear = 0;
        internal const int DatePartDayOfYear = 1;
        internal const int DatePartMonth = 2;
        internal const int DatePartDay = 3;
        internal const int MonthsPerYear = 12;

        internal static int[] DaysToMonth = { 0, 31, 62, 93, 124, 155, 186, 216, 246, 276, 306, 336, 366 };

        internal const int MaxCalendarYear = 9378;
        internal const int MaxCalendarMonth = 10;
        internal const int MaxCalendarDay = 13;

        // Persian calendar (year: 1, month: 1, day:1 ) = Gregorian (year: 622, month: 3, day: 22)
        // This is the minimal Gregorian date that we support in the PersianCalendar.
        internal static DateTime minDate = new DateTime(622, 3, 22);
        internal static DateTime maxDate = DateTime.MaxValue;

        /*=================================GetDefaultInstance==========================
        **Action: Internal method to provide a default intance of PersianCalendar.  Used by NLS+ implementation
        **       and other calendars.
        **Returns:
        **Arguments:
        **Exceptions:
        ============================================================================*/
        /*
        internal static Calendar GetDefaultInstance() {
            if (m_defaultInstance == null) {
                m_defaultInstance = new PersianCalendar();
            }
            return (m_defaultInstance);
        }
        */



        public override DateTime MinSupportedDateTime
        {
            get
            {
                return (minDate);
            }
        }


        public override DateTime MaxSupportedDateTime
        {
            get
            {
                return (maxDate);
            }
        }

        // Construct an instance of Persian calendar.

        public PersianCalendar()
        {
        }

        internal const int CAL_GREGORIAN = 1;     // Gregorian (localized) calendar
        internal int BaseCalendarID
        {
            get
            {
                return (CAL_GREGORIAN);
            }
        }

        internal const int CAL_PERSIAN = 22;
        internal int ID
        {
            get
            {
                return (CAL_PERSIAN);
            }
        }


        /*=================================GetAbsoluteDatePersian==========================
        **Action: Gets the Absolute date for the given Persian date.  The absolute date means
        **       the number of days from January 1st, 1 A.D.
        **Returns:
        **Arguments:
        **Exceptions:
        ============================================================================*/

        long GetAbsoluteDatePersian(int year, int month, int day)
        {
            if (year >= 1 && year <= MaxCalendarYear && month >= 1 && month <= 12)
            {
                int ordinalDay = DaysInPreviousMonths(month) + day - 1; // day is one based, make 0 based since this will be the number of days we add to beginning of year below
                int approximateDaysFromEpochForYearStart = (int)(CalendricalCalculationsHelper.MeanTropicalYearInDays * (year - 1));
                long yearStart = CalendricalCalculationsHelper.PersianNewYearOnOrBefore(PersianEpoch + approximateDaysFromEpochForYearStart + ApproximateHalfYear);
                yearStart += ordinalDay;
                return yearStart;
            }
            throw new ArgumentOutOfRangeException(null, "ArgumentOutOfRange_BadYearMonthDay");
        }

        static internal void CheckTicksRange(long ticks)
        {
            if (ticks < minDate.Ticks || ticks > maxDate.Ticks)
            {
                throw new ArgumentOutOfRangeException(
                            "time",
                            String.Format(
                                CultureInfo.InvariantCulture,
                                "ArgumentOutOfRange_CalendarRange",
                                minDate,
                                maxDate));
            }
        }

        static internal void CheckEraRange(int era)
        {
            if (era != CurrentEra && era != PersianEra)
            {
                throw new ArgumentOutOfRangeException("era", "ArgumentOutOfRange_InvalidEraValue");
            }
        }

        static internal void CheckYearRange(int year, int era)
        {
            CheckEraRange(era);
            if (year < 1 || year > MaxCalendarYear)
            {
                throw new ArgumentOutOfRangeException(
                            "year",
                            String.Format(
                                CultureInfo.CurrentCulture,
                                "ArgumentOutOfRange_Range",
                                1,
                                MaxCalendarYear));
            }
        }

        static internal void CheckYearMonthRange(int year, int month, int era)
        {
            CheckYearRange(year, era);
            if (year == MaxCalendarYear)
            {
                if (month > MaxCalendarMonth)
                {
                    throw new ArgumentOutOfRangeException(
                                "month",
                                String.Format(
                                    CultureInfo.CurrentCulture,
                                    "ArgumentOutOfRange_Range",
                                    1,
                                    MaxCalendarMonth));
                }
            }

            if (month < 1 || month > 12)
            {
                throw new ArgumentOutOfRangeException("month", "ArgumentOutOfRange_Month");
            }
        }

        static int MonthFromOrdinalDay(int ordinalDay)
        {
            int index = 0;
            while (ordinalDay > DaysToMonth[index])
                index++;

            return index;
        }

        static int DaysInPreviousMonths(int month)
        {
            --month; // months are one based but for calculations use 0 based
            return DaysToMonth[month];
        }

        /*=================================GetDatePart==========================
        **Action: Returns a given date part of this <i>DateTime</i>. This method is used
        **       to compute the year, day-of-year, month, or day part.
        **Returns:
        **Arguments:
        **Exceptions:  ArgumentException if part is incorrect.
        ============================================================================*/

        internal int GetDatePart(long ticks, int part)
        {
            long NumDays;                 // The calculation buffer in number of days.

            CheckTicksRange(ticks);

            //
            //  Get the absolute date.  The absolute date is the number of days from January 1st, 1 A.D.
            //  1/1/0001 is absolute date 1.
            //
            NumDays = ticks / TicksPerDay + 1;

            //
            //  Calculate the appromixate Persian Year.
            //

            long yearStart = CalendricalCalculationsHelper.PersianNewYearOnOrBefore(NumDays);
            int y = (int)(Math.Floor(((yearStart - PersianEpoch) / CalendricalCalculationsHelper.MeanTropicalYearInDays) + 0.5)) + 1;

            if (part == DatePartYear)
            {
                return y;
            }

            //
            //  Calculate the Persian Month.
            //

            int ordinalDay = (int)(NumDays - CalendricalCalculationsHelper.GetNumberOfDays(this.ToDateTime(y, 1, 1, 0, 0, 0, 0, 1)));

            if (part == DatePartDayOfYear)
            {
                return ordinalDay;
            }

            int m = MonthFromOrdinalDay(ordinalDay);
            if (part == DatePartMonth)
            {
                return m;
            }

            int d = ordinalDay - DaysInPreviousMonths(m);

            //
            //  Calculate the Persian Day.
            //

            if (part == DatePartDay)
            {
                return (d);
            }

            // Incorrect part value.
            throw new InvalidOperationException("InvalidOperation_DateTimeParsing");
        }

        // Returns the DateTime resulting from adding the given number of
        // months to the specified DateTime. The result is computed by incrementing
        // (or decrementing) the year and month parts of the specified DateTime by
        // value months, and, if required, adjusting the day part of the
        // resulting date downwards to the last day of the resulting month in the
        // resulting year. The time-of-day part of the result is the same as the
        // time-of-day part of the specified DateTime.
        //
        // In more precise terms, considering the specified DateTime to be of the
        // form y / m / d + t, where y is the
        // year, m is the month, d is the day, and t is the
        // time-of-day, the result is y1 / m1 / d1 + t,
        // where y1 and m1 are computed by adding value months
        // to y and m, and d1 is the largest value less than
        // or equal to d that denotes a valid day in month m1 of year
        // y1.
        //


        public override DateTime AddMonths(DateTime time, int months)
        {
            if (months < -120000 || months > 120000)
            {
                throw new ArgumentOutOfRangeException(
                            "months",
                            String.Format(
                                CultureInfo.CurrentCulture,
                                "ArgumentOutOfRange_Range",
                                -120000,
                                120000));
            }
            // Get the date in Persian calendar.
            int y = GetDatePart(time.Ticks, DatePartYear);
            int m = GetDatePart(time.Ticks, DatePartMonth);
            int d = GetDatePart(time.Ticks, DatePartDay);
            int i = m - 1 + months;
            if (i >= 0)
            {
                m = i % 12 + 1;
                y = y + i / 12;
            }
            else
            {
                m = 12 + (i + 1) % 12;
                y = y + (i - 11) / 12;
            }
            int days = GetDaysInMonth(y, m);
            if (d > days)
            {
                d = days;
            }
            long ticks = GetAbsoluteDatePersian(y, m, d) * TicksPerDay + time.Ticks % TicksPerDay;
            CheckAddResult(ticks, MinSupportedDateTime, MaxSupportedDateTime);
            return (new DateTime(ticks));
        }

        internal static void CheckAddResult(long ticks, DateTime minValue, DateTime maxValue)
        {
            if (ticks < minValue.Ticks || ticks > maxValue.Ticks)
            {
                throw new ArgumentException(
                    String.Format(CultureInfo.InvariantCulture, "Argument_ResultCalendarRange",
                        minValue, maxValue));
            }
        }

        // Returns the DateTime resulting from adding the given number of
        // years to the specified DateTime. The result is computed by incrementing
        // (or decrementing) the year part of the specified DateTime by value
        // years. If the month and day of the specified DateTime is 2/29, and if the
        // resulting year is not a leap year, the month and day of the resulting
        // DateTime becomes 2/28. Otherwise, the month, day, and time-of-day
        // parts of the result are the same as those of the specified DateTime.
        //


        public override DateTime AddYears(DateTime time, int years)
        {
            return (AddMonths(time, years * 12));
        }

        // Returns the day-of-month part of the specified DateTime. The returned
        // value is an integer between 1 and 31.
        //


        public override int GetDayOfMonth(DateTime time)
        {
            return (GetDatePart(time.Ticks, DatePartDay));
        }

        // Returns the day-of-week part of the specified DateTime. The returned value
        // is an integer between 0 and 6, where 0 indicates Sunday, 1 indicates
        // Monday, 2 indicates Tuesday, 3 indicates Wednesday, 4 indicates
        // Thursday, 5 indicates Friday, and 6 indicates Saturday.
        //


        public override DayOfWeek GetDayOfWeek(DateTime time)
        {
            return ((DayOfWeek)((int)(time.Ticks / TicksPerDay + 1) % 7));
        }

        // Returns the day-of-year part of the specified DateTime. The returned value
        // is an integer between 1 and 366.
        //


        public override int GetDayOfYear(DateTime time)
        {
            return (GetDatePart(time.Ticks, DatePartDayOfYear));
        }

        // Returns the number of days in the month given by the year and
        // month arguments.
        //


        public override int GetDaysInMonth(int year, int month, int era)
        {
            CheckYearMonthRange(year, month, era);

            if ((month == MaxCalendarMonth) && (year == MaxCalendarYear))
            {
                return MaxCalendarDay;
            }

            int daysInMonth = DaysToMonth[month] - DaysToMonth[month - 1];
            if ((month == MonthsPerYear) && !IsLeapYear(year))
            {
                --daysInMonth;
            }
            return daysInMonth;
        }

        // Returns the number of days in the year given by the year argument for the current era.
        //

        public override int GetDaysInYear(int year, int era)
        {
            CheckYearRange(year, era);
            if (year == MaxCalendarYear)
            {
                return DaysToMonth[MaxCalendarMonth - 1] + MaxCalendarDay;
            }
            // Common years have 365 days.  Leap years have 366 days.
            return (IsLeapYear(year, CurrentEra) ? 366 : 365);
        }

        // Returns the era for the specified DateTime value.


        public override int GetEra(DateTime time)
        {
            CheckTicksRange(time.Ticks);
            return (PersianEra);
        }



        public override int[] Eras
        {
            get
            {
                return (new int[] { PersianEra });
            }
        }

        // Returns the month part of the specified DateTime. The returned value is an
        // integer between 1 and 12.
        //


        public override int GetMonth(DateTime time)
        {
            return (GetDatePart(time.Ticks, DatePartMonth));
        }

        // Returns the number of months in the specified year and era.


        public override int GetMonthsInYear(int year, int era)
        {
            CheckYearRange(year, era);
            if (year == MaxCalendarYear)
            {
                return MaxCalendarMonth;
            }
            return (12);
        }

        // Returns the year part of the specified DateTime. The returned value is an
        // integer between 1 and MaxCalendarYear.
        //


        public override int GetYear(DateTime time)
        {
            return (GetDatePart(time.Ticks, DatePartYear));
        }

        // Checks whether a given day in the specified era is a leap day. This method returns true if
        // the date is a leap day, or false if not.
        //


        public override bool IsLeapDay(int year, int month, int day, int era)
        {
            // The year/month/era value checking is done in GetDaysInMonth().
            int daysInMonth = GetDaysInMonth(year, month, era);
            if (day < 1 || day > daysInMonth)
            {
                throw new ArgumentOutOfRangeException(
                            "day",
                            String.Format(
                                CultureInfo.CurrentCulture,
                                "ArgumentOutOfRange_Day",
                                daysInMonth,
                                month));
            }
            return (IsLeapYear(year, era) && month == 12 && day == 30);
        }

        // Returns  the leap month in a calendar year of the specified era. This method returns 0
        // if this calendar does not have leap month, or this year is not a leap year.
        //


        public int GetLeapMonth(int year, int era)
        {
            CheckYearRange(year, era);
            return (0);
        }

        // Checks whether a given month in the specified era is a leap month. This method returns true if
        // month is a leap month, or false if not.
        //


        public override bool IsLeapMonth(int year, int month, int era)
        {
            CheckYearMonthRange(year, month, era);
            return (false);
        }

        // Checks whether a given year in the specified era is a leap year. This method returns true if
        // year is a leap year, or false if not.
        //

        public override bool IsLeapYear(int year, int era)
        {
            CheckYearRange(year, era);

            if (year == MaxCalendarYear)
            {
                return false;
            }

            return (GetAbsoluteDatePersian(year + 1, 1, 1) - GetAbsoluteDatePersian(year, 1, 1)) == 366;
        }

        // Returns the date and time converted to a DateTime value.  Throws an exception if the n-tuple is invalid.
        //


        public override DateTime ToDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, int era)
        {
            // The year/month/era checking is done in GetDaysInMonth().
            int daysInMonth = GetDaysInMonth(year, month, era);
            if (day < 1 || day > daysInMonth)
            {
                throw new ArgumentOutOfRangeException(
                            "day",
                            String.Format(
                                CultureInfo.CurrentCulture,
                                "ArgumentOutOfRange_Day",
                                daysInMonth,
                                month));
            }

            long lDate = GetAbsoluteDatePersian(year, month, day);

            if (lDate >= 0)
            {
                return (new DateTime(lDate * TicksPerDay + TimeToTicks(hour, minute, second, millisecond)));
            }
            else
            {
                throw new ArgumentOutOfRangeException(null, "ArgumentOutOfRange_BadYearMonthDay");
            }
        }

        private const int DEFAULT_TWO_DIGIT_YEAR_MAX = 1410;

        internal int twoDigitYearMax = 1429;

        public override int TwoDigitYearMax
        {
            get
            {
                return (twoDigitYearMax);
            }

            set
            {
                if (value < 99 || value > MaxCalendarYear)
                {
                    throw new ArgumentOutOfRangeException(
                                "value",
                                String.Format(
                                    CultureInfo.CurrentCulture,
                                    "ArgumentOutOfRange_Range",
                                    99,
                                    MaxCalendarYear));
                }
                twoDigitYearMax = value;
            }
        }



        public override int ToFourDigitYear(int year)
        {
            if (year < 0)
            {
                throw new ArgumentOutOfRangeException("year", "ArgumentOutOfRange_NeedNonNegNum");
            }

            if (year < 100)
            {
                return (base.ToFourDigitYear(year));
            }

            if (year > MaxCalendarYear)
            {
                throw new ArgumentOutOfRangeException(
                            "year",
                            String.Format(
                                CultureInfo.CurrentCulture,
                                "ArgumentOutOfRange_Range",
                                1,
                                MaxCalendarYear));
            }
            return (year);
        }
    }
}