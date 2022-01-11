using System.Collections.Generic;
using Analyzer;
using Parser;
using TrueMogician.Extensions.Enumerable;
using TrueMogician.Extensions.Enumerator;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class Declaration {
		internal Declaration(FullType finalType, IEnumerator<SyntaxTreeNode> enumerator) {
			var list = new List<SyntaxTreeNode>();
			while (enumerator.Current.GetLexemeType() != LexemeType.Identifier)
				list.Add(enumerator.GetAndMoveNext());
			Type = new FullType(finalType, list);
			Name = new Identifier(enumerator.GetAndMoveNext());
			Type = enumerator.Current.GetLexemeType() switch {
				LexemeType.IndexStartSymbol => new ArrayType(Type, enumerator),
				LexemeType.LeftParenthesis  => new FunctionType(Type, enumerator),
				_                           => Type
			};
			if (enumerator.Current.GetTokenValue() == "=") {
				enumerator.MoveNext();
				DefaultValue = enumerator.GetAndMoveNext();
			}
		}

		public Declaration(IEnumerable<SyntaxTreeNode> nodes) {
			var nds = nodes.AsArray();
			var i = 0;
			for (; i < nds.Length && nds[i].Value.Lexeme?.GetNameAsEnum<LexemeType>() != LexemeType.Identifier; ++i) { }
			if (i == nds.Length)
				throw new UnexpectedSyntaxNodeException();
			Type = new FullType(nds[..i]);
			Name = new Identifier(nds[i++]);
			if (i < nds.Length) {
				ThrowHelper.IsTerminal(nds[i++], "=");
				if (i == nds.Length)
					throw new UnexpectedSyntaxNodeException();
				DefaultValue = nds[i++];
				if (i != nds.Length)
					throw new UnexpectedSyntaxNodeException();
			}
		}

		public FullType Type { get; }

		public Identifier Name { get; }

		public Expression? DefaultValue { get; }

		public static IEnumerable<Declaration> FromDeclarationStatement(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.DeclarationStatement);
			using var e = node.Children.GetEnumerator().ToExtended();
			var list = new List<SyntaxTreeNode>();
			while (e.MoveNextAndGet().GetLexemeType() == LexemeType.Keyword)
				list.Add(e.Current);
			ThrowHelper.IsNonterminal(e.Current, NonterminalType.FundamentalType);
			list.Add(e.Current);
			var type = new FullType(list);
			do {
				e.MoveNext();
				yield return new Declaration(type, e);
				switch (e.Current.GetLexemeType()) {
					case LexemeType.Separator: continue;
					case LexemeType.Delimiter:
						if (e.MoveNext())
							throw new UnexpectedSyntaxNodeException("Unexpected node after delimiter") { Node = e.Current };
						break;
					default: throw new UnexpectedSyntaxNodeException { Node = e.Current };
				}
				break;
			} while (true);
		}
	}
}