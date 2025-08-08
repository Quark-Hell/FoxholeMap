namespace FoxholeMap.Models;

public class MapViewport
{
    public double X { get; set; } = 0;
    public double Y { get; set; } = 0;
    public double Zoom { get; set; } = 1.0;
    public int TileSize { get; set; } = 256;
    public int ViewportWidth { get; set; }
    public int ViewportHeight { get; set; }
}