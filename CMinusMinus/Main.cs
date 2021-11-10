using System.Collections;
using System.Collections.Generic;
using Lexer;
using Parser.LR.LR1;

namespace CMinusMinus {
	public partial class CMinusMinus {
		public CMinusMinus() {
			InitializeLexer();
			Parser = new CanonicalLRParser<NonterminalType, TokenType>(InitializeGrammar());
		}
	}
}