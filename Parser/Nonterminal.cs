using System;

#nullable enable
namespace Parser {
	public record Nonterminal(string Name, bool Temporary = false) {
		/// <summary>
		///     Create a new temporary nonterminal, whose name will be a GUID
		/// </summary>
		public Nonterminal() : this(Guid.NewGuid().ToString("N"), true) { }

		public override string ToString() => Name;

		public static implicit operator Nonterminal(string name) => new(name);

		public static implicit operator Nonterminal(Enum @enum) => new(Enum.GetName(@enum.GetType(), @enum)!);

		public static SentenceForm operator +(Nonterminal left, Symbol right) => (SentenceForm)left + right;
	}
}