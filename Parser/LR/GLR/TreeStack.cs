using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TrueMogician.Extensions.Collections.Tree;

namespace Parser.LR.GLR {
	public class TreeStack<T> : IEnumerable<TreeStack<T>.BranchStack> {
		private readonly LinkedList<ValuedTreeNode<List<T>>> _leaves = new();

		public TreeStack() : this(1) { }

		public TreeStack(int initialBranch) {
			if (initialBranch < 1)
				throw new ArgumentOutOfRangeException(nameof(initialBranch));
			var root = new ValuedTreeNode<List<T>>(null!);
			while (initialBranch-- > 0)
				_leaves.AddLast(CreateNewNode());
			root.Children.AddRange(_leaves);
		}

		public IEnumerator<BranchStack> GetEnumerator() {
			for (var listNode = _leaves.First; listNode is not null;) {
				var next = listNode.Next;
				yield return new BranchStack(listNode);
				listNode = next;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public int Count => _leaves.Count;

		public BranchStack? First => _leaves.First is null ? null : new BranchStack(_leaves.First);

		public BranchStack? Last => _leaves.Last is null ? null : new BranchStack(_leaves.Last);

		private static ValuedTreeNode<List<T>> CreateNewNode() => new(new List<T>());

		private static ValuedTreeNode<List<T>> CreateNewNode(IEnumerable<T> items) => new(new List<T>(items));

		/// <summary>
		///     If <paramref name="node" /> has only one child, merge <paramref name="node" /> it into this child
		/// </summary>
		/// <returns>
		///     If <paramref name="node" /> is merged, that specific child that <paramref name="node" /> merged into;
		///     Otherwise, <paramref name="node" /> itself
		/// </returns>
		private static ValuedTreeNode<List<T>> Maintain(ValuedTreeNode<List<T>> node) {
			if (node.Children.Count == 1) {
				var child = node.Children[0];
				child.Parent = node.Parent;
				node.Parent = null;
				child.Value.InsertRange(0, node.Value);
				return child;
			}
			return node;
		}

		public class BranchStack : IEnumerable<T> {
			private LinkedListNode<ValuedTreeNode<List<T>>>? _listNode;

			internal BranchStack(LinkedListNode<ValuedTreeNode<List<T>>> listNode) => _listNode = listNode;

			public IEnumerator<T> GetEnumerator() {
				for (int i = CurrentList.Count - 1; i >= 0; --i)
					yield return CurrentList[i];
				foreach (var ancestor in Leaf.Ancestors) {
					// ReSharper disable once ConditionIsAlwaysTrueOrFalse
					if (ancestor.Value is null)
						break;
					for (int i = ancestor.Value.Count - 1; i >= 0; --i)
						yield return ancestor.Value[i];
				}
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			// ReSharper disable once ConstantConditionalAccessQualifier
			public int Count => CurrentList.Count + Leaf.Ancestors.Sum(n => n.Value?.Count ?? 0);

			private LinkedListNode<ValuedTreeNode<List<T>>> ListNode => _listNode ?? throw new InvalidOperationException("Branch has been forked or deleted");

			private LinkedList<ValuedTreeNode<List<T>>> List => ListNode.List ?? throw new NullReferenceException("List node doesn't belong to a linked list");

			private ref ValuedTreeNode<List<T>> Leaf => ref ListNode.ValueRef;

			private List<T> CurrentList => Leaf.Value;

			public void Push(T item) => CurrentList.Add(item);

			public void Push(IEnumerable<T> items) => CurrentList.AddRange(items);

			public void Push(params T[] items) => CurrentList.AddRange(items);

			/// <summary>
			///     Pop out the top item;
			/// </summary>
			public T Pop() => Pop(1)[0];

			/// <summary>
			///     Pop out multiple items from top.
			/// </summary>
			/// <param name="count">Number of items to pop out</param>
			public List<T> Pop(int count) {
				if (count > Count)
					throw new ArgumentOutOfRangeException(nameof(count));
				List<T> result;
				if (count <= CurrentList.Count) {
					result = CurrentList.GetRange(CurrentList.Count - count, count);
					result.Reverse();
					CurrentList.RemoveRange(CurrentList.Count - count, count);
				}
				else {
					result = new List<T>(count);
					var targetNode = Leaf;
					int idx;
					do {
						idx = result.Count;
						result.AddRange(targetNode.Value);
						result.Reverse(idx, targetNode.Value.Count);
						targetNode = targetNode.Parent!;
					} while (count > result.Count && targetNode.Value.Count <= count - result.Count);
					var parent = Leaf.Parent!;
					if (count == result.Count)
						Leaf.Parent = targetNode;
					else {
						var list = targetNode.Value;
						int index = list.Count + result.Count - count;
						var slice = list.GetRange(index, count - result.Count);
						idx = result.Count;
						result.AddRange(slice);
						result.Reverse(idx, slice.Count);
						var newNode = CreateNewNode(list.GetRange(0, index));
						list.RemoveRange(0, index);
						newNode.Parent = targetNode.Parent;
						targetNode.Parent = newNode;
						Leaf.Parent = newNode;
					}
					Maintain(parent);
					CurrentList.Clear();
				}
				return result;
			}

			/// <summary>
			///     Get the top item.
			/// </summary>
			public T Peek() => this.First();

			/// <summary>
			///     Fork this branch into <paramref name="count" /> new branches. After forking, this object is no longer a branch,
			///     thus any operation will throw <see cref="InvalidOperationException" />
			/// </summary>
			/// <param name="count">Number of new branches to create. Must be at least 2.</param>
			/// <returns>New branches created</returns>
			public BranchStack[] Fork(int count) {
				if (count < 2)
					throw new ArgumentOutOfRangeException(nameof(count), $"Number of branches to fork must be at least 2");
				var leaves = new LinkedListNode<ValuedTreeNode<List<T>>>[count];
				if (Leaf.Value.Count == 0) {
					var parent = Leaf.Parent!;
					int index = parent.Children.IndexOf(Leaf);
					leaves[0] = ListNode;
					for (var i = 1; i < count; ++i)
						leaves[i] = List.AddAfter(leaves[i - 1], CreateNewNode());
					parent.Children.InsertRange(index + 1, leaves.Skip(1).Select(l => l.Value));
				}
				else {
					leaves[0] = List.AddAfter(ListNode, CreateNewNode());
					for (var i = 1; i < count; ++i)
						leaves[i] = List.AddAfter(leaves[i - 1], CreateNewNode());
					List.Remove(ListNode);
					Leaf.Children.AddRange(leaves.Select(l => l.Value));
					_listNode = null;
				}
				return leaves.Select(l => new BranchStack(l)).ToArray();
			}

			/// <summary>
			///     Delete the branch itself. After deletion, any operation on this object will throw
			///     <see cref="InvalidOperationException" />
			/// </summary>
			public void Delete() {
				var parent = Leaf.Parent!;
				Leaf.Parent = null;
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (parent.Value is not null)
					Maintain(parent);
				List.Remove(ListNode);
				_listNode = null;
			}
		}
	}
}