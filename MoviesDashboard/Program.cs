namespace MoviesDashboard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();



            #region Added By Me    

            builder.Services.AddDIBusiness(builder);

            // Add DI for Repository
            //builder.Services.AddIdentity<AppUser, IdentityRole>(option =>
            //{
            //    option.Password.RequiredLength = 8;
            //    option.Password.RequireDigit = false;
            //    option.User.RequireUniqueEmail = true;
            //}).AddEntityFrameworkStores<AppDbContext>();


            // External Authentication - Google
            //builder.Services.AddAuthentication()
            //    .AddGoogle(options =>
            //    {
            //        // Load credentials from appsettings.json
            //        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
            //        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
            //    });
            #endregion



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.UseStaticFiles();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{Area=Customer}/{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
