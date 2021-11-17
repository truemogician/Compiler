using System.Linq;
using System.Text.RegularExpressions;
using Lexer;

namespace CMinusMinus {
	public partial class CMinusMinus {
		private static Lexicon InitializeLexicon() {
			var lexicon = new Lexicon();
			var keywordToken = new Token(TokenType.Keyword, new Regex($@"{string.Join('|', Keywords.Select(k => k.Value))}"), Keywords.Max(k => k.Value.Length));
			lexicon.Add(keywordToken);
			var identifierPattern = new Regex($@"^@({string.Join('|', Keywords.Select(k => k.Value))})|^[a-zA-Z_][a-zA-Z0-9_]*", RegexOptions.Compiled);
			lexicon.Add(
				TokenType.Identifier,
				code => keywordToken.Match(code) is not null ? null : identifierPattern.Match(code)
			);
			lexicon.Add(TokenType.FloatLiteral, new Regex(@"[-+]?\d+(?:(?:\.\d+)?[eE][-+]?\d+|\.\d+)"));
			lexicon.Add(TokenType.IntegerLiteral, new Regex(@"[-+]?(?:\d+(?![-+\.eE])|0b[01]+|0x[\da-fA-F])"));
			const string escapedCharPattern = @"\\(?:[abefnrtv\\'""]|[0-7]{1,3}|x[\da-fA-F]{1,2}|u[\da-fA-F]{4})";
			lexicon.Add(TokenType.CharacterLiteral, new Regex($@"'(?:[^\\']|{escapedCharPattern})'"), 8);
			lexicon.Add(TokenType.StringLiteral, new Regex($@"L?""(?:[^\\""]|{escapedCharPattern})*"""));
			lexicon.Add(TokenType.AssignmentOperator, '=');
			lexicon.Add(TokenType.ArithmeticOperator, new Regex(@"[-+*/%]|<<|>>>?"), 3);
			lexicon.Add(TokenType.ComparisonOperator, new Regex(@"[<>]|[<>=!]="), 2);
			lexicon.Add(TokenType.LogicalOperator, new Regex(@"&&|\|\|"), 2);
			lexicon.Add(TokenType.SentenceTerminator, ';');
			lexicon.Add(TokenType.Separator, ',');
			lexicon.Add(TokenType.SingleLineCommentStartSymbol, "//");
			lexicon.Add(TokenType.CommentBlockStartSymbol, "/*");
			lexicon.Add(TokenType.CommentBlockEndSymbol, "*/");
			lexicon.Add(TokenType.LeftParenthesis, '(');
			lexicon.Add(TokenType.RightParenthesis, ')');
			lexicon.Add(TokenType.BlockStartSymbol, '{');
			lexicon.Add(TokenType.BlockEndSymbol, '}');
			lexicon.Add(TokenType.LineBreak, new Regex(@"\n|\r\n?"), 2);
			lexicon.Add(TokenType.WhiteSpace, new Regex(@"[^\S\n\r]+"));
			return lexicon;
		}
	}

	public enum TokenType : byte {
		Keyword,

		Identifier,

		IntegerLiteral,

		FloatLiteral,

		CharacterLiteral,

		StringLiteral,

		AssignmentOperator,

		ArithmeticOperator,

		ComparisonOperator,

		LogicalOperator,

		SentenceTerminator,

		Separator,

		SingleLineCommentStartSymbol,

		CommentBlockStartSymbol,

		CommentBlockEndSymbol,

		LeftParenthesis,

		RightParenthesis,

		BlockStartSymbol,

		BlockEndSymbol,

		LineBreak,

		WhiteSpace
	}
}
