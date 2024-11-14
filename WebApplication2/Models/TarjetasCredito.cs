using System;
using System.Collections.Generic;

namespace WebApplication2.Models;

public partial class TarjetasCredito
{
    public int CreditoId { get; set; }

    public int? IdCliente { get; set; }

    public string TarjetaCredito { get; set; } = null!;

    public decimal? LimiteTarjetaCredito { get; set; }

    public decimal? Deuda { get; set; }

    public DateOnly? FechaExpiracionCredito { get; set; }

    public string NumeroCuentaCredito { get; set; } = null!;

    public string NipTarjetaCredito { get; set; } = null!;

    public virtual Cliente? oCliente { get; set; }
}
