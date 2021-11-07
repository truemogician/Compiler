using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;

#nullable enable
namespace Lexer {
	public static class Utility {
		/// <returns>A match object using <paramref name="input"/>'s value as base string</returns>
		public static Match Match(this Regex regex, StringSegment input) => regex.Match(input.Value);

		/// <inheritdoc cref="Match(Regex, StringSegment)"/>
		public static Match Match(this Regex regex, StringSegment input, int startAt) => regex.Match(input.Value, startAt);

		/// <inheritdoc cref="Match(Regex, StringSegment)"/>
		public static Match Match(this Regex regex, StringSegment input, int beginning, int length) {
			var substr = input.Value;
			return regex.Match(substr, beginning, Math.Min(length, substr.Length - beginning));
		}
	}
}