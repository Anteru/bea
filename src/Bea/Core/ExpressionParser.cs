using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bea.Core
{
	public class ExpressionEvaluationContext
	{
		public string Configuration { get; set; }
		public string Platform { get; set; }
	}

	public interface IExpression
	{
		string Evaluate (ExpressionEvaluationContext ctx);
	}

	public class LiteralExpression : IExpression
	{
		public LiteralExpression (string v)
		{
			Value = v;
		}

		public string Value { get; }

		public string Evaluate (ExpressionEvaluationContext ctx) 
		{
			return Value;
		}
	}

	public class ChainedExpression : IExpression
	{
		public ChainedExpression (IExpression left, IExpression right)
		{
			Expressions.Add (left);
			Expressions.Add (right);
		}

		public List<IExpression> Expressions { get; } = new List<IExpression> ();
		
		public string Evaluate (ExpressionEvaluationContext ctx)
		{
			return Expressions.Select (e => e.Evaluate (ctx)).Aggregate (new StringBuilder (),
				(sb, s) => sb.Append (s), (sb) => sb.ToString());
		}
	}

	public sealed class OrExpression : IExpression
	{
		public OrExpression (IList<IExpression> conditions)
		{
			conditions_ = conditions;
		}

		public string Evaluate (ExpressionEvaluationContext ctx)
		{
			return (conditions_.Select ((c) => c.Evaluate (ctx)).Any ((c) => c == "1")) ? "1" : "0";
		}

		private IList<IExpression> conditions_;
	}

	public sealed class AndExpression : IExpression
	{
		public AndExpression (IList<IExpression> conditions)
		{
			conditions_ = conditions;
		}

		public string Evaluate (ExpressionEvaluationContext ctx)
		{
			return (conditions_.Select ((c) => c.Evaluate (ctx)).All ((c) => c == "1")) ? "1" : "0";
		}

		private IList<IExpression> conditions_;
	}

	public sealed class BooleanOutputExpression : IExpression
	{
		public BooleanOutputExpression (IExpression b, IExpression value)
		{
			query_ = b;
			value_ = value;
		}

		public string Evaluate (ExpressionEvaluationContext ctx)
		{
			if (query_.Evaluate (ctx) == "0") {
				return string.Empty;
			} else {
				return value_.Evaluate (ctx);
			}
		}

		private IExpression query_;
		private IExpression value_;
	}

	public sealed class ConfigExpression : IExpression
	{
		public ConfigExpression (IExpression query)
		{
			query_ = query;
		}

		public string Evaluate (ExpressionEvaluationContext ctx)
		{
			if (query_.Evaluate (ctx) == ctx.Configuration){
				return "1";
			} else {
				return "0";
			}
		}

		private IExpression query_;
	}

	public sealed class InvalidExpressionException : Exception
	{

	}

	public sealed class ExpressionParser
	{
		private static void Consume (ref IEnumerable<char> i, char c)
		{
			if (i.FirstOrDefault () != c) {
				throw new InvalidExpressionException ();
			} else {
				i = i.Skip (1);
			}
		}

		private IExpression ParseExpression (ref IEnumerable<char> input)
		{
			if (input.First () == '$') {
				input = input.Skip (1);

				if (input.First () == '<') {
					// $<, find out what kind of expression we're parsing
					input = input.Skip (1);

					if (input.First () == '$') {
						// Nested expression
						var query = ParseExpression (ref input);

						// Once we return, we must be at a :, because this
						// expression can be only of the form
						// $<A:B>, where A is something evaluating to 0 or 1,
						// and B can be an arbitrary nested expression
						Consume (ref input, ':');
						var value = ParseExpression (ref input);

						Consume (ref input, '>');
						return new BooleanOutputExpression (query, value);
					} else {
						// We're parsing $<ABC: or $<ABC>
						// Note that $<ABC$< ... is not valid -- left hand side
						// must not be a generator expression

						var variable = input.TakeWhile ((c) => c != ':' && c != '>')
							.Aggregate (new StringBuilder(), (sb, c) => sb.Append (c), (sb) => sb.ToString ());
						input = input.Skip (variable.Length);

						IExpression expression = null;
						switch (variable) {
							case "0":
							case "1": {
									Consume (ref input, ':');
									var value = ParseExpression (ref input);
									expression = new BooleanOutputExpression (new LiteralExpression (variable),
										value);
									break;
								}

							case "OR": {
									Consume (ref input, ':');						
									expression = new OrExpression (ParseExpressionList (ref input));

									break;
								}

							case "AND": {
									Consume (ref input, ':');
									expression = new AndExpression (ParseExpressionList (ref input));

									break;
								}
								
							case "CONFIGURATION": // Deprecated in CMake
								{
									// Warn
									goto case "CONFIG";
								}
							case "CONFIG": {
									Consume (ref input, ':');
									expression = new ConfigExpression (ParseExpression (ref input));

									break;
								}


							case "COMMA":
							{
								expression = new LiteralExpression (",");
								break;
							}

							case "SEMICOLON": {
									expression = new LiteralExpression (";");
									break;
							}

							case "ANGLE-R": // CMake
							case "LEFT-ANGLE": {
									expression = new LiteralExpression ("<");
									break;
								}

							case "ANGLE-L": // CMake
							case "RIGHT-ANGLE": {
									expression = new LiteralExpression (">");
									break;
								}
						}

						Consume (ref input, '>');
						return expression;
					}
				} else {
					return ParseLiteralOrChained (ref input);
				}
			} else {
				return ParseLiteralOrChained (ref input);
			}
		}

		private List<IExpression> ParseExpressionList (ref IEnumerable<char> input)
		{
			var expressions = new List<IExpression> ();
			while (input.First () != '>') {
				expressions.Add (ParseExpression (ref input));

				if (input.First () == ',') {
					input = input.Skip (1);
				} else if (input.First () == '>') {
					break;
				} else {
					throw new InvalidExpressionException ();
				}
			}

			return expressions;
		}

		private IExpression ParseLiteralOrChained (ref IEnumerable<char> input)
		{
			IExpression expression;
			var literal = input.TakeWhile ((c) => c != '$' && c != '>' && c != ',')
				.Aggregate (new StringBuilder (), (sb, c) => sb.Append (c), (sb) => sb.ToString ());
			expression = new LiteralExpression (literal);

			input = input.Skip (literal.Length);

			// Need to abort if this expression ends or we're running into a contiuation
			while (input.Any () && input.First () != '>' && input.First () != ',') {
				expression = new ChainedExpression (expression, ParseExpression (ref input));
			}

			return expression;
		}

		public IExpression Parse (IEnumerable<char> s)
		{
			if (s.Any ()) { 
				return ParseExpression (ref s);
			} else {
				return new LiteralExpression (String.Empty);
			}
		}
	}
}
