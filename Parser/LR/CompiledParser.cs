﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Lexer;

#nullable enable
namespace Parser.LR {
	public class CompiledParser : IParser {
		private readonly CompiledParsingTable _table;

		public CompiledParser(CompiledParsingTable table) => _table = table;

		public Grammar Grammar => throw new NotSupportedException("Compiled parser doesn't contains an instance of Grammar");

		[SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
		public AbstractSyntaxTree Parse(IEnumerable<Token> tokens) {
			Stack<int> stateStack = new();
			Stack<SyntaxTreeNode> symbolStack = new(), symbolTemp = new();
			stateStack.Push(0);
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
					terminalIndex = _table.Terminals.Count - 1;
				}
				else {
					++position;
					token = enumerator.Current;
					terminalIndex = _table.Match(token) ?? throw new TerminalNotMatchedException(tokens, position) {CurrentStack = symbolStack};
				}
				return true;
			}
			MoveNext();
			(ActionType? Action, int Index) action;
			do {
				action = _table[stateStack.Peek(), terminalIndex];
				switch (action.Action!.Value) {
					case ActionType.Error:  throw new NotRecognizedException(tokens, position) {CurrentStack = symbolStack};
					case ActionType.Accept: return symbolStack.Single();
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
						stateStack.Push(_table[stateStack.Peek(), nonterminalIndex + _table.Terminals.Count] is {Index: >= 0} idx ? idx.Index : throw new NotRecognizedException(tokens, position) {CurrentStack = symbolStack});
						symbolStack.Push(newNode);
						break;
				}
			} while (action.Action != ActionType.Shift || MoveNext());
			throw new Exception();
		}
	}
}