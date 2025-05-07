using HackerRank.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure the HTTP request pipeline.
builder.Services.AddHttpClient();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add services to the container.
builder.Services.AddScoped<IHackerRankService, HackerRankService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (!context.Request.Path.Value.StartsWith("/api") &&
        !Path.HasExtension(context.Request.Path.Value))
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
    }
    else
    {
        await next();
    }
});

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
