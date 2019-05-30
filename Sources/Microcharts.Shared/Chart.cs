// Copyright (c) Aloïs DENIEL. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microcharts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SkiaSharp;

    public abstract class Chart
    {
        #region Properties

        /// <summary>
        /// Gets or sets the global margin.
        /// </summary>
        /// <value>The margin.</value>
        public float Margin { get; set; } = 20;

        /// <summary>
        /// Gets or sets the text size of the labels.
        /// </summary>
        /// <value>The size of the label text.</value>
        public float LabelTextSize { get; set; } = 16;

        /// <summary>
        /// Gets or sets the color of the chart background.
        /// </summary>
        /// <value>The color of the background.</value>
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets or sets the data entries.
        /// </summary>
        /// <value>The entries.</value>
        public IList<IEnumerable<Entry>> EntriesCollection { get; set; }

        public bool ShowYLabels { get; set; } = true;
        public bool ShowYLabelOnAllRows { get; set; } = true;
        public string YUnitMeasure { get; set; } = "°C";
        public SKColor YLabelsColor { get; set; } = SKColors.Gray;
        public int YLinesOffset { get; set; } = 10;

        public SKTypeface Typeface { get; set; } = null;
        public SKTextEncoding TextEncoding { get; set; } = SKTextEncoding.Utf8;

        /// <summary>
        /// Gets or sets the minimum value from entries. If not defined, it will be the minimum between zero and the 
        /// minimal entry value.
        /// </summary>
        /// <value>The minimum value.</value>
        public float MinValue
        {
            get
            {
                if (!this.EntriesCollection.Any())
                    return 0;

                float? minValueOfCollections = null;

                foreach (var entries in this.EntriesCollection)
                {
                    if (entries.Any())
                    {
                        if (minValueOfCollections == null)
                            minValueOfCollections = entries.Min(x => x.Value);
                        else
                            minValueOfCollections = Math.Min(minValueOfCollections.Value, entries.Min(x => x.Value));
                    }
                }

                if (minValueOfCollections == null)
                    return 0;

                if (this.InternalMinValue == null)
                    return minValueOfCollections.Value;

                return Math.Min(this.InternalMinValue.Value, minValueOfCollections.Value);
            }

            set => this.InternalMinValue = value;
        }

        /// <summary>
        /// Gets or sets the maximum value from entries. If not defined, it will be the maximum between zero and the 
        /// maximum entry value.
        /// </summary>
        /// <value>The minimum value.</value>
        public float MaxValue
        {
            get
            {
                if (!this.EntriesCollection.Any())
                    return 0;

                float? maxValueOfCollections = null;

                foreach (var entries in this.EntriesCollection)
                {
                    if (entries.Any())
                    {
                        if (maxValueOfCollections == null)
                            maxValueOfCollections = entries.Max(x => x.Value);
                        else
                            maxValueOfCollections = Math.Max(maxValueOfCollections.Value, entries.Max(x => x.Value));
                    }
                }

                if (maxValueOfCollections == null)
                    return 0;

                if (this.InternalMaxValue == null)
                    return maxValueOfCollections.Value;

                return Math.Max(this.InternalMaxValue.Value, maxValueOfCollections.Value);
            }

            set => this.InternalMaxValue = value;
        }

        /// <summary>
        /// Gets or sets the internal minimum value (that can be null).
        /// </summary>
        /// <value>The internal minimum value.</value>
        protected float? InternalMinValue { get; set; }

        /// <summary>
        /// Gets or sets the internal max value (that can be null).
        /// </summary>
        /// <value>The internal max value.</value>
        protected float? InternalMaxValue { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Draw the  graph onto the specified canvas.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public void Draw(SKCanvas canvas, int width, int height)
        {
            canvas.Clear(this.BackgroundColor);

            this.DrawContent(canvas, width, height);
        }

        protected float ComputeYLabelHeight()
        {
            using (var paint = new SKPaint())
            {
                paint.Typeface = this.Typeface;
                paint.TextEncoding = this.TextEncoding;
                paint.TextSize = this.LabelTextSize;
                paint.IsStroke = false;

                var bounds = new SKRect();
                var text = "0";
                paint.MeasureText(text, ref bounds);

                return bounds.Height + YLinesOffset;
            }
        }

        /// <summary>
        /// Draws the chart content.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public abstract void DrawContent(SKCanvas canvas, int width, int height);

        /// <summary>
        /// Draws caption elements on the right or left side of the chart.
        /// </summary>
        /// <param name="canvas">The canvas.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="entries">The entries.</param>
        /// <param name="isLeft">If set to <c>true</c> is left.</param>
        protected void DrawCaptionElements(SKCanvas canvas, int width, int height, List<Entry> entries, bool isLeft)
        {
            var margin = 2 * this.Margin;
            var availableHeight = height - (2 * margin);
            var x = isLeft ? this.Margin : (width - this.Margin - this.LabelTextSize);
            var ySpace = (availableHeight - this.LabelTextSize) / ((entries.Count <= 1) ? 1 : entries.Count - 1);

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries.ElementAt(i);
                var y = margin + (i * ySpace);

                if (entries.Count <= 1)
                    y += (availableHeight - this.LabelTextSize) / 2;

                var hasLabel = !string.IsNullOrEmpty(entry.Label);
                var hasValueLabel = !string.IsNullOrEmpty(entry.ValueLabel);

                if (hasLabel || hasValueLabel)
                {
                    var hasOffset = hasLabel && hasValueLabel;
                    var captionMargin = this.LabelTextSize * 0.60f;
                    var space = hasOffset ? captionMargin : 0;
                    var captionX = isLeft ? this.Margin : width - this.Margin - this.LabelTextSize;

                    using (var paint = new SKPaint
                    {
                        Style = SKPaintStyle.Fill,
                        Color = entry.Color,
                    })
                    {
                        var rect = SKRect.Create(captionX, y, this.LabelTextSize, this.LabelTextSize);
                        canvas.DrawRect(rect, paint);
                    }

                    if (isLeft)
                    {
                        captionX += this.LabelTextSize + captionMargin;
                    }
                    else
                    {
                        captionX -= captionMargin;
                    }

                    canvas.DrawCaptionLabels(entry.Label, entry.TextColor, entry.ValueLabel, entry.Color, this.LabelTextSize, new SKPoint(captionX, y + (this.LabelTextSize / 2)), isLeft ? SKTextAlign.Left : SKTextAlign.Right);
                }
            }
        }

        #endregion
    }
}