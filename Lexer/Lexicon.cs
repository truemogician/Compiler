using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

#nullable enable
namespace Lexer {
	public class Lexicon : ICollection<Token> {
		private readonly Dictionary<string, Token> _tokens = new();

		public int Count => _tokens.Count;

		public bool IsReadOnly => false;

		public Token this[string type] {
			get => _tokens[type];
			set => _tokens[type] = value;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public IEnumerator<Token> GetEnumerator() => _tokens.Values.GetEnumerator();

		public void Add(Token token) => _tokens.Add(token.Name, token);

		public void Clear() => _tokens.Clear();

		public bool Contains(Token token) => _tokens.ContainsKey(token.Name);

		public void CopyTo(Token[] array, int arrayIndex) => _tokens.Values.CopyTo(array, arrayIndex);

		public bool Remove(Token token) => _tokens.Remove(token.Name);

		public Lexeme? Match(StringSegment code) {
			foreach (var (_, token) in _tokens)
				if (token.Match(code) is { } result)
					return result;
			return null;
		}

		public IEnumerable<Lexeme> MatchAll(StringSegment code) {
			foreach (var (_, token) in _tokens)
				if (token.Match(code) is { } result)
					yield return result;
		}
	}
}