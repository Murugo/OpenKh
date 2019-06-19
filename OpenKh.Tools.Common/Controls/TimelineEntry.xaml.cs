using System;
using System.Windows;
using System.Windows.Controls;
using Xe.Tools.Wpf.Extensions;
using static OpenKh.Tools.Common.DependencyPropertyUtils;

namespace OpenKh.Tools.Common.Controls
{
    /// <summary>
    /// Interaction logic for TimelineEntry.xaml
    /// </summary>
    public partial class TimelineEntry : UserControl
    {
        public static readonly DependencyProperty FrameStartProperty =
            GetDependencyProperty<TimelineEntry, double>(nameof(FrameStart), (o, x) => o.InvalidateFrameStartEnd());

        public static readonly DependencyProperty FrameEndProperty =
            GetDependencyProperty<TimelineEntry, double>(nameof(FrameEnd), (o, x) => o.InvalidateFrameStartEnd());

        public double FrameStart
        {
            get => (double)GetValue(FrameStartProperty);
            set => SetValue(FrameStartProperty, value);
        }

        public double FrameEnd
        {
            get => (double)GetValue(FrameEndProperty);
            set => SetValue(FrameEndProperty, value);
        }

        public Thickness Margins
        {
            get
            {
                var width = ActualWidth;
                var left = FrameStart / width;
                var right = left + FrameEnd / width;
                return new Thickness(FrameStart, 0, 0, 0);
            }
        }

        public TimelineEntry()
        {
            InitializeComponent();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            InvalidateFrameStartEnd();
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void InvalidateFrameStartEnd()
        {
            var actualWidth = ActualWidth;
            if (actualWidth > 0)
            {
                var timeline = this.GetParent<Timeline>(x => true);
                var maxValue = timeline?.MaxValue ?? 500;

                var left = Math.Max(0, FrameStart) * maxValue / actualWidth;
                var width = Math.Min(maxValue, FrameEnd) * actualWidth / maxValue;
                if (double.IsNaN(width))
                    return;

                var newMargin = new Thickness(left, 0, actualWidth - width, 0);
                if (Margin != newMargin)
                    Margin = newMargin;
            }
        }
    }
}
