using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parser {
	public class SentenceForm : IList<Symbol> {
		private readonly List<Symbol> _list = new();

		public SentenceForm(params Symbol[] items) => _list.AddRange(items);

		public int Count => _list.Count;

		public bool IsReadOnly => false;

		public IEnumerable<Terminal> Terminals => _list.Where(s => s.IsTerminal).Select(s => s.AsTerminal).Distinct();

		public IEnumerable<Nonterminal> Nonterminals => _list.Where(s => !s.IsTerminal).Select(s => s.AsNonterminal).Distinct();

		public Symbol this[int index] {
			get => _list[index];
			set => _list[index] = value;
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

		public static implicit operator SentenceForm(Nonterminal nonTerminal) => new(nonTerminal);

		public static implicit operator SentenceForm(Terminal terminal) => new(terminal);
	}
}