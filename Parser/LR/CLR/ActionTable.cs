namespace Parser.LR.CLR {
	using ActionFactory = ActionFactory<Item>;
	using ItemSet = ItemSet<Item>;

	public class ActionTable : ActionTable<Item> {
		public override IAction this[ItemSet state, Terminal terminal] {
			get => Table.ContainsKey(state) && Table[state].ContainsKey(terminal) ? Table[state][terminal] : ActionFactory.ErrorAction;
			set => base[state, terminal] = value;
		}
	}
}