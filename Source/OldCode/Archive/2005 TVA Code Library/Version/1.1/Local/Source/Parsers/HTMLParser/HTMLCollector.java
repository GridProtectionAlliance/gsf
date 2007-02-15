/*
 * HTMLCollector.java -- structures an HTML document tree.
 * Copyright (C) 1999 Quiotix Corporation.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License, version 2, as
 * published by the Free Software Foundation.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License (http://www.gnu.org/copyleft/gpl.txt)
 * for more details.
 */

package TVA.Parsers.HTMLParser;

import java.util.*;
import java.io.*;

/** An HTMLVisitor which modifies the structure of the document so that
 * begin tags are matched properly with end tags and placed in TagBlock
 * elements.  Typically, an HTMLDocument is created by the parser, which
 * simply returns a flat list of elements.  The HTMLCollector takes this
 * flat list and gives it the structure that is implied by the HTML content.
 *
 * @author Brian Goetz, Quiotix
 */

public class HTMLCollector extends HTMLVisitor {

  protected MyVector tagStack = new MyVector();
  protected MyVector elements;
  protected boolean collected;
  protected static Hashtable dontMatch = new Hashtable();
  protected static String[] dontMatchStrings
    = { "AREA", "BASE", "BASEFONT", "BR", "COL", "HR", "IMG", "INPUT",
        "ISINDEX", "LINK", "META", "P", "PARAM" };
  static {
    Integer dummy = new Integer(0);
    for (int i=0; i < dontMatchStrings.length; i++)
      dontMatch.put(dontMatchStrings[i], dummy);
  };

  private static class TagStackEntry {
    String tagName;
    int index;
  };

  private static class MyVector extends Vector {
    MyVector()      { super();  }
    MyVector(int n) { super(n); }
    public void popN(int n) { elementCount -= n; }
  };

  protected int pushNode(HTMLDocument.HTMLElement e) {
    elements.addElement(e);
    return elements.size()-1;
  };

  public void visit(HTMLDocument.Comment c)     { pushNode(c); };
  public void visit(HTMLDocument.Text t)        { pushNode(t); };
  public void visit(HTMLDocument.Newline n)     { pushNode(n); };

  public void visit(HTMLDocument.Tag t)         {
    TagStackEntry ts = new TagStackEntry();
    int index;

    // Push the tag onto the element stack, and push an entry on the tag
    // stack if it's a tag we care about matching
    index = pushNode(t);
    if (!t.emptyTag
        && !dontMatch.containsKey(t.tagName.toUpperCase())) {
      ts.tagName = t.tagName;
      ts.index = index;
      tagStack.addElement(ts);
    };
  };

  public void visit(HTMLDocument.EndTag t)      {
    int i;
    for (i=tagStack.size()-1; i >= 0; i--) {
      TagStackEntry ts = (TagStackEntry) tagStack.elementAt(i);
      if (t.tagName.equalsIgnoreCase(ts.tagName)) {
        HTMLDocument.TagBlock block;
        HTMLDocument.ElementSequence blockElements;
        HTMLDocument.Tag tag;

        // Create a new ElementSequence and copy the elements to it
        blockElements =
          new HTMLDocument.ElementSequence(elements.size() - ts.index - 1);
        for (int j=ts.index+1; j<elements.size(); j++)
          blockElements.addElement((HTMLDocument.HTMLElement)
                                   elements.elementAt(j));
        tag = (HTMLDocument.Tag) elements.elementAt(ts.index);
        block = new HTMLDocument.TagBlock(tag.tagName,
                                          tag.attributeList, blockElements);

        // Pop the elements off the stack, push the new block
        elements.popN(elements.size() - ts.index);
        elements.addElement(block);

        // Pop the matched tag and intervening unmatched tags
        tagStack.popN(tagStack.size()-i);

        collected = true;
        break;
      };
    };

    // If we didn't find a match, just push the end tag
    if (i < 0)
      pushNode(t);
  };

  public void visit(HTMLDocument.TagBlock bl) {
    HTMLCollector c = new HTMLCollector();

    c.start();
    c.visit(bl.body);
    c.finish();
    pushNode(bl);
  }

  public void visit(HTMLDocument.ElementSequence s) {
    elements = new MyVector(s.elements.size());
    collected = false;

    for (Enumeration e = s.elements.elements();
         e.hasMoreElements(); )
      ((HTMLDocument.HTMLElement)e.nextElement()).accept(this);
    if (collected)
      s.elements = elements;
  }

  public void start() {}
  public void finish() {}


/*
  public static void main (String[] args) throws Exception {
    InputStream r = new FileInputStream(args[0]);
    HTMLDocument document;

    try {
      document = new HTMLParser(r).HTMLDocument();
      document.accept(new HTMLScrubber());
      document.accept(new HTMLCollector());
      document.accept(new HTMLDumper(System.out));
    }
    finally {
      r.close();
    };
  };
*/

	// .NET HTML parsing hooks
	public static HTMLDocument CreateDocTreeFromFile(String FileName) throws Exception
	{
		HTMLDocument doc = HTMLParser.ParseFile(FileName);

		doc.accept(new HTMLScrubber());
		doc.accept(new HTMLCollector());

		return doc;
	}
	
	public static HTMLDocument CreateDocTreeFromSource(String HTMLSource) throws Exception
	{
		HTMLDocument doc = HTMLParser.ParseSource(HTMLSource);

		doc.accept(new HTMLScrubber());
		doc.accept(new HTMLCollector());

		return doc;
	}
}