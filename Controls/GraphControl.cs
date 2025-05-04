using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinUI;
using LiveChartsCore;
using System.Collections.Generic;
using System.ComponentModel;
using System;

namespace PowerTaskMan.Controls
{
    public sealed class GraphControl : Control
    {
        public GraphControl()
        {
            this.DefaultStyleKey = typeof(GraphControl);
        }

        public static readonly DependencyProperty GraphSeriesProperty = DependencyProperty.Register(
            nameof(GraphSeries),
            typeof(IEnumerable<ISeries>),
            typeof(GraphControl),
            new PropertyMetadata(null, OnGraphSeriesChanged));

        public IEnumerable<ISeries> GraphSeries
        {
            get => (IEnumerable<ISeries>)GetValue(GraphSeriesProperty);
            set => SetValue(GraphSeriesProperty, value);
        }

        public static readonly DependencyProperty XAxesProperty = DependencyProperty.Register(
            nameof(XAxes),
            typeof(IEnumerable<Axis>),
            typeof(GraphControl),
            new PropertyMetadata(null, OnAxesChanged));

        public IEnumerable<Axis> XAxes
        {
            get => (IEnumerable<Axis>)GetValue(XAxesProperty);
            set => SetValue(XAxesProperty, value);
        }

        public static readonly DependencyProperty YAxesProperty = DependencyProperty.Register(
            nameof(YAxes),
            typeof(IEnumerable<Axis>),
            typeof(GraphControl),
            new PropertyMetadata(null, OnAxesChanged));

        public IEnumerable<Axis> YAxes
        {
            get => (IEnumerable<Axis>)GetValue(YAxesProperty);
            set => SetValue(YAxesProperty, value);
        }

        public static readonly DependencyProperty DataLabelProperty = DependencyProperty.Register(
            nameof(DataLabel),
            typeof(string),
            typeof(GraphControl),
            new PropertyMetadata(null, OnDataLabelChanged));

        public string DataLabel
        {
            get => (string)GetValue(DataLabelProperty);
            set => SetValue(DataLabelProperty, value);
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(GraphControl),
            new PropertyMetadata(null, (DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            {
                var gc = d as GraphControl;
                gc.Title = e.NewValue?.ToString() ?? "";
            }));

        public string Title { get; set; } = "Graph";

        // New Dependency Property for Easing Function
        public static readonly DependencyProperty EasingFunctionProperty = DependencyProperty.Register(
            nameof(EasingFunction),
            typeof(Func<float, float>),
            typeof(GraphControl),
            new PropertyMetadata(null, OnEasingFunctionChanged));

        public Func<float, float> EasingFunction
        {
            get => (Func<float, float>)GetValue(EasingFunctionProperty);
            set => SetValue(EasingFunctionProperty, value);
        }


        private static void OnGraphSeriesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GraphControl control && e.NewValue is IEnumerable<ISeries> series)
            {
                if (e.OldValue is INotifyPropertyChanged oldSeries)
                {
                    oldSeries.PropertyChanged -= control.OnSeriesPropertyChanged;
                }
                if (e.NewValue is INotifyPropertyChanged newSeries)
                {
                    newSeries.PropertyChanged += control.OnSeriesPropertyChanged;
                }
                control.UpdateChart(series);
            }
        }

        private static void OnAxesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GraphControl control)
            {
                control.UpdateAxes();
            }
        }

        private void OnSeriesPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_chart != null && GraphSeries != null)
            {
                _chart.Series = GraphSeries;
            }
        }

        private static void OnDataLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var gc = d as GraphControl;
            gc.SetDataLabelText(e.NewValue?.ToString() ?? "");
        }

        private void SetDataLabelText(string v)
        {
            if(_dataLabel != null)
            {
                _dataLabel.Text = v;
            }
        }

        private static void OnEasingFunctionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GraphControl control && control._chart != null)
            {
                // Apply the new easing function to the chart (if applicable)
                control._chart.EasingFunction = control.EasingFunction;
            }
        }

        private CartesianChart? _chart;
        private TextBlock? _dataLabel;
        private TextBlock? _title;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _chart = GetTemplateChild("PART_Chart") as CartesianChart;
            if (_chart != null)
            {
                if (GraphSeries != null)
                {
                    _chart.Series = GraphSeries;
                }
                if (XAxes != null)
                {
                    _chart.XAxes = XAxes;
                }
                if (YAxes != null)
                {
                    _chart.YAxes = YAxes;
                }

                _chart.EasingFunction = EasingFunction;
                
            }
            else
            {
                throw new System.Exception("Very Bad");
            }

            _dataLabel = GetTemplateChild("DataLabelTextBlock") as TextBlock;
            if (_dataLabel != null)
            {
                _dataLabel.Text = DataLabel;
            }

            _title = GetTemplateChild("ChartTitleTextBlock") as TextBlock;
            if(_title != null)
            {
                _title.Text = Title;
            }
        }

        private void UpdateChart(IEnumerable<ISeries> series)
        {
            if (_chart != null)
            {
                _chart.Series = series;
            }
        }

        private void UpdateAxes()
        {
            if (_chart != null)
            {
                _chart.XAxes = XAxes;
                _chart.YAxes = YAxes;
            }
        }
    }
}



