using System;
using System.Collections.Generic;
using Analyzer;
using CMinusMinus.Analyzers.SyntaxComponents;
using Parser;

namespace CMinusMinus.Analyzers {
	public class PropertyAnalyzer : IRootAnalyzer<Program> {
		public string Name => nameof(PropertyAnalyzer);

		public Program Analyze(SyntaxTree source, out IEnumerable<SemanticError> errors) {
			errors = Array.Empty<SemanticError>();
			return new Program(source);
		}
	}
}