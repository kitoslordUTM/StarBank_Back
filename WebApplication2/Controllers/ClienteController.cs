using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;


using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Net;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {

        public readonly StarBankContext _dbcontext;

        public ClienteController(StarBankContext _context) {

            _dbcontext= _context;
        }

        // inicio de sesion 

        [HttpPost("IniciarSesion")]
        public IActionResult IniciarSesion([FromBody] InicioSesion inicioSesion)
        {
            if (ModelState.IsValid) // Verifica si el modelo es válido
            {
                var usuario = _dbcontext.InicioSesion.FirstOrDefault(u => u.correo == inicioSesion.correo && u.contraseña == inicioSesion.contraseña);

                if (usuario != null)
                {
                    // Asegúrate de incluir el punto y coma aquí
                    return Ok(new { message = "Sesión iniciada correctamente.", correoID = usuario.CorreoID });
                }
                else
                {
                    return Unauthorized("Correo o contraseña incorrectos.");
                }
            }
            else
            {
                return BadRequest(ModelState); // Retorna el estado de error del modelo si no es válido
            }
        }


        [HttpPost("EnviarCorreo")]
        public IActionResult EnviarCorreo([FromBody] CorreoDto correoInfo)
        {
            try
            {
                var mensaje = new MailMessage();
                mensaje.To.Add(new MailAddress(correoInfo.Destinatario));
                mensaje.Subject = correoInfo.Asunto;
                mensaje.Body = $"{correoInfo.Mensaje} Monto: {correoInfo.Monto}, Número de Cuenta: {correoInfo.NumeroCuenta}";
                mensaje.IsBodyHtml = true;
                mensaje.From = new MailAddress("starbankoff@gmail.com");

                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("starbankoff@gmail.com", "starbank123");
                    smtp.EnableSsl = true;
                    smtp.Send(mensaje);
                }

                return Ok("Correo enviado con éxito.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al enviar el correo: " + ex.Message);
            }
        }


        public class CorreoDto
        {
            public string Destinatario { get; set; }
            public string Asunto { get; set; }
            public string Mensaje { get; set; }
            public decimal Monto { get; set; } 
            public string NumeroCuenta { get; set; } 
            public string TipoTransaccion { get; set; }
        }

        // LIST


        [ApiController]
        [Route("[controller]")]
        public class MovementsController : ControllerBase
        {
            private static readonly List<Movement> Movements = new List<Movement>();

            [HttpGet]
            public IActionResult GetMovements()
            {
                return Ok(Movements);
            }

            [HttpPost]
            public IActionResult AddMovement([FromBody] Movement movement)
            {
                // Aquí deberías agregar validaciones según las reglas de negocio
                Movements.Add(movement);
                return Ok(new { message = "Movement added successfully", movement });
            }
        }

        public class Movement
        {
            public string AccountNumber { get; set; }
            public decimal Amount { get; set; }
            public DateTime Date { get; set; }
        }




        [HttpPost("Transacciones")]
        public async Task<IActionResult> PostTransaccion([FromBody] HistorialTransacciones transaccion)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _dbcontext.HistorialTransacciones.Add(transaccion);

            try
            {
                await _dbcontext.SaveChangesAsync();
                return CreatedAtAction("GetTransaccion", new { id = transaccion.T_ID }, transaccion);
            }
            catch (DbUpdateException ex)
            {
                // Captura y maneja errores específicos de la base de datos
                return StatusCode(500, "A database error occurred: " + ex.Message);
            }
        }



        // Método GET para recuperar una transacción por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<HistorialTransacciones>> GetTransaccion(int id)
        {
            var transaccion = await _dbcontext.HistorialTransacciones.FindAsync(id);
            if (transaccion == null)
            {
                return NotFound();
            }
            return transaccion;
        }

        [HttpGet("PorCorreo/{correoID}")]
        public async Task<ActionResult<List<HistorialTransacciones>>> GetTransaccionesPorCorreo(int correoID)
        {
            var transacciones = await _dbcontext.HistorialTransacciones
                                                .Where(t => t.CorreoID == correoID)
                                                .ToListAsync();

            if (!transacciones.Any())
            {
                return NotFound(new { message = "No se encontraron transacciones para el CorreoID proporcionado." });
            }
            return transacciones;
        }


        // TODO LO DE DEBITO

        [HttpGet]
        [Route("Lista")]

        public IActionResult Lista() {
            
            List<TarjetasDebito> lista = new List<TarjetasDebito>();

            try
            {
                lista = _dbcontext.TarjetasDebitos.Include(a =>a.Cliente ).ToList();

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", response = lista });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status200OK, new { mensaje = ex.Message, response = lista });
            }
        }


            [HttpPost("iniciarsesiondebito")]
            public IActionResult IniciarSesionDebito([FromBody] DatosDebito datos)
            {
                var tarjeta = _dbcontext.TarjetasDebitos
                    .FirstOrDefault(t => t.TarjetaDebito == datos.NumeroTarjeta && t.NiptarjetaDebito == datos.Nip);

                if (tarjeta != null)
                {
                    // Realiza aquí cualquier otra lógica necesaria después del inicio de sesión exitoso.
                    return Ok("Sesión iniciada con éxito.");
                }
                else
                {
                    return Unauthorized("Número de tarjeta o NIP incorrectos.");
                }
            }
        

        public class DatosDebito
        {
            public string NumeroTarjeta { get; set; }
            public string Nip { get; set; }
        }


        //deposito con saldo propio

        [HttpPost("transferir")]
        public IActionResult TransferirSaldo([FromBody] TransferenciaDatos datos)
        {
            if (datos.Monto < 0 || datos.Monto > 9000 )
            {
                return BadRequest("El monto debe ser  no mayor a 9000.");
            }

            // Buscar las tarjetas de débito origen y destino incluyendo los datos del cliente
            var tarjetaOrigen = _dbcontext.TarjetasDebitos
                .Include(t => t.Cliente)
                .FirstOrDefault(t => t.TarjetaDebito == datos.TarjetaOrigen);

            var tarjetaDestino = _dbcontext.TarjetasDebitos
                .Include(t => t.Cliente)
                .FirstOrDefault(t => t.TarjetaDebito == datos.TarjetaDestino);

            if (tarjetaOrigen == null || tarjetaDestino == null)
            {
                return NotFound("Una o ambas tarjetas no fueron encontradas.");
            }

            if (tarjetaOrigen.Saldo < datos.Monto)
            {
                return BadRequest("Saldo insuficiente en la tarjeta origen.");
            }

            tarjetaOrigen.Saldo -= datos.Monto;
            tarjetaDestino.Saldo += datos.Monto;

            // Guardar cambios en la base de datos
            _dbcontext.SaveChanges();

            return Ok(new
            {
                Mensaje = "Transferencia realizada con éxito.",
                NombreOrigen = tarjetaOrigen.Cliente?.Nombre ?? "Nombre no disponible",
                ApellidosOrigen = tarjetaOrigen.Cliente?.Apellidos ?? "Apellidos no disponibles",
                SaldoDestino = tarjetaDestino.Saldo,
                NombreDestino = tarjetaDestino.Cliente?.Nombre ?? "Nombre no disponible",
                ApellidosDestino = tarjetaDestino.Cliente?.Apellidos ?? "Apellidos no disponibles"
            });
        }



        public class TransferenciaDatos
        {
            public string TarjetaOrigen { get; set; }
            public string TarjetaDestino { get; set; }
            public decimal Monto { get; set; }
        }







        //deposito a la tarjeta de debito

        [HttpPost]
        [Route("Depositar")]
        public IActionResult DepositarATarjetaDebito([FromBody] DepositoRequest request)
        {
            try
            {
                // Buscar la tarjeta de débito por número de tarjeta, incluyendo los datos del cliente
                var tarjeta = _dbcontext.TarjetasDebitos
                    .Include(t => t.Cliente)  // Asegúrate de tener using Microsoft.EntityFrameworkCore;
                    .FirstOrDefault(t => t.TarjetaDebito == request.NumeroTarjeta);

                if (tarjeta == null)
                {
                    return NotFound("No se encontró la tarjeta de débito");
                }

                // Validar que la cantidad sea un múltiplo de 50 y esté en el rango de 20 a 9000
                if (request.Cantidad < 20 || request.Cantidad > 9000 || request.Cantidad % 50 != 0)
                {
                    return BadRequest("La cantidad de depósito debe ser un múltiplo de 50 y estar en el rango de 20 a 9000");
                }

                // Realizar el depósito
                tarjeta.Saldo += request.Cantidad;
                _dbcontext.SaveChanges();

                // Preparar los datos adicionales para la respuesta
                var respuesta = new
                {
                    mensaje = "Depósito realizado exitosamente",
                    saldoActual = tarjeta.Saldo,
                    nombreCliente = tarjeta.Cliente?.Nombre ?? "Nombre no disponible",
                    apellidoCliente = tarjeta.Cliente?.Apellidos ?? "Apellido no disponible",
                    numeroCuenta = tarjeta.NumeroCuentaDebito,
                    tipoTransaccion = "Depósito"
                };

                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }

        public class DepositoRequest
        {
            public string NumeroTarjeta { get; set; }
            public decimal Cantidad { get; set; }
        }


        // retiro de tarjeta de debito 

        [HttpPost]
        [Route("Retirar")]
        public IActionResult RetirarDeTarjetaDebito([FromBody] RetiroRequest request)
        {
            try
            {
                // Buscar la tarjeta de débito por número de tarjeta
                var tarjeta = _dbcontext.TarjetasDebitos.FirstOrDefault(t => t.TarjetaDebito == request.NumeroTarjeta);

                if (tarjeta == null)
                {
                    return NotFound("No se encontró la tarjeta de débito");
                }


                // Validar que la cantidad sea un múltiplo de 50 y esté en el rango de 20 a 9000
                if (request.Cantidad < 20 || request.Cantidad > 9000 || request.Cantidad % 50 != 0)
                {
                    return BadRequest("La cantidad de retiro debe ser un múltiplo de 50 y estar en el rango de 20 a 9000");
                }

                // Validar que haya suficiente saldo para el retiro
                if (request.Cantidad > tarjeta.Saldo)
                {
                    return BadRequest("Saldo insuficiente para realizar el retiro");
                }

                // Realizar el retiro
                tarjeta.Saldo -= request.Cantidad;
                _dbcontext.SaveChanges();

                return Ok(new { mensaje = "Retiro realizado exitosamente", saldoActual = tarjeta.Saldo,
                    nombreCliente = tarjeta.Cliente?.Nombre ?? "Nombre no disponible",
                    apellidoCliente = tarjeta.Cliente?.Apellidos ?? "Apellido no disponible",
                    numeroCuenta = tarjeta.NumeroCuentaDebito,
                    tipoTransaccion = "Retiro"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }


        public class RetiroRequest
        {
            public string NumeroTarjeta { get; set; }
            public decimal Cantidad { get; set; }
        }



        //editar NIP

        [HttpPost]
        [Route("EditarNIP")]
        public IActionResult EditarNIP([FromBody] EditarNIPRequest request)
        {
            try
            {
                // Buscar la tarjeta de débito por número de tarjeta
                var tarjeta = _dbcontext.TarjetasDebitos.FirstOrDefault(t => t.TarjetaDebito == request.NumeroTarjeta);

                if (tarjeta == null)
                {
                    return NotFound("No se encontró la tarjeta de débito");
                }

                // Validar que el NIP anterior sea correcto
                if (tarjeta.NiptarjetaDebito != request.NIPAnterior)
                {
                    return BadRequest("NIP anterior incorrecto");
                }

                // Validar que el nuevo NIP tenga 4 dígitos
                if (request.NIPNuevo.Length != 4)
                {
                    return BadRequest("El nuevo NIP debe tener exactamente 4 dígitos");
                }

                // Validar que el nuevo NIP no tenga más de 3 veces el mismo número
                var digitCounts = new Dictionary<char, int>();
                foreach (char digit in request.NIPNuevo)
                {
                    if (!char.IsDigit(digit))
                    {
                        return BadRequest("El nuevo NIP solo puede contener dígitos");
                    }

                    if (!digitCounts.ContainsKey(digit))
                    {
                        digitCounts[digit] = 1;
                    }
                    else
                    {
                        digitCounts[digit]++;
                        if (digitCounts[digit] > 3)
                        {
                            return BadRequest("El nuevo NIP no puede tener más de 3 veces el mismo número");
                        }
                    }
                }

                // Validar que el nuevo NIP no sea una secuencia consecutiva
                if (EsConsecutivo(request.NIPNuevo))
                {
                    return BadRequest("El nuevo NIP no puede ser una secuencia consecutiva");
                }

                // Actualizar el NIP
                tarjeta.NiptarjetaDebito = request.NIPNuevo;
                _dbcontext.SaveChanges();

                return Ok("NIP actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }

        // Método para verificar si una cadena de dígitos es una secuencia consecutiva
        private bool EsConsecutivo(string digits)
        {
            for (int i = 0; i < digits.Length - 1; i++)
            {
                if (digits[i] + 1 == digits[i + 1])
                {
                    return true;
                }
            }
            return false;
        }


        public class EditarNIPRequest
        {
            public string NumeroTarjeta { get; set; }
            public string NIPAnterior { get; set; }
            public string NIPNuevo { get; set; }
        }




        //TODO LO DE CREDITO



        [HttpPost("IniciarSesionCredito")]
        public IActionResult IniciarSesionCredito(ConsultaDeudaRequest request)
        {
            // Busca la información de la tarjeta de crédito basada en el número de tarjeta y NIP proporcionados.
            // Asegúrate de incluir las relaciones necesarias usando .Include()
            var tarjeta = _dbcontext.TarjetasCreditos
                            .Include(t => t.oCliente) // Asegúrate de cargar el cliente relacionado
                            .FirstOrDefault(t => t.TarjetaCredito == request.NumeroTarjeta && t.NipTarjetaCredito == request.Nip);

            if (tarjeta != null && tarjeta.oCliente != null)
            {
                // Si se encuentra la tarjeta y el cliente asociado no es nulo, devuelve el mensaje con la deuda total y otros detalles.
                var respuesta = new
                {
                    mensaje = "Sesión iniciada correctamente.",
                    nombreCliente = tarjeta.oCliente.Nombre + " " + tarjeta.oCliente.Apellidos,
                    deuda = tarjeta.Deuda ?? 0, // Por defecto a 0 si la deuda es null.
                };

                return Ok(respuesta);
            }
            else
            {
                // Si los detalles de la tarjeta son incorrectos, no existen o el cliente asociado es nulo, devuelve un mensaje de error.
                return NotFound("Tarjeta no encontrada, NIP incorrecto o cliente no definido.");
            }
        }


        public class ConsultaDeudaRequest
        {
            public string NumeroTarjeta { get; set; }
            public string Nip { get; set; }
        }





        [HttpPost]
        [Route("DepositarParaDeuda")]
        public IActionResult DepositarParaDeuda([FromBody] DepositoRequestCredito request)
        {
            try
            {
                // Buscar la tarjeta de crédito por número de tarjeta
                var tarjeta = _dbcontext.TarjetasCreditos.FirstOrDefault(t => t.TarjetaCredito == request.TarjetaCredito);

                if (tarjeta == null)
                {
                    return NotFound("No se encontró la tarjeta de crédito");
                }

                // Validar que la cantidad sea un múltiplo de 50 y esté en el rango de 50 a 9000 y sea multiplo de 50
                if (request.CantidadCredito < 50 || request.CantidadCredito > 9000 || request.CantidadCredito % 50 != 0)
                {
                    return BadRequest("La cantidad de depósito debe ser un múltiplo de 50 y estar en el rango de 50 a 9000");
                }

                // Validar que la cantidad de depósito no supere el monto de la deuda
                if (request.CantidadCredito > tarjeta.Deuda)
                {
                    return BadRequest("La cantidad de depósito no puede ser mayor que el monto de la deuda");
                }

                // Realizar el depósito para disminuir la deuda
                tarjeta.Deuda -= request.CantidadCredito;
                _dbcontext.SaveChanges();

                return Ok(new { mensaje = "Depósito realizado exitosamente para disminuir la deuda", deudaActual = tarjeta.Deuda });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }




        public class DepositoRequestCredito
        {
            public string TarjetaCredito { get; set; }
            public decimal CantidadCredito { get; set; }
        }







        [HttpPost]
        [Route("PagarDeuda")]
        public IActionResult PagarDeuda([FromBody] PagoDeudaRequest request)
        {
            if (request.MontoPago <= 0)
            {
                return BadRequest("El monto del pago debe ser mayor que cero.");
            }

            try
            {
                var tarjeta = _dbcontext.TarjetasCreditos.FirstOrDefault(t => t.TarjetaCredito == request.TarjetaCredito);
                if (tarjeta == null)
                {
                    return NotFound("No se encontró la tarjeta de crédito.");
                }

                if (request.MontoPago != tarjeta.Deuda)
                {
                    return BadRequest("El monto del pago debe ser exactamente igual al monto de la deuda.");
                }

                tarjeta.Deuda = 0; // Salda la deuda
                _dbcontext.SaveChanges();

                return Ok(new { mensaje = "Pago realizado exitosamente para saldar la deuda." });
            }
            catch (Exception ex)
            {
                // Considera capturar tipos específicos de excepciones si es posible
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }

 
        public class PagoDeudaRequest
        {
            public string TarjetaCredito { get; set; }
            public decimal MontoPago { get; set; }
        }



        [HttpPost]
        [Route("EditarNIPCredito")]
        public IActionResult EditarNIPCredito([FromBody] EditarNIPRequest request)
        {
            try
            {
                // Buscar la tarjeta de crédito por número de tarjeta
                var tarjeta = _dbcontext.TarjetasCreditos.FirstOrDefault(t => t.TarjetaCredito == request.NumeroTarjeta);

                if (tarjeta == null)
                {
                    return NotFound("No se encontró la tarjeta de crédito");
                }

                // Validar que el NIP anterior sea correcto
                if (tarjeta.NipTarjetaCredito != request.NIPAnterior)
                {
                    return BadRequest("NIP anterior incorrecto");
                }

                // Validar que el nuevo NIP tenga 4 dígitos
                if (request.NIPNuevo.Length != 4)
                {
                    return BadRequest("El nuevo NIP debe tener exactamente 4 dígitos");
                }

                // Validar que el nuevo NIP no tenga más de 3 veces el mismo número
                var digitCounts = new Dictionary<char, int>();
                foreach (char digit in request.NIPNuevo)
                {
                    if (!char.IsDigit(digit))
                    {
                        return BadRequest("El nuevo NIP solo puede contener dígitos");
                    }

                    if (!digitCounts.ContainsKey(digit))
                    {
                        digitCounts[digit] = 1;
                    }
                    else
                    {
                        digitCounts[digit]++;
                        if (digitCounts[digit] > 3)
                        {
                            return BadRequest("El nuevo NIP no puede tener más de 3 veces el mismo número");
                        }
                    }
                }

                // Validar que el nuevo NIP no sea una secuencia consecutiva
                if (EsConsecutivoC(request.NIPNuevo))
                {
                    return BadRequest("El nuevo NIP no puede ser una secuencia consecutiva");
                }

                // Actualizar el NIP
                tarjeta.NipTarjetaCredito = request.NIPNuevo;
                _dbcontext.SaveChanges();

                return Ok("NIP de tarjeta de crédito actualizado exitosamente");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }














        // Reutiliza el mismo método EsConsecutivo definido previamente
        private bool EsConsecutivoC(string digits)
        {
            for (int i = 0; i < digits.Length - 1; i++)
            {
       
                if ((int)char.GetNumericValue(digits[i]) + 1 == (int)char.GetNumericValue(digits[i + 1]))
                {
                    return true;
                }
            }
            return false;
        }


        //HIPOTECA

        

        // deposito entero 

        [HttpPost]
        [Route("DepositarParaDeudaHipoteca")]
        public IActionResult DepositarParaDeudaHipoteca([FromBody] DepositoRequestHipoteca request)
        {
            try
            {
                // Buscar la hipoteca por número de cuenta
                var hipoteca = _dbcontext.Hipotecas.FirstOrDefault(h => h.NumeroCuenta == request.NumeroCuenta);

                if (hipoteca == null)
                {
                    return NotFound("No se encontró la cuenta ");
                }

                // Realizar el depósito para disminuir la deuda
                hipoteca.DeudaHipoteca -= request.CantidadDeposito;
                _dbcontext.SaveChanges();

                return Ok(new { mensaje = "Depósito realizado exitosamente para disminuir la deuda de la hipoteca", deudaActual = hipoteca.DeudaHipoteca });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }

        public class DepositoRequestHipoteca
        {
            public string NumeroCuenta { get; set; }
            public decimal CantidadDeposito { get; set; }
        }

        // mostrar mensualidad 


        [HttpGet]
        [Route("ObtenerDeudaHipoteca")]
        public IActionResult ObtenerDeudaHipoteca(string NumeroCuenta)
        {
            try
            {
                var hipoteca = _dbcontext.Hipotecas
                    .Include(h => h.oCliente)
                    .FirstOrDefault(h => h.NumeroCuenta == NumeroCuenta.ToString());

                if (hipoteca == null)
                {
                    return NotFound("Hipoteca no encontrada, por favor intente de nuevo");
                }

                var respuesta = new
                {
                    mensaje = "Información de la deuda",
                    nombreCliente = hipoteca.oCliente.Nombre + " " + hipoteca.oCliente.Apellidos,
                    deuda = hipoteca.DeudaHipoteca,
                    mensualidad = hipoteca.Mensualidad,
                    mesesDelAdeudo = hipoteca.MesesDelAdeudo
                };

                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }



        // depositar a la mensualidad 

        [HttpPost]
        [Route("DepositarMensualidadDeudaHipoteca")]
        public IActionResult DepositarMensualidadDeudaHipoteca([FromBody] DepositoMensualidadRequest request)
        {
            try
            {
                // Buscar la hipoteca por número de cuenta
                var hipoteca = _dbcontext.Hipotecas.FirstOrDefault(h => h.NumeroCuenta == request.NumeroCuenta);

                if (hipoteca == null)
                {
                    return NotFound("No se encontró el numero de cuenta");
                }

                // Verificar que el depósito sea exactamente igual a la mensualidad
                if (request.CantidadDeposito != hipoteca.Mensualidad)
                {
                    return BadRequest($"La cantidad de depósito debe ser exactamente igual a la mensualidad de la deuda, que es {hipoteca.Mensualidad}");
                }

                // Reducir el número de meses de deuda
                hipoteca.MesesDelAdeudo--;

                // Guardar los cambios en la base de datos
                _dbcontext.SaveChanges();

                return Ok(new { mensaje = "Depósito de la mensualidad de la deuda realizado exitosamente", mesesDeudaActualizados = hipoteca.MesesDelAdeudo });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }

        public class DepositoMensualidadRequest
        {
            public string NumeroCuenta { get; set; }
            public decimal CantidadDeposito { get; set; }
        }



        //ESTUDIANTIL



        [HttpGet("DatosCE")]
        public IActionResult GetCreditoEducativoByNumeroCuenta(string numeroCuenta)
        {
            var credito = _dbcontext.CreditoEducativos
                .Include(c => c.oCliente)  // Incluir los datos del cliente asociado
                .Where(c => c.NumeroCuenta == numeroCuenta)
                .Select(c => new {
                    NumeroCuenta = c.NumeroCuenta,
                    Tipo = c.Tipo,
                    Plazo = c.Plazo,
                    InicioPago = c.InicioPago,
                    TotalDeuda = c.TotalDeuda,
                    IngresoMensual = c.IngresoMensual,
                    Mensualidad = c.Mensualidad,
                    NombreCliente = c.oCliente != null ? c.oCliente.Nombre : "Desconocido",
                    ApellidosCliente = c.oCliente != null ? c.oCliente.Apellidos : "Desconocido"
                })
                .FirstOrDefault();

            if (credito == null)
            {
                return NotFound(new { Message = "Crédito educativo no encontrado para el número de cuenta proporcionado." });
            }

            return Ok(credito);
        }





        [HttpPost]
        [Route("DepositarMensualidadDeudaCreditoEducativo")]
        public IActionResult DepositarMensualidadDeudaCreditoEducativo([FromBody] DepositoMensualidadRequestE request)
        {
            try
            {
                // Buscar el crédito educativo por número de cuenta
                var credito = _dbcontext.CreditoEducativos.FirstOrDefault(c => c.NumeroCuenta == request.NumeroCuenta);

                if (credito == null)
                {
                    return NotFound("No se encontró el crédito educativo");
                }

                // Verificar que el depósito sea exactamente igual a la mensualidad
                if (request.CantidadDeposito != credito.Mensualidad)
                {
                    return BadRequest($"La cantidad de depósito debe ser exactamente igual a la mensualidad de la deuda, que es {credito.Mensualidad}");
                }

                // Restar la cantidad depositada de la deuda
                credito.TotalDeuda = (credito.TotalDeuda ?? 0) - request.CantidadDeposito;

                // Reducir el plazo en uno
                credito.Plazo--;

                // Guardar los cambios en la base de datos
                _dbcontext.SaveChanges();

                return Ok(new { mensaje = "Depósito de la mensualidad de la deuda realizado exitosamente", plazoActualizado = credito.Plazo });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }

        public class DepositoMensualidadRequestE
        {
            public string NumeroCuenta { get; set; }
            public decimal CantidadDeposito { get; set; }
        }







        //CARRO

        // abono total

        [HttpPost]
        [Route("DepositarParaDeudaAutomovil")]
        public IActionResult DepositarParaDeudaAutomovil([FromBody] DepositoRequestAutomovil request)
        {
            try
            {
                // Validar que la cantidad sea un múltiplo de 50 y esté dentro del rango permitido
                if (request.CantidadDeposito < 50 || request.CantidadDeposito > 9000 || request.CantidadDeposito % 50 != 0)
                {
                    return BadRequest("La cantidad de depósito debe ser un múltiplo de 50 y estar entre 50 y 9000.");
                }

                // Buscar el automóvil por número de cuenta
                var automovil = _dbcontext.Automóvils.FirstOrDefault(a => a.NumeroCuenta == request.NumeroCuenta);

                if (automovil == null)
                {
                    return NotFound("No se encontró el automóvil");
                }

                // Realizar el depósito para disminuir la deuda
                automovil.DeudaAuto -= request.CantidadDeposito;
                _dbcontext.SaveChanges();

                return Ok(new { mensaje = "Depósito realizado exitosamente para disminuir la deuda del automóvil", deudaActual = automovil.DeudaAuto });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }

        public class DepositoRequestAutomovil
        {
            public string NumeroCuenta { get; set; }
            public decimal CantidadDeposito { get; set; }
        }


        // mensualidad 

        [HttpGet]
        [Route("ObtenerMensualidadDeudaAutomovil")]
        public IActionResult ObtenerMensualidadDeudaAutomovil(string NumeroCuenta)
        {
            try
            {
                // Incluir la información del cliente al buscar el automóvil por número de cuenta
                var automovil = _dbcontext.Automóvils
                    .Include(a => a.oCliente) // Incluir la información del cliente asociado
                    .FirstOrDefault(a => a.NumeroCuenta == NumeroCuenta);

                if (automovil == null)
                {
                    return NotFound("No se encontró el automóvil");
                }

                // Devolver la mensualidad de la deuda del automóvil junto con el nombre y apellido del cliente
                return Ok(new
                {
                    mensaje = "Mensualidad de la deuda obtenida exitosamente",
                    mensualidadDeuda = automovil.Mensualidad,
                    deudaActual = automovil.DeudaAuto,
                    MesesDelAdeudo = automovil.MesesDelAdeudo,
                    nombreCliente = automovil.oCliente?.Nombre,
                    apellidoCliente = automovil.oCliente?.Apellidos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }


        // pago al mes 

        [HttpPost]
        [Route("DepositarMensualidadDeudaAutomovil")]
        public IActionResult DepositarMensualidadDeudaAutomovil([FromBody] DepositoMensualidadAutoRequest request)
        {
            try
            {
                // Buscar el automóvil por número de cuenta
                var automovil = _dbcontext.Automóvils.FirstOrDefault(a => a.NumeroCuenta == request.NumeroCuentaM);

                if (automovil == null)
                {
                    return NotFound("No se encontró el automóvil");
                }

                // Verificar que el depósito sea exactamente igual a la mensualidad
                if (request.CantidadDepositoM != automovil.Mensualidad)
                {
                    return BadRequest($"La cantidad de depósito debe ser exactamente igual a la mensualidad de la deuda, que es {automovil.Mensualidad}");
                }

                // Reducir el número de meses de deuda
                automovil.MesesDelAdeudo--;

                // Guardar los cambios en la base de datos
                _dbcontext.SaveChanges();

                return Ok(new { mensaje = "Depósito de la mensualidad de la deuda realizado exitosamente", mesesDeudaActualizados = automovil.MesesDelAdeudo });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }

        public class DepositoMensualidadAutoRequest
        {
            public string NumeroCuentaM { get; set; }
            public decimal CantidadDepositoM { get; set; }
        }



        //SERVICIOS

        [HttpPost("Login")]
        public ActionResult Login(LoginRequest request)
        {
            // Busca en la tabla 'servicios' si algún campo coincide con el número de cuenta proporcionado
            var cuenta = _dbcontext.Servicios
                         .FirstOrDefault(s => s.CFE == request.NumeroCuenta ||
                                              s.JAPAY == request.NumeroCuenta ||
                                              s.Telmex == request.NumeroCuenta ||
                                              s.Totalplay == request.NumeroCuenta ||
                                              s.Izzi == request.NumeroCuenta);

            if (cuenta != null)
            {
                // La cuenta existe, obtén la mensualidad correspondiente
                decimal mensualidad = 0;
                if (cuenta.CFE == request.NumeroCuenta)
                {
                    mensualidad = cuenta.CFE_Mensualidad;
                }
                else if (cuenta.JAPAY == request.NumeroCuenta)
                {
                    mensualidad = cuenta.JAPAY_Mensualidad;
                }
                else if (cuenta.Telmex == request.NumeroCuenta)
                {
                    mensualidad = cuenta.Telmex_Mensualidad;
                }
                else if (cuenta.Totalplay == request.NumeroCuenta)
                {
                    mensualidad = cuenta.Totalplay_Mensualidad;
                }
                else if (cuenta.Izzi == request.NumeroCuenta)
                {
                    mensualidad = cuenta.Izzi_Mensualidad;
                }

                // Devuelve la mensualidad
                return Ok(new { message = "Inicio de sesión exitoso", mensualidad });
            }
            else
            {
                // No se encontró la cuenta
                return NotFound(new { message = "Número de cuenta no encontrado" });
            }
        }



        public class LoginRequest
        {
            public string NumeroCuenta { get; set; }
        }


        [HttpPost("PagarMensualidad")]
        public ActionResult PagarMensualidad(PagoMensualidadRequest request)
        {
            var servicio = _dbcontext.Servicios.FirstOrDefault(s => s.CFE == request.NumeroCuenta ||
                s.JAPAY == request.NumeroCuenta || s.Telmex == request.NumeroCuenta ||
                s.Totalplay == request.NumeroCuenta || s.Izzi == request.NumeroCuenta);

            if (servicio == null)
            {
                return NotFound(new { message = "Número de cuenta no encontrado." });
            }

            decimal mensualidadRequerida = servicio.CFE == request.NumeroCuenta ? servicio.CFE_Mensualidad :
                servicio.JAPAY == request.NumeroCuenta ? servicio.JAPAY_Mensualidad :
                servicio.Telmex == request.NumeroCuenta ? servicio.Telmex_Mensualidad :
                servicio.Totalplay == request.NumeroCuenta ? servicio.Totalplay_Mensualidad :
                servicio.Izzi_Mensualidad;  // Asegúrate de que esto está correctamente asignado

            if (request.CantidadDeposito != mensualidadRequerida)
            {
                return BadRequest(new { message = $"El depósito debe ser exactamente igual a la mensualidad de ${mensualidadRequerida:0.00}" });
            }

            // Procesar el pago
            // Actualizar el balance de la cuenta, registrar la transacción, etc.

            return Ok(new { message = "Pago realizado con éxito." });
        }

        public class PagoMensualidadRequest
        {
            public string NumeroCuenta { get; set; }
            public decimal CantidadDeposito { get; set; }
        }



    }


}
