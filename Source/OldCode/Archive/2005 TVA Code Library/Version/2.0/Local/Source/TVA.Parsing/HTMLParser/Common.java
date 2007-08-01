package TVA.Parsing.HTMLParser;

import System.Net.*;
import System.IO.*;

/**
 * Common methods for parsing HTML text to plain text.
 */
public class Common
{
	private Common()
	{
		// This class contains only global functions and is not meant to be instantiated
	}

	/**
	 * Extracts plain text from HTML source text.
	 */
	public static String ExtractTextFromSource(String htmlSource) throws Exception
	{
		return new HTMLTextExtractor().ExtractText(HTMLParser.ParseSource(htmlSource));
	}

	/**
	 * Extracts plain text from an HTML source stream.
	 */
	public static String ExtractTextFromStream(Stream htmlSource) throws Exception
	{
		return ExtractTextFromSource(new StreamReader(htmlSource).ReadToEnd());
	}

	/**
	 * Extracts plain text from an HTML file located locally.
	 */
	public static String ExtractTextFromFile(String htmlFile) throws Exception
	{
		return new HTMLTextExtractor().ExtractText(HTMLParser.ParseFile(htmlFile));
	}

	/**
	 * Extracts plain text from an HTML file located on the web.
	 */
	public static String ExtractTextFromWeb(String htmlFile) throws Exception
	{
		return ExtractTextFromStream(new WebClient().OpenRead(htmlFile));
	}

}
