using System.Linq;
using System.Threading.Tasks;

namespace Parser.LR.CLR {
	using ActionFactory = ActionFactory<Item>;

	public class ParsingTable : ParsingTable<Item> {
		public ParsingTable(Grammar grammar) : base(grammar) { }

		protected override void Initialize(Grammar extendedGrammar, out ItemSetCollectionBase<Item> itemSets, out ActionTable<Item> actionTable, out GotoTable<Item> gotoTable) {
			var @is = new ItemSetCollection(extendedGrammar);
			var at = new ActionTable();
			var gt = new GotoTable();
			var acceptItem = new Item(extendedGrammar[extendedGrammar.InitialState].Single(), 1, Terminal.Terminator);
			Task.WaitAll(
				@is.Select(
						state => Task.Run(
							() => {
								lock (at) {
									foreach (var item in state)
										if (item == acceptItem)
											at[state, Terminal.Terminator] = ActionFactory.AcceptAction;
										else if (item.NextSymbol is null)
											at[state, item.Lookahead] = ActionFactory.CreateReduceAction(item.ProductionRule);
										else if (item.NextSymbol.IsTerminal) {
											var nextTerminal = item.NextSymbol.AsTerminal;
											at[state, nextTerminal] = ActionFactory.CreateShiftAction(@is.Go(state, nextTerminal));
										}
										else {
											var nextSymbol = item.NextSymbol;
											lock (gt)
												gt[state, nextSymbol.AsNonterminal] = @is.Go(state, nextSymbol);
										}
								}
							}
						)
					)
					.ToArray()
			);
			itemSets = @is;
			actionTable = at;
			gotoTable = gt;
		}
	}
}