using System.Collections.Generic;
using System.Linq;
using Parser.LR.CLR;

namespace Parser.LR.GLR {
	using ActionFactory = ActionFactory<Item>;

	public class ParsingTable : ParsingTableBase<Item, List<IAction>> {
		internal ParsingTable(Grammar grammar) : base(grammar) { }

		/// <param name="checkConflicts">
		///     GLR parser preserve all conflicts, thus <paramref name="checkConflicts" /> will be ignored
		///     in this implementation
		/// </param>
		protected override void Initialize(Grammar extendedGrammar, bool checkConflicts = true) {
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
						ActionTable[state, Terminal.Terminator] = new List<IAction> {ActionFactory.AcceptAction};
					else if (item.NextSymbol is null) {
						var reduceAction = ActionFactory.CreateReduceAction(item.ProductionRule);
						var actions = ActionTable[state, item.Lookahead];
						if (actions.Count == 0) {
							actions.Add(reduceAction);
							ActionTable[state, item.Lookahead] = actions;
						}
						else if (actions.IndexOf(reduceAction) == -1)
							actions.Add(reduceAction);
					}
					else if (item.NextSymbol.IsTerminal) {
						var nextTerminal = item.NextSymbol.AsTerminal;
						var shiftAction = ActionFactory.CreateShiftAction(ItemSets.Go(state, nextTerminal));
						var actions = ActionTable[state, nextTerminal];
						if (actions.Count == 0) {
							actions.Add(shiftAction);
							ActionTable[state, nextTerminal] = actions;
						}
						else if (actions.IndexOf(shiftAction) == -1)
							actions.Add(shiftAction);
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