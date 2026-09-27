# Summary
<details open><summary>Summary</summary>

|||
|:---|:---|
| Generated on: | 27/9/2026 - 02:53:22 |
| Coverage date: | 27/9/2026 - 02:53:20 |
| Parser: | MultiReport (2x Cobertura) |
| Assemblies: | 2 |
| Classes: | 73 |
| Files: | 73 |
| **Line coverage:** | 84.1% (551 of 655) |
| Covered lines: | 551 |
| Uncovered lines: | 104 |
| Coverable lines: | 655 |
| Total lines: | 1762 |
| **Branch coverage:** | 93.7% (105 of 112) |
| Covered branches: | 105 |
| Total branches: | 112 |
| **Method coverage:** | [Feature is only available for sponsors](https://reportgenerator.io/pro) |

</details>

## Coverage
<details><summary>Logistica.Application - 64%</summary>

|**Name**|**Line**|**Branch**|
|:---|---:|---:|
|**Logistica.Application**|**64%**|**80%**|
|Logistica.Application.IntegrationEvents.ConstanciaDto|100%||
|Logistica.Application.IntegrationEvents.DireccionGeoDto|100%||
|Logistica.Application.IntegrationEvents.EntregaConfirmadaIntegrationEvent|100%||
|Logistica.Application.IntegrationEvents.EtiquetaDto|100%||
|Logistica.Application.IntegrationEvents.IncidenciaEntregaRegistradaIntegrat<br/>ionEvent|100%||
|Logistica.Application.IntegrationEvents.PaqueteListoDto|100%||
|Logistica.Application.IntegrationEvents.PaquetesListosParaEntrega|100%||
|Logistica.Application.IntegrationEvents.ParadaOrdenadaDto|100%||
|Logistica.Application.IntegrationEvents.RutaOptimizadaGeneradaIntegrationEv<br/>ent|100%||
|Logistica.Application.PaquetesRecibidos.PaqueteRecibidoErrors|100%||
|Logistica.Application.PaquetesRecibidos.ProcesarPaquetesListosCommandHandle<br/>r|100%||
|Logistica.Application.PaquetesRecibidos.Queries.PaqueteRecibidoDto|0%||
|Logistica.Application.Repartidores.Queries.RepartidorDto|0%||
|Logistica.Application.Repartidores.RegistrarRepartidor.RegistrarRepartidorC<br/>ommand|0%||
|Logistica.Application.Repartidores.RegistrarRepartidor.RegistrarRepartidorC<br/>ommandHandler|0%||
|Logistica.Application.Rutas.CancelarRuta.CancelarRutaCommandHandler|0%||
|Logistica.Application.Rutas.CompletarRuta.CompletarRutaCommandHandler|0%||
|Logistica.Application.Rutas.ConfirmarEntrega.ConfirmarEntregaCommand|100%||
|Logistica.Application.Rutas.ConfirmarEntrega.ConfirmarEntregaCommandHandler|100%||
|Logistica.Application.Rutas.CrearRuta.CrearRutaCommand|100%||
|Logistica.Application.Rutas.CrearRuta.CrearRutaCommandHandler|100%||
|Logistica.Application.Rutas.EvidenciaArchivoValidator|100%|75%|
|Logistica.Application.Rutas.IniciarRuta.IniciarRutaCommandHandler|0%||
|Logistica.Application.Rutas.IntegrationEventHandlers.DomainToIntegrationMap<br/>per|100%|100%|
|Logistica.Application.Rutas.IntegrationEventHandlers.PublicarEntregaConfirm<br/>adaHandler|100%||
|Logistica.Application.Rutas.IntegrationEventHandlers.PublicarIncidenciaEntr<br/>egaRegistradaHandler|100%||
|Logistica.Application.Rutas.IntegrationEventHandlers.PublicarRutaOptimizada<br/>GeneradaHandler|100%||
|Logistica.Application.Rutas.OptimizarRuta.OptimizarRutaCommand|0%||
|Logistica.Application.Rutas.OptimizarRuta.OptimizarRutaCommandHandler|0%||
|Logistica.Application.Rutas.Policies.LiberarRepartidorAlCancelarRutaPolicy|100%||
|Logistica.Application.Rutas.Policies.LiberarRepartidorAlCompletarRutaPolicy|100%||
|Logistica.Application.Rutas.Policies.ReponerPaquetesAlCancelarRutaPolicy|100%||
|Logistica.Application.Rutas.Queries.ConstanciaEntregaDto|0%||
|Logistica.Application.Rutas.Queries.EstadoEntregaDto|0%||
|Logistica.Application.Rutas.Queries.GetHistorialConstanciasQuery|0%||
|Logistica.Application.Rutas.Queries.HistorialConstanciaDto|0%||
|Logistica.Application.Rutas.Queries.IncidenciaEntregaDto|0%||
|Logistica.Application.Rutas.Queries.ParadaDetalleDto|0%||
|Logistica.Application.Rutas.Queries.RutaDetalleDto|0%||
|Logistica.Application.Rutas.ReportarIncidencia.ReportarIncidenciaCommand|100%||
|Logistica.Application.Rutas.ReportarIncidencia.ReportarIncidenciaCommandHan<br/>dler|100%||

</details>
<details><summary>Logistica.Domain - 96%</summary>

|**Name**|**Line**|**Branch**|
|:---|---:|---:|
|**Logistica.Domain**|**96%**|**95%**|
|Logistica.Domain.Repartidores.Repartidor|96.1%|100%|
|Logistica.Domain.Repartidores.RepartidorErrors|100%||
|Logistica.Domain.Rutas.Events.EntregaConfirmada|100%||
|Logistica.Domain.Rutas.Events.IncidenciaEntregaRegistrada|100%||
|Logistica.Domain.Rutas.Events.ParadaOrdenada|100%||
|Logistica.Domain.Rutas.Events.RutaCancelada|100%||
|Logistica.Domain.Rutas.Events.RutaCreada|100%||
|Logistica.Domain.Rutas.Events.RutaOptimizadaGenerada|100%||
|Logistica.Domain.Rutas.PaqueteParaRuta|100%|100%|
|Logistica.Domain.Rutas.PaqueteParaRutaErrors|100%||
|Logistica.Domain.Rutas.ParadaEntrega|97.4%|100%|
|Logistica.Domain.Rutas.ParadaEntregaErrors|100%||
|Logistica.Domain.Rutas.RutaEntrega|95.9%|91.6%|
|Logistica.Domain.Rutas.RutaErrors|83.3%||
|Logistica.Domain.Shared.ConstanciaEntrega|90.9%|66.6%|
|Logistica.Domain.Shared.ConstanciaEntregaErrors|100%||
|Logistica.Domain.Shared.ContratoId|100%||
|Logistica.Domain.Shared.Coordenadas|92.8%|100%|
|Logistica.Domain.Shared.CoordenadasErrors|100%||
|Logistica.Domain.Shared.DireccionGeo|96.8%|100%|
|Logistica.Domain.Shared.DireccionGeoErrors|100%||
|Logistica.Domain.Shared.IncidenciaEntrega|85.7%|100%|
|Logistica.Domain.Shared.IncidenciaEntregaErrors|100%||
|Logistica.Domain.Shared.PacienteId|100%||
|Logistica.Domain.Shared.PaqueteId|100%||
|Logistica.Domain.Shared.ParadaId|100%||
|Logistica.Domain.Shared.RepartidorId|100%||
|Logistica.Domain.Shared.RutaId|100%||
|Logistica.Domain.Shared.SharedErrors|100%||
|Logistica.Domain.Shared.TypedIdValidator|100%|100%|
|Logistica.Domain.Shared.Vehiculo|93.7%|100%|
|Logistica.Domain.Shared.VehiculoErrors|100%||

</details>
