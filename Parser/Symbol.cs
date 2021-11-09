using System;

#nullable enable
namespace Parser {
	public class Symbol<TNonterminal, TToken> where TNonterminal : struct, Enum where TToken : struct, Enum {
		private readonly TNonterminal? _nonterminal;

		private readonly Terminal<TToken>? _terminal;

		public Symbol(TNonterminal nonterminal) => _nonterminal = nonterminal;

		public Symbol(Terminal<TToken> terminal) => _terminal = terminal;

		public bool IsTerminal => _terminal is not null;

		public Terminal<TToken> AsTerminal => _terminal ?? throw new InvalidOperationException($"Not a terminal");

		public TNonterminal AsNonterminal => _nonterminal ?? throw new InvalidOperationException($"Not a nonterminal");

		public static implicit operator Symbol<TNonterminal, TToken>(TNonterminal nonterminal) => new(nonterminal);

		public static implicit operator Symbol<TNonterminal, TToken>(Terminal<TToken> terminal) => new(terminal);
	}
}