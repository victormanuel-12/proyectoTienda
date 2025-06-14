using proyectoTienda.Models;
using System.Collections.Generic;

namespace proyectoTienda.Servicios
{
  public interface IExportService
  {
    byte[] GenerateAllPedidosPdf(List<Pedido> pedidos);
    byte[] GenerateAllPedidosExcel(List<Pedido> pedidos);
  }
}