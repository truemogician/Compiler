namespace Parser.LR.CLR {
	public class Parser : ParserBase<Item> {
		public Parser(Grammar grammar) : base(grammar) { }

		public override ParsingTable ParsingTable => (base.ParsingTable as ParsingTable)!;

		protected override ParsingTableBase<Item> CreateParsingTable() => new ParsingTable(Grammar);
	}
}