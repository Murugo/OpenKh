using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static OpenKh.Tools.Common.DependencyPropertyUtils;

namespace OpenKh.Tools.Common.Controls
{
    /// <summary>
    /// Interaction logic for Timeline.xaml
    /// </summary>
    public partial class Timeline : UserControl
    {
        public class TimelineEntryCollection : CollectionBase
        {
            public TimelineEntry this[int Index] => (TimelineEntry)List[Index];
            public bool Contains(TimelineEntry itemType) => List.Contains(itemType);
            public int Add(TimelineEntry itemType) => List.Add(itemType);
            public void Remove(TimelineEntry itemType) => List.Remove(itemType);
            public void Insert(int index, TimelineEntry itemType) => List.Insert(index, itemType);
            public int IndexOf(TimelineEntry itemType) => List.IndexOf(itemType);
        }

        public static readonly DependencyProperty ValueProperty =
            GetDependencyProperty<Timeline, double>(nameof(Value), (o, x) => o.SetValue(x));

        public static readonly DependencyProperty MaxValueProperty =
            GetDependencyProperty<Timeline, double>(nameof(MaxValue), (o, x) => o.SetMaxValue(x));

        public static readonly DependencyProperty ItemsProperty =
            GetDependencyProperty<Timeline, TimelineEntryCollection>(nameof(Items), (o, x) => { });

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TimelineEntryCollection Items
        {
            get => (TimelineEntryCollection)GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public Timeline()
        {
            InitializeComponent();
            Items = new TimelineEntryCollection();
        }

        private void SetValue(double x)
        {
            UpdateCursorPosition();
        }

        private void SetMaxValue(double x)
        {
            UpdateCursorPosition();
        }

        private void UpdateCursorPosition()
        {
            Canvas.SetLeft(cursor, Math.Min(Value, MaxValue) / MaxValue * ActualWidth);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateCursorPosition();
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e) =>
            HandleMouse(sender, e);

        private void UserControl_MouseMove(object sender, MouseEventArgs e) =>
            HandleMouse(sender, e);

        private void HandleMouse(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePosition = e.GetPosition(sender as FrameworkElement);
                Value = mousePosition.X / ActualWidth * MaxValue;
            }
        }
    }
}
