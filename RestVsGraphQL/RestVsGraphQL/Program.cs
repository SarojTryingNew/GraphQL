using RestVsGraphQL.Services;
using RestVsGraphQL.Metrics;
using RestVsGraphQL.Middleware;
using RestVsGraphQL.GraphQL.DataLoaders.ECO;
using RestVsGraphQL.GraphQL.DataLoaders.POC;
using RestVsGraphQL.GraphQL.Query;
using RestVsGraphQL.GraphQL.Mutation;
using RestVsGraphQL.GraphQL.Types.ECO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<DataStore>();
builder.Services.AddSingleton<MetricsCollector>();
builder.Services.AddSingleton<OrderService>(); // Service layer for shared business logic
builder.Services.AddSingleton<DashboardService>(); // Service layer for dashboard calculations
builder.Services.AddSingleton<GraphQLExecutorService>(); // Service to execute GraphQL internally

builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "REST vs GraphQL Comparison API",
        Version = "v1",
        Description = "A comprehensive API demonstrating REST and GraphQL implementations with bulk operations, nested data, and dashboard aggregations.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "REST vs GraphQL Project"
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddFiltering()      // Enable [UseFiltering] attribute
    // Add DeviceApplication Query and Mutation extensions
    .AddTypeExtension<DeviceApplicationQueries>()
    .AddTypeExtension<DeviceApplicationExtendedQueries>()
    .AddTypeExtension<DeviceApplicationMutations>()
    // Add DeviceApplication Types with field resolvers
    .AddType<DeviceApplicationType>()
    .AddType<FunctionGroupType>()
    .AddType<FunctionType>()
    .AddType<FunctionBlockType>()
    .AddType<SignalType>()
    .AddType<SubsignalType>()
    .AddType<CdcConversionType>()
    .AddType<RoutingType>()
    // Register DataLoaders for efficient batching
    .AddDataLoader<CustomerByIdDataLoader>()
    .AddDataLoader<ProductByIdDataLoader>()
    .AddDataLoader<CategoryByIdDataLoader>()
    .AddDataLoader<OrderItemsByOrderIdDataLoader>()
    .AddDataLoader<OrderItemNotesByOrderItemIdDataLoader>()
    .AddDataLoader<OrdersByCustomerIdDataLoader>()
    .AddDataLoader<ProductsByCategoryIdDataLoader>()
    // DeviceApplication DataLoaders
    .AddDataLoader<FunctionGroupsByDeviceDataLoader>()
    .AddDataLoader<FunctionBlocksByDeviceDataLoader>()
    .AddDataLoader<FunctionsByFunctionGroupDataLoader>()
    .AddDataLoader<SignalsByFunctionBlockDataLoader>()
    .AddDataLoader<SubsignalsBySignalDataLoader>()
    .AddDataLoader<CdcConversionsBySignalDataLoader>()
    .AddDataLoader<RoutingsByParentPathDataLoader>()
    // NEW: Missing DataLoaders added
    .AddDataLoader<FunctionBlocksByFunctionGroupDataLoader>()
    .AddDataLoader<FunctionBlocksByFunctionDataLoader>()
    .AddDataLoader<SignalsByFunctionDataLoader>()
    // Scenario 7: Statistical DataLoaders for Device Statistics Dashboard
    .AddDataLoader<FunctionGroupCountByDeviceDataLoader>()
    .AddDataLoader<FunctionBlockCountByDeviceDataLoader>()
    .AddDataLoader<FunctionCountByDeviceDataLoader>()
    .AddDataLoader<SignalCountByDeviceDataLoader>()
    .AddDataLoader<AnalogSignalCountByDeviceDataLoader>()
    .AddDataLoader<StatusSignalCountByDeviceDataLoader>()
    .AddDataLoader<SubsignalCountByDeviceDataLoader>()
    .AddDataLoader<CdcConversionCountByDeviceDataLoader>()
    .AddDataLoader<RoutingCountByDeviceDataLoader>()
    .AddDataLoader<HasConfigurationByDeviceDataLoader>()
    .AddDataLoader<ConfiguredRoutingCountByDeviceDataLoader>()
    .AddDataLoader<EditableRoutingCountByDeviceDataLoader>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "REST vs GraphQL API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "REST vs GraphQL API";
        options.DefaultModelsExpandDepth(2);
        options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
    });
}

app.UseHttpsRedirection();

app.UseCors();

// Enable static files for front-end demo
app.UseDefaultFiles();
app.UseStaticFiles();

// Add metrics middleware
app.UseMiddleware<MetricsMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.MapGraphQL("/graphql");

app.Run();
