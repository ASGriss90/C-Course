using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;


var builder = WebApplication.CreateBuilder(args);
//Must add controllers before building the app.

builder.Services.AddControllers(); // Adds the controllers
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors((options) => {
    options.AddPolicy("DevCors",(corsBuilder) => {
        corsBuilder.WithOrigins("http://localhost:3000")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });

    options.AddPolicy("ProdCors",(corsBuilder) => {
            corsBuilder.WithOrigins("http://localhost:3000") // Change to the actual domain if you had one
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
        });
});


string? tokenKeyString = builder.Configuration.GetSection("AppSettings:TokenKey").Value;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>{

    options.TokenValidationParameters = new TokenValidationParameters(){
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKeyString !=null ? tokenKeyString : ""))
        ,ValidateIssuer = false, 
        ValidateAudience =false
         }; 
    });
    




var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevCors");
}else{
    app.UseHttpsRedirection();
    app.UseCors("ProdCors");
}

app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
// app.MapGet("/GetWeatherForcast", ()=>{

// })


// .WithName("GetWeatherForecast")
// .WithOpenApi();

app.Run();

