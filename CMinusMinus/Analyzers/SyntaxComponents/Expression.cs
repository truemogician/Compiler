﻿using System;
using System.Collections.Generic;
using System.Linq;
using Analyzer;
using Parser;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	using Op = Operator;

	public class Expression : SyntaxComponent, IUnaryExpression, IBinaryExpression {
		private readonly SyntaxComponent? _value;

		public Expression(SyntaxTreeNode node) : this(node.GetNonterminalType() == NonterminalType.Expression ? node.Children : new[] { node }) { }

		public Expression(ICollection<SyntaxTreeNode> nodes) : base(nodes.Single()) {
			var node = nodes.Single();
			var type = node.GetNonterminalType();
			var exception = new UnexpectedSyntaxNodeException { Node = node };
			if (type == NonterminalType.PrimaryExpression) {
				if (node.Children.Count == 3)//Remove parenthesis
					do {
						var c = node.Children;
						if (c[0].GetLexemeType() != LexemeType.LeftParenthesis || c[1].GetNonterminalType() != NonterminalType.Expression || c[2].GetLexemeType() != LexemeType.RightParenthesis)
							break;
						node = c[1];
						type = node.GetNonterminalType();
					} while (type == NonterminalType.PrimaryExpression && node.Children.Count == 3);
				node = node.Children.Single();
				if (node.GetLexemeType() == LexemeType.Identifier)
					_value = new Identifier(node);
				else if (node.GetNonterminalType() is NonterminalType.Literal)
					_value = new Literal(node);
			}
			else {
				var operands = new List<Expression>(2);
				switch (type) {
					case null: throw exception;
					case NonterminalType.ConditionalExpression:
						ThrowHelper.ChildrenCountIs(node, 5);
						ThrowHelper.IsTerminal(node.Children[1], LexemeType.QuestionMark);
						ThrowHelper.IsTerminal(node.Children[3], LexemeType.Colon);
						operands.Add(new Expression(node.Children[0]));
						operands.Add(new Expression(node.Children[2]));
						operands.Add(new Expression(node.Children[4]));
						Operator = Op.Ternary;
						break;
					case NonterminalType.UnaryExpression:
						operands.Add(new Expression(node.Children[^1]));
						switch (node.Children.Count) {
							case 2:
								Operator = node.Children[0].GetTokenValue() switch {
									null     => throw exception,
									"+"      => Op.Plus,
									"-"      => Op.Minus,
									"++"     => Op.PrefixIncrement,
									"--"     => Op.PrefixDecrement,
									"!"      => Op.LogicalNot,
									"~"      => Op.BitwiseNot,
									"*"      => Op.Dereference,
									"&"      => Op.AddressOf,
									"sizeof" => Op.SizeOf
								};
								break;
							case >= 4:
								ThrowHelper.IsTerminal(node.Children[0], LexemeType.LeftParenthesis);
								ThrowHelper.IsTerminal(node.Children[^2], LexemeType.RightParenthesis);
								Operator = Op.Cast;
								Type = new FullType(node.Children[1..^2]);
								break;
						}
						break;
					case NonterminalType.PostfixExpression:
						ThrowHelper.IsNonterminal(node.Children[0], NonterminalType.PostfixExpression, NonterminalType.PrimaryExpression);
						ThrowHelper.IsTerminal(node.Children[1]);
						switch (node.Children[1].GetLexemeType()) {
							case LexemeType.IndexStartSymbol:
								ThrowHelper.ChildrenCountIs(node, 4);
								ThrowHelper.IsTerminal(node.Children[3], LexemeType.IndexEndSymbol);
								operands.Add(new Expression(node.Children[0]));
								operands.Add(new Expression(node.Children[2]));
								Operator = Op.Subscript;
								break;
							case LexemeType.LeftParenthesis:
								ThrowHelper.IsTerminal(node.Children[^1], LexemeType.RightParenthesis);
								operands.Add(new Expression(node.Children[0]));
								if (node.Children.Count == 4) {
									ThrowHelper.IsNonterminal(node.Children[2], NonterminalType.Expression);
									var exp = node.Children[2].Children;
									if (exp.Count == 1 && exp[0].Value.Nonterminal?.GetNameAsEnum<NonterminalType>() == NonterminalType.CommaExpression) {
										var expression = new Expression(exp);
										do {
											operands.Add((expression as IBinaryExpression).LeftOperand);
											expression = (expression as IBinaryExpression).RightOperand;
										} while (expression.Operator == Op.Comma);
										operands.Add(expression);
									}
									else
										operands.Add(new Expression(exp));
								}
								Operator = Op.FunctionCall;
								break;
							case LexemeType.IncrementOperator:
							case LexemeType.DecrementOperator:
								ThrowHelper.ChildrenCountIs(node, 2);
								operands.Add(new Expression(node.Children[0]));
								Operator = node.Children[1].GetLexemeType() == LexemeType.IncrementOperator ? Op.SuffixIncrement : Op.SuffixDecrement;
								break;
							case LexemeType.MembershipOperator: goto BinaryExpression;
							default:                            throw new UnexpectedSyntaxNodeException { Node = node };
						}
						break;
					default:
					BinaryExpression:
						ThrowHelper.ChildrenCountIs(node, 3);
						operands.Add(new Expression(node.Children[0]));
						operands.Add(new Expression(node.Children[2]));
						Operator = node.Children[1].GetTokenValue() switch {
							","   => Op.Comma,
							"|="  => Op.BitwiseOrAssignment,
							"^="  => Op.BitwiseXorAssignment,
							"&="  => Op.BitwiseAndAssignment,
							"<<=" => Op.LeftShiftAssignment,
							">>=" => Op.RightShiftAssignment,
							"*="  => Op.ProductAssignment,
							"/="  => Op.QuotientAssignment,
							"%="  => Op.RemainderAssignment,
							"+="  => Op.SumAssignment,
							"-="  => Op.DifferenceAssignment,
							"="   => Op.Assignment,
							"||"  => Op.LogicalOr,
							"&&"  => Op.LogicalAnd,
							"|"   => Op.BitwiseOr,
							"^"   => Op.BitwiseXor,
							"&"   => Op.BitwiseAnd,
							"=="  => Op.Equality,
							"!="  => Op.Inequality,
							">"   => Op.Greater,
							">="  => Op.GreaterEqual,
							"<"   => Op.Less,
							"<="  => Op.LessEqual,
							"<<"  => Op.LeftShift,
							">>"  => Op.RightShift,
							"+"   => Op.Addition,
							"-"   => Op.Subtraction,
							"*"   => Op.Multiplication,
							"/"   => Op.Division,
							"%"   => Op.Remainder,
							"->"  => Op.PointerMember,
							"."   => Op.Member
						};
						break;
				}
				Operands = operands;
			}
		}

		public FullType? Type { get; set; }

		public Operator? Operator { get; }

		public IReadOnlyList<Expression>? Operands { get; }

		public int Arity => Operands?.Count ?? 0;

		public bool Atomic => Operator is null;

		public Identifier? Identifier => _value as Identifier;

		public Literal? Literal => _value as Literal;

		public string AsIdentifier => Identifier ?? throw new InvalidOperationException("Not an identifier");

		public Literal AsLiteral => Literal ?? throw new InvalidOperationException("Not a literal");

		public Expression this[int index] => Operands?[index] ?? throw new ArgumentOutOfRangeException(nameof(index));

		Operator IBinaryExpression.Operator => Operator ?? throw new InvalidOperationException("Not a binary expression");

		Expression IBinaryExpression.LeftOperand => Arity == 2 ? Operands![0] : throw new InvalidOperationException("Not a binary expression");

		Expression IBinaryExpression.RightOperand => Arity == 2 ? Operands![1] : throw new InvalidOperationException("Not a binary expression");

		Operator IUnaryExpression.Operator => Operator ?? throw new InvalidOperationException("Not an unary expression");

		Expression IUnaryExpression.Operand => Arity == 1 ? Operands![0] : throw new InvalidOperationException("Not an unary expression");

		public static implicit operator Expression(SyntaxTreeNode node) => new(node);
	}

	public interface IUnaryExpression {
		public Operator Operator { get; }

		public Expression Operand { get; }
	}

	public interface IBinaryExpression {
		public Operator Operator { get; }

		public Expression LeftOperand { get; }

		public Expression RightOperand { get; }
	}

	public enum Operator : byte {
		Comma,

		BitwiseOrAssignment,

		BitwiseXorAssignment,

		BitwiseAndAssignment,

		LeftShiftAssignment,

		RightShiftAssignment,

		ProductAssignment,

		QuotientAssignment,

		RemainderAssignment,

		SumAssignment,

		DifferenceAssignment,

		Assignment,

		Ternary,

		LogicalOr,

		LogicalAnd,

		BitwiseOr,

		BitwiseXor,

		BitwiseAnd,

		Equality,

		Inequality,

		Greater,

		GreaterEqual,

		Less,

		LessEqual,

		LeftShift,

		RightShift,

		Addition,

		Subtraction,

		Multiplication,

		Division,

		Remainder,

		SizeOf,

		AddressOf,

		Dereference,

		Cast,

		LogicalNot,

		BitwiseNot,

		Plus,

		Minus,

		PrefixIncrement,

		PrefixDecrement,

		PointerMember,

		Member,

		Subscript,

		FunctionCall,

		SuffixIncrement,

		SuffixDecrement
	}
}