using System.Collections.Generic;
using System.Linq;
using Analyzer;
using Parser;
using TrueMogician.Extensions.Enumerator;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class FunctionDefinition : SyntaxComponent {
		public FunctionDefinition(SyntaxTreeNode node) : base(node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.FunctionDefinition);
			using var e = node.Children.GetEnumerator();
			var list = new List<SyntaxTreeNode>();
			while (e.MoveNextAndGet().GetLexemeType() != LexemeType.Identifier)
				list.Add(e.Current);
			Name = new Identifier(e.Current);
			Type = new FunctionType(new FullType(list), e.Move());
			Body = new Block(e.Current);
			if (e.MoveNext())
				throw new UnexpectedSyntaxNodeException("Unexpected node after block") { Node = e.Current };
		}

		public Identifier Name { get; }

		public FunctionType Type { get; }

		public Block Body { get; }

		public static implicit operator FunctionDefinition(SyntaxTreeNode node) => new(node);
	}
}