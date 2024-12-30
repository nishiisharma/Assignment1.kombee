using System.ComponentModel.DataAnnotations;

namespace PracticeAPI.Models
{

    public class User
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string ContactNumber { get; set; }
        public required string Postcode { get; set; }
        public required string PasswordHash { get; set; }
        //public string Role { get; set; }
        public required string Gender { get; set; }
        public required string Address { get; set; }
        public required string City { get; set; }
        public required string State { get; set; }
        public required List<Hobby> Hobbies { get; set; }
        public required List<FileUpload> Files { get; set; }
    }

    public class Hobby
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int UserId { get; set; }
        public required User User { get; set; }
    }

    public class FileUpload
    {
        public int Id { get; set; }
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public int UserId { get; set; }
        public required User User { get; set; }
    }
}
