using System.Collections.Generic;
using System.Linq;
using Analyzer;
using CMinusMinus.Analyzers.SyntaxComponents;

namespace CMinusMinus.Analyzers {
	public class IdentifierAnalyzer : IReadOnlyAnalyzer<Program> {
		private static readonly SemanticErrorType NotDefinedError = new("ID0001", ErrorLevel.Error, Name) { DefaultMessage = "Identifier not defined." };

		private static readonly SemanticErrorType RedeclarationError = new("ID0002", ErrorLevel.Error, Name) { DefaultMessage = "Identifier already declared." };

		private static readonly SemanticErrorType NotUsedWarning = new("ID0003", ErrorLevel.Warning, Name) { DefaultMessage = "Identifier is never used." };

		private static readonly SemanticErrorType NeverUpdatedWarning = new("ID0004", ErrorLevel.Warning, Name) { DefaultMessage = "Identifier is never updated." };

		private static readonly SemanticErrorType MissingParamNameError = new("ID0005", ErrorLevel.Error, Name) { DefaultMessage = "Function parameter name is missing." };

		public static string Name => nameof(IdentifierAnalyzer);

		string IAnalyzer.Name => Name;

		IEnumerable<SemanticError> IReadOnlyAnalyzer<Program>.Analyze(Program source) {
			Stack<string> idStack = new();
			List<int> scopeCount = new();
			SemanticError? Push(string identifier, SyntaxComponent component) {
				if (idStack.Contains(identifier))
					return component.CreateError(RedeclarationError);
				idStack.Push(identifier);
				++scopeCount[^1];
				return null;
			}
			void Enter() => scopeCount.Add(0);
			void Quit() {
				int count = scopeCount[^1];
				scopeCount.RemoveAt(scopeCount.Count - 1);
				while (count-- > 0)
					idStack.Pop();
			}
			IEnumerable<SemanticError> AnalyzeBlock(IEnumerable<BlockComponent> components, bool newScope = true) {
				IEnumerable<SemanticError> ValidateExpression(Expression? exp) {
					if (exp is null)
						yield break;
					if (exp.Identifier is { } id) {
						if (!idStack.Contains(id))
							yield return id.CreateError(NotDefinedError);
					}
					else if (exp.Operands is { } ops)
						foreach (var operand in ops)
							foreach (var e in ValidateExpression(operand))
								yield return e;
				}
				IEnumerable<SemanticError> HandleStatement(Statement statement) {
					switch (statement) {
						case DeclarationStatement s:
							foreach (var v in s.VariableDeclarations)
								Push(v.Name, v.Name);
							break;
						case ReturnStatement s:
							foreach (var e in ValidateExpression(s.ReturnValue))
								yield return e;
							break;
						case ExpressionStatement s:
							foreach (var e in ValidateExpression(s.Expression))
								yield return e;
							break;
					}
				}
				if (newScope)
					Enter();
				foreach (var comp in components)
					switch (comp.Content) {
						case Block b:
							foreach (var e in AnalyzeBlock(b.Components))
								yield return e;
							break;
						case Statement s:
							foreach (var e in HandleStatement(s))
								yield return e;
							break;
						case IfBlock b:
							foreach (var (condition, body) in b.Branches) {
								foreach (var e in ValidateExpression(condition).Concat(AnalyzeBlock(body)))
									yield return e;
							}
							break;
						case SwitchBlock b:
							foreach (var e in ValidateExpression(b.Expression))
								yield return e;
							foreach (var (value, body) in b.Cases) {
								foreach (var e in ValidateExpression(value).Concat(AnalyzeBlock(body)))
									yield return e;
							}
							break;
						case ForBlock b:
							Enter();
							foreach (var e in HandleStatement(b.Initialization).Concat(ValidateExpression(b.Condition)).Concat(ValidateExpression(b.Iteration)))
								yield return e;
							foreach (var e in AnalyzeBlock(b.Body, false))
								yield return e;
							Quit();
							break;
						case WhileBlock:
						case DoWhileBlock:
							var block = (LoopBlock)comp.Content;
							foreach (var e in ValidateExpression(block.Condition).Concat(AnalyzeBlock(block.Body)))
								yield return e;
							break;
					}
				if (newScope)
					Quit();
			}
			Enter();
			foreach (var func in source.FunctionDefinitions)
				if (Push(func.Name, func) is { } e)
					yield return e;
			foreach (var v in source.GlobalVariableDeclarations)
				if (Push(v.Name, v.Name) is { } e)
					yield return e;
			foreach (var func in source.FunctionDefinitions) {
				Enter();
				foreach (var (_, name) in func.Type.Parameters)
					if (name is null)
						yield return func.Name.CreateError(MissingParamNameError);
					else
						Push(name, name);
				foreach (var e in AnalyzeBlock(func.Body.Components, false))
					yield return e;
				Quit();
			}
		}
	}
}