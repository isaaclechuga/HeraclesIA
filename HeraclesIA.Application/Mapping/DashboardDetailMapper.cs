using HeraclesIA.Types.Dashboards;

namespace HeraclesIA.Application.Mapping;

public sealed class DashboardDetailMapper
{
    public IEnumerable<DashboardDetalle> MapToDetalles(IEnumerable<Dashboard> dashboards)
    {
        var resultadoDetallado = dashboards
            .SelectMany(dashboard => dashboard.DataProviders, (dashboard, dataProvider) => new { dashboard, dataProvider })
            .SelectMany(t => ObtenerBaseDatos(t.dataProvider.Query), (t, pair) => new
            {
                t.dashboard,
                t.dataProvider,
                Servidor = pair.Servidor,
                BaseDatos = pair.BaseDatos
            })
            .SelectMany(t => t.BaseDatos.Tablas, (t, tablaNombre) => new DashboardDetalle
            {
                DashboardId = t.dashboard.DashboardNum,
                DataProviderId = t.dataProvider.DataProviderId,
                Servidor = t.Servidor.Nombre,
                BaseDatos = t.BaseDatos.Nombre,
                Tabla = tablaNombre
            })
            .ToList();

        foreach (var detalle in resultadoDetallado)
        {
            if (string.IsNullOrEmpty(detalle.Servidor) && string.IsNullOrEmpty(detalle.BaseDatos))
            {
                var resultado = resultadoDetallado
                    .Where(x => x.Tabla.Trim() == detalle.Tabla.Trim())
                    .Select(x => new DashboardDetalle { Servidor = x.Servidor, BaseDatos = x.BaseDatos })
                    .FirstOrDefault();

                detalle.Servidor = resultado != null ? resultado.Servidor : "ONBSDB.P.GSLB";
                detalle.BaseDatos = resultado != null ? resultado.BaseDatos : "Nautilus";
            }
        }

        return resultadoDetallado;
    }

    private static IEnumerable<(Servidor Servidor, BaseDatos BaseDatos)> ObtenerBaseDatos(Query? query)
    {
        if (query == null)
            yield break;

        if (query.Servidores != null)
        {
            foreach (var servidor in query.Servidores)
            {
                if (servidor.BaseDatos != null)
                {
                    foreach (var baseDatos in servidor.BaseDatos)
                        yield return (servidor, baseDatos);
                }
            }
        }

        if (query.Children != null)
        {
            foreach (var childQuery in query.Children)
            {
                foreach (var pair in ObtenerBaseDatos(childQuery))
                    yield return pair;
            }
        }
    }
}
