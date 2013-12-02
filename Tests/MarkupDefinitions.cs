﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Moq;
using Mut.Data;
using Xunit;
using erecruit.Utils;
using Xunit.Extensions;

namespace Mut.Tests
{
	public class MarkupDefinitions
	{
		[Fact]
		public void Bold_italic_underline() {
			t( "123 **abc** xyz", "123 <b>abc</b> xyz" );
			t( "123 *abc* xyz", "123 <i>abc</i> xyz" );
			t( "123 _abc_ xyz", "123 <u>abc</u> xyz" );
		}

		[Fact]
		public void List() {
			t( @"abc
						* one
						* two
						* three
					xyz",
					@"abc<ul>
<li> one</li>
<li> two</li>
<li> three</li></ul>
					xyz");
		}

		[Fact]
		public void List_starting_at_first_line() {
			t( @" * one
						* two
						* three
					xyz",
					@"<ul><li> one</li>
<li> two</li>
<li> three</li></ul>
					xyz" );
		}

		[Fact]
		public void List_starting_at_last_line() {
			t( @" abc
	* one
						* two
						* three",
					@" abc<ul>
<li> one</li>
<li> two</li>
<li> three</li></ul>" );
		}

		[Fact]
		public void Html() {
			t( "[html]<b>ddd</b>[/html]", "<b>ddd</b>" );
			t( "**bold** [html]<b>ddd</b>[/html]", "<b>bold</b> <b>ddd</b>" );
			t( "**bold** [html]abc <b>ddd</b>[/html] xyz", "<b>bold</b> abc <b>ddd</b> xyz" );
			t( "**bold** [html]<b>ddd</b> abc[/html]", "<b>bold</b> <b>ddd</b> abc" );
		}

		[Theory]
		[InlineData( "-", "h2" )]
		[InlineData( "--", "h1" )]
		public void Headings( string kind, string tag ) {
			t( string.Format( "{0}abc{0}", kind ), string.Format( "<{0}>abc</{0}>", tag ) );
			t( string.Format( 
				@"{0}abc{0}
				fff", kind ), 
				string.Format( 
				@"<{0}>abc</{0}>
				fff", tag ) );
			t( string.Format( 
				@"xyz
				{0}abc{0}
				fff", kind ),
				string.Format( 
				@"xyz
<{0}>abc</{0}>
				fff", tag ) );
			t( string.Format( "one{0}abc{0}", kind ), string.Format( "one{0}abc{0}", kind ) );
			t( string.Format( "one{0}abc", kind ), string.Format( "one{0}abc", kind ) );
			t( string.Format( "xyz one{0}abc{0} xyz{0}", kind ), string.Format( "xyz one{0}abc{0} xyz{0}", kind ) );
			t( string.Format( "{0}one{0}abc", kind ), string.Format( "{0}one{0}abc", kind ) );
		}

		[Fact]
		public void HtmlEntityCodes() {
			t( "&amp;", "&amp;" );
			t( "abc &amp; abc", "abc &amp; abc" );
			t( "&amp", "&amp;amp" );
		}

		[Fact]
		public void HtmlEntityCodes_with_Html() {
			t( "[html]&amp;[/html]", "&amp;" );
		}

		void t( string source, string result = null ) {
			var res = new Mut.MarkupParser<Mut.UI.MarkupParseArgs>().Parse( source, null,
				new Mut.UI.MarkupUI {
					Video = new UI.VideoEmbedUI(),
					Attachments = new AttachmentUI()
				}.Defs );
			var actual = string.Join( "", res.Select( r => r.Instance.ToHtml() ) );
			Assert.Equal( result, actual );
		}
	}
}