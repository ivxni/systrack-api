﻿using System.ComponentModel.DataAnnotations;

namespace systrack_api.Models
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public string Role { get; set; } = "Customer";
    }

}
