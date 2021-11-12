using System;

#nullable enable
namespace Parser {
	public class Symbol {
		private readonly Nonterminal? _nonterminal;

		private readonly Terminal? _terminal;

		public Symbol(Nonterminal nonterminal) => _nonterminal = nonterminal;

		public Symbol(Terminal terminal) => _terminal = terminal;

		public bool IsTerminal => _terminal is not null;

		public Terminal AsTerminal => _terminal ?? throw new InvalidOperationException($"Not a terminal");

		public Nonterminal AsNonterminal => _nonterminal ?? throw new InvalidOperationException($"Not a nonterminal");

		public override string? ToString() => IsTerminal ? AsTerminal.ToString() : AsNonterminal.ToString();

		public static implicit operator Symbol(Nonterminal nonterminal) => new(nonterminal);

		public static implicit operator Symbol(Terminal terminal) => new(terminal);
	}
}