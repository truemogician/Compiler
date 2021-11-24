using System.Linq;
using TrueMogician.Exceptions;

// ReSharper disable VirtualMemberCallInConstructor
namespace Parser.LR {
	public abstract class ParsingTable<TItem> where TItem : ItemBase {
		protected readonly Grammar Grammar;

		protected internal ParsingTable(Grammar grammar) {
			Grammar = grammar;
			Initialize(ExtendGrammar(grammar), out var itemSets, out var actionTable, out var gotoTable);
			ItemSets = itemSets;
			ActionTable = actionTable;
			GotoTable = gotoTable;
		}

		public ActionTable<TItem> ActionTable { get; }

		public GotoTable<TItem> GotoTable { get; }

		public ItemSetCollectionBase<TItem> ItemSets { get; }

		public IAction this[ItemSet<TItem> state, Terminal terminal] {
			get => ActionTable[state, terminal];
			set => ActionTable[state, terminal] = value;
		}

		public ItemSet<TItem> this[ItemSet<TItem> state, Nonterminal nonterminal] {
			get => GotoTable[state, nonterminal];
			set => GotoTable[state, nonterminal] = value;
		}

		protected static Grammar ExtendGrammar(Grammar grammar) {
			var initial = new Nonterminal(grammar.InitialState.Name + "'");
			var nonterminals = grammar.Nonterminals.ToList();
			while (nonterminals.Contains(initial))
				initial = new Nonterminal(initial.Name + "'");
			var newGrammar = new Grammar(grammar);
			newGrammar.Add(initial, newGrammar.InitialState!);
			newGrammar.InitialState = initial;
			return newGrammar;
		}

		protected abstract void Initialize(Grammar extendedGrammar, out ItemSetCollectionBase<TItem> itemSets, out ActionTable<TItem> actionTable, out GotoTable<TItem> gotoTable);
	}
}