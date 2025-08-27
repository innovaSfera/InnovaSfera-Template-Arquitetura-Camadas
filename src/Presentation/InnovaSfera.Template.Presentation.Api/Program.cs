using DomainDrivenDesign.Infrastructure.IoC;
using FluentValidation;
using InnovaSfera.Template.Presentation.Api.Extensions;

    
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();

// FluentValidation registration
builder.Services.AddValidatorsFromAssemblyContaining<InnovaSfera.Template.Presentation.Api.Validators.v1.SampleValidator>();

builder.Services.Register(builder.Configuration);
builder.Services.AddDbContext(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "InnovaSfera Template API v1");
    });
}

app.UseHttpsRedirection();

// Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
