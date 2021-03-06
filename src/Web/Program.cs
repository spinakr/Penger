using Domain;
using Domain.ValueObjects;
using MediatR;
using PocketCqrs;
using PocketCqrs.EventStore;
using PocketCqrs.Projections;
using Web;
using Web.Commands;
using Domain.Projections;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHandlers(typeof(CreatePortfolioCommandHandler).Assembly);
builder.Services.AddHandlers(typeof(Portfolio).Assembly);

builder.Services.AddSingleton<IMessaging, Messaging>();
builder.Services.AddSingleton<IEventStore, EventStore>();
builder.Services.AddSingleton<IAppendOnlyStore>(new FileAppendOnlyStore("penger-eventstore"));
builder.Services.AddHostedService<StartupWorker>();
builder.Services.AddHostedService<PriceWorker>();
builder.Services.AddSingleton<IProjectionStore<string, PortfolioProjection>>(new InMemoryProjectionStore<string, PortfolioProjection>());
builder.Services.AddSingleton<IProjectionStore<string, List<RegisteredInvestmentsProjection>>>(new InMemoryProjectionStore<string, List<RegisteredInvestmentsProjection>>());

builder.Services.AddMediatR(typeof(Program));
builder.Services.AddMediatR(typeof(Portfolio));


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



