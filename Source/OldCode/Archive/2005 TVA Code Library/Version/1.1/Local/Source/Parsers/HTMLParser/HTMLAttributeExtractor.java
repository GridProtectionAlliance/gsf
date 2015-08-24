// James Ritchie Carroll - 2004
package TVA.Parsers.HTMLParser;

import java.util.*;
import System.Web.*;
import System.ComponentModel.*;
import TVA.Parsers.HTMLParser.*;
import TVA.Parsers.HTMLParser.HTMLDocument.*;
import TVA.Parsers.HTMLParser.HTMLDocument.Attribute;
import TVA.Parsers.HTMLParser.HTMLDocument.AttributeList;

public class HTMLAttributeExtractor extends HTMLVisitor
{
	// We expose this .NET based collection class instead of the Java "vector" based AttributeList so the
	// consumer can use this class without having to reference J# library and deal with the vector class
	/** @attribute DefaultProperty("Item") */
	public class MetaTagAttributes implements System.Collections.IEnumerable
	{
		protected System.Collections.ArrayList attrList;

		public MetaTagAttributes()
		{
			attrList = new System.Collections.ArrayList();
		}

		public void Add(Attribute metaTag)
		{
			attrList.Add(metaTag);
		}

		public Attribute GetAttribute(String Name)
		{
			MetaTagAttributes attrs = new MetaTagAttributes();

			GetAttributes(attrs, Name, true);

			if (attrs.get_Count() > 0)
				return attrs.get_Item(0);
			else
				return null;
		}

		public MetaTagAttributes GetAttributes(String Name)
		{
			MetaTagAttributes attrs = new MetaTagAttributes();

			GetAttributes(attrs, Name, false);

			return attrs;
		}

		private void GetAttributes(MetaTagAttributes attrs, String Name, boolean stopAfterFirst)
		{
			Attribute attr;

			for (int x = 0; x < attrList.get_Count(); x++)
			{
				attr = get_Item(x);
				if (attr.name.equalsIgnoreCase(Name))
				{
					attrs.Add(attr);
					if (stopAfterFirst)
						break;
				}
			}
		}

		/** @property */
		public Attribute get_Item(int Index)
		{
			return (Attribute)attrList.get_Item(Index);
		}

		/** @property */
		public int get_Count()
		{
			return attrList.get_Count();
		}

		public System.Collections.IEnumerator GetEnumerator()
		{
			return attrList.GetEnumerator();
		}
	}

	public class MetaTag
	{
		private Tag sourceTag;
		private MetaTagAttributes attrList;

		public MetaTag(Tag sourceTag)
		{
			this.sourceTag = sourceTag;
			attrList = new MetaTagAttributes();
		}

		/** @property */
		public String get_Name()
		{
			return get_AttributeValue("Name");
		}

		/** @property */
		public String get_AttributeValue(String Name)
		{
			String strValue = "";
			Attribute attrName = attrList.GetAttribute(Name);

			if (attrName != null)
				strValue = attrName.value;

			return strValue;
		}

		/** @property */
		public Tag get_SourceTag()
		{
			return sourceTag;
		}

		/** @property */
		public MetaTagAttributes get_Attributes()
		{
			return attrList;
		}
	}

	/** @attribute DefaultProperty("Item") */
	public class MetaTags implements System.Collections.IEnumerable
	{
		private System.Collections.ArrayList tagList;

		public MetaTags()
		{
			tagList = new System.Collections.ArrayList();
		}

		public void Add(MetaTag metaTag)
		{
			tagList.Add(metaTag);
		}

		public MetaTag GetMetaTag(String Name)
		{
			MetaTags tags = new MetaTags();

			GetMetaTags(tags, Name, true);

			if (tags.get_Count() > 0)
				return tags.get_Item(0);
			else
				return null;
		}

		public MetaTags GetMetaTags(String Name)
		{
			MetaTags tags = new MetaTags();

			GetMetaTags(tags, Name, false);

			return tags;
		}

		private void GetMetaTags(MetaTags tags, String Name, boolean stopAfterFirst)
		{
			MetaTag tag;

			for (int x=0; x < tagList.get_Count(); x++)
			{
				tag = get_Item(x);
				if (tag != null)
				{
					if (tag.get_Name().equalsIgnoreCase(Name))
					{
						tags.Add(tag);
						if (stopAfterFirst)
							break;
					}
				}
			}
		}

		/** @property */
		public MetaTag get_Item(int Index)
		{
			return (MetaTag)tagList.get_Item(Index);
		}

		/** @property */
		public int get_Count()
		{
			return tagList.get_Count();
		}

		public System.Collections.IEnumerator GetEnumerator()
		{
			return tagList.GetEnumerator();
		}
	}

	private MetaTags tagList;
	private String pageTitle;

	public HTMLAttributeExtractor()
	{
		tagList = new MetaTags();
	}

	// Once collected, you get only one visit from the root tag block (hopefully HTML)
	public void visit(TagBlock tb)
	{
		// We are not going to parse attribute information in pages that don't have an HTML tag block
		if (tb.startTag.tagName.equalsIgnoreCase("HTML"))
		{
			HTMLElement he;
			TagBlock tbe;

			// Let's go looking for the head tag
			for (Enumeration be=tb.body.elements(); be.hasMoreElements(); )
			{
				he = (HTMLElement)be.nextElement();

				if (he.getClass() == TagBlock.class)
				{
					tbe = (TagBlock)he;

					if (tbe.startTag.tagName.equalsIgnoreCase("HEAD"))
					{
						// Found the head tag block, now let's look for the page title and any meta tags
						for (Enumeration hbe=tbe.body.elements(); hbe.hasMoreElements(); )
						{
							he = (HTMLElement)hbe.nextElement();

							if (he.getClass() == TagBlock.class)
							{
								tbe = (TagBlock)he;

								if (tbe.startTag.tagName.equalsIgnoreCase("TITLE"))
								{
									// Should only be one item in title body, hopefully it's a text element
									for (Enumeration e=tbe.body.elements(); e.hasMoreElements(); )
									{
										he = (HTMLElement)e.nextElement();

										if (he.getClass() == Text.class)
										{
											pageTitle = HttpUtility.HtmlDecode(((Text)he).text).Trim();
											break;
										}
									}
								}
							}
							else if(he.getClass() == Tag.class)
							{
								Tag tg = (Tag)he;

								// See if this is a META tag
								if (tg.tagName.equalsIgnoreCase("META"))
								{
									// Convert this tag and it's attributes into a more .NET
									// friendly collection
									MetaTag metaTag = new MetaTag(tg);

									// Copy attributes in Java vector into .NET ArrayList...
									for (Enumeration ae=tg.attributeList.attributes.elements(); ae.hasMoreElements(); )
										metaTag.get_Attributes().Add((Attribute)ae.nextElement());

									tagList.Add(metaTag);
								}
							}
						}

						// Once we've found the head element, we can leave...
						break;
					}					
				}
			}
		}
	}

	/** @property */
	public String get_PageTitle()
	{
		return pageTitle;
	}

	/** @property */
	public MetaTags get_MetaTags()
	{
		return tagList;
	}

	/** @property */
	public static HTMLAttributeExtractor get_Attributes(HTMLDocument htmlDoc)
	{
		HTMLAttributeExtractor extractor = new HTMLAttributeExtractor();

		htmlDoc.accept(new HTMLCollector());	// Group start and end tags into tag blocks...
		htmlDoc.accept(extractor);

		return extractor;
	}
}