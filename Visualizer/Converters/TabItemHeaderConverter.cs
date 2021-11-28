using System;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;

namespace Visualizer.Converters {
	public class TabItemHeaderConverter : IMultiValueConverter {
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
			var tabControl = (values[0] as TabControl)!;
			double width = tabControl.ActualWidth / tabControl.Items.Count;
			return width <= 1 ? 0 : width - 2;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();
	}
}