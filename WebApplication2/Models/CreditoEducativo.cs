using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Models;

public partial class CreditoEducativo
{
    public int IdCreditoEducativo { get; set; }

    [Column("ID_Cliente")]
    public int? IdCliente { get; set; }

    public string NumeroCuenta { get; set; } = null!;

    public string? Tipo { get; set; }

    public int? Plazo { get; set; }

    public DateOnly? InicioPago { get; set; }

    public decimal? TotalDeuda { get; set; }

    public decimal? IngresoMensual { get; set; }

    public decimal? Mensualidad { get; set; }

    public virtual Cliente? oCliente { get; set; }
}
