using System.Collections.Generic;
using TrueMogician.Extensions.Enumerable;

namespace Parser.LR {
	public abstract class ActionTable<TItem> where TItem : ItemBase {
		protected readonly Dictionary<ItemSet<TItem>, Dictionary<Terminal, IAction>> Table = new();

		public IReadOnlyDictionary<ItemSet<TItem>, IReadOnlyDictionary<Terminal, IAction>> RawTable => Table.ToValueReadOnly<ItemSet<TItem>, Dictionary<Terminal, IAction>, IReadOnlyDictionary<Terminal, IAction>>();

		public virtual IAction this[ItemSet<TItem> state, Terminal terminal] {
			get => Table[state][terminal];
			set {
				if (!Table.ContainsKey(state))
					Table[state] = new Dictionary<Terminal, IAction> {[terminal] = value};
				else
					Table[state][terminal] = value;
			}
		}
	}

	public enum ActionType : byte {
		Error,

		Shift,

		Reduce,

		Accept
	}

	public interface IAction {
		public ActionType Type { get; }
	}

	public record ShiftAction<TItem>(ItemSet<TItem> NextState) : IAction where TItem : ItemBase {
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

	public static class ActionFactory<TItem> where TItem : ItemBase {
		public static AcceptAction AcceptAction { get; } = new();

		public static ErrorAction ErrorAction { get; } = new();

		public static ShiftAction<TItem> CreateShiftAction(ItemSet<TItem> nextState) => new(nextState);

		public static ReduceAction CreateReduceAction(ProductionRule productionRule) => new(productionRule);
	}
}