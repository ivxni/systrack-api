using System.ComponentModel.DataAnnotations;

namespace systrack_api.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password_Hash { get; set; }

        public Role Role { get; set; } = Role.Customer;
        public Customer? Customer { get; set; }
    }
}
