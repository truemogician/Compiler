using System.Linq;
using System.Text.RegularExpressions;
using Lexer;

namespace CMinusMinus {
	public partial class CMinusMinus {
		public static readonly string[] Keywords = {"char", "short", "int", "long", "float", "double", "void", "if", "else", "do", "while", "return"};

		public Lexer<TokenType> Lexer { get; } = new();

		private void InitializeLexer() {
			var keywordToken = new Token<TokenType>(TokenType.Keyword, $@"^({string.Join('|', Keywords)})", Keywords.Max(k => k.Length));
			Lexer.Rules.Add(keywordToken);
			var identifierPattern = new Regex($@"^@({string.Join('|', Keywords)})|^[a-zA-Z_][a-zA-Z0-9_]*", RegexOptions.Compiled);
			Lexer.AddRule(
				TokenType.Identifier,
				code => keywordToken.Match(code) is null ? identifierPattern.Match(code) : null
			);
			Lexer.AddRule(TokenType.FloatLiteral, @"[-+]?\d+(?:(?:\.\d+)?[eE][-+]?\d+|\.\d+)");
			Lexer.AddRule(TokenType.IntegerLiteral, @"[-+]?(?:\d+(?![-+\.eE])|0b[01]+|0x[\da-fA-F])");
			const string escapedCharPattern = @"\\(?:[abefnrtv\\'""]|[0-7]{1,3}|x[\da-fA-F]{1,2}|u[\da-fA-F]{4})";
			Lexer.AddRule(TokenType.CharacterLiteral, $@"'(?:[^\\']|{escapedCharPattern})'", 8);
			Lexer.AddRule(TokenType.StringLiteral, $@"L?""(?:[^\\""]|{escapedCharPattern})*""");
			Lexer.AddRule(TokenType.AssignmentOperator, @"=", 1);
			Lexer.AddRule(TokenType.ArithmeticOperator, @"[-+*/%^&|]|<<|>>>?", 3);
			Lexer.AddRule(TokenType.ComparisonOperator, @"[<>]|[<>=!]=", 2);
			Lexer.AddRule(TokenType.LogicalOperator, @"&&|\|\|", 2);
			Lexer.AddRule(TokenType.SentenceTerminator, @";", 1);
			Lexer.AddRule(TokenType.Separator, @",", 1);
			Lexer.AddRule(TokenType.CommentBoundary, @"\/\/|\/\*|\*\/", 2);
			Lexer.AddRule(TokenType.Parenthesis, @"[()]", 1);
			Lexer.AddRule(TokenType.BlockBoundary, @"[{}]", 1);
			Lexer.AddRule(TokenType.WhiteSpace, @"\s+");
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

		CommentBoundary,

		Parenthesis,

		BlockBoundary,

		WhiteSpace
	}
}
