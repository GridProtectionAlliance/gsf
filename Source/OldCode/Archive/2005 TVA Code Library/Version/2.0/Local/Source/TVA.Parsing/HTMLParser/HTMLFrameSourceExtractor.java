// James Ritchie Carroll - 2004
package TVA.Parsing.HTMLParser;

import java.util.*;
import TVA.Parsing.HTMLParser.*;
import TVA.Parsing.HTMLParser.HTMLDocument.Attribute;
import TVA.Parsing.HTMLParser.HTMLDocument.AttributeList;

public class HTMLFrameSourceExtractor extends HTMLVisitor
{
	private System.Collections.ArrayList srcList;

	public HTMLFrameSourceExtractor()
	{
		srcList = new System.Collections.ArrayList();
	}

	// We are only concerned about visits to tags
	public void visit(HTMLDocument.Tag t)
	{
		// Specifically frame or iframe tags
		if (t.tagName.equalsIgnoreCase("FRAME") || t.tagName.equalsIgnoreCase("IFRAME"))
		{
			// Loop through attributes until we find SRC
			for (Enumeration ae=t.attributeList.attributes.elements(); ae.hasMoreElements(); )
			{
				Attribute a = (Attribute) ae.nextElement();
				if (a.name.equalsIgnoreCase("SRC"))
				{
					srcList.Add(deQuote(a.value));
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

	/** @property */
	public static System.Collections.ArrayList get_FrameSourceList(HTMLDocument htmlDoc)
	{
		HTMLFrameSourceExtractor extractor = new HTMLFrameSourceExtractor();

		htmlDoc.accept(extractor);

		return extractor.srcList;
	}
}