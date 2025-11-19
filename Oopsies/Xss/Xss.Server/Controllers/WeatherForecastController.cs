using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Reflection.Emit;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Xss.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("Search")]
        public IActionResult Search(string query)
        {
            // assume query is '<script>alert('XSS')</script>'
            string result = "You searched for: " + query;
            return new ContentResult() { Content = result };
        }

        [HttpPost("UploadFile")]
        public void SaveFile(IFormFile file)
        {
            string filePath = Path.Combine(@"C:\temp", file.FileName); //oef...
            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);
        }

        [HttpPost("ExecuteCommand")]
        public void ExecuteCommand(string command)
        {
            Process.Start("cmd.exe", "/c " + command); //no no no
        }

        [HttpPost("CallWebService")]
        public void CallWebService(string url)
        {
            WebClient client = new WebClient();
            string response = client.DownloadString(url); //also no
        }

        [HttpGet("ReDosMe")]
        public ActionResult ReDosMe(string input)
        {
            const string pattern = "^[a-zA-Z0-9]*$";
            var match = Regex.IsMatch(input, pattern);
            return new OkResult();
        }

        [HttpPost("Inject")]
        public void InjectSomething(string code)
        {
            var method = new DynamicMethod("Run", typeof(void), null);
            var il = method.GetILGenerator();
            il.EmitWriteLine($"Hello, {code}");
            il.Emit(OpCodes.Ret);
            method.CreateDelegate<Action>()();
        }
    }
}
