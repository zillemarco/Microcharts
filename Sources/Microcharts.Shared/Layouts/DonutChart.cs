// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SkiaSharp;

    /// <summary>
    /// ![chart](../images/Donut.png)
    /// 
    /// A donut chart.
    /// </summary>
    public class DonutChart : Chart
    {
        #region Properties

        /// <summary>
        /// Gets or sets the radius of the hole in the center of the chart.
        /// </summary>
        /// <value>The hole radius.</value>
        public float HoleRadius { get; set; } = 0;

        #endregion

        #region Methods

        public override void DrawContent(SKCanvas canvas, int width, int height)
        {
            this.DrawCaption(canvas, width, height);

            if (!this.EntriesCollection.Any())
                return;

            using (new SKAutoCanvasRestore(canvas))
            {
                var entries = this.EntriesCollection[0];

                canvas.Translate(width / 2, height / 2);
                var sumValue = entries.Sum(x => Math.Abs(x.Value));
                var radius = (Math.Min(width, height) - (2 * Margin)) / 2;

                var start = 0.0f;
                for (int i = 0; i < entries.Count(); i++)
                {
                    var entry = entries.ElementAt(i);
                    var end = start + (Math.Abs(entry.Value) / sumValue);

                    // Sector
                    var path = RadialHelpers.CreateSectorPath(start, end, radius, radius * this.HoleRadius);
                    using (var paint = new SKPaint
                    {
                        Style = SKPaintStyle.Fill,
                        Color = entry.Color,
                        IsAntialias = true,
                    })
                    {
                        canvas.DrawPath(path, paint);
                    }

                    start = end;
                }
            }
        }

        private void DrawCaption(SKCanvas canvas, int width, int height)
        {
            if (!this.EntriesCollection.Any())
                return;

            var entries = this.EntriesCollection[0];

            var sumValue = entries.Sum(x => Math.Abs(x.Value));
            var rightValues = new List<Entry>();
            var leftValues = new List<Entry>();

            int i = 0;
            var current = 0.0f;
            while (i < entries.Count() && (current < sumValue / 2))
            {
                var entry = entries.ElementAt(i);
                rightValues.Add(entry);
                current += Math.Abs(entry.Value);
                i++;
            }

            while (i < entries.Count())
            {
                var entry = entries.ElementAt(i);
                leftValues.Add(entry);
                i++;
            }

            leftValues.Reverse();

            this.DrawCaptionElements(canvas, width, height, rightValues, false);
            this.DrawCaptionElements(canvas, width, height, leftValues, true);
        }

        #endregion
    }
}