using Comercio.Domain.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comercio.Domain.Interfaces
{
    public interface IClientesRepository
    {
        Task<IEnumerable<Cliente>> ObtenerTodos(bool incluirEliminados = false);
        Task<Cliente?> ObtenerPorId(int id);
        Task<bool> ExisteRepetido(int tipoPersona, string dniCuit, int? idCliente = null);
        Task<int> Crear(Cliente cliente);
        Task Actualizar(Cliente cliente);
        Task EliminarPermanentemente(int id);
        Task<bool> DarDeBaja(int id);
        Task<bool> Restaurar(int id);
    }
}
