#nullable enable
namespace Parser.LR.CLR {
	using ItemSet = ItemSet<Item>;

	public class GotoTable : GotoTable<Item> {
		public ItemSet? this[ItemSet state, Nonterminal nonterminal] {
			get => Table.ContainsKey(state) && Table[state].ContainsKey(nonterminal) ? Table[state][nonterminal] : null;
			set => base[state, nonterminal] = value;
		}
	}
}