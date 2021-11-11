using System;
using System.Collections.Generic;

#nullable enable
namespace Parser.LR {
	public class GotoTable<TNonterminal, TToken, TItem> where TNonterminal : struct, Enum where TToken : struct, Enum where TItem : IItem<TNonterminal, TToken> {
		private readonly Dictionary<ItemSetBase<TNonterminal, TToken, TItem>, Dictionary<TNonterminal, ItemSetBase<TNonterminal, TToken, TItem>?>> _table = new();

		public ItemSetBase<TNonterminal, TToken, TItem>? this[ItemSetBase<TNonterminal, TToken, TItem> set, TNonterminal nonTerminal] {
			get => _table[set][nonTerminal];
			set {
				if (!_table.ContainsKey(set))
					_table[set] = new Dictionary<TNonterminal, ItemSetBase<TNonterminal, TToken, TItem>?> {[nonTerminal] = value};
				else
					_table[set][nonTerminal] = value;
			}
		}
	}
}