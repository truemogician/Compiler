using System;
using System.Collections.Generic;
using System.Linq;
using Analyzer;
using Parser;
using TrueMogician.Extensions.Enumerable;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	//TODO: array type
	public class CommonType {
		internal CommonType(Qualifier qualifier, FundamentalType type) {
			Qualifier = qualifier;
			Type = type;
			ValueType = null;
		}

		internal CommonType(Qualifier qualifier, CommonType valueType) {
			Qualifier = qualifier;
			Type = null;
			ValueType = valueType;
		}

		public CommonType(IEnumerable<SyntaxTreeNode> nodes) {
			var nds = nodes.AsArray();
			var i = 0;
			var qualifiers = new List<Qualifier>();
			var qualifier = Qualifier.None;
			for (; i < nds.Length && nds[i].Value.Token is { } t; ++i) {
				ThrowHelper.IsTerminal(nds[i], LexemeType.Keyword);
				qualifier |= t.Value switch {
					"const"    => Qualifier.Const,
					"volatile" => Qualifier.Volatile,
					_          => throw new UnexpectedSyntaxNodeException { Node = nds[i] }
				};
			}
			qualifiers.Add(qualifier);
			var fType = ParseFundamentalType(nds[i++]);
			while (i < nds.Length) {
				ThrowHelper.IsTerminal(nds[i++], "*");
				qualifier = Qualifier.None;
				for (; i < nds.Length && nds[i].Value.Lexeme?.GetNameAsEnum<LexemeType>() == LexemeType.Keyword; ++i)
					qualifier |= nds[i].Value.Token!.Value switch {
						"const"    => Qualifier.Const,
						"volatile" => Qualifier.Volatile,
						_          => throw new UnexpectedSyntaxNodeException { Node = nds[i] }
					};
				qualifiers.Add(qualifier);
			}
			if (qualifiers.Count == 1) {
				Qualifier = qualifiers[0];
				Type = fType;
				ValueType = null;
			}
			else {
				var prev = new CommonType(qualifiers[0], fType);
				for (var j = 1; j < qualifiers.Count - 1; ++j) {
					var cur = new CommonType(qualifiers[j], prev);
					prev = cur;
				}
				Qualifier = qualifiers[^1];
				Type = null;
				ValueType = prev;
			}
		}

		public Qualifier Qualifier { get; }

		public FundamentalType? Type { get; }

		public CommonType? ValueType { get; }

		public bool IsPointer => ValueType is not null;

		private static FundamentalType ParseFundamentalType(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.FundamentalType);
			var tokens = node.Children.Select(n => n.Value.AsToken);
			return string.Join(' ', tokens.Select(t => t.Value)) switch {
				"void"                                                                         => FundamentalType.Void,
				"char"                                                                         => FundamentalType.Char,
				"signed char"                                                                  => FundamentalType.SignedChar,
				"unsigned char"                                                                => FundamentalType.UnsignedChar,
				"short" or "short int" or "signed short" or "signed short int"                 => FundamentalType.Short,
				"unsigned short" or "unsigned short int"                                       => FundamentalType.UnsignedShort,
				"int" or "signed" or "signed int"                                              => FundamentalType.Int,
				"unsigned" or "unsigned int"                                                   => FundamentalType.UnsignedInt,
				"long" or "long int" or "signed long" or "signed long int"                     => FundamentalType.Long,
				"unsigned long" or "unsigned long int"                                         => FundamentalType.UnsignedLong,
				"long long" or "long long int" or "signed long long" or "signed long long int" => FundamentalType.LongLong,
				"unsigned long long" or "unsigned long long int"                               => FundamentalType.UnsignedLongLong,
				"float"                                                                        => FundamentalType.Float,
				"double"                                                                       => FundamentalType.Double,
				"long double"                                                                  => FundamentalType.LongDouble,
				_                                                                              => throw new UnexpectedSyntaxNodeException { Node = node }
			};
		}
	}

	[Flags]
	public enum Qualifier : byte {
		None = 0,

		Const = 1,

		Volatile = 2,

		ConstVolatile = 3
	}

	#pragma warning disable CA1720
	public enum FundamentalType : byte {
		Void,

		Char,

		SignedChar,

		UnsignedChar,

		Short,

		UnsignedShort,

		Int,

		UnsignedInt,

		Long,

		UnsignedLong,

		LongLong,

		UnsignedLongLong,

		Float,

		Double,

		LongDouble
	}
	#pragma warning restore CA1720
}