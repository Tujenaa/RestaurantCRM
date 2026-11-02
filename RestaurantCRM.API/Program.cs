using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RestaurantCRM.BusinessLogic.Services;
using RestaurantCRM.DataAccess;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddHttpClient();
builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
         c.SwaggerDoc("v1", new OpenApiInfo { Title = "RestaurantCRM API", Version = "v1" });
    
//     // Cấu hình truyền JWT Token trong Swagger UI
//     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
//         Name = "Authorization",
//         In = ParameterLocation.Header,
//         Type = SecuritySchemeType.ApiKey,
//         Scheme = "Bearer"
//     });

//     c.AddSecurityRequirement(new OpenApiSecurityRequirement
//     {
//         {
//             new OpenApiSecurityScheme
//             {
//                 Reference = new OpenApiReference
//                 {
//                     Type = ReferenceType.SecurityScheme,
//                     Id = "Bearer"
//                 }
//             },
//             Array.Empty<string>()
//         }
//     });
});

// Cấu hình Authentication & JWT
var key = Encoding.ASCII.GetBytes("Chuoi_Bao_Mat_Bi_Mat_Cua_Nha_Hang_CRM_123456");
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var app = builder.Build();

// Bổ sung cột DANH_GIA.daXuLy cho CSDL cũ (chạy 1 lần, an toàn khi cột đã có).
// Phản hồi đã có câu trả lời được coi là đã xử lý.
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RestaurantCRM.DataAccess.Data.RestaurantCrmContext>();
    Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.ExecuteSqlRaw(db.Database, @"
IF COL_LENGTH('DANH_GIA', 'daXuLy') IS NULL
BEGIN
    ALTER TABLE DANH_GIA ADD daXuLy BIT NOT NULL CONSTRAINT DF_DANH_GIA_daXuLy DEFAULT 0;
    EXEC('UPDATE DANH_GIA SET daXuLy = 1 WHERE EXISTS (SELECT 1 FROM TRA_LOI_DANH_GIA t WHERE t.maDanhGia = DANH_GIA.maDanhGia)');
END");
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Không thể kiểm tra/bổ sung cột DANH_GIA.daXuLy");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
