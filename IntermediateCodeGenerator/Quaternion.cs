using System;

namespace IntermediateCodeGenerator {
	public record Quaternion<TOperation, TOperand>(TOperation Operation, TOperand LeftOperand, TOperand? RightOperand, TOperand Result) where TOperation : struct, Enum where TOperand : class {
		public Quaternion(TOperation operation, TOperand operand, TOperand result) : this(operation, operand, null, result) { }
	}
}
