using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parser.LR {
	public abstract class ItemSetCollectionBase<TItem> : IReadOnlyCollection<ItemSet<TItem>> where TItem : ItemBase {
		protected readonly FirstSetCollection FirstSetCollection;

		protected readonly Grammar Grammar;

		protected readonly HashSet<ItemSet<TItem>> ItemSets = new();

		protected readonly Dictionary<ItemSet<TItem>, Dictionary<Symbol, ItemSet<TItem>>> Transform = new();

		// ReSharper disable once VirtualMemberCallInConstructor
		protected ItemSetCollectionBase(Grammar grammar) {
			Grammar = grammar;
			FirstSetCollection = new FirstSetCollection(grammar);
			var queue = new Queue<ItemSet<TItem>>();
			var first = InitialState;
			queue.Enqueue(first);
			ItemSets.Add(first);
			while (queue.Count > 0) {
				var cur = queue.Dequeue();
				foreach (var item in cur) {
					if (item.NextSymbol is null)
						continue;
					var newSet = Go(cur, item.NextSymbol);
					if (!ItemSets.Contains(newSet) && !queue.Contains(newSet)) {
						queue.Enqueue(newSet);
						ItemSets.Add(newSet);
						if (!Transform.ContainsKey(cur))
							Transform[cur] = new Dictionary<Symbol, ItemSet<TItem>>();
						Transform[cur][item.NextSymbol] = newSet;
					}
				}
			}
		}

		public abstract ItemSet<TItem> InitialState { get; }

		public int Count => ItemSets.Count;

		public IEnumerator<ItemSet<TItem>> GetEnumerator() => ItemSets.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public ItemSet<TItem> Closure(IEnumerable<TItem> items) {
			if (ItemSets.SingleOrDefault(@is => @is.IsSupersetOf(items)) is { } result)
				return result;
			return CalculateClosure(items);
		}

		public ItemSet<TItem> Closure(params TItem[] items) => Closure(items.AsEnumerable());

		public ItemSet<TItem> Go(ItemSet<TItem> itemSet, Symbol symbol) {
			if (Transform.ContainsKey(itemSet) && Transform[itemSet].ContainsKey(symbol))
				return Transform[itemSet][symbol];
			return CalculateGo(itemSet, symbol);
		}

		protected abstract ItemSet<TItem> CalculateClosure(IEnumerable<TItem> items);

		protected abstract ItemSet<TItem> CalculateGo(ItemSet<TItem> itemSet, Symbol symbol);
	}
}