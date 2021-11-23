using System;
using System.Collections.Generic;
using System.Linq;

namespace Parser.LR.CLR {
	using ItemSet = ItemSet<Item>;

	public class ItemSetCollection : ItemSetCollectionBase<Item> {
		public ItemSetCollection(Grammar grammar) : base(grammar) { }

		public override ItemSet InitialState => Closure(new Item(InitialProductionRule, 0, Terminal.Terminator));

		private ProductionRule InitialProductionRule => Grammar[Grammar.InitialState].Single();

		protected override ItemSet CalculateClosure(IEnumerable<Item> items) {
			var queue = new Queue<Item>(items);
			var itemSet = new ItemSet();
			while (queue.Count > 0) {
				var cur = queue.Dequeue();
				itemSet.Add(cur);
				if (cur.NextSymbol?.IsTerminal == false)
					foreach (var pr in Grammar[cur.NextSymbol.AsNonterminal]) {
						foreach (var newItem in from lookahead in FirstSetCollection
								[cur.ProductionRule.Production[(cur.Marker + 1)..] + cur.Lookahead]
							where lookahead is not null
							select new Item(pr, 0, lookahead)
							into newItem
							where !itemSet.Contains(newItem) && !queue.Contains(newItem)
							select newItem)
							queue.Enqueue(newItem);
					}
			}
			return itemSet;
		}

		protected override ItemSet CalculateGo(ItemSet itemSet, Symbol symbol) => Closure(itemSet.Where(item => symbol.Equals(item.NextSymbol)).Select(item => new Item(item.ProductionRule, item.Marker + 1, item.Lookahead)));
	}
}