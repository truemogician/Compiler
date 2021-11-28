namespace Parser.LR.CLR {
	public class Parser : ParserBase<Item> {
		public Parser(Grammar grammar) : base(grammar) { }

		protected override ParsingTable<Item> CreateParsingTable() => new ParsingTable(Grammar);
	}
}