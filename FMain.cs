using MathNet.Numerics.Integration;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Series;
using System.Diagnostics;
using System.Reflection;

namespace T2G
{
    public partial class FMain : Form
    {
        static Random _rnd = new Random();
        ChartFile? _cf;

        public FMain()
        {
            InitializeComponent();

            string? name = Environment.GetCommandLineArgs().Skip(1).FirstOrDefault();

            if (string.IsNullOrEmpty(name))
            {
                var g = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                g = Path.Combine(g, "default.chartjson");
                if (File.Exists(g))
                    name = g;
            }

            if (!string.IsNullOrEmpty(name))
            {
                LoadChartFile(name);
            }

            plotView.Model = new OxyPlot.PlotModel { Title = _cf?.Title ?? "Example" };
            plotView.BackColor = Color.White;
            plotView.Click += PlotView1_Click;
            var cms = new ContextMenuStrip();
            cms.Items.Add("Save", null, OnSave);
            cms.Items.Add("Save Image", null, OnSaveImg);
            cms.Items.Add("Axes Font...", null, OnAxesFont);
            cms.Items.Add("-");
            cms.Items.Add("Load...", null, OnLoadChart);
            cms.Items.Add("-");
            cms.Items.Add("Visit 1spb.org", null, OnAbout);
            cms.Items.Add("github.com/1spb-org/t2g", null, OnAbout2);
            plotView.ContextMenuStrip = cms;

            if (_cf == null)
                RandPlot();
            else
                InitPlotFromFile();
        }

        private void OnAbout(object? sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = "https://1spb.org", UseShellExecute = true, Verb = "Open" });
        }
        private void OnAbout2(object? sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = "https://github.com/1spb-org/t2g", UseShellExecute = true, Verb = "Open" });
        }

        private void OnLoadChart(object? sender, EventArgs e)
        {
            OpenFileDialog sfd = new OpenFileDialog();
            sfd.Filter = "Chart Json|*.chartjson|All files|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
              LoadChartFile(sfd.FileName);
                if (_cf != null)
                    InitPlotFromFile();
            }

        }

        private void OnAxesFont(object? sender, EventArgs e)
        {
            FontDialog fd = new FontDialog();
            fd.Font = plotView.Font;
            if (fd.ShowDialog() == DialogResult.OK)
            {
                plotView.Font = fd.Font;

                if (_cf != null)
                {
                    _cf.FontHeight = plotView.Model.DefaultFontSize;
                    _cf.FontName = plotView.Model.DefaultFont;
                }

                plotView.Model.DefaultFontSize = fd.Font.Height;
                plotView.Model.DefaultFont = fd.Font.Name;
                plotView.Model.TitleFont = fd.Font.Name;
                plotView.InvalidatePlot(true);
            }
        }

        private void OnSaveImg(object? sender, EventArgs e)
        {

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PNG|*.png|SVG|*.svg|All files|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                switch (sfd.FilterIndex)
                {
                    case 1:
                        OxyPlot.WindowsForms.PngExporter.Export(plotView.Model, sfd.FileName, plotView.Width, plotView.Height);
                        break;

                    case 2:
                        using (var stream = File.Create(sfd.FileName))
                        {
                            OxyPlot.WindowsForms.SvgExporter.Export(plotView.Model, stream, plotView.Width, plotView.Height, true);
                        }
                        break;

                    default:
                        MessageBox.Show("Specify export format, please");
                        break;
                }
            
            }

        }

        private void InitPlotFromFile()
        {
            plotView.Model.Series.Clear();
            plotView.Model.Title = Text = _cf?.Title ?? "Untitled";

            plotView.Model.DefaultFontSize = _cf?.FontHeight ?? plotView.Model.DefaultFontSize;
            plotView.Model.DefaultFont = _cf?.FontName ?? plotView.Model.DefaultFont;
            plotView.Model.TitleFont = _cf?.FontName ?? plotView.Model.TitleFont;

            if (_cf.Series != null)
            foreach(ChSeries cs in _cf.Series)
            {
                if (cs.Points == null)
                    continue;
                FunctionSeries fs = new FunctionSeries();
                fs.Title = cs.Name;
                
                fs.MarkerSize = cs.MarkerSize;
                fs.MarkerType = MarkerType.Circle;
                    
                foreach (var L in cs.Points)                
                    fs.Points.Add(new OxyPlot.DataPoint(L[0], L[1]));
                
                if(!string.IsNullOrEmpty(cs.Color))
                try { fs.Color = OxyColor.Parse(cs.Color); } catch { }

                plotView.Model.Series.Add(fs);
            }

            plotView.InvalidatePlot(true);


        }

        private void LoadChartFile(string name)
        {
            try
            {
                _cf = JsonConvert.DeserializeObject<ChartFile>(File.ReadAllText(name));
            }
            catch(Exception e)
            {
                _cf = null;
                MessageBox.Show(e.Message);
                plotView.Model.Title = "";
                plotView.Model.Series.Clear();
                plotView.InvalidatePlot(true);
            }
        }
        private void SaveChartFile(string name, ChartFile? CF)
        {
            try
            {
                File.WriteAllText(name, JsonConvert.SerializeObject(CF, Formatting.Indented));
            }
            catch
            { 
            }
        }

        private void OnSave(object? sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Chart Json|*.chartjson|All files|*.*";
            if (sfd.ShowDialog() == DialogResult.OK)
            {                     
                SaveChartFile(sfd.FileName, _cf ?? ChartFromPlot());              
            }

        }

        private ChartFile? ChartFromPlot()
        {
            ChartFile cf = new ChartFile { Title = plotView.Model.Title,  Series = new List<ChSeries> { } };

            cf.FontHeight = plotView.Model.DefaultFontSize;
            cf.FontName = plotView.Model.DefaultFont;

            // cf.FontName = plotView.Model.TitleFont;

            foreach (FunctionSeries t in plotView.Model.Series)
            {
                ChSeries s = new ChSeries {
                    Points = new List<List<double>>()
                };

                foreach (DataPoint p in t.Points)
                   s.Points.Add(new List<double> { p.X, p.Y });

                s.Name = t.Title;
                s.Color = t.Color.ToByteString();

                cf.Series.Add(s);
            }


            return cf; 
        }

        private void RandPlot()
        {
            if (_rnd.NextDouble() > 0.5)
                AddLissajous(Rand(1, 9), Rand(1, 9));
            else
                EulerSpiral();
        }

        private void PlotView1_Click(object? sender, EventArgs e)
        {
            if (_cf == null)
                RandPlot();


        }

        private int Rand(int v1, int v2)
        {
            return _rnd.Next(v1, v2);
        }

        private void AddLissajous(int vx, int vy)
        {
            plotView.Model.Series.Clear();
            plotView.Model.Title = Text = $"Lissajous({vx},{vy})";

            FunctionSeries fs = new FunctionSeries();
            double s = 0.005 * Math.PI, x, y;
            for (double t = 0; t <= Math.PI * 2; t += s)
            {
                x = Math.Cos(vx * t);
                y = Math.Sin(vy * t);
                fs.Points.Add(new OxyPlot.DataPoint(x, y));
            }

            fs.Color = RandColor(Color.Black, Color.LightGray);


            plotView.Model.Series.Add(fs);

            plotView.InvalidatePlot(true);

        }


        private void EulerSpiral()
        {
            plotView.Model.Series.Clear();
            plotView.Model.Title = Text = $"EulerSpiral";

            FunctionSeries fs = new FunctionSeries();
            double s = 0.005 * Math.PI, x, y;
            for (double t = - Math.PI * 2; t <= Math.PI * 2; t += s)
            {
                x = NewtonCotesTrapeziumRule.IntegrateAdaptive(x => Math.Cos(x * x), 0, t, 1e-5);
                y = NewtonCotesTrapeziumRule.IntegrateAdaptive(x => Math.Sin(x * x), 0, t, 1e-5);
                fs.Points.Add(new OxyPlot.DataPoint(x, y));
            }
             

            fs.Color = RandColor(Color.Black, Color.LightGray);


            plotView.Model.Series.Add(fs);

            plotView.InvalidatePlot(true);

        }

        private OxyColor RandColor(Color d, Color l)
        {
            var r = OxyColor.FromRgb(
           (byte)Rand(d.R, l.R),
           (byte)Rand(d.G, l.G),
           (byte)Rand(d.B, l.B));
            return r;
        }
    }
}