using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Models;

public partial class StarBankContext : DbContext
{
  
    public StarBankContext(DbContextOptions<StarBankContext> options): base(options)
    {
    }

    public DbSet<Automóvil> Automóvils { get; set; }

    public  DbSet<Cliente> Clientes { get; set; }

    public DbSet<CreditoEducativo> CreditoEducativos { get; set; }

    public  DbSet<Hipoteca> Hipotecas { get; set; }

    public DbSet<TarjetasCredito> TarjetasCreditos { get; set; }

    public  DbSet<TarjetasDebito> TarjetasDebitos { get; set; }

    public DbSet<HistorialTransacciones> HistorialTransacciones { get; set; }

    public DbSet<InicioSesion> InicioSesion { get; set; }

    public DbSet<Servicio> Servicios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Automóvil>(entity =>
        {
            entity.HasKey(e => e.AutoId).HasName("PK__Automóvi__6B23296593CD5C4C");

            entity.ToTable("Automóvil");

            entity.Property(e => e.AutoId).HasColumnName("AutoID");
            entity.Property(e => e.NumeroCuenta).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DeudaAuto).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IdCliente).HasColumnName("ID_Cliente");
            entity.Property(e => e.Mensualidad).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.oCliente).WithMany(p => p.Automóvils)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK__Automóvil__ID_Cl__4D94879B");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.ClienteId).HasName("PK__Clientes__71ABD08730F73DEE");

            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CreditoEducativo>(entity =>
        {
            entity.HasKey(e => e.IdCreditoEducativo).HasName("PK__CreditoE__B0B690537E28B01F");

            entity.ToTable("CreditoEducativo");

            entity.Property(e => e.IdCreditoEducativo).HasColumnName("IdCreditoEducativo");
            entity.Property(e => e.IdCliente).HasColumnName("IdCliente");
            entity.Property(e => e.NumeroCuenta).IsRequired().HasMaxLength(20);
            entity.Property(e => e.IngresoMensual).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.Mensualidad).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TotalDeuda).HasColumnType("decimal(15, 2)");

            entity.HasOne(d => d.oCliente).WithMany(p => p.CreditoEducativos)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK__CreditoEd__ID_Cl__5070F446");
        });

        modelBuilder.Entity<Hipoteca>(entity =>
        {
            entity.HasKey(e => e.HipotecaId).HasName("PK__Hipoteca__ADD0DB0577E6E1C7");

            entity.ToTable("Hipoteca");

            entity.Property(e => e.HipotecaId).HasColumnName("HipotecaID");
            entity.Property(e => e.NumeroCuenta).IsRequired().HasMaxLength(20); 
            entity.Property(e => e.DeudaHipoteca).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IdCliente).HasColumnName("ID_Cliente");
            entity.Property(e => e.Mensualidad).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.oCliente).WithMany(p => p.Hipotecas)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK__Hipoteca__ID_Cli__4AB81AF0");
        });

        modelBuilder.Entity<TarjetasCredito>(entity =>
        {
            entity.HasKey(e => e.CreditoId).HasName("PK__Tarjetas__4FE406FD5C0D2296");

            entity.ToTable("TarjetasCredito");

            entity.Property(e => e.CreditoId).HasColumnName("CreditoID");
            entity.Property(e => e.Deuda).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.IdCliente).HasColumnName("ID_Cliente");
            entity.Property(e => e.LimiteTarjetaCredito).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.NipTarjetaCredito)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.NumeroCuentaCredito)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TarjetaCredito)
                .HasMaxLength(16)
                .IsUnicode(false);

            entity.HasOne(d => d.oCliente).WithMany(p => p.TarjetasCreditos)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK__TarjetasC__ID_Cl__47DBAE45");
        });

        modelBuilder.Entity<TarjetasDebito>(entity =>
        {
            entity.HasKey(e => e.DebitoId).HasName("PK__Tarjetas__94F87B9E2F668C11");

            entity.ToTable("TarjetasDebito");

            entity.Property(e => e.DebitoId).HasColumnName("DebitoID");
            entity.Property(e => e.NiptarjetaDebito)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("NIPTarjetaDebito");
            entity.Property(e => e.NumeroCuentaDebito)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Saldo).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TarjetaDebito)
                .HasMaxLength(16)
                .IsUnicode(false);

            entity.HasOne(d => d.Cliente).WithMany(p => p.TarjetasDebitos)
                .HasForeignKey(d => d.ClienteId)
                .HasConstraintName("FK_ClienteId");
        });

        modelBuilder.Entity<InicioSesion>(entity =>
        {
            entity.HasKey(e => e.CorreoID).HasName("PK__InicioSesion__6B23296593CD5C4C");

            entity.ToTable("InicioSesion");

            entity.Property(e => e.CorreoID).HasColumnName("CorreoID");
            entity.Property(e => e.correo).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.contraseña).HasMaxLength(255).IsUnicode(false);
        });

        modelBuilder.Entity<HistorialTransacciones>(entity =>
        {
            entity.ToTable("HistorialTransacciones"); // Asegúrate de que el nombre de la tabla coincida con la base de datos

            entity.Property(e => e.T_ID).HasColumnName("T_ID");
            entity.Property(e => e.CorreoID).HasColumnName("CorreoID").IsRequired();
            entity.Property(e => e.TipoTransaccion).HasColumnName("TipoTransaccion").HasColumnType("varchar(50)");
            entity.Property(e => e.Monto).HasColumnName("Monto").HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Fecha).HasColumnName("Fecha");
            entity.Property(e => e.NumeroCuenta).HasColumnName("NumeroCuenta").HasColumnType("varchar(20)");
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.ToTable("servicios");

            entity.HasKey(e => e.Id); // Definir la clave primaria

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd() // Para AUTO_INCREMENT
                .IsRequired();

            entity.Property(e => e.CFE)
                .HasColumnName("CFE")
                .IsRequired() // Si es un campo obligatorio
                .HasMaxLength(27) // Longitud máxima de la cadena
                .IsUnicode(false); // Para usar VARCHAR en lugar de NVARCHAR

            entity.Property(e => e.CFE_Mensualidad)
                .HasColumnName("CFE_Mensualidad")
                .HasColumnType("decimal(10, 2)") // Especificar el tipo de columna como decimal
                .IsRequired();

            entity.Property(e => e.JAPAY)
                .HasColumnName("JAPAY")
                .IsRequired()
                .HasMaxLength(27)
                .IsUnicode(false);

            entity.Property(e => e.JAPAY_Mensualidad)
                .HasColumnName("JAPAY_Mensualidad")
                .HasColumnType("decimal(10, 2)")
                .IsRequired();

            entity.Property(e => e.Telmex)
                .HasColumnName("Telmex")
                .IsRequired()
                .HasMaxLength(9)
                .IsUnicode(false);

            entity.Property(e => e.Telmex_Mensualidad)
                .HasColumnName("Telmex_Mensualidad")
                .HasColumnType("decimal(10, 2)")
                .IsRequired();

            entity.Property(e => e.Totalplay)
                .HasColumnName("Totalplay")
                .IsRequired()
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.Property(e => e.Totalplay_Mensualidad)
                .HasColumnName("Totalplay_Mensualidad")
                .HasColumnType("decimal(10, 2)")
                .IsRequired();

            entity.Property(e => e.Izzi)
                .HasColumnName("Izzi")
                .IsRequired()
                .HasMaxLength(8)
                .IsUnicode(false);

            entity.Property(e => e.Izzi_Mensualidad)
                .HasColumnName("Izzi_Mensualidad")
                .HasColumnType("decimal(10, 2)")
                .IsRequired();

            // Configuraciones adicionales si son necesarias
        });






        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
