using System;
using System.Collections.Generic;

namespace IntermediateCodeGenerator {
	public interface IIntermediateCodeGenerator<in TSource, TOperation, TOperand> where TOperation : struct, Enum where TOperand : class {
		public IEnumerable<Quaternion<TOperation, TOperand>> Generate(TSource source);
	}
}