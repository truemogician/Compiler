using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lexer;
using TrueMogician.Exceptions;
using TrueMogician.Extensions.Enumerable;

#pragma warning disable IDE0079// Remove unnecessary suppression
namespace Parser.LR.GLR {
	using Item = CLR.Item;
	using ItemSet = ItemSet<CLR.Item>;

	public class Parser : ParserBase<Item, List<IAction>> {
		public Parser(Grammar grammar) : base(grammar) { }

		public override ParsingTable ParsingTable => (base.ParsingTable as ParsingTable)!;

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public override SyntaxTree Parse(IEnumerable<Token> tokens) {
			if (ParsingTable.ItemSets is null)
				throw new ParserNotInitializedException();
			TreeStack<ItemSet> stateStacks = new();
			TreeStack<SyntaxTreeNode> symbolStacks = new();
			stateStacks.First().Push(ParsingTable.ItemSets.InitialState);
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
					terminal = Grammar.Match(token) ?? throw new TerminalNotMatchedException(tokens, position) {Grammar = Grammar};
				}
				return true;
			}
			void DeleteBranch(TreeStack<ItemSet>.BranchStack stateStack, TreeStack<SyntaxTreeNode>.BranchStack symbolStack) {
				stateStack.Delete();
				symbolStack.Delete();
				if (stateStacks.Count == 0)
					throw new NotRecognizedException(tokens, position) {Grammar = Grammar};
			}
			bool ApplyAction(TreeStack<ItemSet>.BranchStack stateStack, TreeStack<SyntaxTreeNode>.BranchStack symbolStack, IAction action) {
				switch (action) {
					case ShiftAction<Item> shiftAction:
						stateStack.Push(shiftAction.NextState);
						symbolStack.Push(new SyntaxTreeValue(terminal!, token!));
						return true;
					case ReduceAction reduceAction:
						var pr = reduceAction.ProductionRule;
						var newNode = new SyntaxTreeNode(pr.Nonterminal);
						stateStack.Pop(pr.Length);
						var symbols = symbolStack.Pop(pr.Length);
						symbols.Reverse();
						newNode.Children.AddRange(symbols);
						var nextState = ParsingTable[stateStack.Peek(), pr.Nonterminal];
						if (nextState is null) {
							DeleteBranch(stateStack, symbolStack);
							return false;
						}
						stateStack.Push(nextState);
						symbolStack.Push(newNode);
						return true;
					default: throw new BugFoundException();
				}
			}
			SyntaxTreeNode? ApplyActionsUntilShift(TreeStack<ItemSet>.BranchStack stateStack, TreeStack<SyntaxTreeNode>.BranchStack symbolStack, List<IAction> actions) {
				if (actions.Count == 0) {
					DeleteBranch(stateStack, symbolStack);
					return null;
				}
				IEnumerable<(TreeStack<ItemSet>.BranchStack, TreeStack<SyntaxTreeNode>.BranchStack, IAction)> enumerable;
				if (actions.Count > 1) {
					var newStateStacks = stateStack.Fork(actions.Count);
					var newSymbolStacks = symbolStack.Fork(actions.Count);
					enumerable = newStateStacks.IndexJoin(newSymbolStacks, actions);
				}
				else
					enumerable = new[] {(stateStack, symbolStack, actions[0])};
				foreach (var (newStateStack, newSymbolStack, action) in enumerable) {
					if (action.Type == ActionType.Accept)
						return newSymbolStack.Single();
					if (!ApplyAction(newStateStack, newSymbolStack, action) || action.Type == ActionType.Shift)
						continue;
					var newActions = ParsingTable[newStateStack.Peek(), terminal!];
					if (ApplyActionsUntilShift(newStateStack, newSymbolStack, newActions) is { } result)
						return result;
				}
				return null;
			}
			while (MoveNext())
				foreach (var (stateStack, symbolStack) in stateStacks.IndexJoin(symbolStacks)) {
					var actions = ParsingTable[stateStack.Peek(), terminal!];
					if (ApplyActionsUntilShift(stateStack, symbolStack, actions) is { } result)
						return result;
				}
			throw new BugFoundException {BugInformation = "Token enumeration ends without finding a valid syntax tree or throwing exception"};
		}

		public CompiledParser Compile() => CompiledParser.FromParser(this);

		protected override ParsingTableBase<Item, List<IAction>> CreateParsingTable() => new ParsingTable(Grammar);
	}
}