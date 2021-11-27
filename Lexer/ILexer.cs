using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable
namespace Lexer {
	public interface ILexer {
		public IEnumerable<Token> Tokenize(string code);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryTokenize(string code, out IEnumerable<Token>? tokens) {
			try {
				tokens = Tokenize(code);
				return true;
			}
			catch (LexerException) {
				tokens = null;
				return false;
			}
		}
	}
}