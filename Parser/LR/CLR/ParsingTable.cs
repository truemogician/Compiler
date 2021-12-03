using System;
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
					else if (item.NextSymbol is null) {
						var reduceAction = ActionFactory.CreateReduceAction(item.ProductionRule);
						if (ActionTable[state, item.Lookahead] is {Type: not ActionType.Error} action) {
							if (action.Equals(reduceAction))
								continue;
							throw new ActionConflictException<Item> {State = state, Terminal = item.Lookahead, ConflictingActions = new[] {action, reduceAction}};
						}
						ActionTable[state, item.Lookahead] = reduceAction;
					}
					else if (item.NextSymbol.IsTerminal) {
						var nextTerminal = item.NextSymbol.AsTerminal;
						var shiftAction = ActionFactory.CreateShiftAction(ItemSets.Go(state, nextTerminal));
						if (ActionTable[state, nextTerminal] is {Type: not ActionType.Error} action) {
							if (action.Equals(shiftAction))
								continue;
							throw new ActionConflictException<Item> {State = state, Terminal = nextTerminal, ConflictingActions = new[] {action, shiftAction}};
						}
						ActionTable[state, nextTerminal] = shiftAction;
					}
					else {
						var nextSymbol = item.NextSymbol;
						var gotoState = ItemSets.Go(state, nextSymbol);
						if (GotoTable[state, nextSymbol.AsNonterminal] is { } existingState) {
							if (existingState.Equals(gotoState))
								continue;
							throw new GotoConflictException<Item> {State = state, Nonterminal = nextSymbol.AsNonterminal, ConflictingGotoStates = new[] {existingState, gotoState}};
						}
						GotoTable[state, nextSymbol.AsNonterminal] = gotoState;
					}
			}
			OnCompleteTableCalculation();
		}
	}
}