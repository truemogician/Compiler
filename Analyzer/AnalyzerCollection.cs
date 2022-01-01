using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Parser;
using TrueMogician.Exceptions;
using TrueMogician.Extensions.Collections.Tree;
using TrueMogician.Extensions.Enumerable;
using TrueMogician.Extensions.Reflection;

namespace Analyzer {
	using AnalyzerNode = ValuedTreeNode<IAnalyzer>;

	public class AnalyzerCollection : ICollection<IAnalyzer> {
		private static readonly Type _analyzerInterface = typeof(IAnalyzer<,>);

		private static readonly Type _rootAnalyzerInterface = typeof(IRootAnalyzer<>);

		private static readonly MethodInfo _addRootAnalyzerInfo = typeof(AnalyzerCollection).GetMethods(BindingFlags.Public | BindingFlags.Instance).Single(m => m.Name == nameof(Add) && m.ContainsGenericParameters);

		private readonly List<AnalyzerNode> _rootAnalyzers = new();

		public IEnumerator<IAnalyzer> GetEnumerator() => _rootAnalyzers.SelectMany(a => a.Select(n => n.Value)).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(IAnalyzer item) {
			if (item.GetType().GetGenericInterface(_rootAnalyzerInterface) is null)
				throw new InterfaceNotImplementedException(_rootAnalyzerInterface);
			var targetType = item.GetType().GetGenericInterfaceArguments(_rootAnalyzerInterface)!.Single();
			_addRootAnalyzerInfo.MakeGenericMethod(targetType).Invoke(this, item);
		}

		public void Clear() => _rootAnalyzers.Clear();

		public bool Contains(IAnalyzer item) => ((IEnumerable<IAnalyzer>)this).Contains(item);

		public void CopyTo(IAnalyzer[] array, int arrayIndex) => this.ToArray().CopyTo(array, arrayIndex);

		public bool Remove(IAnalyzer item) {
			foreach (var root in _rootAnalyzers)
				if (root.Value.Equals(item)) {
					_rootAnalyzers.Remove(root);
					return true;
				}
				else {
					var target = root.Descendents.FirstOrDefault(n => n.Value.Equals(item));
					if (target is not null) {
						AnalyzerNode.Unlink(target);
						return true;
					}
				}
			return false;
		}

		public int Count => _rootAnalyzers.Sum(n => n.Count());

		public bool IsReadOnly => false;

		public AnalyzerNode Add<T>(IRootAnalyzer<T> rootAnalyzer) {
			var node = new AnalyzerNode(rootAnalyzer);
			_rootAnalyzers.Add(node);
			return node;
		}

		public AnalyzerNode Add(IAnalyzer item, IAnalyzer dependency) {
			var (type, _) = GetTypeParameters(item);
			var (_, dst) = GetTypeParameters(dependency);
			if (!type.IsAssignableFrom(dst))
				throw new InvariantTypeException(type, dst);
			AnalyzerNode? parent = null;
			foreach (var node in _rootAnalyzers.SelectMany(n => n)) {
				if (node.Value.Equals(item))
					throw new InvalidOperationException($"Analyzer {item.Name} already added");
				if (node.Value.Equals(dependency))
					parent = node;
			}
			if (parent is null) {
				if (dependency.GetType().GetGenericInterface(_rootAnalyzerInterface) is not null)
					parent = (AnalyzerNode)_addRootAnalyzerInfo.MakeGenericMethod(dst).Invoke(this, dependency);
				else
					throw new InvalidOperationException($"Dependency analyzer {dependency.Name} not found");
			}
			return new AnalyzerNode(item, parent);
		}

		public IEnumerable<SemanticError> Analyze(SyntaxTree tree) {
			var results = new Dictionary<IAnalyzer, object>();
			var queue = new Queue<AnalyzerNode>();
			foreach (var root in _rootAnalyzers) {
				results[root.Value] = root.Value.Analyze(tree, out var errors);
				foreach (var error in errors)
					yield return error;
				root.Children.Each(child => queue.Enqueue(child));
			}
			while (queue.Count > 0) {
				var cur = queue.Dequeue();
				results[cur.Value] = cur.Value.Analyze(results[cur.Parent!.Value], out var errors);
				foreach (var error in errors)
					yield return error;
				cur.Children.Each(child => queue.Enqueue(child));
			}
		}

		private static (Type Source, Type Target) GetTypeParameters(IAnalyzer analyzer) {
			var types = analyzer.GetType().GetGenericInterfaceArguments(_analyzerInterface);
			if (types is null)
				throw new InterfaceNotImplementedException(_analyzerInterface);
			return (types[0], types[1]);
		}
	}
}