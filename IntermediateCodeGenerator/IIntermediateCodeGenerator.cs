using System.Collections.Generic;
using System.Linq;
using Analyzer;

namespace IntermediateCodeGenerator {
	public interface IIntermediateCodeGenerator {
		public IEnumerable<IIntermediateCode> Generate(IReadOnlyList<AnalyzeResult> sources);
	}

	public interface IIntermediateCodeGenerator<out TCode> : IIntermediateCodeGenerator where TCode : IIntermediateCode {
		IEnumerable<IIntermediateCode> IIntermediateCodeGenerator.Generate(IReadOnlyList<AnalyzeResult> sources) => Generate(sources).Cast<IIntermediateCode>();

		public new IEnumerable<TCode> Generate(IReadOnlyList<AnalyzeResult> sources);
	}
}