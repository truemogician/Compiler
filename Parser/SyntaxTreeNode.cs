using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Lexer;

#nullable enable
namespace Parser {
	public class SyntaxTreeNode {
		private bool _changeParentOnChildrenChanged = true;

		private SyntaxTreeNode? _parent;

		private readonly ObservableCollection<SyntaxTreeNode> _children = new();

		public SyntaxTreeNode(SyntaxTreeValue value) {
			Value = value;
			_children.CollectionChanged += (_, args) => {
				if (!_changeParentOnChildrenChanged)
					return;
				switch (args.Action) {
					case NotifyCollectionChangedAction.Add:
						foreach (var node in args.NewItems!.OfType<SyntaxTreeNode>())
							node._parent = this;
						break;
					case NotifyCollectionChangedAction.Remove:
						foreach (var node in args.OldItems!.OfType<SyntaxTreeNode>())
							node._parent = null;
						break;
				}
			};
		}

		public SyntaxTreeValue Value { get; }

		public SyntaxTreeNode? Parent {
			get => _parent;
			set {
				if (_parent is not null) {
					_changeParentOnChildrenChanged = false;
					_parent.Children.Remove(this);
					_changeParentOnChildrenChanged = true;
				}
				if (value is not null) {
					value._changeParentOnChildrenChanged = false;
					value.Children.Add(this);
					value._changeParentOnChildrenChanged = true;
				}
				_parent = value;
			}
		}

		public IList<SyntaxTreeNode> Children => _children;

		public bool IsLeaf => Value.IsTerminal;

		public string ToString(int indentation) {
			if (IsLeaf)
				return Value.AsTerminal.ToString();
			var builder = new StringBuilder();
			var indent = new string('\t', indentation);
			builder.AppendLine($"{indent}<{Value.AsNonterminal}>");
			foreach (var child in Children)
				builder.AppendLine(child.ToString(indentation + 1));
			builder.AppendLine($"{indent}</{Value.AsNonterminal}>");
			return builder.ToString();
		}

		public override string ToString() => ToString(0);
	}

	public class SyntaxTreeValue {
		private readonly Nonterminal? _nonterminal;

		private readonly Lexeme? _lexeme;

		public SyntaxTreeValue(Nonterminal nonterminal) => _nonterminal = nonterminal;

		public SyntaxTreeValue(Lexeme lexeme) => _lexeme = lexeme;

		public bool IsTerminal => _lexeme is not null;

		public Lexeme AsTerminal => _lexeme ?? throw new InvalidOperationException($"Not a terminal");

		public Nonterminal AsNonterminal => _nonterminal ?? throw new InvalidOperationException($"Not a nonterminal");

		public static implicit operator SyntaxTreeValue(Nonterminal nonterminal) => new(nonterminal);

		public static implicit operator SyntaxTreeValue(Lexeme lexeme) => new(lexeme);
	}
}