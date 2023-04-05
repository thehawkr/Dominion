using Microsoft.AspNetCore.Mvc;

namespace Crucible
{
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // ...

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            endpoints.MapFallbackToController("Index", "Home");
        });
    }

    public class HomeController : ControllerBase
    {
        public IActionResult Index()
        {
            return File("~/ClientApp/build/index.html", "text/html");
        }
    }

}
