﻿using System.Text.RegularExpressions;
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
			var nParenthesizedExpression = new Nonterminal("ParenthesizedExpression", true);
			var nBlock = new Nonterminal("Block", true);
			var nLabeledComponent = new Nonterminal("LabeledComponent", true);
			var nStatement = new Nonterminal("Statement", true);
			var nCaseCommonPart = new Nonterminal();
			var nEmbeddableBody = new Nonterminal("EmbeddableBody", true);
			var nEmbeddedStatement = new Nonterminal("EmbeddedStatement", true);
			var nAtomExpression = new Nonterminal("AtomExpression", true);
			var nLogicalOrExpression = new Nonterminal("LogicalOrExpression", true);
			var nLogicalAndExpression = new Nonterminal("LogicalAndExpression", true);
			var nBitwiseOrExpression = new Nonterminal("BitwiseOrExpression", true);
			var nBitwiseXorExpression = new Nonterminal("BitwiseXorExpression", true);
			var nBitwiseAndExpression = new Nonterminal("BitwiseAndExpression", true);
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
				nBlock
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
				nBlock,
				tBlockStart + (RSF)nLabeledComponent * '*' + tBlockEnd
			);
			grammar.Add(
				nLabeledComponent,
				(RSF)NonterminalType.Label * '?' + ((RSF)nStatement | nBlock | NonterminalType.ControlFlow)
			);

			#region Statements
			grammar.Add(
				nStatement,
				(RSF)NonterminalType.EmptyStatement |
				NonterminalType.ExpressionStatement |
				NonterminalType.DeclarationStatement |
				NonterminalType.ControlStatement |
				NonterminalType.ReturnStatement
			);
			grammar.Add(
				NonterminalType.EmptyStatement,
				tDelimiter
			);
			grammar.Add(
				NonterminalType.ExpressionStatement,
				(RSF)NonterminalType.Expression + tDelimiter
			);
			grammar.Add(
				NonterminalType.DeclarationStatement,
				(RSF)nQualifier + NonterminalType.MainType + ((RSF)tIdentifier | NonterminalType.AssignmentExpression) + (tSeparator + ((RSF)tIdentifier | NonterminalType.AssignmentExpression)) * '*' + tDelimiter
			);
			grammar.Add(
				NonterminalType.ControlStatement,
				((RSF)tContinue | tBreak | ((RSF)tGoto + tIdentifier)) + tDelimiter
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
				nParenthesizedExpression +
				nEmbeddableBody +
				((RSF)tElse + tIf + nParenthesizedExpression + nEmbeddableBody) * '*' +
				((RSF)tElse + nEmbeddableBody) * '?'
			);
			grammar.Add(
				nParenthesizedExpression,
				(RSF)tLeftParenthesis + NonterminalType.Expression + tRightParenthesis
			);
			grammar.Add(
				nEmbeddedStatement,
				(RSF)NonterminalType.ExpressionStatement |
				NonterminalType.ControlStatement |
				NonterminalType.ReturnStatement |
				NonterminalType.ControlFlow
			);
			grammar.Add(
				NonterminalType.SwitchBlock,
				(RSF)tSwitch +
				nParenthesizedExpression +
				tBlockStart +
				((RSF)tCase + NonterminalType.Expression + nCaseCommonPart) * '*' +
				((RSF)tDefault + nCaseCommonPart) * '?' +
				((RSF)tCase + NonterminalType.Expression + nCaseCommonPart) * '*' +
				tBlockEnd
			);
			grammar.Add(
				nCaseCommonPart,
				(RSF)tColon + (((RSF)nLabeledComponent * '*') | nBlock)
			);
			grammar.Add(
				NonterminalType.ForBlock,
				(RSF)tFor + tLeftParenthesis + ((RSF)NonterminalType.DeclarationStatement | tDelimiter) + (RSF)NonterminalType.Expression * '?' + tDelimiter + (RSF)NonterminalType.Expression * '?' + tRightParenthesis + nEmbeddableBody
			);
			grammar.Add(
				nEmbeddableBody,
				(RSF)nEmbeddedStatement | nBlock
			);
			grammar.Add(
				NonterminalType.WhileBlock,
				(RSF)tWhile + nParenthesizedExpression + nEmbeddableBody
			);
			grammar.Add(
				NonterminalType.DoWhileBlock,
				(RSF)tDo + nEmbeddableBody + tWhile + nParenthesizedExpression + tDelimiter
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
				nAtomExpression
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
				(RSF)tIdentifier + tAssignmentOperator + nAssignmentOrHigherPriorityExpression
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
				(RSF)nMultiplicativeOrHigherPriorityExpression + tAdditiveOperator + nAdditiveOrHigherPriorityExpression
			);
			grammar.Add(
				nAdditiveOrHigherPriorityExpression,
				(RSF)NonterminalType.AdditiveExpression | nMultiplicativeOrHigherPriorityExpression
			);
			grammar.Add(
				NonterminalType.MultiplicativeExpression,
				(RSF)nAtomExpression + tMultiplicativeOperator + nMultiplicativeOrHigherPriorityExpression
			);
			grammar.Add(
				nMultiplicativeOrHigherPriorityExpression,
				(RSF)NonterminalType.MultiplicativeExpression | nAtomExpression
			);
			grammar.Add(
				nAtomExpression,
				(RSF)NonterminalType.FunctionCall | NonterminalType.Literal | tIdentifier | (tLeftParenthesis + NonterminalType.Expression + tRightParenthesis)
			);
			grammar.Add(
				NonterminalType.FunctionCall,
				(RSF)tIdentifier + tLeftParenthesis + (RSF)nCommaOrHigherPriorityExpression * '?' + tRightParenthesis
			);
			grammar.Add(
				NonterminalType.Literal,
				(RSF)tCharacterLiteral | tStringLiteral | tIntegerLiteral | tFloatLiteral
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

		EmptyStatement,

		ExpressionStatement,

		DeclarationStatement,

		ControlStatement,

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