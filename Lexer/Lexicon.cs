using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

#nullable enable
namespace Lexer {
	public class Lexicon<T> : ICollection<Token<T>> where T : struct, Enum {
		private readonly Dictionary<T, Token<T>> _tokens = new();

		public int Count => _tokens.Count;

		public bool IsReadOnly => false;

		public Token<T> this[T type] {
			get => _tokens[type];
			set => _tokens[type] = value;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public IEnumerator<Token<T>> GetEnumerator() => _tokens.Values.GetEnumerator();

		public void Add(Token<T> token) => _tokens.Add(token.Type, token);

		public void Clear() => _tokens.Clear();

		public bool Contains(Token<T> token) => _tokens.ContainsKey(token.Type);

		public void CopyTo(Token<T>[] array, int arrayIndex) => _tokens.Values.CopyTo(array, arrayIndex);

		public bool Remove(Token<T> token) => _tokens.Remove(token.Type);

		public Lexeme<T>? Match(StringSegment code) {
			foreach (var (_, token) in _tokens)
				if (token.Match(code) is { } result)
					return result;
			return null;
		}

		public IEnumerable<Lexeme<T>> MatchAll(StringSegment code) {
			foreach (var (_, token) in _tokens)
				if (token.Match(code) is { } result)
					yield return result;
		}
	}
}