using System;
using Parser;

namespace CMinusMinus {
	using CLRParser = Parser.LR.CLR.Parser;

	public partial class CMinusMinus {
		public CLRParser Parser { get; }

		private Grammar InitializeGrammar() => throw new NotImplementedException();
	}

	public enum NonterminalType : byte { }
}