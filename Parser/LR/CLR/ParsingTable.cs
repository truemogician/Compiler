using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.LR.CLR {
	public class ParsingTable : ParsingTable<Item> {
		private readonly HashSet<ItemSet> _itemSets = new();

		private readonly Dictionary<ItemSet, Dictionary<Symbol, ItemSet>> _transform = new();

		public ParsingTable(Grammar grammar) : base(grammar) { }

		protected override bool BuildTables(out ActionTable<Item> actionTable, out GotoTable<Item> gotoTable) => throw new NotImplementedException();

		protected IEnumerable<ItemSet> CreateItemSets() {
			var queue = new Queue<ItemSet>();
			var first = Closure(new Item(ExtendedGrammar[ExtendedGrammar.InitialState][0], 0, Terminal.Terminator));
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
			return _itemSets;
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