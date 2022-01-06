using System.Collections.Generic;
using Parser;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class FunctionCall : SyntaxComponent {
		public FunctionCall(SyntaxTreeNode node) : base(node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.FunctionCall);
			ThrowHelper.IsTerminal(node.Children[^1], LexemeType.RightParenthesis);
			int pos;
			var parameters = new List<Expression>();
			if (node.Children[^2].Value.Nonterminal?.GetNameAsEnum<NonterminalType>() == NonterminalType.Expression) {
				pos = node.Children.Count - 3;
				var exp = node.Children[^2].Children;
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
			else
				pos = node.Children.Count - 2;
			Function = new Expression(node.Children[..pos]);
			Parameters = parameters;
		}

		public Expression Function { get; }

		public IReadOnlyList<Expression> Parameters { get; }
	}
}