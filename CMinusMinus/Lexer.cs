using System.Linq;
using System.Text.RegularExpressions;
using Lexer;

namespace CMinusMinus {
	public partial class CMinusMinus {
		public static readonly string[] Keywords = {"char", "short", "int", "long", "float", "double", "void", "if", "else", "do", "while", "return"};

		public Lexer.Lexer Lexer { get; } = new();

		private void InitializeLexer() {
			var keywordToken = new Token(TokenType.Keyword, new Regex($@"{string.Join('|', Keywords)}"), Keywords.Max(k => k.Length));
			Lexer.AddToken(keywordToken);
			var identifierPattern = new Regex($@"^@({string.Join('|', Keywords)})|^[a-zA-Z_][a-zA-Z0-9_]*", RegexOptions.Compiled);
			Lexer.AddToken(
				TokenType.Identifier,
				code => keywordToken.Match(code) is not null ? null : identifierPattern.Match(code)
			);
			Lexer.AddToken(TokenType.FloatLiteral, new Regex(@"[-+]?\d+(?:(?:\.\d+)?[eE][-+]?\d+|\.\d+)"));
			Lexer.AddToken(TokenType.IntegerLiteral, new Regex(@"[-+]?(?:\d+(?![-+\.eE])|0b[01]+|0x[\da-fA-F])"));
			const string escapedCharPattern = @"\\(?:[abefnrtv\\'""]|[0-7]{1,3}|x[\da-fA-F]{1,2}|u[\da-fA-F]{4})";
			Lexer.AddToken(TokenType.CharacterLiteral, new Regex($@"'(?:[^\\']|{escapedCharPattern})'"), 8);
			Lexer.AddToken(TokenType.StringLiteral, new Regex($@"L?""(?:[^\\""]|{escapedCharPattern})*"""));
			Lexer.AddToken(TokenType.AssignmentOperator, '=');
			Lexer.AddToken(TokenType.ArithmeticOperator, new Regex(@"[-+*/%^&|]|<<|>>>?"), 3);
			Lexer.AddToken(TokenType.ComparisonOperator, new Regex(@"[<>]|[<>=!]="), 2);
			Lexer.AddToken(TokenType.LogicalOperator, new Regex(@"&&|\|\|"), 2);
			Lexer.AddToken(TokenType.SentenceTerminator, ';');
			Lexer.AddToken(TokenType.Separator, ',');
			Lexer.AddToken(TokenType.SingleLineCommentStartSymbol, "//");
			Lexer.AddToken(TokenType.CommentBlockStartSymbol, "/*");
			Lexer.AddToken(TokenType.CommentBlockEndSymbol, "*/");
			Lexer.AddToken(TokenType.LeftParenthesis, '(');
			Lexer.AddToken(TokenType.RightParenthesis, ')');
			Lexer.AddToken(TokenType.BlockStartSymbol, '{');
			Lexer.AddToken(TokenType.BlockEndSymbol, '}');
			Lexer.AddToken(TokenType.LineBreak, new Regex(@"\n|\r\n?"), 2);
			Lexer.AddToken(TokenType.WhiteSpace, new Regex(@"[^\S\n\r]+"));
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
