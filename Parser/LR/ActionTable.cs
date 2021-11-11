using System;
using System.Collections.Generic;

#nullable enable
namespace Parser.LR {
	public class ActionTable<TNonterminal, TToken, TItem> where TNonterminal : struct, Enum where TToken : struct, Enum where TItem : IItem<TNonterminal, TToken> {
		private readonly Dictionary<ItemSetBase<TNonterminal, TToken, TItem>, Dictionary<Terminal<TToken>, IAction>> _table = new();

		public IAction this[ItemSetBase<TNonterminal, TToken, TItem> set, Terminal<TToken> token] {
			get => _table[set][token];
			set {
				if (!_table.ContainsKey(set))
					_table[set] = new Dictionary<Terminal<TToken>, IAction> {[token] = value};
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

	public record ShiftAction<TNonterminal, TToken, TItem>(ItemSetBase<TNonterminal, TToken, TItem> NextState) : IAction where TNonterminal : struct, Enum where TToken : struct, Enum where TItem : IItem<TNonterminal, TToken> {
		public ActionType Type => ActionType.Shift;
	}

	public record ReduceAction<TNonterminal, TToken>(ProductionRule<TNonterminal, TToken> ProductionRule) : IAction where TNonterminal : struct, Enum where TToken : struct, Enum {
		public ActionType Type => ActionType.Reduce;
	}

	public record AcceptAction : IAction {
		internal AcceptAction() { }

		public ActionType Type => ActionType.Accept;
	}

	public record ErrorAction : IAction {
		internal ErrorAction() { }

		public ActionType Type => ActionType.Error;
	}

	public class ActionFactory<TNonterminal, TToken, TItem> where TNonterminal : struct, Enum where TToken : struct, Enum where TItem : IItem<TNonterminal, TToken> {
		protected internal ActionFactory() { }

		public static AcceptAction AcceptAction { get; } = new();

		public static ErrorAction ErrorAction { get; } = new();

		public static ShiftAction<TNonterminal, TToken, TItem> CreateShiftAction(ItemSetBase<TNonterminal, TToken, TItem> nextSet) => new(nextSet);

		public static ReduceAction<TNonterminal, TToken> CreateReduceAction(ProductionRule<TNonterminal, TToken> productionRule) => new(productionRule);
	}
}