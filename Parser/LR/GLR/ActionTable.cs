using System.Collections.Generic;

namespace Parser.LR.GLR {
	using Item = CLR.Item;
	using ItemSet = ItemSet<CLR.Item>;

	public class ActionTable : ActionTable<Item, List<IAction>> {
		/// <returns>If not found, an empty <see cref="List{IAction}" /> will be returned</returns>
		public override List<IAction> this[ItemSet state, Terminal terminal] {
			get => Table.ContainsKey(state) && Table[state].ContainsKey(terminal) ? Table[state][terminal] : new List<IAction>();
			set => base[state, terminal] = value;
		}
	}
}