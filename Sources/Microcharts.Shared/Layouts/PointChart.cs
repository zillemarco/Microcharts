// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SkiaSharp;

    /// <summary>
    /// ![chart](../images/Point.png)
    /// 
    /// Point chart.
    /// </summary>
    public class PointChart : Chart
    {
        #region Properties

        public float PointSize { get; set; } = 14;

        public PointMode PointMode { get; set; } = PointMode.Circle;

        public byte PointAreaAlpha { get; set; } = 100;

        private float ValueRange => this.MaxValue - this.MinValue;

        #endregion

        #region Methods

        public float CalculateYOrigin(float itemHeight, float headerHeight)
        {
            if (this.MaxValue <= 0)
            {
                return headerHeight;
            } 

            if (this.MinValue > 0)
            {
                return headerHeight + itemHeight;
            }

            return headerHeight + ((this.MaxValue / this.ValueRange) * itemHeight);
        }

        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            var valueLabelSizes = this.MeasureValueLabels();
            var footerHeight = this.CalculateFooterHeight(valueLabelSizes);
            var headerHeight = this.CalculateHeaderHeight(valueLabelSizes);

            var yLabelsWidth = CalculateYLabelsWidth(height, headerHeight, footerHeight);
            var availableWidth = width - yLabelsWidth;

            var itemSize = this.CalculateItemSize(availableWidth, height, footerHeight, headerHeight);
            var origin = this.CalculateYOrigin(itemSize.Height, headerHeight);

            this.DrawGridLines(canvas, availableWidth, height, headerHeight, footerHeight, yLabelsWidth);
            this.DrawYLabels(canvas, height, headerHeight, footerHeight);

            foreach (var entries in this.EntriesCollection)
            {
                var entriesList = entries.ToList();
                var points = this.CalculatePoints(itemSize, origin, headerHeight, entriesList, yLabelsWidth);
                this.DrawPointAreas(canvas, points, origin, entriesList);
                this.DrawPoints(canvas, points, entriesList);
                this.DrawFooter(canvas, points, itemSize, height, footerHeight, entriesList);

                if (this.EntriesCollection.Count == 1)
                    this.DrawValueLabel(canvas, points, itemSize, height, valueLabelSizes);
            }
        }

        protected SKSize CalculateItemSize(int width, int height, float footerHeight, float headerHeight)
        {
            int total = this.EntriesCollection.Any() ? this.EntriesCollection.Max(x => x.Count()) : 5;

            var w = (width - ((total + 1) * this.Margin)) / total;
            var h = height - this.Margin - footerHeight - headerHeight;
            return new SKSize(w, h);
        }

        protected SKPoint[] CalculatePoints(SKSize itemSize, float origin, float headerHeight, List<Entry> entries, float xOffset)
        {
            var result = new List<SKPoint>();

            for (int i = 0; i < entries.Count(); i++)
            {
                var entry = entries.ElementAt(i);

                var x = this.Margin + xOffset + (itemSize.Width / 2) + (i * (itemSize.Width + this.Margin));
                var y = headerHeight + (((this.MaxValue - entry.Value) / this.ValueRange) * itemSize.Height);
                var point = new SKPoint(x, y);
                result.Add(point);
            }

            return result.ToArray();
        }

        protected void DrawFooter(SKCanvas canvas, SKPoint[] points, SKSize itemSize, int height, float footerHeight, List<Entry> entries)
        {
            this.DrawLabels(canvas, points, itemSize, height, footerHeight, entries);
        }

        protected void DrawLabels(SKCanvas canvas, SKPoint[] points, SKSize itemSize, int height, float footerHeight, List<Entry> entries)
        {
            for (int i = 0; i < entries.Count(); i++)
            {
                var entry = entries.ElementAt(i);
                var point = points[i];

                if (!string.IsNullOrEmpty(entry.Label))
                {
                    using (var paint = new SKPaint())
                    {
                        paint.Typeface = this.Typeface;
                        paint.TextEncoding = this.TextEncoding;
                        paint.TextSize = this.LabelTextSize;
                        paint.IsAntialias = true;
                        paint.Color = entry.TextColor;
                        paint.IsStroke = false;

                        var bounds = new SKRect();
                        var text = entry.Label;
                        paint.MeasureText(text, ref bounds);

                        //if (bounds.Width > itemSize.Width)
                        //{
                        //    text = text.Substring(0, Math.Min(3, text.Length));
                        //    paint.MeasureText(text, ref bounds);
                        //}

                        //if (bounds.Width > itemSize.Width)
                        //{
                        //    text = text.Substring(0, Math.Min(1, text.Length));
                        //    paint.MeasureText(text, ref bounds);
                        //}

                        canvas.DrawText(text, point.X - (bounds.Width / 2), height - this.Margin + (this.LabelTextSize / 2), paint);
                    }
                }
            }
        }

        protected void DrawYLabels(SKCanvas canvas, float height, float headerHeight, float footerHeight)
        {
            if (!this.ShowYLabels)
                return;

            float usableHeight = height - this.Margin - footerHeight - headerHeight;
            float gridLinesInterval = 40.0f;
            int linesCount = (int)Math.Floor(usableHeight / gridLinesInterval) + 1;
            float valueDelta = ValueRange / linesCount;

            if (!this.ShowYLabelOnAllRows)
            {
                float minY = height - (footerHeight + this.Margin + (float)Math.Round(0 * gridLinesInterval));
                float maxY = height - (footerHeight + this.Margin + (float)Math.Round((linesCount - 1) * gridLinesInterval));

                var minYLabel = $"{this.MinValue}{this.YUnitMeasure}";
                if (!string.IsNullOrEmpty(minYLabel))
                {
                    using (var paint = new SKPaint())
                    {
                        paint.Typeface = this.Typeface;
                        paint.TextEncoding = this.TextEncoding;
                        paint.TextSize = this.LabelTextSize;
                        paint.IsAntialias = true;
                        paint.Color = this.YLabelsColor;
                        paint.IsStroke = false;

                        var bounds = new SKRect();
                        var text = minYLabel;
                        paint.MeasureText(text, ref bounds);

                        canvas.DrawText(text, this.Margin, minY + (bounds.Height / 2), paint);
                    }
                }

                var maxYLabel = $"{this.MaxValue}{this.YUnitMeasure}";
                if (!string.IsNullOrEmpty(maxYLabel))
                {
                    using (var paint = new SKPaint())
                    {
                        paint.Typeface = this.Typeface;
                        paint.TextEncoding = this.TextEncoding;
                        paint.TextSize = this.LabelTextSize;
                        paint.IsAntialias = true;
                        paint.Color = this.YLabelsColor;
                        paint.IsStroke = false;

                        var bounds = new SKRect();
                        var text = maxYLabel;
                        paint.MeasureText(text, ref bounds);

                        canvas.DrawText(text, this.Margin, maxY + (bounds.Height / 2), paint);
                    }
                }
            }
            else
            {
                for (int i = 0; i < linesCount; i++)
                {
                    var value = Math.Round(this.MinValue + (valueDelta * i), 1);
                    var label = $"{value}{this.YUnitMeasure}";

                    using (var paint = new SKPaint())
                    {
                        float y = height - (footerHeight + this.Margin + (float)Math.Round(i * gridLinesInterval));

                        paint.Typeface = this.Typeface;
                        paint.TextEncoding = this.TextEncoding;
                        paint.TextSize = this.LabelTextSize;
                        paint.IsAntialias = true;
                        paint.Color = this.YLabelsColor;
                        paint.IsStroke = false;

                        var bounds = new SKRect();
                        paint.MeasureText(label, ref bounds);

                        canvas.DrawText(label, this.Margin, y + (bounds.Height / 2), paint);
                    }
                }
            }
        }

        protected void DrawGridLines(SKCanvas canvas, float width, float height, float headerHeight, float footerHeight, float xOffset)
        {
            float usableHeight = height - this.Margin - footerHeight - headerHeight;

            float gridLinesInterval = 40.0f;
            int linesCount = (int)Math.Floor(usableHeight / gridLinesInterval) + 1;
            float valueDelta = ValueRange / linesCount;

            SKColor color = new SKColor(120, 120, 120);

            for (int i = 0; i < linesCount; i++)
            {
                using (var paint = new SKPaint())
                {
                    float y = height - (footerHeight + this.Margin + (float)Math.Round(i * gridLinesInterval));

                    paint.Color = color;
                    canvas.DrawLine(this.Margin + xOffset, y, width - this.Margin, y, paint);
                }
            }
        }

        protected void DrawPoints(SKCanvas canvas, SKPoint[] points, List<Entry> entries)
        {
            if (points.Length > 0 && PointMode != PointMode.None)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    var entry = entries.ElementAt(i);
                    var point = points[i];
                    canvas.DrawPoint(point, entry.Color, this.PointSize, this.PointMode);
                }
            }
        }

        protected void DrawPointAreas(SKCanvas canvas, SKPoint[] points, float origin, List<Entry> entries)
        {
            if (points.Length > 0 && this.PointAreaAlpha > 0)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    var entry = entries.ElementAt(i);
                    var point = points[i];
                    var y = Math.Min(origin, point.Y);

                    using (var shader = SKShader.CreateLinearGradient(new SKPoint(0, origin), new SKPoint(0, point.Y), new[] { entry.Color.WithAlpha(this.PointAreaAlpha), entry.Color.WithAlpha((byte)(this.PointAreaAlpha / 3)) }, null, SKShaderTileMode.Clamp))
                    using (var paint = new SKPaint
                    {
                        Style = SKPaintStyle.Fill,
                        Color = entry.Color.WithAlpha(this.PointAreaAlpha),
                    })
                    {
                        paint.Shader = shader;
                        var height = Math.Max(2, Math.Abs(origin - point.Y));
                        canvas.DrawRect(SKRect.Create(point.X - (this.PointSize / 2), y, this.PointSize, height), paint);
                    }
                }
            }
        }

        protected void DrawValueLabel(SKCanvas canvas, SKPoint[] points, SKSize itemSize, float height, SKRect[] valueLabelSizes)
        {
            if (this.EntriesCollection.Count == 1)
            {
                var entries = this.EntriesCollection[0];
                if (points.Length > 0)
                {
                    for (int i = 0; i < points.Length; i++)
                    {
                        var entry = entries.ElementAt(i);
                        var point = points[i];
                        var isAbove = point.Y > (this.Margin + (itemSize.Height / 2));

                        if (!string.IsNullOrEmpty(entry.ValueLabel))
                        {
                            using (new SKAutoCanvasRestore(canvas))
                            {
                                using (var paint = new SKPaint())
                                {
                                    paint.Typeface = this.Typeface;
                                    paint.TextEncoding = this.TextEncoding;
                                    paint.TextSize = this.LabelTextSize;
                                    paint.FakeBoldText = true;
                                    paint.IsAntialias = true;
                                    paint.Color = entry.Color;
                                    paint.IsStroke = false;

                                    var bounds = new SKRect();
                                    var text = entry.ValueLabel;
                                    paint.MeasureText(text, ref bounds);

                                    canvas.RotateDegrees(90);
                                    canvas.Translate(this.Margin, -point.X + (bounds.Height / 2));

                                    canvas.DrawText(text, 0, 0, paint);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected float CalculateFooterHeight(SKRect[] valueLabelSizes)
        {
            var result = this.Margin;

            if (this.EntriesCollection.Count == 1)
            {
                if (this.EntriesCollection.Any(c => c.Any(e => !string.IsNullOrEmpty(e.Label))))
                    result += this.LabelTextSize + this.Margin;
            }

            return result;
        }

        protected float CalculateHeaderHeight(SKRect[] valueLabelSizes)
        {
            var result = this.Margin;

            if (this.EntriesCollection.Count == 1)
            {
                if (this.EntriesCollection.Any(c => c.Any()))
                {
                    var maxValueWidth = valueLabelSizes.Max(x => x.Width);
                    if (maxValueWidth > 0)
                        result += maxValueWidth + this.Margin;
                }
            }

            return result;
        }

        protected int CalculateYLabelsWidth(float height, float headerHeight, float footerHeight)
        {
            if (!this.ShowYLabels)
                return 0;

            int maxWidth = 0;

            if (!this.ShowYLabelOnAllRows)
            {
                using (var paint = new SKPaint())
                {
                    paint.TextSize = this.LabelTextSize;

                    var bounds = new SKRect();
                    paint.MeasureText($"{this.MinValue}{this.YUnitMeasure}", ref bounds);
                    maxWidth = Math.Max(maxWidth, (int)Math.Round(bounds.Width) + 10);
                }

                using (var paint = new SKPaint())
                {
                    paint.Typeface = this.Typeface;
                    paint.TextEncoding = this.TextEncoding;
                    paint.TextSize = this.LabelTextSize;

                    var bounds = new SKRect();
                    paint.MeasureText($"{this.MaxValue}{this.YUnitMeasure}", ref bounds);
                    maxWidth = Math.Max(maxWidth, (int)Math.Round(bounds.Width) + 10);
                }
            }
            else
            {
                float gridLinesInterval = 40.0f;
                float usableHeight = height - this.Margin - footerHeight - headerHeight;
                int linesCount = (int)Math.Floor(usableHeight / gridLinesInterval) + 1;
                float valueDelta = ValueRange / linesCount;

                for (int i = 0; i < linesCount; i++)
                {
                    using (var paint = new SKPaint())
                    {
                        paint.Typeface = this.Typeface;
                        paint.TextEncoding = this.TextEncoding;
                        paint.TextSize = this.LabelTextSize;

                        var value = Math.Round(this.MinValue + (valueDelta * i), 1);
                        var bounds = new SKRect();
                        paint.MeasureText($"{value}{this.YUnitMeasure}", ref bounds);
                        maxWidth = Math.Max(maxWidth, (int)Math.Round(bounds.Width) + 10);
                    }
                }
            }

            return maxWidth;
        }

        protected SKRect[] MeasureValueLabels()
        {
            if(this.EntriesCollection.Count == 1)
            {
                using (var paint = new SKPaint())
                {
                    paint.Typeface = this.Typeface;
                    paint.TextEncoding = this.TextEncoding;
                    paint.TextSize = this.LabelTextSize;
                    return this.EntriesCollection[0].Select(e =>
                    {
                        if (string.IsNullOrEmpty(e.ValueLabel))
                        {
                            return SKRect.Empty;
                        }

                        var bounds = new SKRect();
                        var text = e.ValueLabel;
                        paint.MeasureText(text, ref bounds);
                        return bounds;
                    }).ToArray();
                }
            }

            return new SKRect[0];
        }

        #endregion
    }
}
