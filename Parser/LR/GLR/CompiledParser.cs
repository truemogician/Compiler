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
			Stack<SyntaxTreeNode> symbolTemp = new();
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
			void ApplyAction(TreeStack<int>.BranchStack stateStack, TreeStack<SyntaxTreeNode>.BranchStack symbolStack, (ActionType Action, int Index) action) {
				switch (action.Action) {
					case ActionType.Shift:
						stateStack.Push(action.Index);
						symbolStack.Push(new SyntaxTreeValue(_table.Terminals[terminalIndex], token ?? throw new Exception()));
						break;
					case ActionType.Reduce:
						var (nonterminalIndex, length) = _table.ProductionRules[action.Index];
						var newNode = new SyntaxTreeNode(_table.Nonterminals[nonterminalIndex]);
						for (var i = 0; i < length; ++i) {
							stateStack.Pop();
							symbolTemp.Push(symbolStack.Pop());
						}
						while (symbolTemp.Count > 0)
							symbolTemp.Pop().Parent = newNode;
						stateStack.Push(_table.GotoTable[stateStack.Peek(), nonterminalIndex] is var idx and >= 0 ? idx : throw new NotRecognizedException(tokens, position));
						symbolStack.Push(newNode);
						break;
				}
			}
			SyntaxTreeNode? ApplyActionsUntilShift(TreeStack<int>.BranchStack stateStack, TreeStack<SyntaxTreeNode>.BranchStack symbolStack, (ActionType Action, int Index)[]? actions) {
				if (actions is null) {
					stateStack.Delete();
					symbolStack.Delete();
					if (!stateStack.Any())
						throw new NotRecognizedException(tokens, position);
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
				foreach (var (newStateStack, newSymbolStack, action) in enumerable)
					while (true) {
						if (action.Action == ActionType.Accept)
							return newSymbolStack.Single();
						ApplyAction(newStateStack, newSymbolStack, action);
						if (action.Action == ActionType.Shift)
							break;
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