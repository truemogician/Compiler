using System.Linq;
using System.Text.RegularExpressions;
using Lexer;

namespace CMinusMinus {
	public partial class CMinusMinus {
		private static Lexicon InitializeLexicon() {
			var lexicon = new Lexicon();
			lexicon.Add(LexemeType.Keyword, new Regex($@"(?:{string.Join('|', Keywords.Select(k => k.Value))})\b"));
			lexicon.Add(LexemeType.Identifier, new Regex(@"[a-zA-Z_][a-zA-Z0-9_]*"));
			lexicon.Add(LexemeType.FloatLiteral, new Regex(@"[-+]?\d+(?:(?:\.\d+)?[eE][-+]?\d+|\.\d+)"));
			lexicon.Add(LexemeType.IntegerLiteral, new Regex(@"[-+]?(?:\d+(?![-+\.eE])|0b[01]+|0x[\da-fA-F])"));
			const string escapedCharPattern = @"\\(?:[abefnrtv\\'""]|[0-7]{1,3}|x[\da-fA-F]{1,2}|u[\da-fA-F]{4})";
			lexicon.Add(LexemeType.CharacterLiteral, new Regex($@"'(?:[^\\']|{escapedCharPattern})'"));
			lexicon.Add(LexemeType.StringLiteral, new Regex($@"L?""(?:[^\\""]|{escapedCharPattern})*"""));
			lexicon.Add(LexemeType.ArithmeticOperator, new Regex(@"[-+*/%]"));
			lexicon.Add(LexemeType.BitwiseOperator, new Regex(@"[&|^~]|<<|>>"));
			lexicon.Add(LexemeType.AssignmentOperator, new Regex(@"(?:[-+*/%&|^~]|<<|>>)?="));
			lexicon.Add(LexemeType.RelationalOperator, new Regex(@"[<>]|[<>=!]="));
			lexicon.Add(LexemeType.LogicalOperator, new Regex(@"!|&&|\|\|"));
			lexicon.Add(LexemeType.MembershipOperator, '.');
			lexicon.Add(LexemeType.Delimiter, ';');
			lexicon.Add(LexemeType.Separator, ',');
			lexicon.Add(LexemeType.Colon, ':');
			lexicon.Add(LexemeType.QuestionMark, '?');
			lexicon.Add(LexemeType.LeftParenthesis, '(');
			lexicon.Add(LexemeType.RightParenthesis, ')');
			lexicon.Add(LexemeType.IndexStartSymbol, '[');
			lexicon.Add(LexemeType.IndexEndSymbol, ']');
			lexicon.Add(LexemeType.BlockStartSymbol, '{');
			lexicon.Add(LexemeType.BlockEndSymbol, '}');
			lexicon.Add(LexemeType.LineComment, new Regex(@"\/\/[^\n\r]*"));
			lexicon.Add(LexemeType.BlockComment, new Regex(@"\/\*.*\*/"));
			lexicon.Add(LexemeType.LineBreak, new Regex(@"\n|\r\n?"));
			lexicon.Add(LexemeType.WhiteSpace, new Regex(@"[^\S\n\r]+"));
			return lexicon;
		}
	}

	public enum LexemeType : byte {
		Keyword,

		Identifier,

		IntegerLiteral,

		FloatLiteral,

		CharacterLiteral,

		StringLiteral,

		AssignmentOperator,

		ArithmeticOperator,

		BitwiseOperator,

		RelationalOperator,

		LogicalOperator,

		MembershipOperator,

		Delimiter,

		Separator,

		Colon,

		QuestionMark,

		LeftParenthesis,

		RightParenthesis,

		IndexStartSymbol,

		IndexEndSymbol,

		BlockStartSymbol,

		BlockEndSymbol,

		LineComment,

		BlockComment,

		LineBreak,

		WhiteSpace
	}
}