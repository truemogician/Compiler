using System.Linq;

namespace Parser.LR.CLR {
	using ActionFactory = ActionFactory<Item>;

	public class ParsingTable : ParsingTable<Item> {
		public ParsingTable(Grammar grammar) : base(grammar) { }

		protected override void Initialize(Grammar extendedGrammar, out ItemSetCollectionBase<Item> itemSets, out ActionTable<Item> actionTable, out GotoTable<Item> gotoTable) {
			itemSets = new ItemSetCollection(extendedGrammar);
			actionTable = new ActionTable();
			gotoTable = new GotoTable();
			var acceptItem = new Item(extendedGrammar[extendedGrammar.InitialState].Single(), 1, Terminal.Terminator);
			foreach (var state in itemSets) {
				foreach (var item in state)
					if (item == acceptItem)
						actionTable[state, Terminal.Terminator] = ActionFactory.AcceptAction;
					else if (item.NextSymbol is null)
						actionTable[state, item.Lookahead] = ActionFactory.CreateReduceAction(item.ProductionRule);
					else if (item.NextSymbol.IsTerminal) {
						var nextTerminal = item.NextSymbol.AsTerminal;
						actionTable[state, nextTerminal] = ActionFactory.CreateShiftAction(itemSets.Go(state, nextTerminal));
					}
					else {
						var nextSymbol = item.NextSymbol;
						gotoTable[state, nextSymbol.AsNonterminal] = itemSets.Go(state, nextSymbol);
					}
			}
		}
	}
}