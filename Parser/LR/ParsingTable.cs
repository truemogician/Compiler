using System;
using TrueMogician.Exceptions;

namespace Parser.LR {
	public class ParsingTable<TNonterminal, TToken, TItem> where TNonterminal : struct, Enum where TToken : struct, Enum where TItem : IItem<TNonterminal, TToken> {
		internal ParsingTable() { }

		public ParsingTable(Grammar<TNonterminal, TToken> grammar) => throw new NotImplementedException();

		public ActionTable<TNonterminal, TToken, TItem> ActionTable { get; } = new();

		public GotoTable<TNonterminal, TToken, TItem> GotoTable { get; } = new();

		public IAction this[ItemSetBase<TNonterminal, TToken, TItem> state, Symbol<TNonterminal, TToken> symbol] {
			get => symbol.IsTerminal
				? ActionTable[state, symbol.AsTerminal]
				: GotoTable[state, symbol.AsNonterminal] is { } nextState
					? new ShiftAction<TNonterminal, TToken, TItem>(nextState)
					: ActionFactory<TNonterminal, TToken, TItem>.ErrorAction;
			set {
				if (symbol.IsTerminal)
					ActionTable[state, symbol.AsTerminal] = value;
				else
					GotoTable[state, symbol.AsNonterminal] = value switch {
						ShiftAction<TNonterminal, TToken, TItem> shiftAction => shiftAction.NextState,
						ErrorAction                                          => null,
						_                                                    => throw new InvariantTypeException(typeof(ShiftAction<TNonterminal, TToken, TItem>), value.GetType())
					};
			}
		}
	}
}