using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeWeb.Controllers;

public class HomeController(IHttpClientFactory httpClientFactory, IConfiguration config) : Controller
{
    public async Task<IActionResult> Index()
    {
        var apiUrl = config["ApiSettings:BaseUrl"] ?? "http://localhost:5002";
        var client = httpClientFactory.CreateClient();

        var response = await client.GetAsync($"{apiUrl}/api/employees");
        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var list = new List<EmployeeItem>();

        if (doc.RootElement.TryGetProperty("employees", out var empArray))
        {
            foreach (var item in empArray.EnumerateArray())
            {
                list.Add(new EmployeeItem(
                item.GetProperty("id").GetInt32(),
                item.GetProperty("departmentId").GetInt32(),
                item.GetProperty("name").GetString() ?? "")
                    );
            }
        }

        ViewBag.EnvName = config["ENVIRONMENT_NAME"] ?? "LOCAL";
        ViewBag.ApiEnvName = doc.RootElement.TryGetProperty("environment", out var env) ? env.GetString() : "UNKNOWN";
        return View(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            var apiUrl = config["ApiSettings:BaseUrl"] ?? "http://localhost:5002";
            var client = httpClientFactory.CreateClient();
            await client.PostAsJsonAsync($"{apiUrl}/api/employees", new { Name = name });
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }
}

public record EmployeeItem(int Id, int DepartmentId, string Name);
