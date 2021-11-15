using System;

#nullable enable
namespace Parser {
	public class Nonterminal : IEquatable<Nonterminal> {
		/// <summary>
		/// Create a new temporary nonterminal, whose name will be a GUID
		/// </summary>
		public Nonterminal() : this(Guid.NewGuid().ToString("N"), true) { }

		public Nonterminal(string name, bool temporary = false) {
			Name = name;
			Temporary = temporary;
		}

		public string Name { get; }

		public bool Temporary { get; }

		public override string ToString() => Name;

		public bool Equals(Nonterminal? other) {
			if (other is null)
				return false;
			return ReferenceEquals(this, other) || Name == other.Name && Temporary == other.Temporary;
		}

		public override bool Equals(object? obj) {
			if (obj is null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals(obj as Nonterminal);
		}

		public override int GetHashCode() => HashCode.Combine(Name, Temporary);

		public static implicit operator Nonterminal(string name) => new(name);

		public static implicit operator Nonterminal(Enum @enum) => new(Enum.GetName(@enum.GetType(), @enum)!);
	}
}