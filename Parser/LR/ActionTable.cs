using System;
using System.Collections.Generic;

#nullable enable
namespace Parser.LR {
	public class ActionTable<TItem> where TItem : ItemBase {
		private readonly Dictionary<ItemSetBase<TItem>, Dictionary<Terminal, IAction>> _table = new();

		public IAction this[ItemSetBase<TItem> set, Terminal token] {
			get => _table[set][token];
			set {
				if (!_table.ContainsKey(set))
					_table[set] = new Dictionary<Terminal, IAction> {[token] = value};
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

	public record ShiftAction<TItem>(ItemSetBase<TItem> NextState) : IAction where TItem : ItemBase {
		public ActionType Type => ActionType.Shift;
	}

	public record ReduceAction(ProductionRule ProductionRule) : IAction {
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

	public class ActionFactory<TItem> where TItem : ItemBase {
		protected internal ActionFactory() { }

		public static AcceptAction AcceptAction { get; } = new();

		public static ErrorAction ErrorAction { get; } = new();

		public static ShiftAction<TItem> CreateShiftAction(ItemSetBase<TItem> nextSet) => new(nextSet);

		public static ReduceAction CreateReduceAction(ProductionRule productionRule) => new(productionRule);
	}
}