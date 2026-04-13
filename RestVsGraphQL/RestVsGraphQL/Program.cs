using RestVsGraphQL.GraphQL;
using RestVsGraphQL.Services;
using RestVsGraphQL.Metrics;
using RestVsGraphQL.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<DataStore>();
builder.Services.AddSingleton<MetricsCollector>();
builder.Services.AddSingleton<OrderService>(); // Service layer for shared business logic
builder.Services.AddSingleton<DashboardService>(); // Service layer for dashboard calculations

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
    // Register DataLoaders for efficient batching
    .AddDataLoader<RestVsGraphQL.GraphQL.DataLoaders.CustomerByIdDataLoader>()
    .AddDataLoader<RestVsGraphQL.GraphQL.DataLoaders.ProductByIdDataLoader>()
    .AddDataLoader<RestVsGraphQL.GraphQL.DataLoaders.CategoryByIdDataLoader>()
    .AddDataLoader<RestVsGraphQL.GraphQL.DataLoaders.OrderItemsByOrderIdDataLoader>()
    .AddDataLoader<RestVsGraphQL.GraphQL.DataLoaders.OrderItemNotesByOrderItemIdDataLoader>()
    .AddDataLoader<RestVsGraphQL.GraphQL.DataLoaders.OrdersByCustomerIdDataLoader>()
    .AddDataLoader<RestVsGraphQL.GraphQL.DataLoaders.ProductsByCategoryIdDataLoader>();

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
