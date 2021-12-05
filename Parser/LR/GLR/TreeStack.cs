using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TrueMogician.Extensions.Collections.Tree;

namespace Parser.LR.GLR {
	internal class TreeStack<T> : IEnumerable<TreeStack<T>.BranchStack> {
		private readonly ValuedSimpleTreeNode<List<T>> _root = CreateNewNode();

		private readonly LinkedList<ValuedSimpleTreeNode<List<T>>> _leaves = new();

		public TreeStack() => _leaves.AddLast(_root);

		public IEnumerator<BranchStack> GetEnumerator() {
			for (var leafNode = _leaves.First; leafNode is not null; leafNode = leafNode.Next) {
				if (!leafNode.ValueRef.IsLeaf) {
					var cur = leafNode;
					foreach (var leaf in leafNode.ValueRef.Leaves)
						cur = _leaves.AddAfter(cur, leaf);
					var oldNode = leafNode;
					leafNode = leafNode.Next!;
					_leaves.Remove(oldNode);
				}
				yield return leafNode.ValueRef;
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public static BranchStack[] Fork(BranchStack branch, int count) => Fork((ValuedSimpleTreeNode<List<T>>)branch, count).Select(n => (BranchStack)n).ToArray();

		public static void Remove(BranchStack branch) {
			var node = (ValuedSimpleTreeNode<List<T>>)branch;
			if (node.IsRoot)
				throw new InvalidOperationException("Root cannot be removed");
			var parent = node.Parent!;
			node.Parent = null;
			Maintain(parent);
		}

		private static ValuedSimpleTreeNode<List<T>> CreateNewNode() => new(new List<T>());

		private static ValuedSimpleTreeNode<List<T>> CreateNewNode(IEnumerable<T> items) => new(new List<T>(items));

		private static ValuedSimpleTreeNode<List<T>>[] Fork(ValuedSimpleTreeNode<List<T>> node, int count) {
			var leaves = new ValuedSimpleTreeNode<List<T>>[count];
			if (node.Value.Count == 0) {
				var parent = node.Parent;
				if (parent is null)
					throw new InvalidOperationException("Empty root cannot be forked");
				int index = parent.Children.IndexOf(node);

				leaves[0] = node;
				for (var i = 1; i < count; ++i)
					leaves[i] = CreateNewNode();
				parent.Children.InsertRange(index + 1, leaves.Skip(1));
			}
			else {
				for (var i = 0; i < count; ++i)
					leaves[i] = CreateNewNode();
				node.Children.AddRange(leaves);
			}
			return leaves;
		}

		/// <summary>
		///     If <paramref name="node" /> has only one child, merge it into <paramref name="node" />
		/// </summary>
		private static void Maintain(ValuedSimpleTreeNode<List<T>> node) {
			if (node.Children.Count == 1) {
				var child = node.Children[0];//TODO: figure out why implicit conversion happens
				node.Children.AddRange(child.Children);
				child.Parent = null;
				node.Value.AddRange(child.Value);
			}
		}

		internal class BranchStack : IEnumerable<T> {
			private ValuedSimpleTreeNode<List<T>> _node;

			private BranchStack(ValuedSimpleTreeNode<List<T>> leaf) => _node = leaf;

			public IEnumerator<T> GetEnumerator() {
				for (int i = CurrentList.Count - 1; i >= 0; --i)
					yield return CurrentList[i];
				foreach (var ancestor in _node.Ancestors)
					for (int i = ancestor.Value.Count - 1; i >= 0; --i)
						yield return ancestor.Value[i];
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			public int Count => CurrentList.Count + _node.Ancestors.Sum(n => n.Value.Count);

			private List<T> CurrentList => _node.Value;

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
					var targetNode = _node;
					do {
						result.AddRange(targetNode.Value);
						targetNode = targetNode.Parent!;
					} while (targetNode.Value.Count <= count - result.Count);
					var list = targetNode.Value;
					int index = list.Count + result.Count - count;
					var slice = list.GetRange(index, count - result.Count);
					result.AddRange(slice);
					Remove(this);
					if (index != list.Count) {
						list.RemoveRange(index, list.Count - index);
						var newNode = CreateNewNode(slice);
						newNode.Children.AddRange(targetNode.Children);
						newNode.Parent = targetNode;
					}
					_node = CreateNewNode();
					_node.Parent = targetNode;
				}
				return result;
			}

			public T Peek() => this.First();

			public static implicit operator BranchStack(ValuedSimpleTreeNode<List<T>> leaf) => new(leaf);

			public static explicit operator ValuedSimpleTreeNode<List<T>>(BranchStack branch) => branch._node;
		}
	}
}