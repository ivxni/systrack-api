using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace systrack_api.Models
{
    public class Computer
    {
        [Key]
        [Column("computer_id")]
        public int ComputerId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        [Column("ComputerName")]
        public string? ComputerName { get; set; }

        [Column("Ram")]
        public double? Ram { get; set; }

        [Column("Cpu")]
        public double? Cpu { get; set; }

        [Column("Mac")]
        public string? Mac { get; set; }
    }
}
