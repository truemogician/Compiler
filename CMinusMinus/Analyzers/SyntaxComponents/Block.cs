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

		public BlockComponent(SyntaxTreeNode contentNode) {
			_content = contentNode.GetNonterminalType() switch {
				NonterminalType.Block       => new Block(contentNode),
				NonterminalType.ControlFlow => ControlFlow.Create(contentNode),
				_                           => Statement.Create(contentNode)
			};
		}

		public BlockComponent(SyntaxTreeNode labelNode, SyntaxTreeNode contentNode) : this(contentNode) {
			ThrowHelper.IsNonterminal(labelNode, NonterminalType.Label);
			ThrowHelper.ChildrenCountIs(labelNode, 2);
			ThrowHelper.IsTerminal(labelNode.Children[0], LexemeType.Identifier);
			ThrowHelper.IsTerminal(labelNode.Children[1], LexemeType.Colon);
			Label = labelNode.Children[0].GetTokenValue()!;
		}

		public string? Label { get; }

		public Statement? Statement => _content as Statement;

		public Block? Block => _content as Block;

		public ControlFlow? ControlFlow => _content as ControlFlow;
	}
}