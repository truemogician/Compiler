using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using Lexer;
using Microsoft.Win32;
using Parser;

#pragma warning disable 8618
namespace Visualizer {
	using CMM = CMinusMinus.CMinusMinus;

	/// <summary>
	///     Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow {
		public MainWindow() {
			InitializeComponent();
			CodeTextBox.Document = new FlowDocument(CodeParagraph);
			var cmm = CMinusMinusLoader.LoadCMinusMinus();
			if (cmm is null) {
				MessageBox.Show("加载CMinusMinus过程中出错", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
				Close();
				return;
			}
			CMinusMinus = cmm;
		}

		public CMM CMinusMinus { get; }

		private Paragraph CodeParagraph { get; } = new();

		private string Code => new TextRange(CodeTextBox.Document.ContentStart, CodeTextBox.Document.ContentEnd).Text;

		private void OpenSourceButtonClicked(object sender, RoutedEventArgs args) {
			var dialog = new OpenFileDialog {CheckFileExists = true, Title = "选择源文件", Filter = "C--源文件（*.cmm）|*.cmm"};
			if (dialog.ShowDialog() != true)
				return;
			var code = File.ReadAllText(dialog.FileName);
			CodeParagraph.Inlines.Clear();
			CodeParagraph.Inlines.Add(code);
		}

		private void SaveSourceButtonClicked(object sender, RoutedEventArgs args) {
			var dialog = new SaveFileDialog {AddExtension = true, Title = "保存源文件", Filter = "C--源文件（*.cmm）|*.cmm"};
			if (dialog.ShowDialog() != true)
				return;
			File.WriteAllText(dialog.FileName, Code);
		}

		private void ParseButtonClicked(object sender, RoutedEventArgs args) {
			XmlTextBox.Text = "";
			try {
				var tree = CMinusMinus.Parse(Code);
				XmlTextBox.Text = tree.ToString();
			}
			catch (LexemeNotMatchedException ex) {
				MessageBox.Show($"词法分析器失配：{ex.Position}", "分析失败", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (TerminalNotMatchedException ex) {
				MessageBox.Show($"语法分析器失配：没有终结符规则匹配词{ex.NotMatchedLexeme}", "分析失败", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			catch (NotRecognizedException ex) {
				MessageBox.Show($"语法分析器未识别，位置：{ex.Tokens?.ToArray()[ex.Position!.Value]}", "分析失败", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}