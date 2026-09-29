using TollFeeCalculator.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<IHolidayProvider, Holiday2013Provider>();
builder.Services.AddSingleton(FeeSchedule.Default);
builder.Services.AddSingleton<TollCalculator>();

var app = builder.Build();

app.MapControllers();

app.Run();