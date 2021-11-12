using System;
using System.Collections;
using System.Collections.Generic;

namespace Parser.LR {
	public abstract class ItemSetBase<TItem> : IList<TItem> where TItem : ItemBase {
		protected readonly List<TItem> Items = new();

		public int Count => Items.Count;

		public bool IsReadOnly => false;

		public TItem this[int index] {
			get => Items[index];
			set => Items[index] = value;
		}

		public IEnumerator<TItem> GetEnumerator() => Items.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(TItem item) => Items.Add(item);

		public void Clear() => Items.Clear();

		public bool Contains(TItem item) => Items.Contains(item);

		public void CopyTo(TItem[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);

		public bool Remove(TItem item) => Items.Remove(item);

		public int IndexOf(TItem item) => Items.IndexOf(item);

		public void Insert(int index, TItem item) => Items.Insert(index, item);

		public void RemoveAt(int index) => Items.RemoveAt(index);
	}
}