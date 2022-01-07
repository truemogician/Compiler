using System;
using System.Collections.Generic;
using Parser;

namespace Analyzer {
	public interface IAnalyzer {
		public string Name { get; }

		public object Analyze(object source, out IEnumerable<SemanticError> errors);
	}

	public interface IAnalyzer<in TSource, out TTarget> : IAnalyzer {
		object IAnalyzer.Analyze(object source, out IEnumerable<SemanticError> errors) => Analyze((TSource)source, out errors)!;

		public TTarget Analyze(TSource source, out IEnumerable<SemanticError> errors);
	}

	public interface IAnalyzer<T> : IAnalyzer<T, T> { }

	public interface IReadOnlyAnalyzer<T> : IAnalyzer<T> {
		T IAnalyzer<T, T>.Analyze(T source, out IEnumerable<SemanticError> errors) {
			errors = Analyze(source);
			return source;
		}

		protected IEnumerable<SemanticError> Analyze(T source);
	}

	public interface IRootAnalyzer<out T> : IAnalyzer<SyntaxTree, T> { }
}
