using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using SkiaSharp;
using System.IO;
using SkiaSharp.Views.Forms;
using Android.Views;
using System.Threading;

namespace Automate
{
    public partial class MainPage : Shell
    {
        class Celluar : ICelluarDrawer
        {
            private SKCanvasView canvas;
            Action<Point[]> recordCells;
            public Celluar(SKCanvasView canvas, Action<Point[]> recordCells)
            {
                this.canvas = canvas;
                this.recordCells = recordCells;
            }

            public void GameOver()
            {
                
            }

            public void RedrawLiveCells(Point[] cell)
            {
                recordCells(cell);
                canvas.InvalidateSurface();
            }
        }
        SKPaint background = new SKPaint()
        {
            Color = SKColors.Black,
            Style = SKPaintStyle.Fill
        };
        SKPaint cell = new SKPaint()
        {
            Color = SKColors.Blue,
            Style = SKPaintStyle.Fill
        };

        private bool[,] map;
        private int[] liveStarts;
        private int[] stillLive;
        private bool isStarted = false;
        private Point[] liveCells;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private CancellationToken token;
        public MainPage()
        {
            InitializeComponent();
            map = new bool[1, 1];
            token = cts.Token;
        }

        private void SaveSettings(object sender, EventArgs e)
        {
            try
            {
                isStarted = false;
                _switch.IsToggled = false;
                int x = int.Parse(_xLenght.Text);
                int y = int.Parse(_yLenght.Text);
                map = new bool[x, y];
                int maxLenght = Math.Max(x, y);
                double minSize = Math.Min(_contentPage.Width, _contentPage.Height - _controls.Height);
                _canvas.WidthRequest = minSize;
                _canvas.HeightRequest = minSize * Math.Max(x, y) / Math.Min(x, y);

                liveStarts = _startLive.Text.Split(',').Select(c => int.Parse(c)).ToArray();
                stillLive = _stillLive.Text.Split(',').Select(c => int.Parse(c)).ToArray();
                _numOfTestTableButtons.Text = "";
                _testTable.Children.Clear();
                cts.Cancel();
            }
            catch (Exception ex) 
            {
                DisplayAlert("Ошибка", ex.Message, "Ок");
            };
        }

        private async void Delay(CelluarAutomat ca, CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;
                if (_switch.IsToggled)
                {
                    ca.NextIteration();
                    await Task.Delay((int)_liveSpeed.Value);
                }
            }
        }
        private async void Start()
        {
            isStarted = true;
            var ca = new CelluarAutomat(map, liveStarts, stillLive, new Celluar(_canvas, (Point[] cells) => liveCells = cells));
            await Task.Run(() => Delay(ca, token));
        }
        private void StartOrPause(object sender, ToggledEventArgs e)
        {
            if (!isStarted)
            {
                cts = new CancellationTokenSource();
                token = cts.Token;
                Start();
            }
        }


        private void Canvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (liveCells != null)
            {
                int x = map.GetLength(0);
                int y = map.GetLength(1);
                int minLenght = Math.Min(x, y);
                double minSize = Math.Min(e.Info.Width, e.Info.Height);
                float cellS = (float)(minSize / minLenght);
                var surface = e.Surface;
                var canv = surface.Canvas;
                canv.Clear();

                canv.DrawRect(0, 0, cellS * x, cellS * y, background);
                foreach (var cell in liveCells)
                {
                    canv.DrawRect(cell.x * cellS, cell.y * cellS, cellS, cellS, this.cell);
                }
                //canv.Save();
                canv.Restore();
            }
        }

        private void EntryCell_Completed(object sender, EventArgs e)
        {
            if (!int.TryParse(_numOfTestTableButtons.Text, out int num) || num <= 0 || num >= 20)
                return;
            var col = new ColumnDefinitionCollection();
            var row = new RowDefinitionCollection();
            double size = (Math.Min(_contentPage.Width, _contentPage.Height) / num) - 4;
            for (int i = 0; i < num; i++)
            {
                col.Add(new ColumnDefinition() { Width = size });
                row.Add(new RowDefinition() { Height = size });
            }
            _testTable.Children.Clear();
            _testTable.RowDefinitions = row;
            _testTable.ColumnDefinitions = col;
            for (int x = 0; x < num; x++)
                for (int y = 0; y < num; y++)
                {
                    var but = new Button() { BackgroundColor = Color.Coral };
                    Grid.SetColumn(but, x);
                    Grid.SetRow(but, y);
                    but.Clicked += But_Clicked;
                    _testTable.Children.Add(but);
                };
        }
        private void But_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(_numOfTestTableButtons.Text, out int num))
                    return;
                var button = sender as Button;
                int x = (map.GetLength(0) / 2) - (num / 2) + Grid.GetColumn(button);
                int y = (map.GetLength(1) / 2) - (num / 2) + Grid.GetRow(button);
                map[x, y] = !map[x, y];
                button.BackgroundColor = map[x, y] ? Color.LightGreen : Color.Coral;
            }
            catch (Exception ex)
            {
                DisplayAlert("Ошибка", ex.Message, "Ок");
            };
        }
    }
    
}
