namespace EnrollmentSystemAPI.Models
{
    public class EnrollmentSettings
    {
        public int EnrollmentSettingsID { get; set; }

        public bool IsEnrollmentOpen { get; set; }

        public DateTime? OpenDate { get; set; }

        public DateTime? CloseDate { get; set; }
    }
}