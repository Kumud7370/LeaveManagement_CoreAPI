namespace AttendanceManagementSystem.Common.Helpers
{
    public static class DateTimeHelper
    {
        private static readonly TimeZoneInfo IndiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

        public static DateTime GetIndiaTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IndiaTimeZone);
        }

        public static DateTime ConvertToUtc(DateTime indiaTime)
        {
            if (indiaTime.Kind == DateTimeKind.Utc)
                return indiaTime;

            var indiaDateTime = DateTime.SpecifyKind(indiaTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(indiaDateTime, IndiaTimeZone);
        }

        public static DateTime ConvertToIndiaTime(DateTime utcTime)
        {
            if (utcTime.Kind != DateTimeKind.Utc)
                utcTime = DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, IndiaTimeZone);
        }

        public static DateTime GetIndiaDateAsUtc(DateTime indiaDateTime)
        {
            var indiaDate = indiaDateTime.Date;

            var indiaMidnight = new DateTime(indiaDate.Year, indiaDate.Month, indiaDate.Day, 0, 0, 0, DateTimeKind.Unspecified);

            return TimeZoneInfo.ConvertTimeToUtc(indiaMidnight, IndiaTimeZone);
        }

        public static DateTime GetTodayIndiaDateAsUtc()
        {
            var indiaTime = GetIndiaTime();
            return GetIndiaDateAsUtc(indiaTime);
        }

        public static DateTime GetTodayStartIndia()
        {
            var indiaTime = GetIndiaTime();
            return new DateTime(indiaTime.Year, indiaTime.Month, indiaTime.Day, 0, 0, 0, DateTimeKind.Unspecified);
        }

        public static DateTime GetTodayEndIndia()
        {
            var indiaTime = GetIndiaTime();
            return new DateTime(indiaTime.Year, indiaTime.Month, indiaTime.Day, 23, 59, 59, DateTimeKind.Unspecified);
        }

        public static string FormatToIndiaDateTime(DateTime utcDateTime)
        {
            var indiaTime = ConvertToIndiaTime(utcDateTime);
            return indiaTime.ToString("dd-MM-yyyy hh:mm tt");
        }

        public static string FormatToIndiaDate(DateTime utcDateTime)
        {
            var indiaTime = ConvertToIndiaTime(utcDateTime);
            return indiaTime.ToString("dd-MM-yyyy");
        }

        public static string FormatToIndiaTime(DateTime utcDateTime)
        {
            var indiaTime = ConvertToIndiaTime(utcDateTime);
            return indiaTime.ToString("hh:mm tt");
        }

        public static bool IsSameDayInIndia(DateTime utcDate1, DateTime utcDate2)
        {
            var india1 = ConvertToIndiaTime(utcDate1).Date;
            var india2 = ConvertToIndiaTime(utcDate2).Date;
            return india1 == india2;
        }
    }
}