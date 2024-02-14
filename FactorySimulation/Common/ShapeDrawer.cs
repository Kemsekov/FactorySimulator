
using System.Drawing;
using GraphSharp.GraphDrawer;
using MathNet.Numerics.LinearAlgebra.Single;
using ScottPlot;

public class PlotShapeDrawer : IShapeDrawer
{
    public Plot plot;

    public PlotShapeDrawer(int imageSize)
    {
        plot = new Plot(imageSize,imageSize);
    }
    public void Clear(Color color)
    {
        plot.Clear();
    }

    public void DrawLine(Vector start, Vector end, Color color, double thickness)
    {
        plot.AddLine(start[0],start[1],end[0],end[1],color, (float)thickness);
    }

    public void DrawText(string text, Vector position, Color color, double size)
    {
        plot.AddText(text,position[0],position[1],(float)size,color);
    }

    public void FillEllipse(Vector position, double width, double height, Color color)
    {
        plot.AddEllipse(position[0],position[1],width,height,color);
    }
}