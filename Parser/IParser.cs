using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Lexer;

#nullable enable
namespace Parser {
	public interface IParser {
		public AbstractSyntaxTree Parse(IEnumerable<Token> tokens);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryParse(IEnumerable<Token> tokens, out AbstractSyntaxTree? ast) {
			try {
				ast = Parse(tokens);
				return true;
			}
			catch {
				ast = null;
				return false;
			}
		}
	}
}