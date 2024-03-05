using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace systrack_api.Models
{
    public class Order
    {
        [Key]
        [Column("order_id")]
        public int OrderId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        [Required(ErrorMessage = "Der Bestellname ist erforderlich.")]
        [Column("OrderName")]
        public string? OrderName { get; set; }

        [Required(ErrorMessage = "Das Bestelldatum ist erforderlich.")]
        [Column("OrderDate")]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Der Kauftyp ist erforderlich.")]
        [Column("PurchaseType")]
        [EnumDataType(typeof(PurchaseType))]
        public PurchaseType PurchaseType { get; set; } // (Barkauf oder Finanzierung)

        [Column("CashPurchasePrice")]
        public double? CashPurchasePrice { get; set; } // Preis bei Barkauf

        [Column("MonthlyRate")]
        public double? MonthlyRate { get; set; } // Monatliche Rate bei Finanzierung

        [Column("Term")]
        public int? Term { get; set; } // Laufzeit in Monaten

        [Column("FinalPrice")]
        public double? FinalPrice { get; set; } // Endpreis nach Ablauf der Finanzierung
    }

    public enum PurchaseType
    {
        CashPurchase,
        Financing
    }
}
