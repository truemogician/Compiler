using System.Collections.Generic;
using System.Linq;
using Analyzer;
using Parser;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class Block {
		public Block(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.Block);
			ThrowHelper.IsTerminal(node.Children[0], LexemeType.BlockStartSymbol);
			ThrowHelper.IsTerminal(node.Children[^1], LexemeType.BlockEndSymbol);
			SyntaxTreeNode? label = null;
			var components = new List<BlockComponent>();
			foreach (var n in node.Children.Skip(1).Take(node.Children.Count - 2))
				switch (n.GetNonterminalType()) {
					case NonterminalType.Label:
						if (label is not null)
							throw new UnexpectedSyntaxNodeException { Node = n };
						label = n;
						break;
					case NonterminalType.Block:
					case NonterminalType.ControlFlow:
					case NonterminalType.EmptyStatement:
					case NonterminalType.ExpressionStatement:
					case NonterminalType.DeclarationStatement:
					case NonterminalType.ControlStatement:
					case NonterminalType.ReturnStatement:
						components.Add(label is null ? new BlockComponent(n) : new BlockComponent(label, n));
						label = null;
						break;
					default: throw new UnexpectedSyntaxNodeException { Node = n };
				}
			Components = components;
		}

		public IReadOnlyList<BlockComponent> Components { get; }
	}

	public class BlockComponent {
		private readonly object _content;

		private readonly string? _label;

		internal BlockComponent(IEnumerator<SyntaxTreeNode> enumerator) {
			SyntaxTreeNode? label = null;
			if (enumerator.Current.GetNonterminalType() == NonterminalType.Literal) {
				label = enumerator.Current;
				enumerator.MoveNext();
			}
			InitializeLabel(out _label, label);
			InitializeContent(out _content, enumerator.Current);
			enumerator.MoveNext();
		}

		public BlockComponent(SyntaxTreeNode contentNode) => InitializeContent(out _content, contentNode);

		public BlockComponent(SyntaxTreeNode? labelNode, SyntaxTreeNode contentNode) : this(contentNode) => InitializeLabel(out _label, labelNode);

		public string? Label => _label;

		public Statement? Statement => _content as Statement;

		public Block? Block => _content as Block;

		public ControlFlow? ControlFlow => _content as ControlFlow;

		private void InitializeLabel(out string? field, SyntaxTreeNode? node) {
			if (node is null) {
				field = null;
				return;
			}
			ThrowHelper.IsNonterminal(node, NonterminalType.Label);
			ThrowHelper.ChildrenCountIs(node, 2);
			ThrowHelper.IsTerminal(node.Children[0], LexemeType.Identifier);
			ThrowHelper.IsTerminal(node.Children[1], LexemeType.Colon);
			field = node.Children[0].GetTokenValue()!;
		}

		private void InitializeContent(out object field, SyntaxTreeNode node) {
			field = node.GetNonterminalType() switch {
				NonterminalType.Block       => new Block(node),
				NonterminalType.ControlFlow => ControlFlow.Create(node),
				_                           => Statement.Create(node)
			};
		}
	}
}