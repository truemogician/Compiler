using System;
using System.Collections.Generic;

namespace Parser.CanonicalLR {
	public class GotoTable<TNonterminal, TTerminal> where TNonterminal : struct, Enum where TTerminal : struct, Enum {
		private readonly Dictionary<ItemSet<TNonterminal, TTerminal>, Dictionary<TNonterminal, ItemSet<TNonterminal, TTerminal>>> _table = new();

		public ItemSet<TNonterminal, TTerminal> this[ItemSet<TNonterminal, TTerminal> set, TNonterminal nonTerminal] {
			get => _table[set][nonTerminal];
			set {
				if (!_table.ContainsKey(set))
					_table[set] = new Dictionary<TNonterminal, ItemSet<TNonterminal, TTerminal>> {[nonTerminal] = value};
				else
					_table[set][nonTerminal] = value;
			}
		}
	}
}