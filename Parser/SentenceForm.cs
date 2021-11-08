using System;
using System.Collections;
using System.Collections.Generic;

namespace Parser {
	public class SentenceForm<TNonterminal, TTerminal> : IList<Component<TNonterminal, TTerminal>> where TNonterminal : struct, Enum where TTerminal : struct, Enum {
		private readonly List<Component<TNonterminal, TTerminal>> _list = new();

		public SentenceForm(params Component<TNonterminal, TTerminal>[] items) => _list.AddRange(items);

		public IEnumerator<Component<TNonterminal, TTerminal>> GetEnumerator() => _list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(Component<TNonterminal, TTerminal> item) => _list.Add(item);

		public void Clear() => _list.Clear();

		public bool Contains(Component<TNonterminal, TTerminal> item) => _list.Contains(item);

		public void CopyTo(Component<TNonterminal, TTerminal>[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

		public bool Remove(Component<TNonterminal, TTerminal> item) => _list.Remove(item);

		public int Count => _list.Count;

		public bool IsReadOnly => false;

		public int IndexOf(Component<TNonterminal, TTerminal> item) => _list.IndexOf(item);

		public void Insert(int index, Component<TNonterminal, TTerminal> item) => _list.Insert(index, item);

		public void RemoveAt(int index) => _list.RemoveAt(index);

		public Component<TNonterminal, TTerminal> this[int index] {
			get => _list[index];
			set => _list[index] = value;
		}

		public static implicit operator SentenceForm<TNonterminal, TTerminal>(TNonterminal nonTerminal) => new(nonTerminal);

		public static implicit operator SentenceForm<TNonterminal, TTerminal>(TTerminal token) => new(token);
	}
}