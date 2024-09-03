using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using PizzaStore.Models;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Pizzas") ?? "Data Source=Pizzas.db";

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("items"));
builder.Services.AddSqlite<PizzaDb>(connectionString);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PizzaStore API",
        Description = "Making the Pizzas you love",
        Version = "v1"
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1"));
}

app.MapGet("/", () => "Hello World!");
app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync());
app.MapPost("/pizza", async (PizzaDb db, Pizza pizza) =>
{
    await db.Pizzas.AddAsync(pizza);
    await db.SaveChangesAsync();

    return Results.Created($"/pizza/{pizza.Id}", pizza);
});
app.MapGet("/pizza/{id}", async (PizzaDb db, int id) => await db.Pizzas.FindAsync(id));
app.MapPut("/pizza/{id}", async(PizzaDb db, Pizza updatePizza, int id) =>
{
    Pizza? pizza = await db.Pizzas.FindAsync(id);
    IResult result = Results.NotFound();

    if (pizza != null)
    {
        pizza.Name = updatePizza.Name;
        pizza.Description = updatePizza.Description;
        await db.SaveChangesAsync();
        result = Results.NoContent();
    }

    return result;
});
app.MapDelete("/pizza/{id}", async (PizzaDb db, int id) =>
{
    Pizza? pizza = await db.Pizzas.FindAsync(id);
    IResult result = Results.NotFound();

    if (pizza != null)
    {
        db.Pizzas.Remove(pizza);
        await db.SaveChangesAsync();
        result = Results.NoContent();
    }

    return result;
});
app.Run();
