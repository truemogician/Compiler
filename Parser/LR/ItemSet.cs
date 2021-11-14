using System.Collections;
using System.Collections.Generic;

namespace Parser.LR {
	public class ItemSet<TItem> : ISet<TItem> where TItem : ItemBase {
		protected readonly HashSet<TItem> Items = new();

		public IEnumerator<TItem> GetEnumerator() => Items.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		void ICollection<TItem>.Add(TItem item) => Add(item);

		public void ExceptWith(IEnumerable<TItem> other) => Items.ExceptWith(other);

		public void IntersectWith(IEnumerable<TItem> other) => Items.IntersectWith(other);

		public bool IsProperSubsetOf(IEnumerable<TItem> other) => Items.IsProperSubsetOf(other);

		public bool IsProperSupersetOf(IEnumerable<TItem> other) => Items.IsProperSupersetOf(other);

		public bool IsSubsetOf(IEnumerable<TItem> other) => Items.IsSubsetOf(other);

		public bool IsSupersetOf(IEnumerable<TItem> other) => Items.IsSupersetOf(other);

		public bool Overlaps(IEnumerable<TItem> other) => Items.Overlaps(other);

		public bool SetEquals(IEnumerable<TItem> other) => Items.SetEquals(other);

		public void SymmetricExceptWith(IEnumerable<TItem> other) => Items.SymmetricExceptWith(other);

		public void UnionWith(IEnumerable<TItem> other) => Items.UnionWith(other);

		bool ISet<TItem>.Add(TItem item) => Add(item);

		public void Clear() => Items.Clear();

		public bool Contains(TItem item) => Items.Contains(item);

		public void CopyTo(TItem[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

		public bool Remove(TItem item) => Items.Remove(item);

		public int Count => Items.Count;

		public bool IsReadOnly => false;

		public bool Add(TItem item) => Items.Add(item);
	}
}