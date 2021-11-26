namespace CMinusMinus {
	public record Keyword(string Value, KeywordCategory Category) {
		public override string ToString() => Value;

		public static implicit operator Keyword((string, KeywordCategory) tuple) => new(tuple.Item1, tuple.Item2);

		public static implicit operator string(Keyword keyword) => keyword.Value;
	}

	public enum KeywordCategory : byte {
		ArithmeticType,

		ControlFlow,

		Operator,

		Special,

		StorageModifier,

		TypeDefinition,

		TypeModifier,

		TypeQualifier
	}

	public partial class CMinusMinusFactory {
		public Keyword[] Keywords { get; } = {
			("auto", KeywordCategory.StorageModifier),
			("break", KeywordCategory.ControlFlow),
			("case", KeywordCategory.ControlFlow),
			("char", KeywordCategory.ArithmeticType),
			("const", KeywordCategory.TypeQualifier),
			("continue", KeywordCategory.ControlFlow),
			("default", KeywordCategory.ControlFlow),
			("do", KeywordCategory.ControlFlow),
			("double", KeywordCategory.ArithmeticType),
			("else", KeywordCategory.ControlFlow),
			("enum", KeywordCategory.TypeDefinition),
			("extern", KeywordCategory.Special),
			("float", KeywordCategory.ArithmeticType),
			("for", KeywordCategory.ControlFlow),
			("goto", KeywordCategory.ControlFlow),
			("if", KeywordCategory.ControlFlow),
			("int", KeywordCategory.ArithmeticType),
			("long", KeywordCategory.TypeModifier),
			("register", KeywordCategory.Special),
			("return", KeywordCategory.ControlFlow),
			("short", KeywordCategory.TypeModifier),
			("signed", KeywordCategory.TypeModifier),
			("sizeof", KeywordCategory.Operator),
			("static", KeywordCategory.StorageModifier),
			("struct", KeywordCategory.TypeDefinition),
			("switch", KeywordCategory.ControlFlow),
			("typeof", KeywordCategory.Operator),
			("union", KeywordCategory.TypeDefinition),
			("unsigned", KeywordCategory.TypeModifier),
			("void", KeywordCategory.Special),
			("volatile", KeywordCategory.TypeQualifier)
		};
	}
}