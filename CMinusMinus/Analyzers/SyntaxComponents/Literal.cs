using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Analyzer;
using Parser;
using TrueMogician.Exceptions;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class Literal : SyntaxComponent {
		public Literal(SyntaxTreeNode node) : base(node) {
			if (!node.Value.IsTerminal) {
				ThrowHelper.IsNonterminal(node, NonterminalType.Literal);
				node = node.Children.Single();
			}
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
					var idx = 0;
					bool negative = value[0] == '-';
					if (!char.IsDigit(value[0]))
						++idx;
					int radix = value[idx] == '0' && idx < value.Length - 1
						? value[idx + 1] switch {
							'b' => 2,
							'x' => 16,
							_   => 8
						}
						: 10;
					if (radix is 2 or 16)
						idx += 2;
					int end = value.Length - 1;
					while (char.ToLower(value[end]) is 'u' or 'l')
						--end;
					++end;
					var type = end == value.Length
						? FundamentalType.Int
						: value[end ..].ToLower() switch {
							"u"            => FundamentalType.UnsignedInt,
							"l"            => FundamentalType.Long,
							"ll"           => FundamentalType.LongLong,
							"ul" or "lu"   => FundamentalType.UnsignedLong,
							"ull" or "llu" => FundamentalType.UnsignedLongLong,
							_              => throw new UnexpectedSyntaxNodeException("Wrong integer suffix") { Node = node }
						};
					Type = new FullType(TypeQualifier.Const, type);
					Value = Convert.ToInt32(value[idx..end], radix);
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