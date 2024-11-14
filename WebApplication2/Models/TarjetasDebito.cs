using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebApplication2.Models;

public partial class TarjetasDebito
{
    public int DebitoId { get; set; }

    public int? ClienteId { get; set; }

    public string TarjetaDebito { get; set; } = null!;

    public string NiptarjetaDebito { get; set; } = null!;

    public DateOnly? FechaExpiracionDebito { get; set; }

    public decimal Saldo { get; set; }

    public string NumeroCuentaDebito { get; set; } = null!;

  
    public virtual Cliente? Cliente { get; set; }
}
