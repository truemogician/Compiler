using System;
using System.Collections.Generic;

#nullable enable
namespace Parser.CanonicalLR {
	public class ActionTable<TNonterminal, TToken> where TNonterminal : struct, Enum where TToken : struct, Enum {
		private readonly Dictionary<ItemSet<TNonterminal, TToken>, Dictionary<TToken, IAction>> _table = new();

		public IAction this[ItemSet<TNonterminal, TToken> set, TToken token] {
			get => _table[set][token];
			set {
				if (!_table.ContainsKey(set))
					_table[set] = new Dictionary<TToken, IAction> {[token] = value};
				else
					_table[set][token] = value;
			}
		}
	}

	public enum ActionType : byte {
		Shift,

		Reduce,

		Accept,

		Error
	}

	public interface IAction {
		public ActionType Type { get; }
	}

	public record ShiftAction<TNonterminal, TToken>(ItemSet<TNonterminal, TToken> NextSet) : IAction where TNonterminal : struct, Enum where TToken : struct, Enum {
		public ActionType Type => ActionType.Shift;
	}

	public record ReduceAction<TNonterminal, TToken>(ProductionRule<TNonterminal, TToken> ProductionRule) : IAction where TNonterminal : struct, Enum where TToken : struct, Enum {
		public ActionType Type => ActionType.Reduce;
	}

	public class AcceptAction : IAction {
		public ActionType Type => ActionType.Accept;
	}

	public class ErrorAction : IAction {
		public ActionType Type => ActionType.Error;
	}

	public static class ActionFactory<TNonterminal, TToken> where TNonterminal : struct, Enum where TToken : struct, Enum {
		public static AcceptAction AcceptAction { get; } = new();

		public static ErrorAction ErrorAction { get; } = new();

		public static ShiftAction<TNonterminal, TToken> CreateShiftAction(ItemSet<TNonterminal, TToken> nextSet) => new(nextSet);

		public static ReduceAction<TNonterminal, TToken> CreateReduceAction(ProductionRule<TNonterminal, TToken> productionRule) => new(productionRule);
	}
}