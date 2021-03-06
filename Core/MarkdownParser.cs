﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using erecruit.Composition;
using erecruit.Utils;

namespace Mut
{
	public interface IMarkupNodeInstance {
		string ToHtml();
	}

	public class MarkdownNodeDefinition<TContext>
	{
		public Regex Start { get; set; }
		public Regex End { get; set; }
		public Func<TContext, Match, Match, IEnumerable<MarkupNode<TContext>>, IMarkupNodeInstance> CreateInstance { get; set; }

		public MarkdownNodeDefinition( string startRegex, string endRegex,
			Func<TContext, Match, Match, IEnumerable<MarkupNode<TContext>>, IMarkupNodeInstance> createInstance ) {
			Contract.Requires( startRegex != null );
			Contract.Requires( createInstance != null );
			this.Start = new Regex( startRegex, RegexOptions.Compiled );
			this.End = endRegex.NullOrEmpty() ? null : new Regex( endRegex, RegexOptions.Compiled );
			this.CreateInstance = createInstance;
		}

		public MarkdownNodeDefinition( string startRegex,
			Func<TContext, Match, Match, IEnumerable<MarkupNode<TContext>>, IMarkupNodeInstance> createInstance )
			: this( startRegex, null, createInstance ) { }

		public MarkdownNodeDefinition() {}
	}

	public class TextNode : IMarkupNodeInstance
	{
		readonly string _text;
		public TextNode( string text, bool encode = true ) { 
			_text = encode ? HttpUtility.HtmlEncode( text ?? "" ) : ( text ?? "" ); 
		}
		public string ToHtml() { return _text; }
	}

	public class SequenceNode : IMarkupNodeInstance
	{
		readonly IEnumerable<MarkupNode> _children;
		public SequenceNode( IEnumerable<MarkupNode> children ) {
			Contract.Requires( children != null );
			_children = children;
		}

		public virtual string ToHtml() {
			return string.Join( "", _children.Select( c => c.Instance.ToHtml() ) );
		}
	}

	public class WrapNode : SequenceNode
	{
		readonly string _htmlTag;

		public WrapNode( string htmlTag, IEnumerable<MarkupNode> inners )
			: base( inners ) {
			Contract.Requires( !String.IsNullOrEmpty( htmlTag ) );
			Contract.Requires( inners != null );
			_htmlTag = htmlTag;
		}

		public override string ToHtml() {
			return "<" + _htmlTag + ">" + base.ToHtml() + "</" + _htmlTag + ">";
		}
	}
	public class MarkupNode
	{
		public IMarkupNodeInstance Instance { get; set; }
		public string SourceString { get; set; }
		public int SourceStartIndex { get; set; }
		public int SourceEndIndex { get; set; }
	}

	public class MarkupNode<TContext> : MarkupNode {
		public MarkdownNodeDefinition<TContext> Def { get; set; }
		public IEnumerable<MarkupNode<TContext>> Children { get; set; }
	}

	[Export]
	public class MarkdownParser<TContext>
	{
		public MarkdownNodeDefinition<TContext> SimpleTag( string tagName, string htmlTag = null ) {
			var escapedTagName = Regex.Escape( tagName );
			return new MarkdownNodeDefinition<TContext>( "\\[" + escapedTagName + "\\]", "\\[\\/" + escapedTagName + "\\]",
				(ctx, _, __, inners) => new WrapNode( htmlTag ?? tagName, inners ) );
		}

		/// <summary>
		/// Produces a markup definition that matches a piece of text wrapped in the
		/// 'wrappingSequence' on both ends and translates that into the same text
		/// wrapped in 'htmlTag'.
		/// </summary>
		/// <remarks>
		/// For example:
		///			**bold**    ==		&lt;b&gt;bold&lt;/b&gt;
		/// </remarks>
		public MarkdownNodeDefinition<TContext> Wrap( string wrappingSequence, string htmlTag ) {
			var regex = new Regex( Regex.Escape( wrappingSequence ) );
			return new MarkdownNodeDefinition<TContext> {
				Start = regex, End = regex,
				CreateInstance = ( ctx, a, b, c ) => new WrapNode( htmlTag, c )
			};
		}

		static string attributeRegex( string attName, string closing = "\\]" ) {
			return Regex.Escape( attName ) + "\\=(?'att_" + attName + "'([^\\s" + closing + "]+)|(\\\"[^\\\"" + closing + "]+\\\"))";
		}

		public MarkdownNodeDefinition<TContext> ComplexTag( 
			string tagName, bool hasClosing, string[] attributes, 
			Func<TContext, IDictionary<string, string>, Range<string>> generateOpenAndCloseHtml ) {
				return ComplexTag( tagName, hasClosing, attributes, ( ctx, atrs, inners ) => {
					var html = generateOpenAndCloseHtml( ctx, atrs );
					return html.Start + string.Join( "", inners.Select( i => i.Instance.ToHtml() ) ) + html.End;
				} );
		}

		public MarkdownNodeDefinition<TContext> ComplexTag( 
			string tagName, bool hasClosing, string[] attributes, 
			Func<TContext, IDictionary<string, string>, IEnumerable<MarkupNode<TContext>>, string> generateHtml ) {

			var defaultAttribute = attributes.Any( s => s.NullOrEmpty() );
			tagName = Regex.Escape( tagName );

			return new MarkdownNodeDefinition<TContext> {

				Start = new Regex(
					"\\[" + tagName +
					(defaultAttribute ? "(" + attributeRegex( "" ) + "){0,1}" : "") +
					"(\\s+(" + string.Join( "|", attributes.Select( a => attributeRegex( a ) ) ) + "))*" +
					"\\s*\\]" ),

				End = hasClosing ? new Regex( "\\[\\/" + tagName + "\\]" ) : null,

				CreateInstance = ( ctx, start, _, inners ) => {
					var attrs = (from a in attributes
											 let g = start.Groups["att_" + a]
											 where g != null && g.Success
											 select new { a, g.Value }
											).ToDictionary( x => x.a, x => x.Value.Trim( '\"' ) );
					return new TextNode( generateHtml( ctx, attrs, inners ), false );
				}
			};
		}

		public MarkdownNodeDefinition<TContext> HtmlTag( string tag, params string[] allowedAttributes ) {
			return HtmlTag( tag, ii => ii, allowedAttributes );
		}

		public MarkdownNodeDefinition<TContext> HtmlTag( string tag, 
			Func<IEnumerable<MarkupNode<TContext>>, IEnumerable<MarkupNode<TContext>>> transformInners,
			params string[] allowedAttributes ) {
			return new MarkdownNodeDefinition<TContext>( 
				@"\<" + tag + 
				@"(\s+(" + string.Join( "|", allowedAttributes.Select( a => attributeRegex( a, ">" ) ) ) + "))*" +
				@"\s*\>",
				@"\<\/" + tag + @"\>",
				(ctx,start,end,inners) => new TextNode( 
					"<" + tag + " " + string.Join( " ",
					from a in allowedAttributes
					let g = start.Groups["att_" + a]
					where g != null && g.Success
					select a + "=" + g.Value ) +
					">" + string.Join( "", transformInners(inners).Select( i => i.Instance.ToHtml() ) ) +
					"</" + tag + ">",

					encode: false
				)
			);
		}

		class ParsingNode
		{
			public MarkdownNodeDefinition<TContext> Def { get; set; }
			public Match Start { get; set; }
			public int StartIndex { get; set; }
			public Match End { get; set; }
			public int EndIndex { get; set; }
			public List<ParsingNode> Children { get; private set; }

			public ParsingNode() {
				Children = new List<ParsingNode>();
			}
		}

		public IEnumerable<MarkupNode<TContext>> Parse( 
			string input, TContext context, IEnumerable<MarkdownNodeDefinition<TContext>> defs ) {

			if ( !(defs is IList<MarkdownNodeDefinition<TContext>>) ) defs = defs.ToList();

			var matches = 
				defs.SelectMany( d => 
					d.Start.Matches( input ).Cast<Match>()
					.Select( m => new { d, end = (Match)null, start = m } ) ).Concat(
				defs.SelectMany( d => 
					(d.End == null ? null : d.End.Matches( input ).Cast<Match>()).EmptyIfNull()
					.Select( m => new { d, end = m, start = (Match)null } ) ) )
				.OrderBy( x => (x.start ?? x.end).Index )
				.ThenBy( x => x.end == null );

			var topNode = new ParsingNode();
			var stack = new List<ParsingNode> { topNode };
			var currentIndex = 0;
			foreach ( var x in matches ) {
				var nextIndex = (x.start ?? x.end).Index;
				if ( nextIndex < currentIndex ) continue;

				if ( x.start != null ) {
					InsertTextNode( input, stack, currentIndex, nextIndex );
					(x.d.End == null ? stack.Last().Children : stack)
						.Add( new ParsingNode { Start = x.start, StartIndex = x.start.Index, EndIndex = x.start.Index + x.start.Length, Def = x.d } );
					currentIndex = x.start.Index + x.start.Length;
				}
				else if ( x.end != null ) {
					var correspondingStartIndex = Enumerable.Range( 0, stack.Count-1 )
						.Select( i => stack.Count - i - 1 )
						.FirstOrDefault( i => stack[i].Def == x.d );
					if ( correspondingStartIndex <= 0 ) continue;

					InsertTextNode( input, stack, currentIndex, nextIndex );

					stack[correspondingStartIndex].End = x.end;
					stack[correspondingStartIndex].EndIndex = x.end.Index + x.end.Length - 1;
					for ( var i = stack.Count - 1; i >= correspondingStartIndex; i-- ) {
						stack[i - 1].Children.Add( stack[i] );
						stack[i - 1].EndIndex = stack[correspondingStartIndex].EndIndex;
					}
					stack.RemoveRange( correspondingStartIndex, stack.Count - correspondingStartIndex );

					currentIndex = x.end.Index + x.end.Length;
				}
			}

			if ( currentIndex < input.Length ) InsertTextNode( input, stack, currentIndex, input.Length );
			for ( var i = stack.Count - 1; i > 0; i-- ) {
				stack[i - 1].Children.Add( stack[i] );
				stack[i - 1].EndIndex = input.Length;
			}

			Func<ParsingNode, MarkupNode<TContext>> map = null;
			map = n => {
				var inners = n.Children.Select( map ).ToList();
				return new MarkupNode<TContext> { 
					Def = n.Def, 
					Instance = n.Def.CreateInstance( context, n.Start, n.End, inners ), 
					Children = inners,
					SourceString = input,
					SourceStartIndex = n.StartIndex,
					SourceEndIndex = n.EndIndex
				};
			};

			return topNode.Children.Select( map ).ToList();
		}

		private static void InsertTextNode( string input, List<ParsingNode> stack, int currentIndex, int nextIndex ) {
			if ( nextIndex <= currentIndex ) return;
			var text = input.Substring( currentIndex, nextIndex - currentIndex );
			stack.Last().Children.Add( new ParsingNode {
				StartIndex = currentIndex,
				EndIndex = nextIndex,
				Def = new MarkdownNodeDefinition<TContext> { CreateInstance = ( ctx, a, b, c ) => new TextNode( text ) },
			} );
		}
	}
}