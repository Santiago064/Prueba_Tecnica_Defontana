using Microsoft.EntityFrameworkCore;
using Prueba_defontana2.Models;
using System.Linq;


    using (var context = new PruebaContext())
    {
    // 1. Total de ventas de los últimos 30 días (monto total y cantidad total de ventas)

        var fechaLimite = DateTime.Now.AddDays(-30);
            var ventasUltimos30Dias = context.Venta
                .Where(v => v.Fecha >= fechaLimite)
                .ToList();

    //Traemos los datos base, 30 días
    Console.WriteLine("Datos base, 30 dias: ");
    foreach (var venta in ventasUltimos30Dias)
        {
            Console.WriteLine($"ID: {venta.IdVenta}, Total: {venta.Total}, Fecha: {venta.Fecha}");
        }

        var totalMontoVentas = ventasUltimos30Dias.Sum(v => v.Total);
        var cantidadTotalVentas = ventasUltimos30Dias.Count;

        Console.WriteLine($"Total de ventas en los últimos 30 dias: {totalMontoVentas}");
        Console.WriteLine($"Cantidad total de ventas en los últimos 30 dias: {cantidadTotalVentas}");

    // 2. Día y hora en que se realizó la venta con el monto más alto
    var ventaMasAlta = (from v in context.Venta
                        where v.Fecha >= DateTime.Today.AddDays(-30)
                        orderby v.Total descending
                        select v).FirstOrDefault();

    Console.WriteLine($"Venta más alta: Fecha: {ventaMasAlta?.Fecha}, Monto: {ventaMasAlta?.Total}");



    // 3. Producto con mayor monto total de ventas
    var productoMayorMontoVentas = (from vd in context.VentaDetalles
                                    join p in context.Productos on vd.IdProducto equals p.IdProducto
                                    group vd by p.Nombre into g
                                    select new
                                    {
                                        NombreProducto = g.Key,
                                        MontoTotalVentas = g.Sum(v => v.TotalLinea)
                                    })
                                .OrderByDescending(x => x.MontoTotalVentas)
                                .FirstOrDefault();

    Console.WriteLine($"Producto con mayor monto total de ventas: {productoMayorMontoVentas?.NombreProducto}, Monto: {productoMayorMontoVentas?.MontoTotalVentas}");



    // 4. Local con mayor monto de ventas
    var localMayorMontoVentas = (from v in context.Venta
                                  where v.Fecha >= DateTime.Today.AddDays(-30)
                                  group v by v.IdLocal into g
                                  select new
                                  {
                                      ID_Local = g.Key,
                                      TotalVentas = g.Sum(v => v.Total)
                                  })
                            .OrderByDescending(x => x.TotalVentas)
                            .FirstOrDefault();

    var local = (from l in context.Locals
                 where l.IdLocal == localMayorMontoVentas.ID_Local
                 select l).FirstOrDefault();

    Console.WriteLine($"Local con mayor monto de ventas: {local?.Nombre}, Monto: {localMayorMontoVentas?.TotalVentas}");
    ;


    // 5. Marca con mayor margen de ganancias
    var marcaMayorMargenGanancias = (from vd in context.VentaDetalles
                                     join p in context.Productos on vd.IdProducto equals p.IdProducto
                                     join m in context.Marcas on p.IdMarca equals m.IdMarca
                                     select new
                                     {
                                         vd,
                                         m,
                                         p
                                     } into t1
                                     group t1 by t1.m.Nombre into pg
                                     select new
                                     {
                                         marca = pg.FirstOrDefault().m.Nombre,
                                         max = pg.Sum(q => q.vd.TotalLinea - q.p.CostoUnitario * q.vd.Cantidad)
                                     }).ToList();

    var marcaConMaxGanancias = marcaMayorMargenGanancias.OrderByDescending(p => p.max).FirstOrDefault();
    Console.WriteLine($"Marca con mayor margen de ganancias: {marcaConMaxGanancias.marca}, Máximo: {marcaConMaxGanancias.max}");



    // 6. Producto que más se vende en cada local
    var fechaInicio = DateTime.Today.AddDays(-30);

    var productosMasVendidosPorLocal = (from vd in context.VentaDetalles
                                        join v in context.Venta on vd.IdVenta equals v.IdVenta
                                        join l in context.Locals on v.IdLocal equals l.IdLocal
                                        join p in context.Productos on vd.IdProducto equals p.IdProducto
                                        where v.Fecha >= fechaInicio
                                        group new { l.Nombre, p.IdProducto, vd.Cantidad } by new { l.Nombre, p.IdProducto } into g
                                        select new
                                        {
                                            Local = g.Key.Nombre,
                                            ProductoId = g.Key.IdProducto,
                                            TotalVendido = g.Sum(x => x.Cantidad)
                                        })
                                        .GroupBy(x => x.Local)
                                        .Select(g => g.OrderByDescending(p => p.TotalVendido).FirstOrDefault())
                                        .ToList();

    Console.WriteLine("Producto que mas se vende en cada local:");
    foreach (var productoPorLocal in productosMasVendidosPorLocal)
    {
        var productoNombre = context.Productos.FirstOrDefault(p => p.IdProducto == productoPorLocal.ProductoId)?.Nombre;

        Console.WriteLine($"Local: {productoPorLocal.Local}, Producto más vendido: {productoNombre}, Total vendido: {productoPorLocal.TotalVendido}");
    }

    Console.ReadLine();
}
