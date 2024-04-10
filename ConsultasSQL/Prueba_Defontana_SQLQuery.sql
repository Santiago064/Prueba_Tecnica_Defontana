--CONSULTAS SQL 

--Punto 1
SELECT SUM(Total) AS TotalMontoVentas, COUNT(*) AS CantidadTotalVentas
FROM Venta
WHERE Fecha >= DATEADD(DAY, -30, GETDATE())

--Punto 2
SELECT TOP 1 Fecha, Total
FROM Venta
WHERE Fecha >= DATEADD(DAY, -30, GETDATE())
ORDER BY Total DESC	

--//Punto 3
SELECT TOP 1 P.Nombre AS Producto, SUM(VD.TotalLinea) AS MontoTotalVentas
FROM VentaDetalle VD
JOIN Producto P ON VD.ID_Producto = P.ID_Producto
GROUP BY P.Nombre
ORDER BY MontoTotalVentas DESC

--//Punto 4
SELECT TOP 1 L.Nombre AS Local, SUM(V.Total) AS MontoTotalVentas
FROM Venta V
JOIN Local L ON V.ID_Local = L.ID_Local
WHERE V.Fecha >= DATEADD(DAY, -30, GETDATE())
GROUP BY L.Nombre
ORDER BY MontoTotalVentas DESC

--//Punto 5
SELECT TOP 1 M.Nombre AS Marca, SUM(VD.TotalLinea - P.Costo_Unitario * VD.Cantidad) AS MargenGanancias
FROM VentaDetalle VD
JOIN Producto P ON VD.ID_Producto = P.ID_Producto
JOIN Marca M ON P.ID_Marca = M.ID_Marca
GROUP BY M.Nombre
ORDER BY MargenGanancias DESC

--PUNTO 6
SELECT 
    Local,
    Producto,
    TotalVendido
FROM (
    SELECT 
        l.Nombre AS Local,
        p.Nombre AS Producto,
        SUM(vd.Cantidad) AS TotalVendido,
        ROW_NUMBER() OVER (PARTITION BY l.ID_Local ORDER BY SUM(vd.Cantidad) DESC) AS clasificación
    FROM 
        VentaDetalle vd
    INNER JOIN 
        Venta v ON vd.ID_Venta = v.ID_Venta
    INNER JOIN 
        Local l ON v.ID_Local = l.ID_Local
    INNER JOIN 
        Producto p ON vd.ID_Producto = p.ID_Producto
    WHERE 
        v.Fecha >= DATEADD(DAY, -30, GETDATE())
    GROUP BY 
        l.ID_Local, l.Nombre, p.Nombre
) AS Subquery
WHERE 
    clasificación = 1;
