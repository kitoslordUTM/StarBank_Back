using System;
using System.Collections.Generic;

namespace WebApplication2.Models;

public partial class Cliente
{
    public int ClienteId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public virtual ICollection<Automóvil> Automóvils { get; set; } = new List<Automóvil>();

    public virtual ICollection<CreditoEducativo> CreditoEducativos { get; set; } = new List<CreditoEducativo>();

    public virtual ICollection<Hipoteca> Hipotecas { get; set; } = new List<Hipoteca>();

    public virtual ICollection<TarjetasCredito> TarjetasCreditos { get; set; } = new List<TarjetasCredito>();

    public virtual ICollection<TarjetasDebito> TarjetasDebitos { get; set; } = new List<TarjetasDebito>();
}
