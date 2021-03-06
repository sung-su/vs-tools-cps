﻿/*
 * Copyright 2017 (c) Samsung Electronics Co., Ltd  All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * 	http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using NetCore.Profiler.Cperf.Core.Model;

namespace NetCore.Profiler.Extension.UI.Summary
{
    /// <summary>
    /// Session Summary CPU Chart
    /// </summary>
    public partial class SessionSummaryCpuChart
    {
        /// <summary>
        /// Series collection for the chart control
        /// </summary>
        public SeriesCollection SeriesCollection { get; set; }

        private static readonly SolidColorBrush PausedSectionBrush = new SolidColorBrush(Colors.DarkGray) { Opacity = 0.4 };

        private const string CpuSeriesTitle = "CPU (%)";

        private static readonly SolidColorBrush CpuSeriesStrokeBrush = Brushes.Blue;

        private static readonly SolidColorBrush CpuSeriesFillBrush = Brushes.Transparent;

        private const string MemorySeriesTitle = "Memory (Gb)";

        private static readonly SolidColorBrush MemorySeriesStrokeBrush = Brushes.Red;

        private static readonly SolidColorBrush MemorySeriesFillBrush = Brushes.Transparent;

        public SessionSummaryCpuChart()
        {
            DataContext = this;

            SeriesCollection = new SeriesCollection()
            {
                new LineSeries(Mappers.Xy<ChartData>()
                    .X(item => item.TimeSeconds)
                    .Y(item => item.Cpu))
                {
                    Stroke = CpuSeriesStrokeBrush,
                    Fill = CpuSeriesFillBrush,
                    PointGeometry = null,
                    Title = CpuSeriesTitle,
                    ScalesYAt = 0,
                    Values = new ChartValues<ChartData>()
                },

                new LineSeries(Mappers.Xy<ChartData>()
                    .X(item => item.TimeSeconds)
                    .Y(item => item.Mem))
                {
                    Stroke = MemorySeriesStrokeBrush,
                    Fill = MemorySeriesFillBrush,
                    PointGeometry = null,
                    Title = MemorySeriesTitle,
                    ScalesYAt = 1,
                    Values = new ChartValues<ChartData>()
                }
            };

            InitializeComponent();

            InitialiseChart();
        }

        private void InitialiseChart()
        {
            LiveTimeline.Zoom = ZoomingOptions.None;
            LiveTimeline.DisableAnimations = true;
        }

        public void LoadChartData(List<SysInfoItem> sysInfoItems)
        {
            LiveTimeline.AxisX[0].Sections.Clear();

            var builder = new ChartDataBuilder();
            builder.BuildChartData(sysInfoItems);

            var values = new ChartValues<ChartData>(builder.ChartValues);
            SeriesCollection[0].Values = values;
            SeriesCollection[1].Values = values;

            LiveTimeline.AxisX[0].MaxValue = builder.TimeMaxValue;
            LiveTimeline.AxisY[0].MaxValue = builder.CpuMaxValue;
            LiveTimeline.AxisY[1].MaxValue = builder.MemoryMaxValue;

            LiveTimeline.AxisX[0].Sections.AddRange(builder.Sections);
        }

        /// <summary>
        /// Part of SysInfo information obtained from profiler to be presented on the chart
        /// </summary>
        private class ChartData
        {
            /// <summary>
            /// Gets or sets a value indicating whether tracing is performed at this moment of time
            /// </summary>
            public bool Running { get; set; } = true;

            /// <summary>
            /// Gets or sets time since profiling start in seconds
            /// </summary>
            public double TimeSeconds { get; set; }

            /// <summary>
            /// Gets or sets CPU usage in percents
            /// </summary>
            public double Cpu { get; set; }

            /// <summary>
            /// Gets or sets memory usage in GB
            /// </summary>
            public double Mem { get; set; }
        }

        private class ChartDataBuilder
        {
            public double CpuMaxValue;

            public double MemoryMaxValue;

            public double TimeMaxValue;

            public readonly List<ChartData> ChartValues = new List<ChartData>();

            private double _startTimeSeconds;

            /// <summary>
            /// Current ("Paused") Chart section
            /// </summary>
            private AxisSection _currentSection;

            public readonly List<AxisSection> Sections = new List<AxisSection>();

            /// <summary>
            /// Previous SysInfo value. Used for calculating CPU utilization
            /// </summary>
            private SysInfoItem _previousSysInfo;

            /// <summary>
            /// Last added chart data. Used to track Pause/Resume sessions
            /// </summary>
            private ChartData _previousChartData;

            public void BuildChartData(List<SysInfoItem> sysInfoItems)
            {
                foreach (var sysInfoItem in sysInfoItems)
                {
                    ChartData chartData = CreateChartData(sysInfoItem);
                    if (chartData != null)
                    {
                        AddDataToChart(chartData);
                        _previousSysInfo = sysInfoItem;
                        _previousChartData = chartData;
                    }
                }
            }

            private void AddDataToChart(ChartData chartData)
            {
                if (_currentSection != null)
                {
                    _currentSection.SectionWidth = chartData.TimeSeconds - _currentSection.Value;
                }

                if (_previousChartData != null)
                {
                    if (_previousChartData.Running && !chartData.Running)
                    {
                        _currentSection = new AxisSection()
                        {
                            Value = _previousChartData.TimeSeconds,
                            SectionWidth = chartData.TimeSeconds - _previousChartData.TimeSeconds,
                            Fill = PausedSectionBrush
                        };

                        Sections.Add(_currentSection);
                    }
                    else if (!_previousChartData.Running && chartData.Running)
                    {
                        _currentSection = null;
                    }
                }

                ChartValues.Add(chartData);
            }

            private ChartData CreateChartData(SysInfoItem sii)
            {
                ChartData chartData;
                if (_startTimeSeconds == 0)
                {
                    _startTimeSeconds = sii.TimeSeconds;
                    CpuMaxValue = sii.CoreNum * 100;
                    MemoryMaxValue = Math.Ceiling(((double)sii.MemTotal) / 1024 / 1024);

                    chartData = new ChartData();
                }
                else
                {
                    if (sii.TimeSeconds < _startTimeSeconds)
                    {
                        return null;
                    }
                    double time = (sii.TimeSeconds - _startTimeSeconds);
                    TimeMaxValue = Math.Max(time, TimeMaxValue);
                    chartData = new ChartData()
                    {
                        Running = ProfilerStatusToBool(sii.ProfilerStatus),
                        TimeSeconds = time,
                        Mem = Math.Round(((double)sii.MemLoad) / 1024 / 1024, 2),
                    };
                    if (_previousSysInfo != null)
                    {
                        double timeDelta = (sii.TimeSeconds - _previousSysInfo.TimeSeconds);
                        if (timeDelta >= 0.001)
                        {
                            chartData.Cpu = (sii.UserSys - _previousSysInfo.UserSys) / timeDelta / sii.CoreNum;
                        }
                    }
                }

                return chartData;
            }

            private static bool ProfilerStatusToBool(string status)
            {
                switch (status)
                {
                    case "W":
                    case "Waiting":
                        return false;
                    case "R":
                    case "Running":
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}
