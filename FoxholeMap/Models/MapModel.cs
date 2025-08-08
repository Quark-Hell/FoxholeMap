namespace FoxholeMap.Models;

public class MapTile
{
    public int X { get; set; }
    public int Y { get; set; }
    public string ImagePath { get; set; }
}

public class MapViewport
{
    public double X { get; set; } = 0;
    public double Y { get; set; } = 0;
    public double Zoom { get; set; } = 1.0;
    public int TileSize { get; set; } = 256;
    public int ViewportWidth { get; set; }
    public int ViewportHeight { get; set; }
}

public class TileRequest
{
    public int StartX { get; set; }
    public int StartY { get; set; }
    public int EndX { get; set; }
    public int EndY { get; set; }
    public double Zoom { get; set; }
}