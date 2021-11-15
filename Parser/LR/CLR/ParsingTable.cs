using System.Collections.Generic;
using System.Linq;

namespace Parser.LR.CLR {
	using ActionFactory = ActionFactory<Item>;
	using ItemSet = ItemSet<Item>;

	public class ParsingTable : ParsingTable<Item> {
		private readonly HashSet<ItemSet> _itemSets = new();

		private readonly Dictionary<ItemSet, Dictionary<Symbol, ItemSet>> _transform = new();

		public ParsingTable(Grammar grammar) : base(grammar) { }

		public override ItemSet InitialState => Closure(new Item(InitialProductionRule, 0, Terminal.Terminator));

		private ProductionRule InitialProductionRule => ExtendedGrammar[ExtendedGrammar.InitialState].Single();

		protected override bool BuildTables(out ActionTable<Item> actionTable, out GotoTable<Item> gotoTable) {
			CreateItemSets();
			actionTable = new ActionTable();
			gotoTable = new GotoTable();
			var acceptItem = new Item(InitialProductionRule, 1, Terminal.Terminator);
			foreach (var state in _itemSets) {
				foreach (var item in state)
					if (item == acceptItem)
						actionTable[state, Terminal.Terminator] = ActionFactory.AcceptAction;
					else if (item.NextSymbol.IsTerminal) {
						var nextTerminal = item.NextSymbol.AsTerminal;
						actionTable[state, nextTerminal] = nextTerminal.Equals(Terminal.Terminator)
							? ActionFactory.CreateShiftAction(Go(state, nextTerminal))
							: ActionFactory.CreateReduceAction(item.ProductionRule);
					}
					else {
						var nextSymbol = item.NextSymbol;
						gotoTable[state, nextSymbol.AsNonterminal] = Go(state, nextSymbol);
					}
			}
			return true;
		}

		protected void CreateItemSets() {
			var queue = new Queue<ItemSet>();
			var first = InitialState;
			queue.Enqueue(first);
			_itemSets.Add(first);
			while (queue.Count > 0) {
				var cur = queue.Dequeue();
				foreach (var item in cur) {
					var newSet = Go(cur, item.NextSymbol);
					if (!_itemSets.Contains(newSet) && !queue.Contains(newSet)) {
						queue.Enqueue(newSet);
						_itemSets.Add(newSet);
						if (!_transform.ContainsKey(cur))
							_transform[cur] = new Dictionary<Symbol, ItemSet>();
						_transform[cur][item.NextSymbol] = newSet;
					}
				}
			}
		}

		private ItemSet Closure(IEnumerable<Item> items) {
			if (_itemSets.SingleOrDefault(@is => @is.IsSupersetOf(items)) is { } result)
				return result;
			var queue = new Queue<Item>(items);
			var itemSet = new ItemSet();
			while (queue.Count > 0) {
				var cur = queue.Dequeue();
				itemSet.Add(cur);
				if (!cur.NextSymbol.IsTerminal &&
					new ProductionRule(cur.NextSymbol.AsNonterminal, SentenceForm.Empty) is var pr &&
					ExtendedGrammar.Contains(pr))
					foreach (var newItem in FirstSetCollection[cur.ProductionRule.Production[(cur.Marker + 1)..] + cur.Lookahead]
						.Select(terminal => new Item(pr, 0, terminal))
						.Where(newItem => !itemSet.Contains(newItem) && !queue.Contains(newItem)))
						queue.Enqueue(newItem);
			}
			return itemSet;
		}

		private ItemSet Closure(params Item[] items) => Closure(items.AsEnumerable());

		private ItemSet Go(ItemSet itemSet, Symbol symbol) {
			if (_transform.ContainsKey(itemSet) && _transform[itemSet].ContainsKey(symbol))
				return _transform[itemSet][symbol];
			return Closure(itemSet.Where(item => item.NextSymbol.Equals(symbol)).Select(item => new Item(item.ProductionRule, item.Marker + 1, item.Lookahead)));
		}
	}
}