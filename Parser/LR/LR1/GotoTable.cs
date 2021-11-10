using System;
using System.Collections.Generic;

namespace Parser.LR.LR1 {
	public class GotoTable<TNonterminal, TToken> where TNonterminal : struct, Enum where TToken : struct, Enum {
		private readonly Dictionary<ItemSet<TNonterminal, TToken>, Dictionary<TNonterminal, ItemSet<TNonterminal, TToken>>> _table = new();

		public ItemSet<TNonterminal, TToken> this[ItemSet<TNonterminal, TToken> set, TNonterminal nonTerminal] {
			get => _table[set][nonTerminal];
			set {
				if (!_table.ContainsKey(set))
					_table[set] = new Dictionary<TNonterminal, ItemSet<TNonterminal, TToken>> {[nonTerminal] = value};
				else
					_table[set][nonTerminal] = value;
			}
		}
	}
}