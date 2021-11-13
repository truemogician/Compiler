using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parser {
	public class SentenceForm : IList<Symbol>, IEquatable<SentenceForm> {
		private readonly List<Symbol> _list = new();

		private SentenceForm() { }

		public SentenceForm(IEnumerable<Symbol> items) : this(items.ToArray()) { }

		public SentenceForm(params Symbol[] items) => _list.AddRange(items);

		public static SentenceForm Empty { get; } = new();

		public int Count => _list.Count;

		public bool IsReadOnly => false;

		public IEnumerable<Terminal> Terminals => _list.Where(s => s.IsTerminal).Select(s => s.AsTerminal).Distinct();

		public IEnumerable<Nonterminal> Nonterminals => _list.Where(s => !s.IsTerminal).Select(s => s.AsNonterminal).Distinct();

		public Symbol this[int index] {
			get => _list[index];
			set => _list[index] = value;
		}

		public Symbol this[Index index] {
			get => _list[index];
			set => _list[index] = value;
		}

		public SentenceForm this[Range range] {
			get {
				var arr = new Symbol[range.End.Value - range.Start.Value];
				for (var i = 0; i < arr.Length; ++i)
					arr[i] = _list[i + range.Start.Value];
				return new SentenceForm(arr);
			}
			set {
				for (var i = 0; i < value.Count; ++i)
					_list[i + range.Start.Value] = value[i];
			}
		}

		public IEnumerator<Symbol> GetEnumerator() => _list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(Symbol item) => _list.Add(item);

		public void Clear() => _list.Clear();

		public bool Contains(Symbol item) => _list.Contains(item);

		public void CopyTo(Symbol[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

		public bool Remove(Symbol item) => _list.Remove(item);

		public int IndexOf(Symbol item) => _list.IndexOf(item);

		public void Insert(int index, Symbol item) => _list.Insert(index, item);

		public void RemoveAt(int index) => _list.RemoveAt(index);

		public bool Equals(SentenceForm other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;
			if (_list.Count != other.Count)
				return false;
			return !_list.Where((t, i) => !t.Equals(other._list[i])).Any();
		}

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
	}
}