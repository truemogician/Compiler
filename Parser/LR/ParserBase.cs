using System.Collections.Generic;
using System.Linq;
using Lexer;

#nullable enable
namespace Parser.LR {
	public abstract class ParserBase<TItem> : ParserBase where TItem : ItemBase {
		#pragma warning disable 8618
		protected ParserBase(Grammar grammar) : base(grammar) { }
		#pragma warning restore 8618

		public virtual ParsingTable<TItem> ParsingTable { get; private set; }

		public override AbstractSyntaxTree Parse(IEnumerable<Lexeme> lexemes) {
			Stack<ItemSet<TItem>> stateStack = new();
			Stack<SyntaxTreeNode> symbolStack = new();
			stateStack.Push(ParsingTable.ItemSets.InitialState);
			using var enumerator = lexemes.GetEnumerator();
			Lexeme? lexeme = null;
			Terminal? terminal = null;
			bool MoveNext() {
				if (!enumerator.MoveNext())
					return false;
				lexeme = enumerator.Current;
				terminal = Grammar.Match(lexeme) ?? throw new ParserException();
				return true;
			}
			if (!MoveNext())
				throw new ParserException();
			IAction action;
			do {
				action = ParsingTable.ActionTable[stateStack.Peek(), terminal!];
				switch (action) {
					case ErrorAction: throw new ParserException();
					case ShiftAction<TItem> shiftAction:
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

		protected override void Initialize(Grammar grammar) => ParsingTable = GenerateParsingTable(grammar);

		protected abstract ParsingTable<TItem> GenerateParsingTable(Grammar grammar);
	}
}