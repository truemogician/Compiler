using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

		public Token this[Enum index] {
			get => _tokens[Enum.GetName(index.GetType(), index)!];
			set => _tokens[Enum.GetName(index.GetType(), index)!] = value;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public IEnumerator<Token> GetEnumerator() => _tokens.Values.GetEnumerator();

		public void Add(Token token) => _tokens.Add(token.Name, token);

		public void Clear() => _tokens.Clear();

		public bool Contains(Token token) => _tokens.ContainsKey(token.Name);

		public void CopyTo(Token[] array, int arrayIndex) => _tokens.Values.CopyTo(array, arrayIndex);

		public bool Remove(Token token) => _tokens.Remove(token.Name);

		/// <inheritdoc cref="Token(string, char)"/>
		public void Add(string name, char character) => Add(new Token(name, character));

		/// <inheritdoc cref="Token(string, string)"/>
		public void Add(string name, string pattern) => Add(new Token(name, pattern));

		/// <inheritdoc cref="Token(string, Regex, int)"/>
		public void Add(string name, Regex pattern, int maxLength = 0) => Add(new Token(name, pattern, maxLength));

		/// <inheritdoc cref="Token(string, LexemeMatcher)"/>
		public void Add(string name, LexemeMatcher match) => Add(new Token(name, match));

		/// <inheritdoc cref="Token(Enum, char)"/>
		public void Add(Enum name, char character) => Add(new Token(name, character));

		/// <inheritdoc cref="Token(Enum, string)"/>
		public void Add(Enum name, string pattern) => Add(new Token(name, pattern));

		/// <inheritdoc cref="Token(Enum, Regex, int)"/>
		public void Add(Enum name, Regex pattern, int maxLength = 0) => Add(new Token(name, pattern, maxLength));

		/// <inheritdoc cref="Token(Enum, LexemeMatcher)"/>
		public void Add(Enum name, LexemeMatcher match) => Add(new Token(name, match));

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