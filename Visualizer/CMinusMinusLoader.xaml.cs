using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using CMinusMinus;
using Microsoft.Win32;

namespace Visualizer {
	using CMM = CMinusMinus.CMinusMinus;

	/// <summary>
	///     Interaction logic for Startup.xaml
	/// </summary>
	public partial class CMinusMinusLoader {
		private CMM? _result;

		public CMinusMinusLoader() => InitializeComponent();

		public static CMM? LoadCMinusMinus() {
			var dialog = new CMinusMinusLoader();
			dialog.ShowDialog();
			return dialog._result;
		}

		private static Task<CMM>? LoadCompiled() {
			var dialog = new OpenFileDialog {Title = "选择GLR分析表", Filter = "GLR分析表文件（*.glr.ptb)|*.glr.ptb"};
			bool? result = dialog.ShowDialog();
			return result != true ? null : Task<CMM>.Factory.StartNew(() => new CMM(ParserAlgorithm.GeneralizedLR, dialog.FileName));
		}

		private Task<CMM>? CreateNew() {
			if (MessageBox.Show("从文法创建分析器耗时较长，确定要继续么？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
				return null;
			return Task<CMM>.Factory.StartNew(
				() => {
					var cmm = new CMM(ParserAlgorithm.GeneralizedLR);
					var table = cmm.GLRParser!.ParsingTable;
					table.CompleteItemSetsCalculation += (_, args) => Dispatcher.Invoke(
						() => {
							ProgressBar.IsIndeterminate = false;
							ProgressBar.Value = 0;
							ProgressBar.Maximum = args.Value!.Count;
						}
					);
					var timer = new Timer(1000) {AutoReset = true};
					timer.Elapsed += (_, _) => Dispatcher.Invoke(() => ProgressBar.Value = table.ActionTable!.RawTable.Count);
					table.StartTableCalculation += (_, _) => timer.Start();
					table.CompleteTableCalculation += (_, _) => timer.Stop();
					cmm.InitializeParser(ParserAlgorithm.GeneralizedLR);
					return cmm;
				}
			);
		}

		private void ClickButton(object sender, RoutedEventArgs e) {
			var task = ReferenceEquals(sender, LoadTableButton) ? LoadCompiled() : CreateNew();
			if (task is null)
				return;
			LoadTableButton.Visibility = Visibility.Collapsed;
			CreateNewButton.Visibility = Visibility.Collapsed;
			ProgressBar.Visibility = Visibility.Visible;
			ProgressBar.IsIndeterminate = true;
			task.ContinueWith(
				tsk => {
					if (tsk.IsCompletedSuccessfully)
						_result = tsk.Result;
					Dispatcher.Invoke(Close);
				}
			);
		}
	}
}