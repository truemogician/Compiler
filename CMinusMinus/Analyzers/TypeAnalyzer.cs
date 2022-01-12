using System.Collections.Generic;
using Analyzer;
using CMinusMinus.Analyzers.SyntaxComponents;

namespace CMinusMinus.Analyzers {
	public class TypeAnalyzer : IReadOnlyAnalyzer<Program> {
		private static readonly SemanticErrorType TypeNotMatchedError = new("TP0001", ErrorLevel.Error, Name) { DefaultMessage = "Identifier not defined." };

		private static readonly SemanticErrorType InvalidCastError = new("TP0002", ErrorLevel.Error, Name) { DefaultMessage = "Identifier already declared." };

		private static readonly SemanticErrorType PrecisionLostWarning = new("TP0003", ErrorLevel.Warning, Name) { DefaultMessage = "Identifier is never used." };

		string IAnalyzer.Name => nameof(TypeAnalyzer);

		IEnumerable<SemanticError> IReadOnlyAnalyzer<Program>.Analyze(Program source) {
			yield break;
		}

		public static string Name => nameof(TypeAnalyzer);
	}
}