// James Ritchie Carroll - 2004
package TVA.Parsing.HTMLParser;

import java.util.*;
import TVA.Parsing.HTMLParser.*;
import TVA.Parsing.HTMLParser.HTMLDocument.Attribute;
import TVA.Parsing.HTMLParser.HTMLDocument.AttributeList;

public class HTMLAnchorExtractor extends HTMLVisitor
{
	private System.Collections.ArrayList anchorList;

	public HTMLAnchorExtractor()
	{
		anchorList = new System.Collections.ArrayList();
	}

	// We are only concerned about visits to tags
	public void visit(HTMLDocument.Tag t)
	{ 
		// Specifically anchor tags
		if (t.tagName.equalsIgnoreCase("A"))
		{
			// Loop through attributes until we find HREF
			for (Enumeration ae=t.attributeList.attributes.elements(); ae.hasMoreElements(); )
			{
				Attribute a = (Attribute) ae.nextElement();
				if (a.name.equalsIgnoreCase("HREF"))
				{
					String link = dePound(deQuote(a.value)).Trim();

					if (link.get_Length() > 0)
						anchorList.Add(link);

					break;
				}
			}
		}
	}

	private String deQuote(String s)
	{
		if (s.startsWith("\"") && s.endsWith("\""))
			return s.substring(1, s.length() - 1);
		else if (s.startsWith("'") && s.endsWith("'"))
			return s.substring(1, s.length() - 1);
		else
			return s;
	}

	private String dePound(String s)
	{
		int pi = s.IndexOf('#');

		if (pi > -1)
		{
			if (pi > 0)
				return s.Substring(0, pi);
			else
				return "";
		}	
		else
			return s;
	}

	/** @property */
	public static System.Collections.ArrayList get_AnchorList(HTMLDocument htmlDoc)
	{
		HTMLAnchorExtractor extractor = new HTMLAnchorExtractor();

		htmlDoc.accept(extractor);

		return extractor.anchorList;
	}
}