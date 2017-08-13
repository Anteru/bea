using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bea.Core;

namespace Bea.Test
{
	[TestClass]
	public class ExpressionParserTest
	{
		[TestMethod]
		public void TestLiteralExpression ()
		{
			ExpressionParser ep = new ExpressionParser ();
			var expr = ep.Parse ("foo");

			Assert.AreEqual ("foo", expr.Evaluate (new ExpressionEvaluationContext ()));
		}

		[TestMethod]
		public void TestBooleanOutputConditionTrue ()
		{
			ExpressionParser ep = new ExpressionParser ();
			var expr = ep.Parse ("$<1:foo>");

			Assert.AreEqual ("foo", expr.Evaluate (new ExpressionEvaluationContext ()));
		}

		[TestMethod]
		public void TestBooleanOutputConditionFalse ()
		{
			ExpressionParser ep = new ExpressionParser ();
			var expr = ep.Parse ("$<0:foo>");

			Assert.AreEqual (string.Empty, expr.Evaluate (new ExpressionEvaluationContext ()));
		}

		[TestMethod]
		public void TestBooleanOutputConditionTrueNested ()
		{
			ExpressionParser ep = new ExpressionParser ();
			var expr = ep.Parse ("foo$<1:b$<1:a>r>");

			Assert.AreEqual ("foobar", expr.Evaluate (new ExpressionEvaluationContext ()));
		}

		[TestMethod]
		public void TestBooleanOutputConditionTrueNestedTrailingText ()
		{
			ExpressionParser ep = new ExpressionParser ();
			var expr = ep.Parse ("foo$<1:b$<1:a>>r");

			Assert.AreEqual ("foobar", expr.Evaluate (new ExpressionEvaluationContext ()));
		}

		[TestMethod]
		public void TestOrCondition ()
		{
			ExpressionParser ep = new ExpressionParser ();
			var expr = ep.Parse ("$<$<OR:1,1,0,$<1:1>>:foo>");

			Assert.AreEqual ("foo", expr.Evaluate (new ExpressionEvaluationContext ()));
		}

		[TestMethod]
		public void TestEmptyCondition ()
		{
			ExpressionParser ep = new ExpressionParser ();
			var expr = ep.Parse (String.Empty);

			Assert.AreEqual (String.Empty, expr.Evaluate (new ExpressionEvaluationContext ()));
		}
	}
}
