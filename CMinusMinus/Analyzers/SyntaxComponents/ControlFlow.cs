using System.Collections;
using System.Collections.Generic;
using Analyzer;
using Parser;
using TrueMogician.Extensions.Enumerator;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public abstract class ControlFlow {
		public static ControlFlow Create(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.ControlFlow);
			ThrowHelper.ChildrenCountIs(node, 1);
			node = node.Children[0];
			return node.GetNonterminalType() switch {
				NonterminalType.IfBlock      => new IfBlock(node),
				NonterminalType.SwitchBlock  => new SwitchBlock(node),
				NonterminalType.ForBlock     => new ForBlock(node),
				NonterminalType.WhileBlock   => new WhileBlock(node),
				NonterminalType.DoWhileBlock => new DoWhileBlock(node),
				_                            => throw new UnexpectedSyntaxNodeException { Node = node }
			};
		}
	}

	public class IfBlock : ControlFlow {
		public IfBlock(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.IfBlock);
			using var enumerator = node.Children.GetEnumerator().ToExtended();
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), "if");
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), LexemeType.LeftParenthesis);
			ThrowHelper.IsNonterminal(enumerator.MoveNextAndGet(), NonterminalType.Expression);
			var expression = new Expression(enumerator.Current);
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), LexemeType.RightParenthesis);
			var branches = new List<(Expression?, ControlFlowBody)>();
			branches.Add((expression, (ControlFlowBody)new BlockComponent(enumerator.Move())));
			if (enumerator.Success) {
				ThrowHelper.IsTerminal(enumerator.GetAndMoveNext(), "else");
				if (enumerator.Current.GetTokenValue() == "if") {
					ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), LexemeType.LeftParenthesis);
					ThrowHelper.IsNonterminal(enumerator.MoveNextAndGet(), NonterminalType.Expression);
					expression = new Expression(enumerator.Current);
					ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), LexemeType.RightParenthesis);
					enumerator.MoveNext();
				}
				branches.Add((expression, (ControlFlowBody)new BlockComponent(enumerator)));
			}
			Branches = branches;
		}

		public IReadOnlyList<(Expression? Condition, ControlFlowBody Body)> Branches { get; }
	}

	public class SwitchBlock : ControlFlow {
		public SwitchBlock(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.SwitchBlock);
			using var enumerator = node.Children.GetEnumerator().ToExtended();
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), "switch");
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), LexemeType.LeftParenthesis);
			ThrowHelper.IsNonterminal(enumerator.MoveNextAndGet(), NonterminalType.Expression);
			Expression = new Expression(enumerator.Current);
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), LexemeType.RightParenthesis);
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), LexemeType.BlockStartSymbol);
			var cases = new List<(Expression?, IReadOnlyList<BlockComponent>)>();
			while (enumerator.MoveNextAndGet().GetNonterminalType() == NonterminalType.CaseBlock) {
				var e = enumerator.Current.Children.GetEnumerator().ToExtended();
				var expression = e.MoveNextAndGet().GetTokenValue() == "case" ? new Expression(e.MoveNextAndGet()) : null;
				ThrowHelper.IsTerminal(e.MoveNextAndGet(), LexemeType.Colon);
				var components = new List<BlockComponent>();
				e.MoveNext();
				while (e.Success)
					components.Add(new BlockComponent(e));
				cases.Add((expression, components));
			}
			ThrowHelper.IsTerminal(enumerator.Current, LexemeType.BlockEndSymbol);
			Cases = cases;
		}

		public Expression Expression { get; }

		public IReadOnlyList<(Expression? Value, IReadOnlyList<BlockComponent> Body)> Cases { get; }
	}

	public class ForBlock : ControlFlow {
		public ForBlock(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.ForBlock);
			using var enumerator = node.Children.GetEnumerator().ToExtended();
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), "for");
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), LexemeType.LeftParenthesis);
			Initialization = Statement.Create(enumerator.MoveNextAndGet());
			Condition = Statement.Create(enumerator.MoveNextAndGet()) is ExpressionStatement es ? es.Expression : null;
			Iteration = enumerator.MoveNextAndGet().GetNonterminalType() == NonterminalType.Expression ? new Expression(enumerator.GetAndMoveNext()) : null;
			ThrowHelper.IsTerminal(enumerator.Current, LexemeType.RightParenthesis);
			Body = (ControlFlowBody)new BlockComponent(enumerator);
			if (enumerator.Success)
				throw new UnexpectedSyntaxNodeException { Node = enumerator.Current };
		}

		public Statement Initialization { get; }

		public Expression? Condition { get; }

		public Expression? Iteration { get; }

		public ControlFlowBody Body { get; }
	}

	public class WhileBlock : ControlFlow {
		public WhileBlock(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.WhileBlock);
			using var enumerator = node.Children.GetEnumerator().ToExtended();
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), "while");
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), LexemeType.LeftParenthesis);
			Condition = new Expression(enumerator.MoveNextAndGet());
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), LexemeType.RightParenthesis);
			Body = (ControlFlowBody)new BlockComponent(enumerator.Move());
			if (enumerator.Success)
				throw new UnexpectedSyntaxNodeException { Node = enumerator.Current };
		}

		public Expression Condition { get; }

		public ControlFlowBody Body { get; }
	}

	public class DoWhileBlock : ControlFlow {
		public DoWhileBlock(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.DoWhileBlock);
			using var enumerator = node.Children.GetEnumerator().ToExtended();
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), "do");
			Body = (ControlFlowBody)new BlockComponent(enumerator.Move());
			ThrowHelper.IsTerminal(enumerator.Current, "while");
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), LexemeType.LeftParenthesis);
			Condition = new Expression(enumerator.MoveNextAndGet());
			ThrowHelper.IsTerminal(enumerator.MoveNextAndGet(), LexemeType.RightParenthesis);
		}

		public ControlFlowBody Body { get; }

		public Expression Condition { get; }
	}

	public class ControlFlowBody : IReadOnlyList<BlockComponent> {
		private readonly IReadOnlyList<BlockComponent> _components;

		public ControlFlowBody(BlockComponent blockComponent) {
			BlockComponent = blockComponent;
			_components = blockComponent.Block is { } b ? b.Components : new[] { blockComponent };
		}

		public BlockComponent BlockComponent { get; }

		public bool Scoped => BlockComponent.Block is not null;

		public IEnumerator<BlockComponent> GetEnumerator() => _components.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public int Count => _components.Count;

		public BlockComponent this[int index] => _components[index];

		public static explicit operator ControlFlowBody(BlockComponent blockComponent) => new(blockComponent);
	}
}