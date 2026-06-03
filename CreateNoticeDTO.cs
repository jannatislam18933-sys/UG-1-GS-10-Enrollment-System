using System.ComponentModel.DataAnnotations;

namespace EnrollmentSystemAPI.DTOs
{
    public class CreateNoticeDTO
    {
        [Required]
        public string Title { get; set; }
            = string.Empty;

        [Required]
        public string Description { get; set; }
            = string.Empty;

        public int? DepartmentID { get; set; }
    }
}