namespace Parser.LR.CLR {
	public class Parser : ParserBase<Item> {
		public Parser(Grammar grammar) : base(grammar) { }

		protected override ParsingTable<Item> GenerateParsingTable(Grammar grammar) => new ParsingTable(grammar);
	}
}