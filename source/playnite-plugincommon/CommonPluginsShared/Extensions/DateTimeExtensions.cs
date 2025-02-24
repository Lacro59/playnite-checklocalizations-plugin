using System;
using System.Collections.Generic;
using System.Text;

namespace CommonPluginsShared.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime EndOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }
            return dt.AddDays(diff).Date;
        }


        public static int GetDaysBetween(this DateTime dt)
        {
            return dt.GetDaysBetween(DateTime.Now);
        }

        public static int GetDaysBetween(this DateTime dt, DateTime dtCompare)
        {
            if (dtCompare == null)
            {
                dtCompare = DateTime.Now;
            }

            if (dt > dtCompare)
            {
                return dtCompare.GetDaysBetween(dt);
            }

            double diff = (DateTime.Parse(dtCompare.ToString("yyyy-MM-dd")) - DateTime.Parse(dt.ToString("yyyy-MM-dd"))).TotalDays;
            return (int)diff;
        }

        public static int GetMonthsBetween(this DateTime dt)
        {
            return dt.GetMonthsBetween(DateTime.Now);
        }

        // https://stackoverflow.com/a/27332466/17923426
        public static int GetMonthsBetween(this DateTime dt, DateTime dtCompare)
        {
            if (dtCompare == null)
            {
                dtCompare = DateTime.Now;
            }

            if (dt > dtCompare)
            {
                return dtCompare.GetYearsBetween(dt);
            }

            int monthDiff = ((dtCompare.Year * 12) + dtCompare.Month) - ((dt.Year * 12) + dt.Month);
            return monthDiff;
        }

        public static int GetYearsBetween(this DateTime dt)
        {
            return dt.GetYearsBetween(DateTime.Now);
        }

        // https://stackoverflow.com/a/27332466/17923426
        public static int GetYearsBetween(this DateTime dt, DateTime dtCompare)
        {
            if (dtCompare == null)
            {
                dtCompare = DateTime.Now;
            }

            if (dt > dtCompare)
            {
                return dtCompare.GetYearsBetween(dt);
            }

            int monthDiff = ((dtCompare.Year * 12) + dtCompare.Month) - ((dt.Year * 12) + dt.Month) + 1;
            int years = (int)Math.Floor((decimal)(monthDiff / 12));
            return years;
        }
    }
}
