using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lexer;
using TrueMogician.Exceptions;
using TrueMogician.Extensions.Enumerable;

#pragma warning disable IDE0079// Remove unnecessary suppression
namespace Parser.LR.GLR {
	public class CompiledParser : IParser {
		private readonly CompiledParsingTable _table;

		public CompiledParser(CompiledParsingTable table) => _table = table;

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public SyntaxTree Parse(IEnumerable<Token> tokens) {
			TreeStack<int> stateStacks = new();
			TreeStack<SyntaxTreeNode> symbolStacks = new();
			stateStacks.First().Push(0);
			using var enumerator = tokens.GetEnumerator();
			int position = -1;
			Token? token = null;
			int terminalIndex = -1;
			var finished = false;
			bool MoveNext() {
				if (finished)
					return false;
				if (!enumerator.MoveNext()) {
					finished = true;
					token = null;
					terminalIndex = _table.Terminals.Count;//Index of terminator
				}
				else {
					++position;
					token = enumerator.Current;
					terminalIndex = _table.Match(token) ?? throw new TerminalNotMatchedException(tokens, position);
				}
				return true;
			}
			void DeleteBranch(TreeStack<int>.BranchStack stateStack, TreeStack<SyntaxTreeNode>.BranchStack symbolStack) {
				stateStack.Delete();
				symbolStack.Delete();
				if (symbolStacks.Count == 0)
					throw new NotRecognizedException(tokens, position);
			}
			bool ApplyAction(TreeStack<int>.BranchStack stateStack, TreeStack<SyntaxTreeNode>.BranchStack symbolStack, (ActionType Action, int Index) action) {
				switch (action.Action) {
					case ActionType.Shift:
						stateStack.Push(action.Index);
						symbolStack.Push(new SyntaxTreeValue(_table.Terminals[terminalIndex], token ?? throw new Exception()));
						return true;
					case ActionType.Reduce:
						var (nonterminalIndex, length) = _table.ProductionRules[action.Index];
						var newNode = new SyntaxTreeNode(_table.Nonterminals[nonterminalIndex]);
						stateStack.Pop(length);
						var symbols = symbolStack.Pop(length);
						symbols.Reverse();
						newNode.Children.AddRange(symbols);
						int nextState = _table.GotoTable[stateStack.Peek(), nonterminalIndex];
						if (nextState < 0) {
							DeleteBranch(stateStack, symbolStack);
							return false;
						}
						stateStack.Push(nextState);
						symbolStack.Push(newNode);
						return true;
					default: throw new BugFoundException();
				}
			}
			SyntaxTreeNode? ApplyActionsUntilShift(TreeStack<int>.BranchStack stateStack, TreeStack<SyntaxTreeNode>.BranchStack symbolStack, (ActionType Action, int Index)[]? actions) {
				if (actions is null) {
					DeleteBranch(stateStack, symbolStack);
					return null;
				}
				IEnumerable<(TreeStack<int>.BranchStack, TreeStack<SyntaxTreeNode>.BranchStack, (ActionType Action, int Index))> enumerable;
				if (actions.Length > 1) {
					var newStateStacks = stateStack.Fork(actions.Length);
					var newSymbolStacks = symbolStack.Fork(actions.Length);
					enumerable = newStateStacks.IndexJoin(newSymbolStacks, actions);
				}
				else
					enumerable = new[] {(stateStack, symbolStack, actions[0])};
				foreach (var (newStateStack, newSymbolStack, action) in enumerable) {
					if (action.Action == ActionType.Accept)
						return newSymbolStack.Single();
					if (!ApplyAction(newStateStack, newSymbolStack, action) || action.Action == ActionType.Shift)
						continue;
					var newActions = _table.ActionTable[newStateStack.Peek(), terminalIndex];
					if (ApplyActionsUntilShift(newStateStack, newSymbolStack, newActions) is { } result)
						return result;
				}
				return null;
			}
			while (MoveNext())
				foreach (var (stateStack, symbolStack) in stateStacks.IndexJoin(symbolStacks)) {
					var actions = _table.ActionTable[stateStack.Peek(), terminalIndex];
					if (ApplyActionsUntilShift(stateStack, symbolStack, actions) is { } result)
						return result;
				}
			throw new BugFoundException {BugInformation = "Token enumeration ends without finding a valid syntax tree or throwing exception"};
		}

		public static CompiledParser FromParser(Parser parser) => new(CompiledParsingTable.FromParsingTable(parser.ParsingTable));

		public static CompiledParser Load(string path) => new(CompiledParsingTable.Load(path));

		public void Save(string path) => _table.Save(path);
	}
}