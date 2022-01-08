using System.Collections.Generic;

namespace IntermediateCodeGenerator {
	public interface IIntermediateCodeGenerator<in TSource, out TCode> where TCode : IIntermediateCode {
		public IEnumerable<TCode> Generate(TSource source);
	}
}