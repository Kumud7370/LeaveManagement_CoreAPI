namespace AttendanceManagementSystem.Common.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo IndiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        /// <summary>
        /// Get current time in India timezone
        /// </summary>
        public static DateTime GetIndiaTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IndiaTimeZone);
        }

        /// <summary>
        /// Convert India time to UTC
        /// </summary>
        public static DateTime ConvertToUtc(DateTime indiaTime)
        {
            // If already UTC, return as is
            if (indiaTime.Kind == DateTimeKind.Utc)
                return indiaTime;

            // Specify that this is India time, then convert to UTC
            var indiaDateTime = DateTime.SpecifyKind(indiaTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(indiaDateTime, IndiaTimeZone);
        }

        /// <summary>
        /// Convert UTC time to India time
        /// </summary>
        public static DateTime ConvertToIndiaTime(DateTime utcTime)
        {
            if (utcTime.Kind != DateTimeKind.Utc)
                utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, IndiaTimeZone);
        }

        /// <summary>
        /// Get date-only in India timezone, converted to UTC midnight
        /// This is CRITICAL for attendance date storage and comparison
        /// </summary>
        public static DateTime GetIndiaDateAsUtc(DateTime indiaDateTime)
        {
            // Get just the date part in India timezone
            var indiaDate = indiaDateTime.Date;

            // Create a DateTime for midnight of that date in India timezone
            var indiaMidnight = new DateTime(indiaDate.Year, indiaDate.Month, indiaDate.Day, 0, 0, 0, DateTimeKind.Unspecified);

            // Convert India midnight to UTC
            return TimeZoneInfo.ConvertTimeToUtc(indiaMidnight, IndiaTimeZone);
        }

        /// <summary>
        /// Get today's date in India timezone, converted to UTC midnight
        /// </summary>
        public static DateTime GetTodayIndiaDateAsUtc()
        {
            var indiaTime = GetIndiaTime();
            return GetIndiaDateAsUtc(indiaTime);
        }

        /// <summary>
        /// Get start of day in India timezone (midnight)
        /// </summary>
        public static DateTime GetTodayStartIndia()
        {
            var indiaTime = GetIndiaTime();
            return new DateTime(indiaTime.Year, indiaTime.Month, indiaTime.Day, 0, 0, 0, DateTimeKind.Unspecified);
        }

        /// <summary>
        /// Get end of day in India timezone (23:59:59)
        /// </summary>
        public static DateTime GetTodayEndIndia()
        {
            var indiaTime = GetIndiaTime();
            return new DateTime(indiaTime.Year, indiaTime.Month, indiaTime.Day, 23, 59, 59, DateTimeKind.Unspecified);
        }

        /// <summary>
        /// Format UTC datetime to India datetime string
        /// </summary>
        public static string FormatToIndiaDateTime(DateTime utcDateTime)
        {
            var indiaTime = ConvertToIndiaTime(utcDateTime);
            return indiaTime.ToString("dd-MM-yyyy hh:mm tt");
        }

        /// <summary>
        /// Format UTC datetime to India date string
        /// </summary>
        public static string FormatToIndiaDate(DateTime utcDateTime)
        {
            var indiaTime = ConvertToIndiaTime(utcDateTime);
            return indiaTime.ToString("dd-MM-yyyy");
        }

        /// <summary>
        /// Format UTC datetime to India time string
        /// </summary>
        public static string FormatToIndiaTime(DateTime utcDateTime)
        {
            var indiaTime = ConvertToIndiaTime(utcDateTime);
            return indiaTime.ToString("hh:mm tt");
        }

        /// <summary>
        /// Check if two dates are the same day in India timezone
        /// </summary>
        public static bool IsSameDayInIndia(DateTime utcDate1, DateTime utcDate2)
        {
            var india1 = ConvertToIndiaTime(utcDate1).Date;
            var india2 = ConvertToIndiaTime(utcDate2).Date;
            return india1 == india2;
        }
    }
}