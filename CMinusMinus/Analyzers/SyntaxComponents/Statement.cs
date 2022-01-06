using System;
using System.Linq;
using Analyzer;
using Parser;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public abstract class Statement : SyntaxComponent {
		protected Statement(SyntaxTreeNode node) : base(node) { }

		public static Statement Create(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node);
			if (!Enum.TryParse<NonterminalType>(node.Value.AsNonterminal.Name, out var type))
				throw new UnexpectedSyntaxNodeException($"Unrecognizable nonterminal name: {node.Value.AsNonterminal.Name}") { Node = node };
			return type switch {
				NonterminalType.EmptyStatement       => new EmptyStatement(node),
				NonterminalType.ExpressionStatement  => new ExpressionStatement(node),
				NonterminalType.DeclarationStatement => new DeclarationStatement(node),
				NonterminalType.JumpStatement =>
					node.Children[0].Value.AsToken.Value switch {
						"continue" => new ContinueStatement(node),
						"break"    => new BreakStatement(node),
						"goto"     => new GotoStatement(node),
						"return"   => new ReturnStatement(node),
						_          => throw new UnexpectedSyntaxNodeException { Node = node }
					},
				_ => throw new UnexpectedSyntaxNodeException { Node = node }
			};
		}
	}

	public class EmptyStatement : Statement {
		public EmptyStatement(SyntaxTreeNode node) : base(node) { }
	}

	public class ContinueStatement : Statement {
		public ContinueStatement(SyntaxTreeNode node) : base(node) { }
	}

	public class BreakStatement : Statement {
		public BreakStatement(SyntaxTreeNode node) : base(node) { }
	}

	public class GotoStatement : Statement {
		internal GotoStatement(SyntaxTreeNode node) : base(node) {
			ThrowHelper.ChildrenCountIs(node, 3);
			ThrowHelper.IsTerminal(node.Children[0], "goto");
			ThrowHelper.IsTerminal(node.Children[1], LexemeType.Identifier);
			ThrowHelper.IsTerminal(node.Children[2], LexemeType.Delimiter);
			Label = node.Children[1].Value.AsToken.Value;
		}

		public string Label { get; }
	}

	public class ReturnStatement : Statement {
		internal ReturnStatement(SyntaxTreeNode node) : base(node) {
			ThrowHelper.IsTerminal(node.Children[0], "return");
			if (node.Children[1].Value.Nonterminal?.GetNameAsEnum<NonterminalType>() == NonterminalType.Expression) {
				ThrowHelper.ChildrenCountIs(node, 3);
				ThrowHelper.IsTerminal(node.Children[2], LexemeType.Delimiter);
				ReturnValue = new Expression(node.Children[1]);
			}
			else {
				ThrowHelper.ChildrenCountIs(node, 2);
				ThrowHelper.IsTerminal(node.Children[1], LexemeType.Delimiter);
			}
		}

		public Expression? ReturnValue { get; }
	}

	public class ExpressionStatement : Statement {
		internal ExpressionStatement(SyntaxTreeNode node) : base(node) {
			ThrowHelper.ChildrenCountIs(node, 2);
			ThrowHelper.IsTerminal(node.Children[1], LexemeType.Delimiter);
			Expression = new Expression(node.Children[0]);
		}

		public Expression Expression { get; }
	}

	public class DeclarationStatement : Statement {
		internal DeclarationStatement(SyntaxTreeNode node) : base(node)
			=> VariableDeclarations = VariableDeclaration.FromDeclarationStatement(node).ToArray();

		public VariableDeclaration[] VariableDeclarations { get; }
	}
}