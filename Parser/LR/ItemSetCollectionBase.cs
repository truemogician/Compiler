﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Parser.LR {
	using static Utilities;

	public abstract class ItemSetCollectionBase<TItem> : IReadOnlyCollection<ItemSet<TItem>> where TItem : ItemBase {
		private readonly Dictionary<HashSet<TItem>, ItemSet<TItem>> _closures = new(SetEqualityComparer<TItem>.Comparer);

		protected readonly FirstSetCollection FirstSetCollection;

		protected readonly Grammar Grammar;

		protected readonly HashSet<ItemSet<TItem>> ItemSets = new(SetEqualityComparer<TItem>.Comparer);

		protected readonly Dictionary<ItemSet<TItem>, Dictionary<Symbol, ItemSet<TItem>>> Transform = new();

		// ReSharper disable once VirtualMemberCallInConstructor
		protected ItemSetCollectionBase(Grammar grammar) {
			Grammar = grammar;
			FirstSetCollection = new FirstSetCollection(grammar);
		}

		public abstract ItemSet<TItem> InitialState { get; }

		public int Count => ItemSets.Count;

		public IEnumerator<ItemSet<TItem>> GetEnumerator() => ItemSets.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Initialize() {
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

		public ItemSet<TItem> Closure(IEnumerable<TItem> items) {
			var arr = items.ToArray();
			var set = new HashSet<TItem>(arr);
			if (_closures.ContainsKey(set))
				return _closures[set];
			var closure = CalculateClosure(arr);
			return _closures[set] = ItemSets.TryGetValue(closure, out var result) ? result : closure;
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