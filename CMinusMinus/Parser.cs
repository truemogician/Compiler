using System.Text.RegularExpressions;
using Parser;

#nullable enable
namespace CMinusMinus {
	using RSF = RegularSentenceForm;

	public partial class CMinusMinusFactory {
		public override Grammar CreateGrammar() {
			var grammar = new Grammar(NonterminalType.SourceCode);

			#region Terminals
			#region Keywords
			var tVoid = NewKeywordTerminal("void");
			var tConst = NewKeywordTerminal("const");
			var tVolatile = NewKeywordTerminal("volatile");
			var tChar = NewKeywordTerminal("char");
			var tInt = NewKeywordTerminal("int");
			var tFloat = NewKeywordTerminal("float");
			var tDouble = NewKeywordTerminal("double");
			var tShort = NewKeywordTerminal("short");
			var tLong = NewKeywordTerminal("long");
			var tGoto = NewKeywordTerminal("goto");
			var tContinue = NewKeywordTerminal("continue");
			var tBreak = NewKeywordTerminal("break");
			var tReturn = NewKeywordTerminal("return");
			var tIf = NewKeywordTerminal("if");
			var tElse = NewKeywordTerminal("else");
			var tSwitch = NewKeywordTerminal("switch");
			var tCase = NewKeywordTerminal("case");
			var tDefault = NewKeywordTerminal("default");
			var tFor = NewKeywordTerminal("for");
			var tWhile = NewKeywordTerminal("while");
			var tDo = NewKeywordTerminal("do");
			var tSignModifier = NewTerminal(LexemeType.Keyword, new Regex(@"(?:un)?signed"));
			#endregion

			var tIdentifier = NewTerminal(LexemeType.Identifier);
			var tCharacterLiteral = NewTerminal(LexemeType.CharacterLiteral);
			var tStringLiteral = NewTerminal(LexemeType.StringLiteral);
			var tIntegerLiteral = NewTerminal(LexemeType.IntegerLiteral);
			var tFloatLiteral = NewTerminal(LexemeType.FloatLiteral);
			var tLeftParenthesis = NewTerminal(LexemeType.LeftParenthesis);
			var tRightParenthesis = NewTerminal(LexemeType.RightParenthesis);
			var tSeparator = NewTerminal(LexemeType.Separator);
			var tQuestionMark = NewTerminal(LexemeType.QuestionMark);
			var tBlockStart = NewTerminal(LexemeType.BlockStartSymbol);
			var tBlockEnd = NewTerminal(LexemeType.BlockEndSymbol);
			var tDelimiter = NewTerminal(LexemeType.Delimiter);
			var tColon = NewTerminal(LexemeType.Colon);

			#region Operators
			var tAssignmentOperator = NewTerminal(LexemeType.AssignmentOperator);
			var tLogicalOrOperator = NewTerminal(LexemeType.LogicalOperator, "||");
			var tLogicalAndOperator = NewTerminal(LexemeType.LogicalOperator, "&&");
			var tBitwiseOrOperator = NewTerminal(LexemeType.BitwiseOperator, "|");
			var tBitwiseXorOperator = NewTerminal(LexemeType.BitwiseOperator, "^");
			var tBitwiseAndOperator = NewTerminal(LexemeType.BitwiseOperator, "&");
			var tRelationalOperator = NewTerminal(LexemeType.RelationalOperator, new Regex(@"[<>]=?"));
			var tShiftOperator = NewTerminal(LexemeType.BitwiseOperator, new Regex(@"<<|>>"));
			var tAdditiveOperator = NewTerminal(LexemeType.ArithmeticOperator, new Regex(@"[-+]"));
			var tMultiplicativeOperator = NewTerminal(LexemeType.ArithmeticOperator, new Regex(@"[*\/%]"));
			var tEqualityOperator = NewTerminal(LexemeType.RelationalOperator, new Regex(@"[=!]="));
			#endregion
			#endregion

			#region Nonterminals
			var nQualifier = new Nonterminal("Qualifier", true);
			var nFunctionBody = new Nonterminal("FunctionBody", true);
			var nExpressionWithParenthesis = new Nonterminal();
			var nIfCommonPart = new Nonterminal();
			var nCaseCommonPart = new Nonterminal();
			var nCaseBody = new Nonterminal("CaseBody", true);
			var nLoopBodyComponent = new Nonterminal();
			var nLoopBody = new Nonterminal("LoopBody", true);
			var nEmbeddedStatement = new Nonterminal("EmbeddedStatement", true);
			var nAtomExpression = new Nonterminal("AtomExpression", true);
			var nLogicalOrExpression = new Nonterminal("LogicalOrExpression", true);
			var nLogicalAndExpression = new Nonterminal("LogicalAndExpression", true);
			var nBitwiseXorExpression = new Nonterminal("BitwiseXorExpression", true);
			var nBitwiseAndExpression = new Nonterminal("BitwiseAndExpression", true);
			var nBitwiseOrExpression = new Nonterminal("BitwiseOrExpression", true);
			#endregion

			#region Production Rules
			grammar.Add(
				NonterminalType.SourceCode,
				((RSF)NonterminalType.FunctionDeclaration | NonterminalType.DeclarationStatement) * '+'
			);
			grammar.Add(
				NonterminalType.FunctionDeclaration,
				((RSF)tVoid | NonterminalType.MainType) +
				tIdentifier +
				tLeftParenthesis +
				((RSF)SentenceForm.Empty | (nQualifier + NonterminalType.MainType + tIdentifier + ((RSF)tSeparator + nQualifier + NonterminalType.MainType + tIdentifier) * '*')) +
				tRightParenthesis +
				nFunctionBody
			);
			grammar.Add(
				nQualifier,
				(RSF)tConst | tVolatile | (tConst + tVolatile) | (tVolatile + tConst) | SentenceForm.Empty
			);
			grammar.Add(
				NonterminalType.MainType,
				((RSF)tSignModifier * '?' + ((RSF)tChar | tInt | (((RSF)tShort | ((RSF)tLong * (1, 2))) + (RSF)tInt * '?'))) |
				tSignModifier |
				tFloat |
				tDouble |
				(tLong + tDouble)
			);
			grammar.Add(
				nFunctionBody,
				(RSF)tBlockStart + ((RSF)NonterminalType.Label * '?' + ((RSF)NonterminalType.CommonStatement | NonterminalType.ControlFlow)) * '*' + tBlockEnd
			);

			#region Statements
			grammar.Add(
				NonterminalType.CommonStatement,
				(RSF)NonterminalType.DeclarationStatement |
				NonterminalType.GotoStatement |
				NonterminalType.ReturnStatement |
				((NonterminalType.FunctionCall | ((RSF)tIdentifier + tAssignmentOperator + NonterminalType.Expression)) + tDelimiter) |
				tDelimiter
			);
			grammar.Add(
				NonterminalType.DeclarationStatement,
				(RSF)nQualifier + NonterminalType.MainType + (tIdentifier + ((RSF)tAssignmentOperator + NonterminalType.Expression) * '?') * '+'
			);
			grammar.Add(
				NonterminalType.GotoStatement,
				(RSF)tGoto + tIdentifier + tDelimiter
			);
			grammar.Add(
				NonterminalType.ContinueStatement,
				(RSF)tContinue + tDelimiter
			);
			grammar.Add(
				NonterminalType.BreakStatement,
				(RSF)tBreak + tDelimiter
			);
			grammar.Add(
				NonterminalType.ReturnStatement,
				(RSF)tReturn + (RSF)NonterminalType.Expression * '?' + tDelimiter
			);
			grammar.Add(
				NonterminalType.Label,
				(RSF)tIdentifier + tColon
			);
			#endregion

			#region Control Flow
			grammar.Add(
				NonterminalType.ControlFlow,
				(RSF)NonterminalType.IfBlock | NonterminalType.SwitchBlock | NonterminalType.ForBlock | NonterminalType.WhileBlock | NonterminalType.DoWhileBlock
			);
			grammar.Add(
				NonterminalType.IfBlock,
				(RSF)tIf +
				nIfCommonPart +
				((RSF)tElse + tIf + nIfCommonPart) * '*' +
				((RSF)tElse + nIfCommonPart) * '?'
			);
			grammar.Add(
				nIfCommonPart,
				((RSF)nExpressionWithParenthesis + nEmbeddedStatement) | nFunctionBody
			);
			grammar.Add(
				nExpressionWithParenthesis,
				(RSF)tLeftParenthesis + NonterminalType.Expression + tRightParenthesis
			);
			grammar.Add(
				nEmbeddedStatement,
				(RSF)NonterminalType.GotoStatement |
				NonterminalType.ReturnStatement |
				((NonterminalType.FunctionCall | ((RSF)tIdentifier + tAssignmentOperator + NonterminalType.Expression)) + tDelimiter) |
				tDelimiter |
				NonterminalType.ControlFlow
			);
			grammar.Add(
				NonterminalType.SwitchBlock,
				(RSF)tSwitch +
				nExpressionWithParenthesis +
				tBlockStart +
				((RSF)tCase + NonterminalType.Expression + nCaseCommonPart) * '*' +
				((RSF)tDefault + nCaseCommonPart) * '?' +
				((RSF)tCase + NonterminalType.Expression + nCaseCommonPart) * '*' +
				tBlockEnd
			);
			grammar.Add(
				nCaseCommonPart,
				(RSF)tColon + (nCaseBody | ((RSF)tBlockStart + nCaseBody + tBlockEnd))
			);
			grammar.Add(
				nCaseBody,
				((RSF)NonterminalType.Label * '?' + ((RSF)NonterminalType.CommonStatement | NonterminalType.BreakStatement | NonterminalType.ControlFlow)) * '*'
			);
			grammar.Add(
				NonterminalType.ForBlock,
				(RSF)tFor + tLeftParenthesis + ((RSF)NonterminalType.DeclarationStatement | tDelimiter) + (RSF)NonterminalType.Expression * '?' + tDelimiter + (RSF)NonterminalType.Expression * '?' + tRightParenthesis + nLoopBody
			);
			grammar.Add(
				nLoopBody,
				(RSF)NonterminalType.ContinueStatement |
				NonterminalType.BreakStatement |
				nEmbeddedStatement |
				((RSF)tBlockStart + ((RSF)NonterminalType.Label * '?' + nLoopBodyComponent) * '*' + tBlockEnd)
			);
			grammar.Add(
				nLoopBodyComponent,
				(RSF)NonterminalType.CommonStatement | NonterminalType.BreakStatement | NonterminalType.ContinueStatement | NonterminalType.ControlFlow
			);
			grammar.Add(
				NonterminalType.WhileBlock,
				(RSF)tWhile + nExpressionWithParenthesis + nLoopBody
			);
			grammar.Add(
				NonterminalType.DoWhileBlock,
				(RSF)tDo + nLoopBody + tWhile + nExpressionWithParenthesis + tDelimiter
			);
			#endregion

			#region Expressions
			grammar.Add(
				nAtomExpression,
				(RSF)NonterminalType.FunctionCall | NonterminalType.Literal | tIdentifier | (tLeftParenthesis + NonterminalType.Expression + tRightParenthesis)
			);
			grammar.Add(
				NonterminalType.FunctionCall,
				(RSF)tIdentifier + tLeftParenthesis + (RSF)NonterminalType.CommaExpression * '?' + tRightParenthesis
			);
			grammar.Add(
				NonterminalType.Literal,
				(RSF)tCharacterLiteral | tStringLiteral | tIntegerLiteral | tFloatLiteral
			);
			grammar.Add(
				NonterminalType.Expression,
				(RSF)NonterminalType.CommaExpression |
				NonterminalType.AssignmentExpression |
				NonterminalType.LogicalExpression |
				NonterminalType.BitwiseExpression |
				NonterminalType.EqualityExpression |
				NonterminalType.RelationalExpression |
				NonterminalType.ShiftExpression |
				NonterminalType.AdditiveExpression |
				NonterminalType.AdditiveExpression |
				nAtomExpression
			);
			grammar.Add(
				NonterminalType.CommaExpression,
				(RSF)NonterminalType.AssignmentExpression + ((RSF)tSeparator + NonterminalType.AssignmentExpression) * '*'
			);
			grammar.Add(
				NonterminalType.AssignmentExpression,
				(RSF)NonterminalType.ConditionalExpression | ((RSF)tIdentifier + tAssignmentOperator + NonterminalType.AssignmentExpression)
			);
			grammar.Add(
				NonterminalType.ConditionalExpression,
				(RSF)NonterminalType.LogicalExpression | ((RSF)NonterminalType.ConditionalExpression + tQuestionMark + NonterminalType.ConditionalExpression + tColon + NonterminalType.ConditionalExpression)
			);
			grammar.Add(
				NonterminalType.LogicalExpression,
				(RSF)NonterminalType.BitwiseExpression | nLogicalOrExpression | nLogicalAndExpression
			);
			grammar.Add(
				NonterminalType.BitwiseExpression,
				(RSF)NonterminalType.EqualityExpression | nBitwiseOrExpression | nBitwiseXorExpression | nBitwiseAndExpression
			);
			grammar.Add(
				nLogicalOrExpression,
				(RSF)nLogicalAndExpression | (nLogicalOrExpression + tLogicalOrOperator + nLogicalOrExpression)
			);
			grammar.Add(
				nLogicalAndExpression,
				(RSF)NonterminalType.BitwiseExpression | (nLogicalAndExpression + tLogicalAndOperator + nLogicalAndExpression)
			);
			grammar.Add(
				NonterminalType.EqualityExpression,
				(RSF)NonterminalType.RelationalExpression | ((RSF)NonterminalType.EqualityExpression + tEqualityOperator + NonterminalType.EqualityExpression)
			);
			grammar.Add(
				nBitwiseOrExpression,
				(RSF)nBitwiseXorExpression | (nBitwiseOrExpression + tBitwiseOrOperator + nBitwiseOrExpression)
			);
			grammar.Add(
				nBitwiseXorExpression,
				(RSF)nBitwiseAndExpression | (nBitwiseXorExpression + tBitwiseXorOperator + nBitwiseXorExpression)
			);
			grammar.Add(
				nBitwiseAndExpression,
				(RSF)NonterminalType.EqualityExpression | (nBitwiseAndExpression + tBitwiseAndOperator + nBitwiseAndExpression)
			);
			grammar.Add(
				NonterminalType.RelationalExpression,
				(RSF)NonterminalType.ShiftExpression | ((RSF)NonterminalType.RelationalExpression + tRelationalOperator + NonterminalType.RelationalExpression)
			);
			grammar.Add(
				NonterminalType.ShiftExpression,
				(RSF)NonterminalType.AdditiveExpression | ((RSF)NonterminalType.ShiftExpression + tShiftOperator + NonterminalType.ShiftExpression)
			);
			grammar.Add(
				NonterminalType.AdditiveExpression,
				(RSF)NonterminalType.MultiplicativeExpression | ((RSF)NonterminalType.AdditiveExpression + tAdditiveOperator + NonterminalType.AdditiveExpression)
			);
			grammar.Add(
				NonterminalType.MultiplicativeExpression,
				(RSF)nAtomExpression | ((RSF)NonterminalType.MultiplicativeExpression + tMultiplicativeOperator + NonterminalType.MultiplicativeExpression)
			);
			#endregion
			#endregion

			grammar.RemoveEmptyProductionRules();
			grammar.Simplify();
			return grammar;
		}

		private Terminal NewTerminal(LexemeType type) => new(Lexicon[type]);

		private Terminal NewKeywordTerminal(string keyword) => new(Lexicon[LexemeType.Keyword], keyword);

		private Terminal NewTerminal(LexemeType type, string pattern) => new(Lexicon[type], pattern);

		private Terminal NewTerminal(LexemeType type, Regex pattern) => new(Lexicon[type], pattern);
	}

	public enum NonterminalType : byte {
		SourceCode,

		FunctionDeclaration,

		MainType,

		CommonStatement,

		DeclarationStatement,

		GotoStatement,

		BreakStatement,

		ContinueStatement,

		ReturnStatement,

		ControlFlow,

		IfBlock,

		SwitchBlock,

		ForBlock,

		WhileBlock,

		DoWhileBlock,

		Label,

		FunctionCall,

		Expression,

		MultiplicativeExpression,

		AdditiveExpression,

		ShiftExpression,

		RelationalExpression,

		EqualityExpression,

		BitwiseExpression,

		LogicalExpression,

		ConditionalExpression,

		AssignmentExpression,

		CommaExpression,

		Literal
	}
}