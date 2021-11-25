using System.Collections.Generic;

#nullable enable
namespace Parser.LR {
	public abstract class GotoTable<TItem> where TItem : ItemBase {
		internal readonly Dictionary<ItemSet<TItem>, Dictionary<Nonterminal, ItemSet<TItem>?>> Table = new();

		public virtual ItemSet<TItem>? this[ItemSet<TItem> state, Nonterminal nonterminal] {
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