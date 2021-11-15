using System;

namespace Parser {
	public class Nonterminal {
		public Nonterminal(string name) => Name = name;

		public string Name { get; }

		public override string ToString() => Name;

		public static implicit operator Nonterminal(string name) => new(name);

		public static implicit operator Nonterminal(Enum @enum) => new(Enum.GetName(@enum.GetType(), @enum));
	}
}