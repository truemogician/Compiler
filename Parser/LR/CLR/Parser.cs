using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lexer;

#pragma warning disable IDE0079// Remove unnecessary suppression
namespace Parser.LR.CLR {
	using ItemSet = ItemSet<Item>;

	public class Parser : ParserBase<Item, IAction> {
		public Parser(Grammar grammar) : base(grammar) { }

		public override ParsingTable ParsingTable => (base.ParsingTable as ParsingTable)!;

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public override SyntaxTree Parse(IEnumerable<Token> tokens) {
			if (ParsingTable.ItemSets is null)
				throw new ParserNotInitializedException();
			Stack<ItemSet> stateStack = new();
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
					case ShiftAction<Item> shiftAction:
						stateStack.Push(shiftAction.NextState);
						symbolStack.Push(new SyntaxTreeValue(terminal!, token ?? throw new UnexpectedActionException<Item, IAction>(ParsingTable, tokens, position)));
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
			throw new UnexpectedActionException<Item, IAction>(ParsingTable, tokens, position);
		}

		protected override ParsingTableBase<Item, IAction> CreateParsingTable() => new ParsingTable(Grammar);
	}
}