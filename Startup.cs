using Microsoft.EntityFrameworkCore;
using DoctorAppointmentSystem.Models; // For AppDbContext

public class Startup
{
    public Startup(IConfiguration configuration) => Configuration = configuration;
    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        // PostgreSQL connection
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();

        // Serve index.html, CSS, JS, images
        app.UseDefaultFiles();  // <-- Find and serve index.html automatically
        app.UseStaticFiles();

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers(); // API endpoints (e.g., /api/auth/login)
        });
    }
}
