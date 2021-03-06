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

namespace NetCore.Profiler.Cperf.Core.Model
{
    /// <summary>
    /// A <see cref="DataContainer"/> data model class for a CPU utilization history record. The source of the
    /// data is %Core %Profiler log. These records are stored in <see cref="CpuUtilizationHistory"/> objects.
    /// </summary>
    public class CpuUtilization : ITimeStamped
    {
        /// <summary>
        /// A timestamp at which utilization was measured
        /// </summary>
        public ulong TimeMilliseconds { get; set; }

        /// <summary>
        /// CPU utilization in percents
        /// </summary>
        public double Utilization { get; set; }

        /// <summary>
        /// <code>true</code> if profiling was suspended at this moment
        /// </summary>
        public bool ProfilingWasPaused { get; set; }

        /// <summary>
        /// <code>true</code> if profiling was resumed at this moment
        /// </summary>
        public bool ProfilingWasResumed { get; set; }
    }
}
