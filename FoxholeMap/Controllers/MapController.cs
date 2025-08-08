using System.Text.Json;
using FoxholeMap.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoxholeMap.Controllers;

[Route("[controller]/[action]")]
public class MapController : Controller
{
    private readonly string _tilesPath = "MapAssets/Sat Tiles/";
    private readonly ILogger<MapController> _logger;

    public MapController(ILogger<MapController> logger)
    {
        _logger = logger;   
    }

    public IActionResult Index()
    {
        return View();
    }
    
    [HttpPost]
    public JsonResult GetTiles([FromBody] TileRequestModel request)
    {
        List<MapTileModel> mapTiles = new List<MapTileModel>();

        for (int x = request.StartX; x < request.EndX; x++)
        {
            for (int y = request.StartY; y < request.EndY; y++)
            {
                int zoom = Convert.ToInt32(Math.Floor(request.Zoom));
                
                string foldedPath = $"{_tilesPath} / {zoom}";
                string tilePath = $"{zoom}_{x}_{y}.png";
                
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", foldedPath, tilePath);

                if (!System.IO.File.Exists(fullPath))
                {
                    tilePath = GeneratePlaceholderTile(x,y);
                }

                mapTiles.Add(new MapTileModel
                {
                    X = x,
                    Y = y,
                    ImagePath = Url.Content($"~/{tilePath}")
                });
            }
        }
        
        return Json(mapTiles);
    }


    [HttpGet]
    public IActionResult GetTile(int x, int y)
    {
        //TODO: Add zoom
        string tilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", _tilesPath, $"{x}_{y}.png");

        if (System.IO.File.Exists(tilePath))
        {
            var fileBytes = System.IO.File.ReadAllBytes(tilePath);
            return File(fileBytes, "image/png");
        }
        
        return RedirectToAction("GeneratePlaceholder", new { x, y });
    }

    [HttpGet]
    public IActionResult GeneratePlaceholder(int x, int y)
    {
        string placeholderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", $"{x}_{y}.png");

        if (System.IO.File.Exists(placeholderPath))
        {
            var fileBytes = System.IO.File.ReadAllBytes(placeholderPath);
            return File(fileBytes, "image/png");
        }
        
        return NotFound();
    }

    private string GeneratePlaceholderTile(int x, int y)
    {
        //TODO: return white/black checker
        return $"{x}_{y}.png";
    }

    [HttpPost]
    public JsonResult SaveViewport([FromBody] MapViewport viewport)
    {
        HttpContext.Session.SetString("MapViewport", System.Text.Json.JsonSerializer.Serialize(viewport));
        
        return Json(new {success = true, message = "Map viewport saved successfully!"});
    }

    public JsonResult LoadViewport([FromBody] MapViewport viewport)
    {
        string viewportJson = HttpContext.Session.GetString("MapViewport");

        if (!string.IsNullOrEmpty(viewportJson))
        {
            MapViewport viewportBytes = System.Text.Json.JsonSerializer.Deserialize<MapViewport>(viewportJson);
            return Json(viewportBytes);
        }
        
        return Json(new MapViewport());
    }

    [HttpPost]
    public JsonResult UpscaleMap()
    {
        const int maxZoom = 6;
        const int minZoom = 0;
        
        var viewport = HttpContext.Session.GetObject<MapViewport>("_mapViewport") 
                       ?? new MapViewport() { X = 0, Y = 0, Zoom = 0.0 };
        
        viewport.Zoom = Math.Clamp(viewport.Zoom + 1, minZoom, maxZoom);
        
        HttpContext.Session.SetObject("_mapViewport", viewport);

        return Json(viewport);
    }

    [HttpPost]
    public JsonResult DownscaleMap()
    {
        const int maxZoom = 6;
        const int minZoom = 0;

        var viewport = HttpContext.Session.GetObject<MapViewport>("_mapViewport") 
                       ?? new MapViewport() { X = 0, Y = 0, Zoom = 0.0 };
        
        viewport.Zoom = Math.Clamp(viewport.Zoom - 1, minZoom, maxZoom);
        
        HttpContext.Session.SetObject("_mapViewport", viewport);

        return Json(viewport);
    }
    
    [HttpPost]
    public JsonResult MoveMap([FromBody] MapViewport request)
    {
        var viewport = HttpContext.Session.GetObject<MapViewport>("_mapViewport") 
                       ?? new MapViewport() { X = 0, Y = 0, Zoom = 0.0 };
        
        viewport.X = request.X;
        viewport.Y = request.Y;
        
        HttpContext.Session.SetObject("_mapViewport", viewport);

        return Json(viewport);
    }
    
    [HttpPost]
    public JsonResult ResetMap()
    {
        var viewport = HttpContext.Session.GetObject<MapViewport>("_mapViewport") 
                       ?? new MapViewport() { X = 0, Y = 0, Zoom = 1.0 };
        
        viewport.X = 0;
        viewport.Y = 0;
        viewport.Zoom = 0;
        
        HttpContext.Session.SetObject("_mapViewport", viewport);

        return Json(viewport);
    }
}

public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}