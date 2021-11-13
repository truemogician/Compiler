using System;

#nullable enable
namespace Parser {
	public class Symbol : IEquatable<Symbol> {
		private readonly Nonterminal? _nonterminal;

		private readonly Terminal? _terminal;

		public Symbol(Nonterminal nonterminal) => _nonterminal = nonterminal;

		public Symbol(Terminal terminal) => _terminal = terminal;

		public bool IsTerminal => _terminal is not null;

		public Terminal AsTerminal => _terminal ?? throw new InvalidOperationException("Not a terminal");

		public Nonterminal AsNonterminal => _nonterminal ?? throw new InvalidOperationException("Not a nonterminal");

		public bool Equals(Symbol? other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;
			if (IsTerminal != other.IsTerminal)
				return false;
			return IsTerminal ? _terminal!.Equals(other._terminal) : _nonterminal!.Equals(other._nonterminal);
		}

		public override string? ToString() => IsTerminal ? AsTerminal.ToString() : AsNonterminal.ToString();

		public static implicit operator Symbol(Nonterminal nonterminal) => new(nonterminal);

		public static implicit operator Symbol(Terminal terminal) => new(terminal);

		public override bool Equals(object? obj) {
			if (obj is null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals((Symbol)obj);
		}

		public override int GetHashCode() => HashCode.Combine(_nonterminal, _terminal);
	}
}