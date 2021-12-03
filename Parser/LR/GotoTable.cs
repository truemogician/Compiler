using System.Collections.Generic;
using TrueMogician.Extensions.Enumerable;

namespace Parser.LR {
	public abstract class GotoTable<TItem> where TItem : ItemBase {
		protected readonly Dictionary<ItemSet<TItem>, Dictionary<Nonterminal, ItemSet<TItem>?>> Table = new();

		public IReadOnlyDictionary<ItemSet<TItem>, IReadOnlyDictionary<Nonterminal, ItemSet<TItem>?>> RawTable => Table.ToValueReadOnly<ItemSet<TItem>, Dictionary<Nonterminal, ItemSet<TItem>?>, IReadOnlyDictionary<Nonterminal, ItemSet<TItem>?>>();

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