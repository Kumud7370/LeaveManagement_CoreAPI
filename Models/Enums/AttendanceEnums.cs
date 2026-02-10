namespace AttendanceManagementSystem.Models.Enums
{
    public enum AttendanceStatus
    {
        Present = 1,
        Absent = 2,
        HalfDay = 3,
        Leave = 4,
        Holiday = 5,
        WeekOff = 6,
        OnDuty = 7,
        WorkFromHome = 8
    }

    public enum CheckInMethod
    {
        Biometric = 1,
        WebApp = 2,
        MobileApp = 3,
        Manual = 4,
        RFID = 5
    }

    public enum RegularizationType
    {
        MissedPunch = 1,
        LateEntry = 2,
        EarlyExit = 3,
        FullDayRegularization = 4
    }

    public enum RegularizationStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Cancelled = 4
    }
}