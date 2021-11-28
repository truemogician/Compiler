using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lexer;

namespace Parser.LR {
	public abstract class ParserBase<TItem> : ParserBase where TItem : ItemBase {
		// ReSharper disable once VirtualMemberCallInConstructor
		protected ParserBase(Grammar grammar) : base(grammar) => ParsingTable = CreateParsingTable();

		public virtual ParsingTable<TItem> ParsingTable { get; private set; }

		public event EventHandler StartItemSetsCalculation {
			add => ParsingTable.StartItemSetsCalculation += value;
			remove => ParsingTable.StartItemSetsCalculation -= value;
		}

		public event EventHandler CompleteItemSetsCalculation {
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

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public override AbstractSyntaxTree Parse(IEnumerable<Token> tokens) {
			if (ParsingTable.ItemSets is null)
				throw new Exception("Parsing table not initialized");
			Stack<ItemSet<TItem>> stateStack = new();
			Stack<SyntaxTreeNode> symbolStack = new(), symbolTemp = new();
			stateStack.Push(ParsingTable.ItemSets.InitialState);
			using var enumerator = tokens.GetEnumerator();
			int position = -1;
			Token? token = null;
			Terminal? terminal = null;
			var finished = false;
			bool MoveNext() {
				if (finished)
					return false;
				if (!enumerator.MoveNext()) {
					finished = true;
					token = null;
					terminal = Terminal.Terminator;
				}
				else {
					++position;
					token = enumerator.Current;
					terminal = Grammar.Match(token) ?? throw new TerminalNotMatchedException(tokens, position) {CurrentStack = symbolStack, Grammar = Grammar};
				}
				return true;
			}
			MoveNext();
			IAction action;
			do {
				action = ParsingTable[stateStack.Peek(), terminal!];
				switch (action) {
					case ErrorAction:  throw new NotRecognizedException(tokens, position) {CurrentStack = symbolStack, Grammar = Grammar};
					case AcceptAction: return symbolStack.Single();
					case ShiftAction<TItem> shiftAction:
						stateStack.Push(shiftAction.NextState);
						symbolStack.Push(new SyntaxTreeValue(terminal!, token ?? throw new UnexpectedActionException<TItem>(ParsingTable, tokens, position)));
						break;
					case ReduceAction reduceAction:
						var pr = reduceAction.ProductionRule;
						var newNode = new SyntaxTreeNode(pr.Nonterminal);
						for (var i = 0; i < pr.Length; ++i) {
							stateStack.Pop();
							symbolTemp.Push(symbolStack.Pop());
						}
						while (symbolTemp.Count > 0)
							symbolTemp.Pop().Parent = newNode;
						stateStack.Push(ParsingTable[stateStack.Peek(), pr.Nonterminal] ?? throw new NotRecognizedException(tokens, position) {CurrentStack = symbolStack, Grammar = Grammar});
						symbolStack.Push(newNode);
						break;
				}
			} while (action.Type != ActionType.Shift || MoveNext());
			throw new UnexpectedActionException<TItem>(ParsingTable, tokens, position);
		}

		public CompiledParser Compile() => new(ParsingTable.Compile());

		public override void Initialize() => ParsingTable.Initialize();

		/// <summary>
		///     Create a parsing table, but don't initialize it
		/// </summary>
		/// <returns></returns>
		protected abstract ParsingTable<TItem> CreateParsingTable();
	}
}