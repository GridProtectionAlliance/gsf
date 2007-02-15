// James Ritchie Carroll - 2004
package TVA.Parsers.HTMLParser;

import System.Web.*;
import System.Collections.*;

public class HTMLTextExtractor extends HTMLVisitor
{
	private System.Collections.ArrayList ignoreTags;
	private System.Collections.ArrayList textList;
	private boolean inTagToIgnore;

	public HTMLTextExtractor()
	{
		textList = new System.Collections.ArrayList();
		ignoreTags = new System.Collections.ArrayList();
		inTagToIgnore = false;

		// By default, we ignore SCRIPT, STYLE and OPTION tags
		ignoreTags.Add("SCRIPT");
		ignoreTags.Add("STYLE");
		ignoreTags.Add("OPTION");
	}

	/** @property */
	public System.Collections.ArrayList get_TagsToIgnore()
	{
		return ignoreTags;
	}

	public String ExtractText(HTMLDocument htmlDoc)
	{
		StringBuffer rawText = new StringBuffer();
		
		htmlDoc.accept(this);

		// Extract all text into a single string
		for (int x = 0; x < textList.get_Count(); x++)
		{
			rawText.append((String)textList.get_Item(x));
			rawText.append(" ");
		}

		return rawText.toString();
	}

	public void visit(HTMLDocument.Tag t)
	{ 
		inTagToIgnore = false;
		
		// See if this a tag we should ignore...
		for (int x = 0; x < ignoreTags.get_Count(); x++)
		{
			if (t.tagName.equalsIgnoreCase((String)ignoreTags.get_Item(x)))
			{
				inTagToIgnore = true;
				break;
			}
		}
	}

	public void visit(HTMLDocument.EndTag t)
	{
		inTagToIgnore = false;
	}

	// We are only concerned about visits to text elements
	public void visit(HTMLDocument.Text t)
	{
		if (!inTagToIgnore)
		{
			String text = HttpUtility.HtmlDecode(t.text).Trim();

			if (text.get_Length() > 0)			
				textList.Add(text);
		}
	}

	/** @property */
	public static String get_RawText(HTMLDocument htmlDoc)
	{
		// This is just a shortcut method to keep user from having to instantiate this class
		// if they are happy with the default TagsToIgnore list...
		HTMLTextExtractor extractor = new HTMLTextExtractor();
		return extractor.ExtractText(htmlDoc);
	}

	/** @property */
	public static ArrayList get_DistinctWordList(HTMLDocument htmlDoc)
	{
		ArrayList wordList = new ArrayList();
		char separator[] = {' '};
		String words[] = get_RawText(htmlDoc).Split(separator);
		
		for (int x = 0; x < words.length; x++)
		{
			if (wordList.BinarySearch(words[x]) < 0)
			{
				wordList.Add(words[x]);
				wordList.Sort(CaseInsensitiveComparer.get_Default());
			}
		}

		return wordList;
	}
}