using System.Linq;

namespace Parser.LR.CLR {
	using ActionFactory = ActionFactory<Item>;

	public class ParsingTable : ParsingTableBase<Item> {
		public ParsingTable(Grammar grammar) : base(grammar) { }

		protected override void Initialize(Grammar extendedGrammar) {
			ItemSets = new ItemSetCollection(extendedGrammar);
			OnStartItemSetsCalculation();
			ItemSets.Initialize();
			OnCompleteItemSetsCalculation();
			ActionTable = new ActionTable();
			GotoTable = new GotoTable();
			var acceptItem = new Item(extendedGrammar[extendedGrammar.InitialState].Single(), 1, Terminal.Terminator);
			OnStartTableCalculation();
			foreach (var state in ItemSets) {
				foreach (var item in state)
					if (item == acceptItem)
						ActionTable[state, Terminal.Terminator] = ActionFactory.AcceptAction;
					else if (item.NextSymbol is null)
						ActionTable[state, item.Lookahead] = ActionFactory.CreateReduceAction(item.ProductionRule);
					else if (item.NextSymbol.IsTerminal) {
						var nextTerminal = item.NextSymbol.AsTerminal;
						ActionTable[state, nextTerminal] = ActionFactory.CreateShiftAction(ItemSets.Go(state, nextTerminal));
					}
					else {
						var nextSymbol = item.NextSymbol;
						GotoTable[state, nextSymbol.AsNonterminal] = ItemSets.Go(state, nextSymbol);
					}
			}
			OnCompleteTableCalculation();
		}
	}
}