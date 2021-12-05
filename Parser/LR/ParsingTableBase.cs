using System;
using System.Linq;
using TrueMogician.Exceptions;

namespace Parser.LR {
	using static Utilities;

	public abstract class ParsingTableBase<TItem, TAction> where TItem : ItemBase {
		protected internal ParsingTableBase(Grammar grammar) => Grammar = grammar;

		public Grammar Grammar { get; }

		public ActionTable<TItem, TAction>? ActionTable { get; protected set; }

		public GotoTable<TItem>? GotoTable { get; protected set; }

		public ItemSetCollectionBase<TItem>? ItemSets { get; protected set; }

		public bool Initialized => ActionTable is not null && GotoTable is not null && ItemSets is not null;

		public TAction this[ItemSet<TItem> state, Terminal terminal] {
			get => ActionTable is not null ? ActionTable[state, terminal] : throw new ParserNotInitializedException("Action table not initialized");
			set {
				if (ActionTable is null)
					throw new ParserNotInitializedException("Action table not initialized");
				ActionTable[state, terminal] = value;
			}
		}

		public ItemSet<TItem>? this[ItemSet<TItem> state, Nonterminal nonterminal] {
			get => GotoTable?[state, nonterminal] ?? throw new ParserNotInitializedException("Goto table not initialized");
			set {
				if (GotoTable is null)
					throw new ParserNotInitializedException("Goto table not initialized");
				GotoTable[state, nonterminal] = value;
				GotoTable[state, nonterminal] = value;
			}
		}

		public event EventHandler StartItemSetsCalculation = delegate { };

		public event EventHandler<EventArgs<ItemSetCollectionBase<TItem>>> CompleteItemSetsCalculation = delegate { };

		public event EventHandler StartTableCalculation = delegate { };

		public event EventHandler CompleteTableCalculation = delegate { };

		public void Initialize() {
			Initialize(ExtendGrammar(Grammar));
			if (!Initialized)
				throw new BadImplementationException(nameof(Initialize), null, null);
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

		/// <summary>
		///     The implementation should create ItemSets, ActionTable and GotoTable in this method
		/// </summary>
		/// <param name="extendedGrammar">The extended grammar</param>
		protected abstract void Initialize(Grammar extendedGrammar, bool checkConflicts = true);

		protected void OnStartItemSetsCalculation() => StartItemSetsCalculation(this, EventArgs.Empty);

		protected void OnCompleteItemSetsCalculation() => CompleteItemSetsCalculation(this, ItemSets ?? throw new BadImplementationException());

		protected void OnStartTableCalculation() => StartTableCalculation(this, EventArgs.Empty);

		protected void OnCompleteTableCalculation() => CompleteTableCalculation(this, EventArgs.Empty);
	}
}