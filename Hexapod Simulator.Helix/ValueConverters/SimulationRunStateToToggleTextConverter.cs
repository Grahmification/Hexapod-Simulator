﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Hexapod_Simulator.Helix.ValueConverters
{
    /// <summary>
    /// Converts the actuator solution state to the color for actuator 3D visuals
    /// </summary>
    public class SimulationRunStateToToggleTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var tmp = (bool)value;

            if (tmp)
                return "Stop Simulation";
            else
                return "Start Simulation";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
