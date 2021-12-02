using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using Lexer;
using Microsoft.Extensions.Primitives;

namespace Parser {
	public class SyntaxTreeNode: IFormattable {
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

				public string ToString(string? format, IFormatProvider? formatProvider)
			=> format?.ToLower(CultureInfo.CurrentCulture) switch {
				null or "xml"             => ToString(),
				"xml-temp" or "xml-debug" => ToString(0, false),
				"source" or "code"        => ToCodeSegment().Value,
				_                         => throw new ArgumentOutOfRangeException(nameof(format), "Unrecognized format")
			};

		private StringSegment ToCodeSegment() {
			var node = this;
			while (!node.IsLeaf)
				node = node.Children[0];
			var from = node.Value.AsTerminalInstance.Token.Segment;
			node = this;
			while (!node.IsLeaf)
				node = node.Children[^1];
			var to = node.Value.AsTerminalInstance.Token.Segment;
			return new StringSegment(from.Buffer, from.Offset, to.Offset - from.Offset + to.Length);
		}

		public static implicit operator SyntaxTreeNode(SyntaxTreeValue value) => new(value);
	}

	public record TerminalInstance(Terminal Terminal, Token Token) {
		public override string ToString() => Token.ToString();

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