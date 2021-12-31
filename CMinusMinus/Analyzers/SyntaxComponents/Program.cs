using System.Collections.Generic;
using Analyzer;
using Parser;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class Program {
		public Program(SyntaxTreeNode node) {
			ThrowHelper.IsNonterminal(node, NonterminalType.Program);
			var functions = new List<FunctionDeclaration>();
			var globals = new List<VariableDeclaration>();
			foreach (var child in node.Children)
				switch (child.Value.Nonterminal?.Name) {
					case nameof(NonterminalType.FunctionDeclaration):
						functions.Add(child);
						break;
					case nameof(NonterminalType.DeclarationStatement):
						globals.AddRange(VariableDeclaration.FromDeclarationStatement(child));
						break;
					default: throw new UnexpectedSyntaxNodeException { Node = child };
				}
			FunctionDeclarations = functions;
			GlobalVariableDeclarations = globals;
		}

		public IReadOnlyList<FunctionDeclaration> FunctionDeclarations { get; }

		public IReadOnlyList<VariableDeclaration> GlobalVariableDeclarations { get; }

		public static implicit operator Program(SyntaxTreeNode node) => new(node);
	}
}