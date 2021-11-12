using System;
using System.Collections.Generic;

#nullable enable
namespace Parser.LR {
	public class GotoTable<TItem> where TItem : ItemBase {
		private readonly Dictionary<ItemSetBase<TItem>, Dictionary<Nonterminal, ItemSetBase<TItem>?>> _table = new();

		public ItemSetBase<TItem>? this[ItemSetBase<TItem> set, Nonterminal nonTerminal] {
			get => _table[set][nonTerminal];
			set {
				if (!_table.ContainsKey(set))
					_table[set] = new Dictionary<Nonterminal, ItemSetBase<TItem>?> {[nonTerminal] = value};
				else
					_table[set][nonTerminal] = value;
			}
		}
	}
}