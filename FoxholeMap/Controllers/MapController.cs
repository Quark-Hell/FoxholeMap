using System.Text;
using System.Text.Json;
using FoxholeMap.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    
    public class MapTile
    {
        public int row { get; set; }
        public int col { get; set; }
        public double zoom { get; set; }
    }

    [HttpPost]
    public JsonResult GetTiles([FromBody] TileRequestModel request)
    {
        var tiles = new List<MapTile>();

        int count = 1 << (int)request.Zoom;

        for (int row = 0; row < count; row++)
        {
            for (int col = 0; col < count; col++)
            {
                tiles.Add(new MapTile
                {
                    row = row,
                    col = col,
                    zoom = request.Zoom
                });
            }
        }

        return Json(tiles);
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
    public JsonResult MoveMap([FromBody] MapViewport request)
    {
        var viewport = HttpContext.Session.GetObject<MapViewport>("_mapViewport") 
                       ?? new MapViewport() { X = 0, Y = 0, Zoom = 0.0 };
        
        viewport.X = request.X;
        viewport.Y = request.Y;
        
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