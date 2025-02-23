namespace FashionApp.core.draw
{
    internal class DrawingViewDrawable : IDrawable
    {
        private readonly List<DrawingLine> _lines;

        public DrawingViewDrawable(List<DrawingLine> lines)
        {
            _lines = lines;
        }
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            foreach (var line in _lines)
            {
                if (line.Points.Count > 1)
                {
                    canvas.StrokeColor = line.Color;
                    canvas.StrokeSize = line.Thickness;
                    canvas.StrokeLineCap = LineCap.Round;
                    canvas.StrokeLineJoin = LineJoin.Round;

                    var path = new PathF();
                    path.MoveTo(line.Points[0]);

                    for (int i = 1; i < line.Points.Count; i++)
                    {
                        path.LineTo(line.Points[i]);
                    }

                    canvas.DrawPath(path);
                }
            }
        }
    }
}
