using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Analyzer;
using Parser;
using TrueMogician.Extensions.Enumerable;
using TrueMogician.Extensions.Enumerator;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class FullType {
		private readonly TypeQualifier _qualifier;

		private readonly BasicType? _type;

		private readonly FullType? _valueType;

		internal FullType(TypeQualifier qualifier, BasicType type) {
			_qualifier = qualifier;
			_type = type;
			_valueType = null;
		}

		internal FullType(TypeQualifier qualifier, FullType valueType) {
			_qualifier = qualifier;
			_type = null;
			_valueType = valueType;
		}

		internal FullType(FullType finalType, IEnumerable<SyntaxTreeNode> modifiers)
			=> Initialize(out _qualifier, out _type, out _valueType, finalType, modifiers);

		public FullType(IEnumerable<SyntaxTreeNode> nodes) {
			var nds = nodes.AsArray();
			var i = 0;
			var qualifier = TypeQualifier.None;
			for (; i < nds.Length && nds[i].Value.Token is { } t; ++i) {
				ThrowHelper.IsTerminal(nds[i], LexemeType.Keyword);
				qualifier |= t.Value switch {
					"const"    => TypeQualifier.Const,
					"volatile" => TypeQualifier.Volatile,
					_          => throw new UnexpectedSyntaxNodeException { Node = nds[i] }
				};
			}
			var finalType = FundamentalType.Parse(nds[i++]);
			Initialize(out _qualifier, out _type, out _valueType, new FullType(qualifier, finalType), nds[i..]);
		}

		public TypeQualifier Qualifier => _qualifier;

		public BasicType? Type => _type;

		public FullType? ValueType => _valueType;

		public bool IsPointer => ValueType is not null;

		public override string ToString() {
			var builder = new StringBuilder();
			if (!IsPointer) {
				if (Qualifier.HasFlag(TypeQualifier.Const))
					builder.Append("const ");
				if (Qualifier.HasFlag(TypeQualifier.Volatile))
					builder.Append("volatile ");
				builder.Append(Type!);
			}
			else {
				builder.Append($"{ValueType}*");
				if (Qualifier.HasFlag(TypeQualifier.Const))
					builder.Append(" const");
				if (Qualifier.HasFlag(TypeQualifier.Volatile))
					builder.Append(" volatile");
			}
			return builder.ToString();
		}

		private static void Initialize(out TypeQualifier qualifier, out BasicType? type, out FullType? valueType, FullType finalType, IEnumerable<SyntaxTreeNode> modifiers) {
			if (finalType.IsPointer)
				throw new ArgumentException("Final type shouldn't be a pointer type", nameof(finalType));
			var mods = modifiers.AsArray();
			if (mods.Length == 0) {
				qualifier = finalType.Qualifier;
				type = finalType.Type;
				valueType = null;
			}
			else {
				var i = 0;
				var qualifiers = new List<TypeQualifier>();
				while (i < mods.Length) {
					ThrowHelper.IsTerminal(mods[i++], "*");
					var q = TypeQualifier.None;
					for (; i < mods.Length && mods[i].Value.Lexeme?.GetNameAsEnum<LexemeType>() == LexemeType.Keyword; ++i)
						q |= mods[i].Value.Token!.Value switch {
							"const"    => TypeQualifier.Const,
							"volatile" => TypeQualifier.Volatile,
							_          => throw new UnexpectedSyntaxNodeException { Node = mods[i] }
						};
					qualifiers.Add(q);
				}
				var prev = finalType;
				for (var j = 0; j < qualifiers.Count - 1; ++j) {
					var cur = new FullType(qualifiers[j], prev);
					prev = cur;
				}
				qualifier = qualifiers[^1];
				type = null;
				valueType = prev;
			}
		}

		public static implicit operator FullType(BasicType type) => new(TypeQualifier.None, type);
	}

	[Flags]
	public enum TypeQualifier : byte {
		None = 0,

		Const = 1,

		Volatile = 2,

		ConstVolatile = 3
	}

	public abstract class BasicType { }

	public class FundamentalType : BasicType {
		private FundamentalType(string name) => Name = name;

		public static FundamentalType Void { get; } = new("void");

		public static FundamentalType Char { get; } = new("char");

		public static FundamentalType SignedChar { get; } = new("signed char");

		public static FundamentalType UnsignedChar { get; } = new("unsigned char");

		public static FundamentalType Short { get; } = new("short");

		public static FundamentalType UnsignedShort { get; } = new("unsigned short");

		public static FundamentalType Int { get; } = new("int");

		public static FundamentalType UnsignedInt { get; } = new("unsigned int");

		public static FundamentalType Long { get; } = new("long");

		public static FundamentalType UnsignedLong { get; } = new("unsigned long");

		public static FundamentalType LongLong { get; } = new("long long");

		public static FundamentalType UnsignedLongLong { get; } = new("unsigned long long");

		public static FundamentalType Float { get; } = new("float");

		public static FundamentalType Double { get; } = new("double");

		public static FundamentalType LongDouble { get; } = new("long double");

		public static FundamentalType[] All { get; } = { Void, Char, SignedChar, UnsignedChar, Short, UnsignedShort, Int, UnsignedInt, Long, UnsignedLong, LongLong, UnsignedLongLong, Float, Double, LongDouble };

		public string Name { get; }

		public static FundamentalType Parse(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.FundamentalType);
			var tokens = node.Children.Select(n => n.Value.AsToken);
			return string.Join(' ', tokens.Select(t => t.Value)) switch {
				"void"                                                                         => Void,
				"char"                                                                         => Char,
				"signed char"                                                                  => SignedChar,
				"unsigned char"                                                                => UnsignedChar,
				"short" or "short int" or "signed short" or "signed short int"                 => Short,
				"unsigned short" or "unsigned short int"                                       => UnsignedShort,
				"int" or "signed" or "signed int"                                              => Int,
				"unsigned" or "unsigned int"                                                   => UnsignedInt,
				"long" or "long int" or "signed long" or "signed long int"                     => Long,
				"unsigned long" or "unsigned long int"                                         => UnsignedLong,
				"long long" or "long long int" or "signed long long" or "signed long long int" => LongLong,
				"unsigned long long" or "unsigned long long int"                               => UnsignedLongLong,
				"float"                                                                        => Float,
				"double"                                                                       => Double,
				"long double"                                                                  => LongDouble,
				_                                                                              => throw new UnexpectedSyntaxNodeException { Node = node }
			};
		}

		public override string ToString() => Name;
	}

	public class FunctionType : BasicType {
		public FunctionType(FullType returnType, IEnumerable<SyntaxTreeNode> paramList) : this(returnType, paramList.GetEnumerator().Move()) { }

		public FunctionType(FullType returnType, IEnumerator<SyntaxTreeNode> paramList) {
			ReturnType = returnType;
			ThrowHelper.IsTerminal(paramList.GetAndMoveNext(), LexemeType.LeftParenthesis);
			var parameters = new List<Parameter>();
			var typeNodes = new List<SyntaxTreeNode>();
			for (Identifier? name = null; paramList.Current.GetLexemeType() is var type && type != LexemeType.RightParenthesis; paramList.MoveNext()) {
				if (type != LexemeType.Separator && name is not null)
					throw new UnexpectedSyntaxNodeException();
				switch (type) {
					case LexemeType.Separator:
						parameters.Add(new Parameter(new FullType(typeNodes), name));
						name = null;
						typeNodes.Clear();
						break;
					case LexemeType.Identifier:
						name = new Identifier(paramList.Current);
						break;
					default:
						typeNodes.Add(paramList.Current);
						break;
				}
			}
			Parameters = parameters;
			paramList.MoveNext();
		}

		public FullType ReturnType { get; }

		public IReadOnlyList<Parameter> Parameters { get; }

		public override string ToString() {
			var builder = new StringBuilder();
			builder.Append($"{ReturnType} (");
			for (var i = 0; i < Parameters.Count; ++i) {
				var (type, name) = Parameters[i];
				builder.Append(type);
				if (name is not null)
					builder.Append($" {name}");
				if (i != Parameters.Count - 1)
					builder.Append(", ");
			}
			builder.Append(')');
			return builder.ToString();
		}

		public record Parameter(FullType Type, Identifier? Name);
	}

	public class ArrayType : BasicType {
		public ArrayType(FullType itemType, IEnumerable<SyntaxTreeNode> dimensions) : this(itemType, dimensions.GetEnumerator()) { }

		public ArrayType(FullType itemType, IEnumerator<SyntaxTreeNode> dimensionNodes) {
			ItemType = itemType;
			var dimensions = new List<Expression?>();
			while (dimensionNodes.Current.GetLexemeType() == LexemeType.IndexStartSymbol) {
				dimensions.Add(new Expression(dimensionNodes.MoveNextAndGet()));
				ThrowHelper.IsTerminal(dimensionNodes.MoveNextAndGet(), LexemeType.IndexEndSymbol);
				dimensionNodes.MoveNext();
			}
			Dimensions = dimensions;
		}

		public FullType ItemType { get; }

		public IReadOnlyList<Expression?> Dimensions { get; }
	}
}