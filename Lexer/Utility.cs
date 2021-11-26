using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;

#nullable enable
namespace Lexer {
	public static class Utility {
		/// <returns>A match object using <paramref name="input" />'s buffer as base string</returns>
		public static Match Match(this Regex regex, StringSegment input) => regex.Match(input.Buffer, input.Offset, input.Length);

		/// <inheritdoc cref="Match(Regex, StringSegment)" />
		public static Match Match(this Regex regex, StringSegment input, int startAt) => regex.Match(input.Buffer, startAt + input.Offset, input.Length);

		/// <inheritdoc cref="Match(Regex, StringSegment)" />
		public static Match Match(this Regex regex, StringSegment input, int beginning, int length) => regex.Match(input.Buffer, beginning + input.Offset, Math.Min(length, input.Length));
	}
}