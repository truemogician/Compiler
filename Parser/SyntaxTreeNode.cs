using System;
using System.Collections.Generic;
using System.Text;
using Lexer;

#nullable enable
namespace Parser {
	public class SyntaxTreeNode<TNonterminal, TToken> where TNonterminal : struct, Enum where TToken : struct, Enum {
		public SyntaxTreeValue<TNonterminal, TToken> Value { get; init; }

		public SyntaxTreeNode<TNonterminal, TToken> Parent { get; set; }

		public List<SyntaxTreeNode<TNonterminal, TToken>> Children { get; } = new();

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