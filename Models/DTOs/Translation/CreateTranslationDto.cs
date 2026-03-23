namespace AttendanceManagementSystem.Models.DTOs.Translation
{
    public class CreateTranslationDto
    {
        public string Key { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public string Mr { get; set; } = string.Empty;
        public string? En { get; set; }
        public string? Hi { get; set; }
    }
}
