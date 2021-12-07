using System;
using System.Collections.Generic;
using Lexer;

namespace Parser.LR {
	using static Utilities;

	public abstract class ParserBase<TItem, TAction> : ParserBase where TItem : ItemBase {
		// ReSharper disable once VirtualMemberCallInConstructor
		protected ParserBase(Grammar grammar) : base(grammar) => ParsingTable = CreateParsingTable();

		public virtual ParsingTableBase<TItem, TAction> ParsingTable { get; }

		public override bool Initialized { get; protected set; } = false;

		public event EventHandler StartItemSetsCalculation {
			add => ParsingTable.StartItemSetsCalculation += value;
			remove => ParsingTable.StartItemSetsCalculation -= value;
		}

		public event EventHandler<EventArgs<ItemSetCollectionBase<TItem>>> CompleteItemSetsCalculation {
			add => ParsingTable.CompleteItemSetsCalculation += value;
			remove => ParsingTable.CompleteItemSetsCalculation -= value;
		}

		public event EventHandler StartTableCalculation {
			add => ParsingTable.StartTableCalculation += value;
			remove => ParsingTable.StartTableCalculation -= value;
		}

		public event EventHandler CompleteTableCalculation {
			add => ParsingTable.CompleteTableCalculation += value;
			remove => ParsingTable.CompleteTableCalculation -= value;
		}

		public abstract override SyntaxTree Parse(IEnumerable<Token> tokens);

		/// <summary>
		///     Initialize the parsing table if not initialized
		/// </summary>
		public override void Initialize(bool checkConflicts = true) {
			if (!Initialized)
				ParsingTable.Initialize(checkConflicts);
			Initialized = true;
		}

		/// <summary>
		///     Create a parsing table, but don't initialize it
		/// </summary>
		/// <returns></returns>
		protected abstract ParsingTableBase<TItem, TAction> CreateParsingTable();
	}
}