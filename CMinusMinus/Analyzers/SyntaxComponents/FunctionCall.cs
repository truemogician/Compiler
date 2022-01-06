using System.Collections.Generic;
using Parser;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class FunctionCall : SyntaxComponent {
		public FunctionCall(SyntaxTreeNode node) : base(node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.PostfixExpression);
			Function = new Expression(node.Children[0]);
			ThrowHelper.IsTerminal(node.Children[1], LexemeType.LeftParenthesis);
			ThrowHelper.IsTerminal(node.Children[^1], LexemeType.LeftParenthesis);
			var parameters = new List<Expression>();
			if (node.Children.Count == 4) {
				ThrowHelper.IsNonterminal(node.Children[2], NonterminalType.Expression);
				var exp = node.Children[2].Children;
				if (exp.Count == 1 && exp[0].Value.Nonterminal?.GetNameAsEnum<NonterminalType>() == NonterminalType.CommaExpression) {
					var commaExp = new Expression(exp) as IBinaryExpression;
					do {
						parameters.Add(commaExp.LeftOperand);
						commaExp = commaExp.RightOperand;
					} while (commaExp.Operator == Operator.Comma);
					parameters.Add((Expression)commaExp);
				}
				else
					parameters.Add(new Expression(exp));
			}
			Parameters = parameters;
		}

		public Expression Function { get; }

		public IReadOnlyList<Expression> Parameters { get; }
	}
}