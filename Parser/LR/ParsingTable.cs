using System.Linq;
using TrueMogician.Exceptions;

// ReSharper disable VirtualMemberCallInConstructor
namespace Parser.LR {
	public abstract class ParsingTable<TItem> where TItem : ItemBase {
		protected internal ParsingTable(Grammar grammar) {
			Grammar = grammar;
			ExtendedGrammar = ExtendGrammar(grammar);
			FirstSetCollection = new FirstSetCollection(ExtendedGrammar);
			if (!BuildTables(out var actionTable, out var gotoTable))
				throw new ParserException();
			ActionTable = actionTable;
			GotoTable = gotoTable;
		}

		public Grammar Grammar { get; }

		public ActionTable<TItem> ActionTable { get; }

		public GotoTable<TItem> GotoTable { get; }

		protected Grammar ExtendedGrammar { get; }

		protected FirstSetCollection FirstSetCollection { get; }

		public IAction this[ItemSet<TItem> state, Symbol symbol] {
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

		protected static Grammar ExtendGrammar(Grammar grammar) {
			var newGrammar = new Grammar(grammar);
			var initial = new Nonterminal("_Start");
			var nonterminals = newGrammar.Nonterminals.ToList();
			while (nonterminals.Contains(initial))
				initial = new Nonterminal("_" + initial.Name);
			newGrammar.AddProductionRule(initial, newGrammar.InitialState!);
			newGrammar.InitialState = initial;
			return newGrammar;
		}

		protected abstract bool BuildTables(out ActionTable<TItem> actionTable, out GotoTable<TItem> gotoTable);
	}
}