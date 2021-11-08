using System;

#nullable enable
namespace Parser {
	public class Component<TNonterminal, TTerminal> where TNonterminal : struct, Enum where TTerminal : struct, Enum {
		private readonly TNonterminal? _nonterminal;

		private readonly TTerminal? _terminal;

		public Component(TNonterminal nonterminal) => _nonterminal = nonterminal;

		public Component(TTerminal terminal) => _terminal = terminal;

		public bool IsTerminal => _terminal is not null;

		public TTerminal AsTerminal => _terminal ?? throw new InvalidOperationException($"Not a terminal");

		public TNonterminal AsNonterminal => _nonterminal ?? throw new InvalidOperationException($"Not a nonterminal");

		public static implicit operator Component<TNonterminal, TTerminal>(TNonterminal nonterminal) => new(nonterminal);

		public static implicit operator Component<TNonterminal, TTerminal>(TTerminal terminal) => new(terminal);
	}
}