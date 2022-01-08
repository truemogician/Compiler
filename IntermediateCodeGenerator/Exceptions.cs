using System.Collections.Generic;
using Analyzer;
using TrueMogician.Exceptions;

namespace IntermediateCodeGenerator {
	public class SemanticErrorException : ExceptionWithDefaultMessage {
		public IReadOnlyList<SemanticError>? Errors { get; init; }

		protected override string? DefaultMessage => "Unhandled semantic errors detected";
	}
}