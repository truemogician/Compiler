using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Lexer;

namespace Parser {
	public class SyntaxTreeNode {
		private readonly ObservableCollection<SyntaxTreeNode> _children = new();

		private bool _changeParentOnChildrenChanged = true;

		private SyntaxTreeNode? _parent;

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

		public string ToString(int indentation, bool skipTempNonterminal = true) {
			if (IsLeaf)
				return new string('\t', indentation) + Value.AsTerminalInstance + Environment.NewLine;
			var builder = new StringBuilder();
			if (skipTempNonterminal && Value.AsNonterminal.Temporary)
				foreach (var child in Children)
					builder.Append(child.ToString(indentation, skipTempNonterminal));
			else {
				var indent = new string('\t', indentation);
				builder.AppendLine($"{indent}<{Value.AsNonterminal}>");
				foreach (var child in Children)
					builder.Append(child.ToString(indentation + 1, skipTempNonterminal));
				builder.AppendLine($"{indent}</{Value.AsNonterminal}>");
			}
			return builder.ToString();
		}

		public override string ToString() => ToString(0);

		public static implicit operator SyntaxTreeNode(SyntaxTreeValue value) => new(value);
	}

	public record TerminalInstance(Terminal Terminal, Token Lexeme) {
		public override string ToString() => Lexeme.ToString();

		public static implicit operator TerminalInstance((Terminal, Token) tuple) => new(tuple.Item1, tuple.Item2);
	}

	public class SyntaxTreeValue {
		private readonly Nonterminal? _nonterminal;

		private readonly TerminalInstance? _terminalInstance;

		public SyntaxTreeValue(Nonterminal nonterminal) => _nonterminal = nonterminal;

		public SyntaxTreeValue(TerminalInstance terminalInstance) => _terminalInstance = terminalInstance;

		public SyntaxTreeValue(Terminal terminal, Token token) : this(new TerminalInstance(terminal, token)) { }

		public bool IsTerminal => _terminalInstance is not null;

		public TerminalInstance AsTerminalInstance => _terminalInstance ?? throw new InvalidOperationException("Not a terminal");

		public Nonterminal AsNonterminal => _nonterminal ?? throw new InvalidOperationException("Not a nonterminal");

		public static implicit operator SyntaxTreeValue(Nonterminal nonterminal) => new(nonterminal);

		public static implicit operator SyntaxTreeValue(TerminalInstance terminalInstance) => new(terminalInstance);
	}
}