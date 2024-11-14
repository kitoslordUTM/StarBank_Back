using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplication2.Models
{
    public class HistorialTransacciones
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int T_ID { get; set; }

        [Required] // Indica que el campo CorreoID no puede ser nulo
        public int CorreoID { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string TipoTransaccion { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Monto { get; set; }

        public DateTime Fecha { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string NumeroCuenta { get; set; }
    }
}
