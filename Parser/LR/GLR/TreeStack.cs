using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TrueMogician.Extensions.Collections.Tree;

namespace Parser.LR.GLR {
	internal class TreeStack<T> : IEnumerable<TreeStack<T>.BranchStack> {
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
			for (var listNode = _leaves.First; listNode is not null; listNode = listNode.Next)
				yield return listNode;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private static ValuedTreeNode<List<T>> CreateNewNode() => new(new List<T>());

		private static ValuedTreeNode<List<T>> CreateNewNode(ValuedTreeNode<List<T>>? parent) => new(new List<T>(), parent);

		private static ValuedTreeNode<List<T>> CreateNewNode(IEnumerable<T> items) => new(new List<T>(items));

		/// <summary>
		///     If <paramref name="node" /> has only one child, merge <paramref name="node" /> it into this child
		/// </summary>
		private static void Maintain(ValuedTreeNode<List<T>> node) {
			if (node.Children.Count == 1) {
				var child = node.Children[0];//TODO: figure out why implicit conversion happens
				child.Parent = node.Parent;
				node.Parent = null;
				child.Value.InsertRange(0, node.Value);
			}
		}

		internal class BranchStack : IEnumerable<T> {
			private LinkedListNode<ValuedTreeNode<List<T>>>? _listNode;

			private BranchStack(ref LinkedListNode<ValuedTreeNode<List<T>>> listNode) => _listNode = listNode;

			public IEnumerator<T> GetEnumerator() {
				for (int i = CurrentList.Count - 1; i >= 0; --i)
					yield return CurrentList[i];
				foreach (var ancestor in Leaf.Ancestors)
					for (int i = ancestor.Value.Count - 1; i >= 0; --i)
						yield return ancestor.Value[i];
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			public int Count => CurrentList.Count + Leaf.Ancestors.Sum(n => n.Value.Count);

			private LinkedListNode<ValuedTreeNode<List<T>>> ListNode => _listNode ?? throw new InvalidOperationException("Branch has been forked or deleted");

			private LinkedList<ValuedTreeNode<List<T>>> List => ListNode.List ?? throw new NullReferenceException("List node doesn't belong to a linked list");

			private ref ValuedTreeNode<List<T>> Leaf => ref ListNode.ValueRef;

			private List<T> CurrentList => Leaf.Value;

			public void Push(T item) => CurrentList.Add(item);

			public T Pop() => Pop(1)[0];

			public List<T> Pop(int count) {
				if (count > Count)
					throw new ArgumentOutOfRangeException(nameof(count));
				List<T> result;
				if (count <= CurrentList.Count) {
					result = CurrentList.GetRange(CurrentList.Count - count, count);
					CurrentList.RemoveRange(CurrentList.Count - count, count);
				}
				else {
					result = new List<T>(count);
					var targetNode = Leaf;
					do {
						result.AddRange(targetNode.Value);
						targetNode = targetNode.Parent!;
					} while (targetNode.Value.Count <= count - result.Count);
					var list = targetNode.Value;
					int index = list.Count + result.Count - count;
					var slice = list.GetRange(index, count - result.Count);
					result.AddRange(slice);
					var linkedList = List;
					Delete();
					if (index != list.Count) {
						list.RemoveRange(index, list.Count - index);
						var splitNode = CreateNewNode(slice);
						splitNode.Children.AddRange(targetNode.Children);
						splitNode.Parent = targetNode;
					}
					var prevNode = targetNode;
					do {
						prevNode = prevNode.Children[^1];
					} while (!prevNode.IsLeaf);
					_listNode = linkedList.AddAfter(linkedList.Find(prevNode)!, CreateNewNode(targetNode));
				}
				return result;
			}

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
				return leaves.Select(l => (BranchStack)l).ToArray();
			}

			/// <summary>
			///     Delete the branch itself. After deletion, any operation on this object will throw
			///     <see cref="InvalidOperationException" />
			/// </summary>
			public void Delete() {
				if (Leaf.IsRoot)
					throw new InvalidOperationException("Root cannot be deleted");
				var parent = Leaf.Parent!;
				Leaf.Parent = null;
				Maintain(parent);
				List.Remove(ListNode);
				_listNode = null;
			}

			public static implicit operator BranchStack(LinkedListNode<ValuedTreeNode<List<T>>> listNode) => new(ref listNode);

			public static explicit operator LinkedListNode<ValuedTreeNode<List<T>>>(BranchStack branch) => branch.ListNode;
		}
	}
}