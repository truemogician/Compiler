using System.Text.RegularExpressions;
using Parser;

namespace CMinusMinus {
	using RSF = RegularSentenceForm;

	public partial class CMinusMinus {
		private Grammar InitializeGrammar() {
			var grammar = new Grammar(NonterminalType.MainType);
			var signedModifier = new Terminal(Lexicon[TokenType.Keyword], new Regex(@"(?:un)?signed"));
			var charTerminal = new Terminal(Lexicon[TokenType.Keyword], "char");
			var intTerminal = new Terminal(Lexicon[TokenType.Keyword], "int");
			var floatTerminal = new Terminal(Lexicon[TokenType.Keyword], "float");
			var doubleTerminal = new Terminal(Lexicon[TokenType.Keyword], "double");
			var shortTerminal = new Terminal(Lexicon[TokenType.Keyword], "short");
			var longTerminal = new Terminal(Lexicon[TokenType.Keyword], "long");
			grammar.AddProductionRule(NonterminalType.MainType, ((RSF)signedModifier * (0, 1) + ((RSF)charTerminal | intTerminal | (((RSF)shortTerminal | ((RSF)longTerminal * (1, 2))) + (RSF)intTerminal * (0, 1)))) | signedModifier | floatTerminal | doubleTerminal | (longTerminal + doubleTerminal));
			//grammar.AddProductionRule(NonterminalType.SourceCode, (RSF)NonterminalType.FunctionDeclaration * (1, null));
			return grammar;
		}
	}

	public enum NonterminalType : byte {
		SourceCode,

		MainType,

		FunctionDeclaration,

		FunctionCall,

		ParameterList,

		Expression
	}
}