using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parser {
	public class SentenceForm : IReadOnlyList<Symbol>, IEquatable<SentenceForm> {
		private readonly List<Symbol> _list;

		private SentenceForm() => _list = new List<Symbol>();

		public SentenceForm(IEnumerable<Symbol> items) : this(items.ToArray()) { }

		public SentenceForm(params Symbol[] items) => _list = new List<Symbol>(items);

		public static SentenceForm Empty { get; } = new();

		public IEnumerable<Terminal> Terminals => _list.Where(s => s.IsTerminal).Select(s => s.AsTerminal).Distinct();

		public IEnumerable<Nonterminal> Nonterminals => _list.Where(s => !s.IsTerminal).Select(s => s.AsNonterminal).Distinct();

		public Symbol this[Index index] => _list[index];

		public SentenceForm this[Range range] {
			get {
				var arr = new Symbol[range.End.Value - range.Start.Value];
				for (var i = 0; i < arr.Length; ++i)
					arr[i] = _list[i + range.Start.Value];
				return new SentenceForm(arr);
			}
			//Bug: different length
			private set {
				for (var i = 0; i < value.Count; ++i)
					_list[i + range.Start.Value] = value[i];
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

		public Symbol this[int index] => _list[index];

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

		public static implicit operator SentenceForm(Symbol symbol) => new(symbol);

		public static implicit operator SentenceForm(Nonterminal nonTerminal) => new(nonTerminal);

		public static implicit operator SentenceForm(Terminal terminal) => new(terminal);

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