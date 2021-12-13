using System;
using System.Linq;
using NUnit.Framework;
using Parser.LR.GLR;

namespace Parser.Test {
	public class TreeStackTests {
		public static TreeStack<int> Stacks { get; } = new();

		[SetUp]
		public void Setup() {
			Stacks.First!.Push(0, 1, 2, 3);
			var newBranches = Stacks.First!.Fork(3);
			Assert.AreEqual(3, Stacks.Count);
			newBranches[0].Push(4, 5, 6);
			newBranches[1].Push(7);
			newBranches[2].Push(8, 9);
			newBranches = newBranches[1].Fork(2);
			newBranches[0].Push(10, 11, 12, 13, 14);
			Assert.AreEqual(4, Stacks.Count);
		}

		[Test]
		public void EnumeratorTest() {
			var stacks = Stacks.ToArray();
			Assert.AreEqual(new[] {6, 5, 4, 3, 2, 1, 0}, stacks[0].ToArray());
			Assert.AreEqual(new[] {14, 13, 12, 11, 10, 7, 3, 2, 1, 0}, stacks[1].ToArray());
			Assert.AreEqual(new[] {7, 3, 2, 1, 0}, stacks[2].ToArray());
			Assert.AreEqual(new[] {9, 8, 3, 2, 1, 0}, stacks[3].ToArray());
		}

		[Test]
		public void PopTest() {
			var stacks = Stacks.ToArray();
			Assert.AreEqual(new[] {6, 5}, stacks[0].Pop(2));
			Assert.AreEqual(14, stacks[1].Peek());
			Assert.AreEqual(14, stacks[1].Pop());
			Assert.AreEqual(new[] {7, 3, 2, 1}, stacks[2].Pop(4));
			Assert.Throws<ArgumentOutOfRangeException>(() => stacks[3].Pop(7));
			stacks[3].Pop(6);
			Assert.AreEqual(0, stacks[3].Count);
			Assert.AreEqual(4, Stacks.Count);
		}

		[Test]
		public void ForkDeleteTest() {
			var stacks = Stacks.ToArray();
			var newStacks = stacks[0].Fork(3);
			newStacks[0].Fork(2);
			Assert.Throws<InvalidOperationException>(
				() => {
					int _ = stacks[0].Count;
				}
			);
			Assert.AreEqual(
				7,
				Stacks.Sum(
					branch => {
						branch.Delete();
						return 1;
					}
				)
			);
			Assert.IsNull(Stacks.Last);
		}
	}
}