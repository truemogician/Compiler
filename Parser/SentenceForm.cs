using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lexer;

namespace Parser {
	public class SentenceForm : IReadOnlyList<Symbol>, IEquatable<SentenceForm> {
		private readonly List<Symbol> _list;

		private SentenceForm() => _list = new List<Symbol>();

		public SentenceForm(IEnumerable<Symbol> items) : this(items.ToArray()) { }

		public SentenceForm(params Symbol[] items) => _list = new List<Symbol>(items);

		public static SentenceForm Empty { get; } = new();

		public IEnumerable<Terminal> Terminals => _list.Where(s => s.IsTerminal).Select(s => s.AsTerminal).Distinct();

		public IEnumerable<Nonterminal> Nonterminals => _list.Where(s => !s.IsTerminal).Select(s => s.AsNonterminal).Distinct();

		public Symbol this[Index index] {
			get => _list[index];
			internal set => _list[index] = value;
		}

		public SentenceForm this[Range range] {
			get {
				int start = range.Start.IsFromEnd ? _list.Count - range.Start.Value : range.Start.Value;
				int end = range.End.IsFromEnd ? _list.Count - range.End.Value : range.End.Value;
				if (start < 0 || end > _list.Count || start > end)
					throw new ArgumentOutOfRangeException(nameof(range));
				return start == end ? Empty : new SentenceForm(_list.GetRange(start, end - start));
			}
			internal set {
				int start = range.Start.IsFromEnd ? _list.Count - range.Start.Value : range.Start.Value;
				int end = range.End.IsFromEnd ? _list.Count - range.End.Value : range.End.Value;
				_list.RemoveRange(start, end - start);
				_list.InsertRange(start, value);
			}
		}

		public bool Equals(SentenceForm other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;
			if (_list.Count != other.Count)
				return false;
			return !_list.Where((t, i) => !t.Equals(other._list[i])).Any();
		}

		public int Count => _list.Count;

		public Symbol this[int index] {
			get => _list[index];
			internal set => _list[index] = value;
		}

		public IEnumerator<Symbol> GetEnumerator() => _list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public override bool Equals(object obj) {
			if (obj is null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((SentenceForm)obj);
		}

		public override int GetHashCode() => _list != null ? _list.GetHashCode() : 0;

		public override string ToString() {
			var builder = new StringBuilder();
			foreach (var symbol in _list)
				builder.Append(symbol.IsTerminal ? $"{{{symbol}}}" : $"<{symbol}>");
			return builder.ToString();
		}

		public static implicit operator SentenceForm(Symbol symbol) => new(symbol);

		public static implicit operator SentenceForm(Nonterminal nonterminal) => new(nonterminal);

		public static implicit operator SentenceForm(string nonterminal) => new(nonterminal);

		public static implicit operator SentenceForm(Enum nonterminal) => new(nonterminal);

		public static implicit operator SentenceForm(Terminal terminal) => new(terminal);

		public static implicit operator SentenceForm(Lexeme lexeme) => new(lexeme);

		public static SentenceForm operator +(SentenceForm left, SentenceForm right) => new(left._list.Concat(right._list).ToArray());

		public static SentenceForm operator +(Symbol left, SentenceForm right) => (SentenceForm)left + right;

		public static SentenceForm operator +(SentenceForm left, Symbol right) => left + (SentenceForm)right;

		public static SentenceForm operator +(Nonterminal left, SentenceForm right) => (SentenceForm)left + right;

		public static SentenceForm operator +(SentenceForm left, Nonterminal right) => left + (SentenceForm)right;

		public static SentenceForm operator +(Terminal left, SentenceForm right) => (SentenceForm)left + right;

		public static SentenceForm operator +(SentenceForm left, Terminal right) => left + (SentenceForm)right;

		public static SentenceForm operator *(SentenceForm self, int count) {
			switch (count) {
				case < 0: throw new ArgumentOutOfRangeException(nameof(count));
				case 0:   return Empty;
			}
			var result = new SentenceForm(self);
			var operand = new SentenceForm(self);
			while (--count > 0)
				result += operand;
			return result;
		}
	}
}