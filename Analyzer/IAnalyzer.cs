using System.Collections.Generic;
using Parser;

namespace Analyzer {
	public interface IAnalyzer {
		public string Name { get; }

		public IEnumerable<SemanticError> Analyze(SyntaxTree ast);
	}
}
