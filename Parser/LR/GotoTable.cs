using System;
using System.Collections.Generic;

#nullable enable
namespace Parser.LR {
	public abstract class GotoTable<TItem> where TItem : ItemBase {
		protected readonly Dictionary<ItemSet<TItem>, Dictionary<Nonterminal, ItemSet<TItem>?>> Table = new();

		internal ItemSet<TItem>? this[ItemSet<TItem> state, Nonterminal nonterminal] {
			get => Table[state][nonterminal];
			set {
				if (!Table.ContainsKey(state))
					Table[state] = new Dictionary<Nonterminal, ItemSet<TItem>?> {[nonterminal] = value};
				else
					Table[state][nonterminal] = value;
			}
		}
	}
}