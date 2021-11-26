using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;

namespace Lexer {
	public class Lexicon : ICollection<Lexeme> {
		private readonly Dictionary<string, Lexeme> _lexemes = new();

		public Lexeme this[string type] {
			get => _lexemes[type];
			set => _lexemes[type] = value;
		}

		public Lexeme this[Enum index] {
			get => _lexemes[Enum.GetName(index.GetType(), index)!];
			set => _lexemes[Enum.GetName(index.GetType(), index)!] = value;
		}

		public int Count => _lexemes.Count;

		public bool IsReadOnly => false;

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public IEnumerator<Lexeme> GetEnumerator() => _lexemes.Values.GetEnumerator();

		public void Add(Lexeme lexeme) => _lexemes.Add(lexeme.Name, lexeme);

		public void Clear() => _lexemes.Clear();

		public bool Contains(Lexeme lexeme) => _lexemes.ContainsKey(lexeme.Name);

		public void CopyTo(Lexeme[] array, int arrayIndex) => _lexemes.Values.CopyTo(array, arrayIndex);

		public bool Remove(Lexeme lexeme) => _lexemes.Remove(lexeme.Name);

		/// <inheritdoc cref="Lexeme(string, char)" />
		public void Add(string name, char character) => Add(new Lexeme(name, character));

		/// <inheritdoc cref="Lexeme(string, string)" />
		public void Add(string name, string pattern) => Add(new Lexeme(name, pattern));

		/// <inheritdoc cref="Lexeme(string, Regex, int)" />
		public void Add(string name, Regex pattern) => Add(new Lexeme(name, pattern));

		/// <inheritdoc cref="Lexeme(Enum, char)" />
		public void Add(Enum name, char character) => Add(new Lexeme(name, character));

		/// <inheritdoc cref="Lexeme(Enum, string)" />
		public void Add(Enum name, string pattern) => Add(new Lexeme(name, pattern));

		/// <inheritdoc cref="Lexeme(Enum, Regex, int)" />
		public void Add(Enum name, Regex pattern) => Add(new Lexeme(name, pattern));

		public Token? Match(StringSegment code) {
			foreach (var (_, lexeme) in _lexemes)
				if (lexeme.Match(code) is { } result)
					return result;
			return null;
		}

		public IEnumerable<Token> MatchAll(StringSegment code) {
			foreach (var (_, lexeme) in _lexemes)
				if (lexeme.Match(code) is { } result)
					yield return result;
		}
	}
}