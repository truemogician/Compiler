using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lexer;

namespace Parser.LR {
	public abstract class ParserBase<TItem> : ParserBase where TItem : ItemBase {
		#pragma warning disable 8618
		protected ParserBase(Grammar grammar) : base(grammar) { }
		#pragma warning restore 8618

		public virtual ParsingTable<TItem> ParsingTable { get; private set; }

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public override AbstractSyntaxTree Parse(IEnumerable<Token> tokens) {
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

		protected override void Initialize(Grammar grammar) => ParsingTable = GenerateParsingTable(grammar);

		protected abstract ParsingTable<TItem> GenerateParsingTable(Grammar grammar);
	}
}