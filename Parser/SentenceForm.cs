using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parser {
	public class SentenceForm<TNonterminal, TToken> : IList<Symbol<TNonterminal, TToken>> where TNonterminal : struct, Enum where TToken : struct, Enum {
		private readonly List<Symbol<TNonterminal, TToken>> _list = new();

		public SentenceForm(params Symbol<TNonterminal, TToken>[] items) => _list.AddRange(items);

		public int Count => _list.Count;

		public bool IsReadOnly => false;

		public IEnumerable<Terminal<TToken>> Terminals => _list.Where(s => s.IsTerminal).Select(s => s.AsTerminal).Distinct();

		public IEnumerable<TNonterminal> Nonterminals => _list.Where(s => !s.IsTerminal).Select(s => s.AsNonterminal).Distinct();

		public Symbol<TNonterminal, TToken> this[int index] {
			get => _list[index];
			set => _list[index] = value;
		}

		public IEnumerator<Symbol<TNonterminal, TToken>> GetEnumerator() => _list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(Symbol<TNonterminal, TToken> item) => _list.Add(item);

		public void Clear() => _list.Clear();

		public bool Contains(Symbol<TNonterminal, TToken> item) => _list.Contains(item);

		public void CopyTo(Symbol<TNonterminal, TToken>[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

		public bool Remove(Symbol<TNonterminal, TToken> item) => _list.Remove(item);

		public int IndexOf(Symbol<TNonterminal, TToken> item) => _list.IndexOf(item);

		public void Insert(int index, Symbol<TNonterminal, TToken> item) => _list.Insert(index, item);

		public void RemoveAt(int index) => _list.RemoveAt(index);

		public static implicit operator SentenceForm<TNonterminal, TToken>(TNonterminal nonTerminal) => new(nonTerminal);

		public static implicit operator SentenceForm<TNonterminal, TToken>(Terminal<TToken> terminal) => new(terminal);
	}
}