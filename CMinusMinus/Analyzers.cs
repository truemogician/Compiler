using Analyzer;
using CMinusMinus.Analyzers;

namespace CMinusMinus {
	public partial class CMinusMinusFactory {
		public override AnalyzerCollection CreateAnalyzers() {
			var collection = new AnalyzerCollection();
			var propertyAnalyzer = new PropertyAnalyzer();
			collection.Add(propertyAnalyzer);
			collection.Add(new JumpStatementAnalyzer(), propertyAnalyzer);
			var identifierAnalyzer = new IdentifierAnalyzer();
			collection.Add(identifierAnalyzer, propertyAnalyzer);
			collection.Add(new TypeAnalyzer(), identifierAnalyzer);
			return collection;
		}
	}
}