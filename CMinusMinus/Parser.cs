using System;
using Parser;
using Parser.LR.LR1;

namespace CMinusMinus {
	public partial class CMinusMinus {
		public CanonicalLRParser<NonterminalType, TokenType> Parser { get; }

		private Grammar<NonterminalType, TokenType> InitializeGrammar() => throw new NotImplementedException();
	}

	public enum NonterminalType : byte { }
}