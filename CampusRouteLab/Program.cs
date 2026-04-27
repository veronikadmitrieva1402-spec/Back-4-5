using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CampusRouteLab.Models;
using CampusRouteLab.Services;
using CampusRouteLab.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCampusServices();

builder.Services.AddRouting();

var app = builder.Build();

app.UseRequestAudit();

app.MapGet("/", () => Results.Json(new
{
    service = "CampusRouteLab",
    description = "Учебный диагностический веб-сервис для демонстрации маршрутизации и DI",
    endpoints = new[]
    {
        "/students - список групп",
        "/students/{group} - группа со студентами",
        "/students/{group}/{id} - конкретный студент",
        "/reports/{section?} - отчёт с необязательным параметром",
        "/portal/{module=home}/{page=index}/{id?} - маршрут со значениями по умолчанию",
        "/files/{**path} - catch-all маршрут",
        "/routes - все зарегистрированные endpoint-ы",
        "/diag/lifetimes - демонстрация жизненных циклов",
        "/diag/lifetimes/check - повторное разрешение transient",
        "/diag/request-services - получение через HttpContext.RequestServices",
        "/diag/app-services - получение через app.Services"
    }
}));

app.MapGet("/students", (IStudentCatalogService catalog) =>
{
    var groups = catalog.GetAllGroups();
    var result = groups.Select(g => new
    {
        group = g.Key,
        studentCount = g.Value.Students.Count
    });
    return Results.Json(result);
});

app.MapGet("/students/{group}", (string group, IStudentCatalogService catalog) =>
{
    var groupData = catalog.GetGroup(group);
    if (groupData == null)
        return Results.NotFound(new { error = $"Группа '{group}' не найдена" });
    
    return Results.Json(new
    {
        groupName = groupData.Name,
        studentCount = groupData.Students.Count,
        students = groupData.Students.Select(s => new { s.Id, s.Name })
    });
});

app.MapGet("/students/{group}/{id}", (string group, int id, IStudentCatalogService catalog) =>
{
    var student = catalog.GetStudent(group, id);
    if (student == null)
        return Results.NotFound(new { error = $"Студент с ID {id} в группе '{group}' не найден" });
    
    return Results.Json(student);
});

app.MapGet("/reports/{section?}", (string? section) =>
{
    var actualSection = string.IsNullOrEmpty(section) ? "overview" : section;
    return Results.Json(new
    {
        section = actualSection,
        message = $"Вы запросили отчёт по разделу: {actualSection}",
        availableSections = new[] { "overview", "students", "groups", "attendance" }
    });
});

app.MapGet("/portal/{module=home}/{page=index}/{id?}", (string module, string page, string? id) =>
{
    return Results.Text($@"
 PORTAL ROUTING DEMO 
Module: {module}
Page: {page}
ID parameter: {(id ?? "не указан")}
Full path: /portal/{module}/{page}{(id != null ? $"/{id}" : "")}
    ");
});

app.MapGet("/files/{**path}", (string path) =>
{
    return Results.Text($@"
 CATCH-ALL ROUTE DEMO 
Captured path: {path}
Full requested: /files/{path}
    ");
});


app.MapGet("/routes", (EndpointDataSource endpointDataSource) =>
{
    var sb = new StringBuilder();
    sb.AppendLine("ЗАРЕГИСТРИРОВАННЫЕ ENDPOINTS\n");
    
    foreach (var endpoint in endpointDataSource.Endpoints)
    {
        if (endpoint is RouteEndpoint routeEndpoint)
        {
            sb.AppendLine($"Route: {routeEndpoint.RoutePattern.RawText}");
            sb.AppendLine($"Order: {routeEndpoint.Order}");
            sb.AppendLine($"DisplayName: {routeEndpoint.DisplayName ?? "N/A"}");
            sb.AppendLine("---");
        }
    }
    
    return Results.Text(sb.ToString());
});

app.MapGet("/diag/lifetimes", (
    IAppInfoService appInfo,
    IRequestContextService requestContext,
    ITransientMarkerService transientMarker,
    DiagnosticsReportService diagnosticsReport) =>
{
    var transientMarker2 = diagnosticsReport;
    
    return Results.Json(new
    {
        direct_resolution = new
        {
            singleton = new
            {
                instanceId = appInfo.AppInstanceId,
                createdAt = appInfo.StartedAt,
                type = "Singleton - один на всё приложение"
            },
            scoped = new
            {
                requestId = requestContext.RequestId,
                createdAt = requestContext.CreatedAt,
                type = "Scoped - один на HTTP-запрос"
            },
            transient = new
            {
                markerId = transientMarker.MarkerId,
                createdAt = transientMarker.CreatedAt,
                type = "Transient - новый при каждом запросе"
            }
        },
        from_diagnostics_report = diagnosticsReport.GetReport(),
        explanation = new
        {
            singleton = "Создаётся один раз при запуске приложения и используется во всех запросах",
            scoped = "Создаётся один раз на каждый HTTP-запрос, переиспользуется в пределах запроса",
            transient = "Создаётся каждый раз при запросе зависимости, даже внутри одного запроса"
        }
    });
});

app.MapGet("/diag/lifetimes/check", (IServiceProvider serviceProvider) =>
{
    var transient1 = serviceProvider.GetRequiredService<ITransientMarkerService>();
    var transient2 = serviceProvider.GetRequiredService<ITransientMarkerService>();
    var transient3 = serviceProvider.GetRequiredService<ITransientMarkerService>();
    
    var scoped1 = serviceProvider.GetRequiredService<IRequestContextService>();
    var scoped2 = serviceProvider.GetRequiredService<IRequestContextService>();
    
    var singleton1 = serviceProvider.GetRequiredService<IAppInfoService>();
    var singleton2 = serviceProvider.GetRequiredService<IAppInfoService>();
    
    return Results.Json(new
    {
        transient_demonstration = new
        {
            first = new { id = transient1.MarkerId, createdAt = transient1.CreatedAt },
            second = new { id = transient2.MarkerId, createdAt = transient2.CreatedAt },
            third = new { id = transient3.MarkerId, createdAt = transient3.CreatedAt },
            conclusion = $"Transient сервисы имеют РАЗНЫЕ ID: {transient1.MarkerId != transient2.MarkerId && transient2.MarkerId != transient3.MarkerId}"
        },
        scoped_demonstration = new
        {
            first = new { id = scoped1.RequestId, createdAt = scoped1.CreatedAt },
            second = new { id = scoped2.RequestId, createdAt = scoped2.CreatedAt },
            conclusion = $"Scoped сервисы имеют ОДИНАКОВЫЙ ID: {scoped1.RequestId == scoped2.RequestId}"
        },
        singleton_demonstration = new
        {
            first = new { id = singleton1.AppInstanceId, createdAt = singleton1.StartedAt },
            second = new { id = singleton2.AppInstanceId, createdAt = singleton2.StartedAt },
            conclusion = $"Singleton сервисы имеют ОДИНАКОВЫЙ ID: {singleton1.AppInstanceId == singleton2.AppInstanceId}"
        }
    });
});

app.MapGet("/diag/request-services", async (HttpContext context) =>
{
    var requestServices = context.RequestServices;
    
    var appInfo = requestServices.GetRequiredService<IAppInfoService>();
    var requestContext = requestServices.GetRequiredService<IRequestContextService>();
    var transientMarker = requestServices.GetRequiredService<ITransientMarkerService>();
    
    return Results.Json(new
    {
        method = "Через HttpContext.RequestServices.GetRequiredService<T>()",
        singleton = new { instanceId = appInfo.AppInstanceId, startedAt = appInfo.StartedAt },
        scoped = new { requestId = requestContext.RequestId, createdAt = requestContext.CreatedAt },
        transient = new { markerId = transientMarker.MarkerId, createdAt = transientMarker.CreatedAt },
        note = "Этот способ полезен, когда нет доступа к DI через конструктор или параметры"
    });
});

app.MapGet("/diag/app-services", (IServiceProvider appServices) =>
{
    var appInfo = appServices.GetRequiredService<IAppInfoService>();
    
    return Results.Json(new
    {
        method = "Через app.Services.GetRequiredService<T>() - подходит только для Singleton",
        warning = "Scoped сервисы НЕЛЬЗЯ получать через app.Services - будет ошибка!",
        singleton = new
        {
            instanceId = appInfo.AppInstanceId,
            startedAt = appInfo.StartedAt,
            message = "Singleton работает корректно, так как существует всё время жизни приложения"
        }
    });
});

app.Run();