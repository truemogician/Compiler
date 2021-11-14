using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Lexer;

#nullable enable
namespace Parser.LR.CLR {
	using ItemSet = ItemSet<Item>;
	using ShiftAction = ShiftAction<Item>;

	public class Parser {
		public Parser(Grammar grammar) => ParsingTable = new ParsingTable(grammar);

		public ParsingTable ParsingTable { get; }

		public AbstractSyntaxTree Parse(IEnumerable<Lexeme> lexemes) {
			Stack<ItemSet> stateStack = new();
			Stack<SyntaxTreeNode> symbolStack = new();
			stateStack.Push(ParsingTable.InitialState);
			using var enumerator = lexemes.GetEnumerator();
			Lexeme? lexeme = null;
			Terminal? terminal = null;
			bool MoveNext() {
				if (!enumerator.MoveNext())
					return false;
				lexeme = enumerator.Current;
				terminal = ParsingTable.Grammar.Match(lexeme) ?? throw new ParserException();
				return true;
			}
			if (!MoveNext())
				throw new ParserException();
			IAction action;
			do {
				action = ParsingTable.ActionTable[stateStack.Peek(), terminal!];
				switch (action) {
					case ErrorAction: throw new ParserException();
					case ShiftAction shiftAction:
						stateStack.Push(shiftAction.NextState);
						symbolStack.Push(new SyntaxTreeValue(terminal!, lexeme!));
						break;
					case ReduceAction reduceAction:
						var pr = reduceAction.ProductionRule;
						var newNode = new SyntaxTreeNode(pr.Nonterminal);
						for (var i = 0; i < pr.Length; ++i) {
							stateStack.Pop();
							symbolStack.Pop().Parent = newNode;
						}
						stateStack.Push(ParsingTable.GotoTable[stateStack.Peek(), pr.Nonterminal] ?? throw new ParserException());
						symbolStack.Push(newNode);
						break;
				}
			} while (action.Type != ActionType.Shift || MoveNext());
			return ParsingTable.ActionTable[stateStack.Peek(), Terminal.Terminator].Type == ActionType.Accept ? symbolStack.Single() : throw new ParserException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryParse(IEnumerable<Lexeme> lexemes, out AbstractSyntaxTree? ast) {
			try {
				ast = Parse(lexemes);
				return true;
			}
			catch {
				ast = null;
				return false;
			}
		}
	}
}