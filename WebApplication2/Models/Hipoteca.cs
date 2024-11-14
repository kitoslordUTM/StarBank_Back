using System;
using System.Collections.Generic;

namespace WebApplication2.Models;

public partial class Hipoteca
{
    public int HipotecaId { get; set; }

    public int? IdCliente { get; set; }

    public string NumeroCuenta { get; set; } = null!;

    public decimal? DeudaHipoteca { get; set; }

    public decimal? Mensualidad { get; set; }

    public int? MesesDelAdeudo { get; set; }

    public virtual Cliente? oCliente { get; set; }
}
