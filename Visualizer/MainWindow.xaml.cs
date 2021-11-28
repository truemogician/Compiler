using System.Windows;

#pragma warning disable 8618
namespace Visualizer {
	using CMM = CMinusMinus.CMinusMinus;

	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {
		public MainWindow() {
			InitializeComponent();
			var cmm = CMinusMinusLoader.LoadCMinusMinus();
			if (cmm is null) {
				MessageBox.Show("加载CMinusMinus过程中出错", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				Close();
				return;
			}
			CMinusMinus = cmm;
		}

		public CMM CMinusMinus { get; }
	}
}