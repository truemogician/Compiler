using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

		public Token? Match(StringSegment code, MatchStrategy strategy = MatchStrategy.Longest) {
			var pStrategy = (MatchStrategy)Math.Max((byte)strategy & 0b111, 1);
			var lStrategy = (MatchStrategy)Math.Max((byte)strategy & 0b111000, 8);
			for (byte count = 0, tmp = (byte)pStrategy; tmp > 0; tmp >>= 1)
				if ((tmp & 1) == 1)
					count = count == 0 ? (byte)1 : throw new ArgumentException("Invalid combination of strategies", nameof(strategy));
			for (byte count = 0, tmp = (byte)lStrategy; tmp > 0; tmp >>= 1)
				if ((tmp & 1) == 1)
					count = count == 0 ? (byte)1 : throw new ArgumentException("Invalid combination of strategies", nameof(strategy));
			var matches = _lexemes.Values.Select(l => l.Match(code)).Where(t => t is not null);
			return (lStrategy, pStrategy) switch {
				(MatchStrategy.AnyLength, MatchStrategy.First)  => matches.FirstOrDefault(),
				(MatchStrategy.AnyLength, MatchStrategy.Last)   => matches.LastOrDefault(),
				(MatchStrategy.AnyLength, MatchStrategy.Single) => matches.SingleOrDefault(),
				(MatchStrategy.Longest, _) => matches.Aggregate<Token?, Token?>(
					null,
					(result, token) => {
						if (result is null || token!.Length > result.Length)
							return token;
						if (token.Length == result.Length)
							return pStrategy switch {
								MatchStrategy.First  => result,
								MatchStrategy.Last   => token,
								MatchStrategy.Single => throw new AmbiguousMatchException()
							};
						return result;
					}
				),
				(MatchStrategy.Shortest, _) => matches.Aggregate<Token?, Token?>(
					null,
					(result, token) => {
						if (result is null || token!.Length < result.Length)
							return token;
						if (token.Length == result.Length)
							return pStrategy switch {
								MatchStrategy.First  => result,
								MatchStrategy.Last   => token,
								MatchStrategy.Single => throw new AmbiguousMatchException()
							};
						return result;
					}
				)
			};
		}
	}

	[Flags]
	public enum MatchStrategy : byte {
		First = 1,

		Last = 2,

		Single = 4,

		AnyLength = 8,

		Longest = 16,

		Shortest = 32
	}
}