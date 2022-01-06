using System.Text.RegularExpressions;
using Parser;

namespace CMinusMinus {
	using RSF = RegularSentenceForm;

	public partial class CMinusMinusFactory {
		public override Grammar CreateGrammar() {
			var grammar = new Grammar(NonterminalType.Program);

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
			var tSizeOf = NewKeywordTerminal("sizeof");
			#endregion

			var tIdentifier = NewTerminal(LexemeType.Identifier);
			var tCharacterLiteral = NewTerminal(LexemeType.CharacterLiteral);
			var tStringLiteral = NewTerminal(LexemeType.StringLiteral);
			var tIntegerLiteral = NewTerminal(LexemeType.IntegerLiteral);
			var tFloatLiteral = NewTerminal(LexemeType.FloatLiteral);
			var tLeftParenthesis = NewTerminal(LexemeType.LeftParenthesis);
			var tRightParenthesis = NewTerminal(LexemeType.RightParenthesis);
			var tIndexStart = NewTerminal(LexemeType.IndexStartSymbol);
			var tIndexEnd = NewTerminal(LexemeType.IndexEndSymbol);
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
			var tPlusOperator = NewTerminal(LexemeType.ArithmeticOperator, "+");
			var tMinusOperator = NewTerminal(LexemeType.ArithmeticOperator, "-");
			var tMultiplyOperator = NewTerminal(LexemeType.ArithmeticOperator, "*");
			var tDivideModuloOperator = NewTerminal(LexemeType.ArithmeticOperator, new Regex(@"[\/%]"));
			var tEqualityOperator = NewTerminal(LexemeType.RelationalOperator, new Regex(@"[=!]="));
			var tLogicalNotOperator = NewTerminal(LexemeType.LogicalOperator, "!");
			var tBitwiseNotOperator = NewTerminal(LexemeType.BitwiseOperator, "~");
			var tDereferenceOperator = tMultiplyOperator;
			var tAddressOfOperator = tBitwiseAndOperator;
			var tMembershipOperator = NewTerminal(LexemeType.MembershipOperator);
			#endregion
			#endregion

			#region Nonterminals
			var nQualifier = new Nonterminal("Qualifier", true);
			var nType = new Nonterminal("Type", true);
			var nStatement = new Nonterminal("Statement", true);
			var nParenthesizedExpression = new Nonterminal("ParenthesizedExpression", true);
			var nLabeledComponent = new Nonterminal("LabeledComponent", true);
			var nDeclarationStatementComponent = new Nonterminal();
			var nLogicalOrExpression = new Nonterminal("LogicalOrExpression", true);
			var nLogicalAndExpression = new Nonterminal("LogicalAndExpression", true);
			var nBitwiseOrExpression = new Nonterminal("BitwiseOrExpression", true);
			var nBitwiseXorExpression = new Nonterminal("BitwiseXorExpression", true);
			var nBitwiseAndExpression = new Nonterminal("BitwiseAndExpression", true);
			var nUnaryPlusMinusExpression = new Nonterminal();
			var nOtherUnaryExpression = new Nonterminal();
			var nCommaOrHigherPriorityExpression = new Nonterminal("CommaExpression+", true);
			var nAssignmentOrHigherPriorityExpression = new Nonterminal("AssignmentExpression+", true);
			var nConditionalOrHigherPriorityExpression = new Nonterminal("ConditionalExpression+", true);
			var nLogicalOrOrHigherPriorityExpression = new Nonterminal("LogicalOrExpression+", true);
			var nLogicalAndOrHigherPriorityExpression = new Nonterminal("LogicalAndExpression+", true);
			var nLogicalOrHigherPriorityExpression = new Nonterminal("LogicalExpression+", true);
			var nBitwiseOrOrHigherPriorityExpression = new Nonterminal("BitwiseOrExpression+", true);
			var nBitwiseXorOrHigherPriorityExpression = new Nonterminal("BitwiseXorExpression+", true);
			var nBitwiseAndOrHigherPriorityExpression = new Nonterminal("BitwiseAndExpression+", true);
			var nBitwiseOrHigherPriorityExpression = new Nonterminal("BitwiseExpression+", true);
			var nEqualityOrHigherPriorityExpression = new Nonterminal("EqualityExpression+", true);
			var nRelationalOrHigherPriorityExpression = new Nonterminal("RelationalExpression+", true);
			var nShiftOrHigherPriorityExpression = new Nonterminal("ShiftExpression+", true);
			var nAdditiveOrHigherPriorityExpression = new Nonterminal("AdditiveExpression+", true);
			var nMultiplicativeOrHigherPriorityExpression = new Nonterminal("MultiplicativeExpression+", true);
			var nUnaryOrHigherPriorityExpression = new Nonterminal("UnaryExpression+", true);
			var nPostfixOrHigherPriorityExpression = new Nonterminal("PrimaryExpression+", true);
			#endregion

			#region Production Rules
			grammar.Add(
				NonterminalType.Program,
				((RSF)NonterminalType.FunctionDeclaration | NonterminalType.DeclarationStatement) * '+'
			);
			grammar.Add(
				NonterminalType.FunctionDeclaration,
				(RSF)nType +
				tIdentifier +
				tLeftParenthesis +
				((RSF)SentenceForm.Empty | ((RSF)nType + tIdentifier + ((RSF)tSeparator + nType + tIdentifier) * '*')) +
				tRightParenthesis +
				NonterminalType.Block
			);
			grammar.Add(
				nType,
				(RSF)nQualifier + NonterminalType.FundamentalType + ((RSF)tDereferenceOperator + nQualifier) * '*'
			);
			grammar.Add(
				nQualifier,
				((RSF)tConst | tVolatile) * '*'
			);
			grammar.Add(
				NonterminalType.FundamentalType,
				((RSF)tSignModifier * '?' + ((RSF)tChar | tInt | (((RSF)tShort | ((RSF)tLong * (1, 2))) + (RSF)tInt * '?'))) |
				tSignModifier |
				tFloat |
				tDouble |
				(tLong + tDouble) |
				tVoid
			);
			grammar.Add(
				NonterminalType.Block,
				tBlockStart + (RSF)nLabeledComponent * '*' + tBlockEnd
			);
			grammar.Add(
				nLabeledComponent,
				(RSF)NonterminalType.Label * '?' + ((RSF)nStatement | NonterminalType.Block | NonterminalType.ControlFlow)
			);

			#region Statements
			grammar.Add(
				nStatement,
				(RSF)NonterminalType.EmptyStatement |
				NonterminalType.ExpressionStatement |
				NonterminalType.DeclarationStatement |
				NonterminalType.JumpStatement
			);
			grammar.Add(
				NonterminalType.EmptyStatement,
				tDelimiter
			);
			grammar.Add(
				NonterminalType.ExpressionStatement,
				(RSF)NonterminalType.Expression + tDelimiter
			);
			//TODO: declaration with parenthesis, function pointer declaration
			grammar.Add(
				NonterminalType.DeclarationStatement,
				(RSF)nQualifier + NonterminalType.FundamentalType + nDeclarationStatementComponent + ((RSF)tSeparator + nDeclarationStatementComponent) * '*' + tDelimiter
			);
			grammar.Add(
				nDeclarationStatementComponent,
				((RSF)tDereferenceOperator + nQualifier) * '*' + (tIdentifier + ((RSF)tIndexStart + NonterminalType.Expression + tIndexEnd) * '?' + ((RSF)tAssignmentOperator + nAssignmentOrHigherPriorityExpression) * '?')
			);
			grammar.Add(
				NonterminalType.JumpStatement,
				((RSF)tContinue | tBreak | ((RSF)tGoto + tIdentifier) | ((RSF)tReturn + (RSF)NonterminalType.Expression * '?')) + tDelimiter
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
				nParenthesizedExpression +
				nLabeledComponent +
				((RSF)tElse + tIf + nParenthesizedExpression + nLabeledComponent) * '*' +
				((RSF)tElse + nLabeledComponent) * '?'
			);
			grammar.Add(
				nParenthesizedExpression,
				(RSF)tLeftParenthesis + NonterminalType.Expression + tRightParenthesis
			);
			grammar.Add(
				NonterminalType.SwitchBlock,
				(RSF)tSwitch +
				nParenthesizedExpression +
				tBlockStart +
				(RSF)NonterminalType.CaseBlock * '*' +
				tBlockEnd
			);
			grammar.Add(
				NonterminalType.CaseBlock,
				((RSF)tDefault | (tCase + NonterminalType.Expression)) + (RSF)tColon + (RSF)nLabeledComponent * '*'
			);
			grammar.Add(
				NonterminalType.ForBlock,
				(RSF)tFor +
				tLeftParenthesis +
				((RSF)NonterminalType.DeclarationStatement | NonterminalType.ExpressionStatement | NonterminalType.EmptyStatement) +
				((RSF)NonterminalType.ExpressionStatement | NonterminalType.EmptyStatement) +
				(RSF)NonterminalType.Expression * '?' +
				tRightParenthesis +
				nLabeledComponent
			);
			grammar.Add(
				NonterminalType.WhileBlock,
				(RSF)tWhile + nParenthesizedExpression + nLabeledComponent
			);
			grammar.Add(
				NonterminalType.DoWhileBlock,
				(RSF)tDo + nLabeledComponent + tWhile + nParenthesizedExpression + tDelimiter
			);
			#endregion

			#region Expressions
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
				NonterminalType.MultiplicativeExpression |
				NonterminalType.UnaryExpression |
				NonterminalType.PostfixExpression |
				NonterminalType.PrimaryExpression
			);
			grammar.Add(
				NonterminalType.CommaExpression,
				(RSF)nAssignmentOrHigherPriorityExpression + (RSF)tSeparator + (RSF)nCommaOrHigherPriorityExpression
			);
			grammar.Add(
				nCommaOrHigherPriorityExpression,
				(RSF)NonterminalType.CommaExpression | nAssignmentOrHigherPriorityExpression
			);
			grammar.Add(
				NonterminalType.AssignmentExpression,
				(RSF)nConditionalOrHigherPriorityExpression + tAssignmentOperator + nAssignmentOrHigherPriorityExpression
			);
			grammar.Add(
				nAssignmentOrHigherPriorityExpression,
				(RSF)NonterminalType.AssignmentExpression | nConditionalOrHigherPriorityExpression
			);
			grammar.Add(
				NonterminalType.ConditionalExpression,
				(RSF)nConditionalOrHigherPriorityExpression + tQuestionMark + nConditionalOrHigherPriorityExpression + tColon + nConditionalOrHigherPriorityExpression
			);
			grammar.Add(
				nConditionalOrHigherPriorityExpression,
				(RSF)NonterminalType.ConditionalExpression | nLogicalOrHigherPriorityExpression
			);
			grammar.Add(
				NonterminalType.LogicalExpression,
				(RSF)nLogicalOrExpression | nLogicalAndExpression
			);
			grammar.Add(
				nLogicalOrHigherPriorityExpression,
				(RSF)NonterminalType.LogicalExpression | nBitwiseOrHigherPriorityExpression
			);
			grammar.Add(
				nLogicalOrExpression,
				(RSF)nLogicalAndOrHigherPriorityExpression + tLogicalOrOperator + nLogicalOrOrHigherPriorityExpression
			);
			grammar.Add(
				nLogicalOrOrHigherPriorityExpression,
				(RSF)nLogicalOrExpression | nLogicalAndOrHigherPriorityExpression
			);
			grammar.Add(
				nLogicalAndExpression,
				(RSF)nBitwiseOrHigherPriorityExpression + tLogicalAndOperator + nLogicalAndOrHigherPriorityExpression
			);
			grammar.Add(
				nLogicalAndOrHigherPriorityExpression,
				(RSF)nLogicalAndExpression | nBitwiseOrHigherPriorityExpression
			);
			grammar.Add(
				NonterminalType.BitwiseExpression,
				(RSF)nBitwiseOrExpression | nBitwiseXorExpression | nBitwiseAndExpression
			);
			grammar.Add(
				nBitwiseOrHigherPriorityExpression,
				(RSF)NonterminalType.BitwiseExpression | nEqualityOrHigherPriorityExpression
			);
			grammar.Add(
				nBitwiseOrExpression,
				(RSF)nBitwiseXorOrHigherPriorityExpression + tBitwiseOrOperator + nBitwiseOrOrHigherPriorityExpression
			);
			grammar.Add(
				nBitwiseOrOrHigherPriorityExpression,
				(RSF)nBitwiseOrExpression | nBitwiseXorOrHigherPriorityExpression
			);
			grammar.Add(
				nBitwiseXorExpression,
				(RSF)nBitwiseAndOrHigherPriorityExpression + tBitwiseXorOperator + nBitwiseXorOrHigherPriorityExpression
			);
			grammar.Add(
				nBitwiseXorOrHigherPriorityExpression,
				(RSF)nBitwiseXorExpression | nBitwiseAndOrHigherPriorityExpression
			);
			grammar.Add(
				nBitwiseAndExpression,
				(RSF)nEqualityOrHigherPriorityExpression + tBitwiseAndOperator + nBitwiseAndOrHigherPriorityExpression
			);
			grammar.Add(
				nBitwiseAndOrHigherPriorityExpression,
				(RSF)nBitwiseAndExpression | nEqualityOrHigherPriorityExpression
			);
			grammar.Add(
				NonterminalType.EqualityExpression,
				(RSF)nRelationalOrHigherPriorityExpression + tEqualityOperator + nEqualityOrHigherPriorityExpression
			);
			grammar.Add(
				nEqualityOrHigherPriorityExpression,
				(RSF)NonterminalType.EqualityExpression | nRelationalOrHigherPriorityExpression
			);
			grammar.Add(
				NonterminalType.RelationalExpression,
				(RSF)nShiftOrHigherPriorityExpression + tRelationalOperator + nRelationalOrHigherPriorityExpression
			);
			grammar.Add(
				nRelationalOrHigherPriorityExpression,
				(RSF)NonterminalType.RelationalExpression | nShiftOrHigherPriorityExpression
			);
			grammar.Add(
				NonterminalType.ShiftExpression,
				(RSF)nAdditiveOrHigherPriorityExpression + tShiftOperator + nShiftOrHigherPriorityExpression
			);
			grammar.Add(
				nShiftOrHigherPriorityExpression,
				(RSF)NonterminalType.ShiftExpression | nAdditiveOrHigherPriorityExpression
			);
			grammar.Add(
				NonterminalType.AdditiveExpression,
				(RSF)nMultiplicativeOrHigherPriorityExpression + ((RSF)tPlusOperator | tMinusOperator) + nAdditiveOrHigherPriorityExpression
			);
			grammar.Add(
				nAdditiveOrHigherPriorityExpression,
				(RSF)NonterminalType.AdditiveExpression | nMultiplicativeOrHigherPriorityExpression
			);
			grammar.Add(
				NonterminalType.MultiplicativeExpression,
				nUnaryOrHigherPriorityExpression + ((RSF)tMultiplyOperator | tDivideModuloOperator) + nMultiplicativeOrHigherPriorityExpression
			);
			grammar.Add(
				nMultiplicativeOrHigherPriorityExpression,
				(RSF)NonterminalType.MultiplicativeExpression | nUnaryOrHigherPriorityExpression
			);
			grammar.Add(
				NonterminalType.UnaryExpression,
				(RSF)nUnaryPlusMinusExpression | nOtherUnaryExpression
			);
			grammar.Add(
				nUnaryPlusMinusExpression,
				((RSF)tPlusOperator | tMinusOperator) + ((RSF)nOtherUnaryExpression | nPostfixOrHigherPriorityExpression)
			);
			grammar.Add(
				nOtherUnaryExpression,
				(((RSF)tPlusOperator * 2) | ((RSF)tMinusOperator * 2) | tLogicalNotOperator | tBitwiseNotOperator | tDereferenceOperator | tAddressOfOperator | tSizeOf | ((RSF)tLeftParenthesis + nType + tRightParenthesis)) + nUnaryOrHigherPriorityExpression
			);
			grammar.Add(
				nUnaryOrHigherPriorityExpression,
				(RSF)NonterminalType.UnaryExpression | nPostfixOrHigherPriorityExpression
			);
			grammar.Add(
				NonterminalType.PostfixExpression,
				(RSF)nPostfixOrHigherPriorityExpression +
				(((RSF)tIndexStart + NonterminalType.Expression + tIndexEnd) |
					((RSF)tLeftParenthesis + (RSF)NonterminalType.Expression * '?' + tRightParenthesis) |
					((RSF)tMembershipOperator + tIdentifier) |
					(((RSF)tPlusOperator * 2) | ((RSF)tMinusOperator * 2)))
			);
			grammar.Add(
				nPostfixOrHigherPriorityExpression,
				(RSF)NonterminalType.PostfixExpression | NonterminalType.PrimaryExpression
			);
			grammar.Add(
				NonterminalType.PrimaryExpression,
				(RSF)NonterminalType.Literal | tIdentifier | (tLeftParenthesis + NonterminalType.Expression + tRightParenthesis)
			);
			grammar.Add(
				NonterminalType.Literal,
				(RSF)tCharacterLiteral | tStringLiteral | tIntegerLiteral | tFloatLiteral
			);
			#endregion
			#endregion

			grammar.Simplify();
			grammar.MergeEquivalentNonterminals();
			grammar.RemoveEmptyProductionRules();
			return grammar;
		}

		private Terminal NewTerminal(LexemeType type) => new(Lexicon[type]);

		private Terminal NewKeywordTerminal(string keyword) => new(Lexicon[LexemeType.Keyword], keyword);

		private Terminal NewTerminal(LexemeType type, string pattern) => new(Lexicon[type], pattern);

		private Terminal NewTerminal(LexemeType type, Regex pattern) => new(Lexicon[type], pattern);
	}

	public enum NonterminalType : byte {
		Program,

		FunctionDeclaration,

		FundamentalType,

		EmptyStatement,

		ExpressionStatement,

		DeclarationStatement,

		JumpStatement,

		ControlFlow,

		Block,

		IfBlock,

		SwitchBlock,

		CaseBlock,

		ForBlock,

		WhileBlock,

		DoWhileBlock,

		Label,

		Expression,

		PrimaryExpression,

		PostfixExpression,

		UnaryExpression,

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