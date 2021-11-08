using System;
using System.Collections.Generic;

#nullable enable
namespace Parser.CanonicalLR {
	public class ActionTable<TNonterminal, TTerminal> where TNonterminal : struct, Enum where TTerminal : struct, Enum {
		private readonly Dictionary<ItemSet<TNonterminal, TTerminal>, Dictionary<TTerminal, IAction>> _table = new();

		public IAction this[ItemSet<TNonterminal, TTerminal> set, TTerminal token] {
			get => _table[set][token];
			set {
				if (!_table.ContainsKey(set))
					_table[set] = new Dictionary<TTerminal, IAction> {[token] = value};
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

	public record ShiftAction<TNonterminal, TTerminal>(ItemSet<TNonterminal, TTerminal> NextSet) : IAction where TNonterminal : struct, Enum where TTerminal : struct, Enum {
		public ActionType Type => ActionType.Shift;
	}

	public record ReduceAction<TNonterminal, TTerminal>(ProductionRule<TNonterminal, TTerminal> ProductionRule) : IAction where TNonterminal : struct, Enum where TTerminal : struct, Enum {
		public ActionType Type => ActionType.Reduce;
	}

	public class AcceptAction : IAction {
		public ActionType Type => ActionType.Accept;
	}

	public class ErrorAction : IAction {
		public ActionType Type => ActionType.Error;
	}

	public static class ActionFactory<TNonterminal, TTerminal> where TNonterminal : struct, Enum where TTerminal : struct, Enum {
		public static AcceptAction AcceptAction { get; } = new();

		public static ErrorAction ErrorAction { get; } = new();

		public static ShiftAction<TNonterminal, TTerminal> CreateShiftAction(ItemSet<TNonterminal, TTerminal> nextSet) => new(nextSet);

		public static ReduceAction<TNonterminal, TTerminal> CreateReduceAction(ProductionRule<TNonterminal, TTerminal> productionRule) => new(productionRule);
	}
}