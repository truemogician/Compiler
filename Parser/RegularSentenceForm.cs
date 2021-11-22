using System;
using System.Collections.Generic;
using System.Linq;
using Lexer;

#nullable enable
namespace Parser {
	using static Utilities;

	public class RegularSentenceForm {
		public enum RegularOperator : byte {
			Concatenation,

			Or
		}

		public RegularSentenceForm(SentenceForm value) => Value = value;

		private RegularSentenceForm(RegularSentenceForm left, RegularOperator @operator, RegularSentenceForm right) {
			LeftOperand = left;
			Operator = @operator;
			RightOperand = right;
		}

		/// <summary>
		///     Copy constructor
		/// </summary>
		/// <param name="other">Copy source</param>
		public RegularSentenceForm(RegularSentenceForm other) {
			Value = other.Value;
			RepeatRange = other.RepeatRange;
			LeftOperand = other.LeftOperand;
			RightOperand = other.RightOperand;
			Operator = other.Operator;
		}

		public SentenceForm? Value { get; }

		public (int Min, int? Max) RepeatRange { get; private init; } = (1, 1);

		public RegularSentenceForm? LeftOperand { get; }

		public RegularSentenceForm? RightOperand { get; }

		public RegularOperator? Operator { get; }

		public bool IsLeaf => Value is not null;

		public RegularSentenceForm Repeat(int min = 0, int? max = null) {
			if (min < 0)
				throw new ArgumentOutOfRangeException(nameof(min));
			if (max == 0)
				throw new ArgumentOutOfRangeException(nameof(max));
			if (min > max)
				throw new ArgumentException("Parameter `min` cannot be larger than `max`");
			return new RegularSentenceForm(this) {RepeatRange = (min, max)};
		}

		public Grammar GenerateGrammar(Nonterminal initial) {
			var result = new Grammar(initial);
			foreach (var pr in GenerateProductionRules(initial))
				result.Add(pr);
			//Chain concatenation operation
			foreach (var nt in result.SourceNonterminals)
				if (nt != initial && nt.Temporary && result[nt].Count == 1)
					result.MergeAndRemove(nt);
			//Chain or operation
			foreach (var nt in result.SourceNonterminals)
				if (nt != initial && nt.Temporary && result.Count(pr => pr.Production.Count(s => s == nt) == 1) == 1)
					result.MergeAndRemove(nt);
			//Merge equivalent nonterminals
			foreach (var group in result.ProductionRules.GroupBy(pair => pair.Value, pair => pair.Key, SetEqualityComparer<ProductionRule>.Comparer)) {
				var arr = group.ToArray();
				if (arr.Length > 1) {
					var @new = arr.FirstOrDefault(nt => !nt.Temporary) ?? arr[0];
					foreach (var old in arr.Where(nt => nt.Temporary && nt != @new)) {
						result.Remove(old);
						result.Replace(old, @new);
					}
				}
			}
			return result;
		}

		private IEnumerable<ProductionRule> GenerateProductionRules(Nonterminal source) {
			var src = source;
			if (RepeatRange == (1, 1)) {
				if (IsLeaf)
					yield return new ProductionRule(src, Value!);
			}
			else {
				src = new Nonterminal();
				var value = Value ?? src;
				if (RepeatRange.Min == 0)
					yield return new ProductionRule(source, SentenceForm.Empty);
				int min = Math.Max(1, RepeatRange.Min);
				if (RepeatRange.Max is not null) {
					var sentence = min == 1 ? value : value * min;
					for (int i = min; i <= RepeatRange.Max; ++i, sentence += value)
						yield return new ProductionRule(source, sentence);
				}
				else {
					var rpt = new Nonterminal();
					yield return new ProductionRule(source, min == 1 ? rpt : value * (min - 1) + rpt);
					yield return new ProductionRule(rpt, value);
					yield return new ProductionRule(rpt, value + rpt);
				}
			}
			if (!IsLeaf) {
				var left = new Nonterminal();
				var right = new Nonterminal();
				if (Operator == RegularOperator.Concatenation)
					yield return new ProductionRule(src, (SentenceForm)left + right);
				else {
					yield return new ProductionRule(src, left);
					yield return new ProductionRule(src, right);
				}
				foreach (var pr in LeftOperand!.GenerateProductionRules(left))
					yield return pr;
				foreach (var pr in RightOperand!.GenerateProductionRules(right))
					yield return pr;
			}
		}

		public static RegularSentenceForm operator *(RegularSentenceForm self, int count) => new(self) {RepeatRange = (count, count)};

		public static RegularSentenceForm operator *(RegularSentenceForm self, (int Min, int? Max) range) => new(self) {RepeatRange = range};

		public static RegularSentenceForm operator *(RegularSentenceForm self, char ch)
			=> self *
				ch switch {
					'?' => (0, 1),
					'*' => (0, null),
					'+' => (1, null),
					_   => throw new ArgumentOutOfRangeException(nameof(ch))
				};

		public static RegularSentenceForm operator +(RegularSentenceForm left, RegularSentenceForm right) => new(left, RegularOperator.Concatenation, right);

		public static RegularSentenceForm operator +(RegularSentenceForm left, SentenceForm right) => new(left, RegularOperator.Concatenation, right);

		public static RegularSentenceForm operator +(SentenceForm left, RegularSentenceForm right) => new(left, RegularOperator.Concatenation, right);

		public static RegularSentenceForm operator |(RegularSentenceForm left, RegularSentenceForm right) => new(left, RegularOperator.Or, right);

		public static RegularSentenceForm operator |(RegularSentenceForm left, SentenceForm right) => new(left, RegularOperator.Or, right);

		public static RegularSentenceForm operator |(SentenceForm left, RegularSentenceForm right) => new(left, RegularOperator.Or, right);

		public static implicit operator RegularSentenceForm(SentenceForm sentenceForm) => new(sentenceForm);

		public static explicit operator RegularSentenceForm(Symbol symbol) => new(symbol);

		public static explicit operator RegularSentenceForm(Nonterminal nonterminal) => new(nonterminal);

		public static explicit operator RegularSentenceForm(string nonterminal) => new(nonterminal);

		public static explicit operator RegularSentenceForm(Enum nonterminal) => new(nonterminal);

		public static explicit operator RegularSentenceForm(Terminal terminal) => new(terminal);

		public static explicit operator RegularSentenceForm(Token token) => new(token);
	}
}