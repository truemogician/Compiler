﻿using System;
using System.Globalization;
using System.Text;
using Lexer;
using Microsoft.Extensions.Primitives;
using TrueMogician.Extensions.Collections.Tree;

namespace Parser {
	public class SyntaxTreeNode : TreeNode<SyntaxTreeNode>, IFormattable {
		public SyntaxTreeNode(SyntaxTreeValue value) => Value = value;

		public SyntaxTreeValue Value { get; }

		public StringSegment CodeRange {
			get {
				var node = this;
				while (!node.IsLeaf)
					node = node.Children[0];
				var from = node.Value.AsTerminalInstance.Token.Segment;
				node = this;
				while (!node.IsLeaf)
					node = node.Children[^1];
				var to = node.Value.AsTerminalInstance.Token.Segment;
				return new StringSegment(from.Buffer, from.Offset, to.Offset - from.Offset + to.Length);
			}
		}

		public string ToString(string? format, IFormatProvider? formatProvider = null)
			=> format?.ToLower(CultureInfo.CurrentCulture) switch {
				null or "xml"             => ToString(),
				"xml-temp" or "xml-debug" => ToString(0, false),
				"source" or "code"        => CodeRange.Value,
				_                         => throw new ArgumentOutOfRangeException(nameof(format), "Unrecognized format")
			};

		public string ToString(int indentation, bool skipTempNonterminal = true) {
			if (IsLeaf)
				return new string('\t', indentation) + Value.AsTerminalInstance + Environment.NewLine;
			var builder = new StringBuilder();
			if (skipTempNonterminal && Value.AsNonterminal.Temporary)
				foreach (var child in Children)
					builder.Append(child.ToString(indentation, skipTempNonterminal));
			else {
				var indent = new string('\t', indentation);
				builder.AppendLine($"{indent}<{Value.AsNonterminal}>");
				foreach (var child in Children)
					builder.Append(child.ToString(indentation + 1, skipTempNonterminal));
				builder.AppendLine($"{indent}</{Value.AsNonterminal}>");
			}
			return builder.ToString();
		}

		public override string ToString() => ToString(0);

		public static implicit operator SyntaxTreeNode(SyntaxTreeValue value) => new(value);
	}

	public record TerminalInstance(Terminal Terminal, Token Token) {
		public override string ToString() => Token.ToString();

		public static implicit operator TerminalInstance((Terminal, Token) tuple) => new(tuple.Item1, tuple.Item2);
	}

	public class SyntaxTreeValue {
		public SyntaxTreeValue(Nonterminal nonterminal) => Nonterminal = nonterminal;

		public SyntaxTreeValue(TerminalInstance terminalInstance) => TerminalInstance = terminalInstance;

		public SyntaxTreeValue(Terminal terminal, Token token) : this(new TerminalInstance(terminal, token)) { }

		public Nonterminal? Nonterminal { get; }

		public TerminalInstance? TerminalInstance { get; }

		public bool IsTerminal => TerminalInstance is not null;

		public TerminalInstance AsTerminalInstance => TerminalInstance ?? throw new InvalidOperationException("Not a terminal");

		public Nonterminal AsNonterminal => Nonterminal ?? throw new InvalidOperationException("Not a nonterminal");

		public static implicit operator SyntaxTreeValue(Nonterminal nonterminal) => new(nonterminal);

		public static implicit operator SyntaxTreeValue(TerminalInstance terminalInstance) => new(terminalInstance);
	}
}