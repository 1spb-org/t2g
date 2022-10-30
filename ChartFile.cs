namespace T2G
{
    public class ChartFile
    {
        public string Title { set; get; } = "Untitled";

        public List<ChSeries>? Series { set; get; }
        public string? FontName { get; set; }
        public double? FontHeight { get; set; }
    }

    public class ChSeries
    {
        public string Name { set; get; } = "Unnamed";
        public List<List <double> >? Points { set; get; }
        public string Color { get; set; }
        public double MarkerSize { get; set; } = 0;
    }
}