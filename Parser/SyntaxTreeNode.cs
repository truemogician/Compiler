using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Lexer;

#nullable enable
namespace Parser {
	public class SyntaxTreeNode<TNonterminal, TToken> where TNonterminal : struct, Enum where TToken : struct, Enum {
		private bool _changeParentOnChildrenChanged = true;

		private SyntaxTreeNode<TNonterminal, TToken>? _parent;

		private readonly ObservableCollection<SyntaxTreeNode<TNonterminal, TToken>> _children = new();

		public SyntaxTreeNode(SyntaxTreeValue<TNonterminal, TToken> value) {
			Value = value;
			_children.CollectionChanged += (_, args) => {
				if (!_changeParentOnChildrenChanged)
					return;
				switch (args.Action) {
					case NotifyCollectionChangedAction.Add:
						foreach (var node in args.NewItems!.OfType<SyntaxTreeNode<TNonterminal, TToken>>())
							node._parent = this;
						break;
					case NotifyCollectionChangedAction.Remove:
						foreach (var node in args.OldItems!.OfType<SyntaxTreeNode<TNonterminal, TToken>>())
							node._parent = null;
						break;
				}
			};
		}

		public SyntaxTreeValue<TNonterminal, TToken> Value { get; }

		public SyntaxTreeNode<TNonterminal, TToken>? Parent {
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

		public IList<SyntaxTreeNode<TNonterminal, TToken>> Children => _children;

		public bool IsLeaf => Value.IsTerminal;

		public string ToString(int indentation) {
			if (IsLeaf)
				return Value.AsTerminal.ToString();
			var builder = new StringBuilder();
			var indent = new string('\t', indentation);
			string name = Enum.GetName(Value.AsNonterminal)!;
			builder.AppendLine($"{indent}<{name}>");
			foreach (var child in Children)
				builder.AppendLine(child.ToString(indentation + 1));
			builder.AppendLine($"{indent}</{name}>");
			return builder.ToString();
		}
	}

	public class SyntaxTreeValue<TNonterminal, TToken> where TNonterminal : struct, Enum where TToken : struct, Enum {
		private readonly TNonterminal? _nonterminal;

		private readonly Lexeme<TToken>? _lexeme;

		public SyntaxTreeValue(TNonterminal nonterminal) => _nonterminal = nonterminal;

		public SyntaxTreeValue(Lexeme<TToken> lexeme) => _lexeme = lexeme;

		public bool IsTerminal => _lexeme is not null;

		public Lexeme<TToken> AsTerminal => _lexeme ?? throw new InvalidOperationException($"Not a terminal");

		public TNonterminal AsNonterminal => _nonterminal ?? throw new InvalidOperationException($"Not a nonterminal");

		public static implicit operator SyntaxTreeValue<TNonterminal, TToken>(TNonterminal nonterminal) => new(nonterminal);

		public static implicit operator SyntaxTreeValue<TNonterminal, TToken>(Lexeme<TToken> lexeme) => new(lexeme);
	}
}