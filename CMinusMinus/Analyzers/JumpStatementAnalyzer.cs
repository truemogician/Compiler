using System.Collections.Generic;
using System.Linq;
using Analyzer;
using CMinusMinus.Analyzers.SyntaxComponents;

namespace CMinusMinus.Analyzers {
	public class JumpStatementAnalyzer : IReadOnlyAnalyzer<Program> {
		private static readonly SemanticErrorType BreakStatementError = new("JS0001", ErrorLevel.Error, Name) { DefaultMessage = "Break statement appears in wrong context." };

		private static readonly SemanticErrorType ContinueStatementError = new("JS0002", ErrorLevel.Error, Name) { DefaultMessage = "Continue statement appears in wrong context." };

		public static string Name => nameof(JumpStatementAnalyzer);

		string IAnalyzer.Name => Name;

		IEnumerable<SemanticError> IReadOnlyAnalyzer<Program>.Analyze(Program source) => source.FunctionDefinitions.Aggregate(Enumerable.Empty<SemanticError>(), (current, func) => current.Concat(Analyze(func.Body.Components, false, false)));

		private static IEnumerable<SemanticError> Analyze(IEnumerable<BlockComponent> components, bool allowBreak, bool allowContinue) {
			foreach (var comp in components) {
				var content = comp.Content;
				switch (content) {
					case BreakStatement:
						if (!allowBreak)
							yield return content.CreateError(BreakStatementError);
						break;
					case ContinueStatement:
						if (!allowContinue)
							yield return content.CreateError(ContinueStatementError);
						break;
					case Block block:
						foreach (var error in Analyze(block.Components, allowBreak, allowContinue))
							yield return error;
						break;
					case IfBlock block:
						foreach (var branch in block.Branches)
							foreach (var error in Analyze(branch.Body, allowBreak, allowContinue))
								yield return error;
						break;
					case SwitchBlock block:
						foreach (var branch in block.Cases)
							foreach (var error in Analyze(branch.Body, true, allowContinue))
								yield return error;
						break;
					case ForBlock:
					case WhileBlock:
					case DoWhileBlock:
						foreach (var error in Analyze(((LoopBlock)content).Body, true, true))
							yield return error;
						break;
				}
			}
		}
	}
}