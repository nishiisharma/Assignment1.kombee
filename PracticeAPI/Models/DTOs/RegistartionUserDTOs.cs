using System.ComponentModel.DataAnnotations;

namespace PracticeAPI.Models.DTOs
{
    public class RegistartionUserDTOs
    {
        [Required, MaxLength(50)]
        public string? FirstName { get; set; }

        [Required, MaxLength(50)]
        public string? LastName { get; set; }

        [Required, EmailAddress]
        public string? Email { get; set; }

        [Required, Phone]
        public string? ContactNumber { get; set; }

        [Required, MaxLength(6)]
        public string? Postcode { get; set; }

        [Required, MinLength(6)]
        public string? Password { get; set; }

        [Compare("Password")]
        public string? ConfirmPassword { get; set; }

        [Required]
        public string? Gender { get; set; }

        [Required]
        public string? Address { get; set; }

        [Required]
        public string? City { get; set; }

        [Required]
        public string? State { get; set; }

        [Required]
        public List<string>? Hobbies { get; set; }

        [Required]
        public ICollection<IFormFile>? Files { get; set; }
    }

}


