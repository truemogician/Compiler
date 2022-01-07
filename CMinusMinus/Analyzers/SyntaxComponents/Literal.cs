using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Parser;
using TrueMogician.Exceptions;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class Literal : SyntaxComponent {
		public Literal(SyntaxTreeNode node) : base(node) {
			ThrowHelper.IsTerminal(node, LexemeType.CharacterLiteral, LexemeType.FloatLiteral, LexemeType.IntegerLiteral, LexemeType.StringLiteral);
			string value = node.Value.AsToken.Value;
			switch (node.Value.Lexeme!.GetNameAsEnum<LexemeType>()) {
				case LexemeType.CharacterLiteral:
					Type = new FullType(TypeQualifier.Const, FundamentalType.Char);
					Value = char.Parse(Regex.Unescape(value[1..^1]));
					break;
				case LexemeType.StringLiteral:
					Type = new FullType(TypeQualifier.None, new FullType(TypeQualifier.Const, FundamentalType.Char));
					Value = Regex.Unescape(value[1..^1]);
					break;
				case LexemeType.IntegerLiteral:
					Type = new FullType(TypeQualifier.Const, FundamentalType.Int);
					var idx = 0;
					bool negative = value[0] == '-';
					if (!char.IsDigit(value[0]))
						++idx;
					int radix = value[idx..(idx + 2)] switch {
						"0b" => 2,
						"0x" => 16,
						_    => 10
					};
					if (radix != 10)
						idx += 2;
					Value = Convert.ToInt32(value[idx..], radix);
					if (negative)
						Value = -(int)Value;
					break;
				case LexemeType.FloatLiteral:
					Type = new FullType(TypeQualifier.Const, FundamentalType.Double);
					Value = double.Parse(value, NumberStyles.Float);
					break;
				default: throw new BugFoundException();
			}
		}

		public FullType Type { get; }

		public object Value { get; }
	}
}