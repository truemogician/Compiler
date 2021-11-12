using System;
using TrueMogician.Exceptions;

namespace Parser.LR {
	public abstract class ParsingTable<TItem> where TItem : ItemBase {
		protected internal ParsingTable() { }

		public ActionTable<TItem> ActionTable { get; } = new();

		public GotoTable<TItem> GotoTable { get; } = new();

		public IAction this[ItemSetBase<TItem> state, Symbol symbol] {
			get => symbol.IsTerminal
				? ActionTable[state, symbol.AsTerminal]
				: GotoTable[state, symbol.AsNonterminal] is { } nextState
					? new ShiftAction<TItem>(nextState)
					: ActionFactory<TItem>.ErrorAction;
			set {
				if (symbol.IsTerminal)
					ActionTable[state, symbol.AsTerminal] = value;
				else
					GotoTable[state, symbol.AsNonterminal] = value switch {
						ShiftAction<TItem> shiftAction => shiftAction.NextState,
						ErrorAction                    => null,
						_                              => throw new InvariantTypeException(typeof(ShiftAction<TItem>), value.GetType())
					};
			}
		}

		public static Grammar ExtendGrammar() => throw new NotImplementedException();
	}
}