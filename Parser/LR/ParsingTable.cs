using System;
using System.Linq;

namespace Parser.LR {
	public abstract class ParsingTable<TItem> where TItem : ItemBase {
		protected internal ParsingTable(Grammar grammar) => Grammar = grammar;

		public Grammar Grammar { get; }

		public ActionTable<TItem>? ActionTable { get; protected set; }

		public GotoTable<TItem>? GotoTable { get; protected set; }

		public ItemSetCollectionBase<TItem>? ItemSets { get; protected set; }

		public IAction this[ItemSet<TItem> state, Terminal terminal] {
			get => ActionTable?[state, terminal] ?? throw new Exception("Action table not initialized");
			set {
				if (ActionTable is null)
					throw new Exception("Action table not initialized");
				ActionTable[state, terminal] = value;
			}
		}

		public ItemSet<TItem>? this[ItemSet<TItem> state, Nonterminal nonterminal] {
			get => GotoTable?[state, nonterminal] ?? throw new Exception("Goto table not initialized");
			set {
				if (GotoTable is null)
					throw new Exception("Goto table not initialized");
				GotoTable[state, nonterminal] = value;
				GotoTable[state, nonterminal] = value;
			}
		}

		public event EventHandler StartItemSetsCalculation = delegate { };

		public event EventHandler CompleteItemSetsCalculation = delegate { };

		public event EventHandler StartTableCalculation = delegate { };

		public event EventHandler CompleteTableCalculation = delegate { };

		public void Initialize() => Initialize(ExtendGrammar(Grammar));

		public CompiledParsingTable Compile() => CompiledParsingTable.FromParsingTable(this);

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
		protected abstract void Initialize(Grammar extendedGrammar);

		protected void OnStartItemSetsCalculation() => StartItemSetsCalculation(this, EventArgs.Empty);

		protected void OnCompleteItemSetsCalculation() => CompleteItemSetsCalculation(this, EventArgs.Empty);

		protected void OnStartTableCalculation() => StartTableCalculation(this, EventArgs.Empty);

		protected void OnCompleteTableCalculation() => CompleteTableCalculation(this, EventArgs.Empty);
	}
}