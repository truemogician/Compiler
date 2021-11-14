namespace CMinusMinus {
	using CLRParser = Parser.LR.CLR.Parser;

	public partial class CMinusMinus {
		public CMinusMinus() {
			InitializeLexer();
			Parser = new CLRParser(InitializeGrammar());
		}
	}
}