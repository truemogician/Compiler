using System.Collections.Generic;
using Analyzer;
using Parser;

namespace CMinusMinus.Analyzers.SyntaxComponents {
	public class Program : SyntaxComponent {
		public Program(SyntaxTree rootTree) : base(rootTree.Root) {
			var node = rootTree.Root;
			ThrowHelper.IsNonterminal(node, NonterminalType.Program);
			var functions = new List<FunctionDefinition>();
			var globals = new List<Declaration>();
			foreach (var child in node.Children)
				switch (child.Value.Nonterminal?.Name) {
					case nameof(NonterminalType.FunctionDefinition):
						functions.Add(child);
						break;
					case nameof(NonterminalType.DeclarationStatement):
						globals.AddRange(Declaration.FromDeclarationStatement(child));
						break;
					default: throw new UnexpectedSyntaxNodeException { Node = child };
				}
			FunctionDefinitions = functions;
			GlobalVariableDeclarations = globals;
		}

		public IReadOnlyList<FunctionDefinition> FunctionDefinitions { get; }

		public IReadOnlyList<Declaration> GlobalVariableDeclarations { get; }

		public static implicit operator Program(SyntaxTreeNode node) => new(node);
	}
}