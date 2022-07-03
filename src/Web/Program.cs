using MediatR;
using PocketCqrs;
using PocketCqrs.EventStore;
using PocketCqrs.Projections;
using Web;
using Web.Commands;
using Web.Projections;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHandlers(typeof(CreatePortfolioCommandHandler).Assembly);

builder.Services.AddSingleton<IMessaging, Messaging>();
builder.Services.AddSingleton<IEventStore, EventStore>();
builder.Services.AddSingleton<IAppendOnlyStore>(new FileAppendOnlyStore("penger"));
builder.Services.AddHostedService<StartupWorker>();
builder.Services.AddSingleton<IProjectionStore<string, PortfolioStatusProjection.PortfolioStatus>>(
    new FileProjectionStore<string, PortfolioStatusProjection.PortfolioStatus>("penger"));

builder.Services.AddMediatR(typeof(Program));


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseRouting();

// app.UseAuthorization();

app.MapRazorPages();

app.Run();
