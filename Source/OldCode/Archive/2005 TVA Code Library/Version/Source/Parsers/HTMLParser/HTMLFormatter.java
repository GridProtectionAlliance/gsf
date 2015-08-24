/*
 * HTMLFormatter.java -- HTML document pretty-printer
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

import java.io.*;
import java.util.*;

/**
 * HTMLFormatter is a Visitor which traverses an HTMLDocument, dumping the
 * contents of the document to a specified output stream.  It assumes that
 * the documents has been preprocessed by HTMLCollector (which matches up
 * beginning and end tags) and by HTMLScrubber (which formats tags in a
 * consistent way).  In particular, HTMLScrubber should be invoked with the
 * TRIM_SPACES option to remove trailing spaces, which can confuse the
 * formatting algorithm.
 *
 * <P>The right margin and indent increment can be specified as properties.
 *
 * @author Brian Goetz, Quiotix
 * @see com.quiotix.html.HTMLVisitor
 * @see com.quiotix.html.HTMLCollector
 * @see com.quiotix.html.HTMLScrubber
 */

public class HTMLFormatter extends HTMLVisitor {
  protected MarginWriter out;
  protected int rightMargin=80;
  protected int indentSize=2;
  protected static Hashtable tagsIndentBlock = new Hashtable();
  protected static Hashtable tagsNewlineBefore = new Hashtable();
  protected static Hashtable tagsPreformatted = new Hashtable();
  protected static Hashtable tagsTryMatch = new Hashtable();
  protected static final String[] tagsIndentStrings
    = { "TABLE", "TR", "TD", "TH", "FORM", "HTML", "HEAD", "BODY", "SELECT" };
  protected static final String[] tagsNewlineBeforeStrings
    = { "P", "H1", "H2", "H3", "H4", "H5", "H6", "BR" };
  protected static final String[] tagsPreformattedStrings
    = { "PRE", "SCRIPT", "STYLE" };
  protected static final String[] tagsTryMatchStrings
    = { "A", "TD", "TH", "TR", "I", "B", "EM", "FONT", "TT", "UL" };
  static {
    Integer dummy = new Integer(0);
    for (int i=0; i < tagsIndentStrings.length; i++)
      tagsIndentBlock.put(tagsIndentStrings[i], dummy);
    for (int i=0; i < tagsNewlineBeforeStrings.length; i++)
      tagsNewlineBefore.put(tagsNewlineBeforeStrings[i], dummy);
    for (int i=0; i < tagsPreformattedStrings.length; i++)
      tagsPreformatted.put(tagsPreformattedStrings[i], dummy);
    for (int i=0; i < tagsTryMatchStrings.length; i++)
      tagsTryMatch.put(tagsTryMatchStrings[i], dummy);
  };
  protected TagBlockRenderer blockRenderer = new TagBlockRenderer();
  protected HTMLDocument.HTMLElement previousElement;
  protected boolean inPreBlock;

  public HTMLFormatter(OutputStream os) throws Exception {
    out = new MarginWriter(new PrintWriter(new BufferedOutputStream(os)));
    out.setRightMargin(rightMargin);
  };

  public void setRightMargin(int margin) {
    rightMargin = margin;
    out.setRightMargin(rightMargin);
  };

  public void setIndent(int indent) {
    indentSize = indent;
  };

  public void visit(HTMLDocument.TagBlock block) {
    boolean indent;
    boolean preformat;
    boolean alreadyDone = false;
    int wasMargin=0;

    if (tagsTryMatch.containsKey(block.startTag.tagName.toUpperCase())) {
      blockRenderer.start();
      blockRenderer.setTargetWidth(out.getRightMargin() - out.getLeftMargin());
      blockRenderer.visit(block);
      blockRenderer.finish();
      if (!blockRenderer.hasBlownTarget()) {
        out.printAutoWrap(blockRenderer.getString());
        previousElement = block.endTag;
        return;
      };
    };

    // Only will get here if we've failed the try-block test
    indent = tagsIndentBlock.containsKey(block.startTag.tagName.toUpperCase());
    preformat = tagsPreformatted.containsKey(block.startTag.tagName.toUpperCase());
    if (preformat) {
      inPreBlock = true;
      visit(block.startTag);
      wasMargin = out.getLeftMargin();
      out.setLeftMargin(0);
      visit(block.body);
      out.setLeftMargin(wasMargin);
      visit(block.endTag);
    } else if (indent) {
      out.printlnSoft();
      visit(block.startTag);
      out.printlnSoft();
      out.setLeftMargin(out.getLeftMargin() + indentSize);
      visit(block.body);
      out.setLeftMargin(out.getLeftMargin() - indentSize);
      out.printlnSoft();
      visit(block.endTag);
      out.printlnSoft();
      inPreBlock = false;
    }
    else {
      visit(block.startTag);
      visit(block.body);
      visit(block.endTag);
    };
  }

  public void visit(HTMLDocument.Tag t) {
    String s = t.toString();
    int hanging;

    if (tagsNewlineBefore.containsKey(t.tagName.toUpperCase())
        || out.getCurPosition() + s.length() > out.getRightMargin())
      out.printlnSoft();

    out.print("<" + t.tagName);
    hanging = t.tagName.length() + 1;
    for (Enumeration ae=t.attributeList.attributes.elements();
         ae.hasMoreElements(); ) {
      HTMLDocument.Attribute a = (HTMLDocument.Attribute) ae.nextElement();
      out.printAutoWrap(" "+a.toString(), hanging);
    };
    if (t.emptyTag) out.print("/");
    out.print(">");
    previousElement = t;
  }

  public void visit(HTMLDocument.EndTag t) {
    out.printAutoWrap(t.toString());
    if (tagsNewlineBefore.containsKey(t.tagName.toUpperCase())) {
      out.printlnSoft();
      out.println();
    };
    previousElement = t;
  }

  public void visit(HTMLDocument.Comment c) {
    out.print(c.toString());
    previousElement = c;
  }

  public void visit(HTMLDocument.Text t) {
    if (inPreBlock)
      out.print(t.text);
    else {
      int start=0;
      while (start < t.text.length()) {
        int index = t.text.indexOf(' ', start) + 1;
        if (index == 0)
          index = t.text.length();
        out.printAutoWrap(t.text.substring(start, index));
        start = index;
      };
    };
    previousElement = t;
  }

  public void visit(HTMLDocument.Newline n) {
    if (inPreBlock)
      out.println();
    else if (previousElement instanceof HTMLDocument.Tag
             || previousElement instanceof HTMLDocument.EndTag
             || previousElement instanceof HTMLDocument.Comment
             || previousElement instanceof HTMLDocument.Newline)
      out.printlnSoft();
    else if (previousElement instanceof HTMLDocument.Text)
      out.print(" ");
    previousElement = n;
  }

  public void start() {
    previousElement = null;
    inPreBlock = false;
  };

  public void finish() {
    out.flush();
  };

/*
  public static void main (String[] args) throws Exception {
    InputStream r = new FileInputStream(args[0]);
    HTMLDocument document;

    try {
      document = new HTMLParser(r).HTMLDocument();
      document.accept(new HTMLCollector());
      document.accept(new HTMLScrubber(HTMLScrubber.DEFAULT_OPTIONS
                                       | HTMLScrubber.TRIM_SPACES));
      document.accept(new HTMLFormatter(System.out));
    }
    catch (Exception e) {
      e.printStackTrace();
    }
    finally {
      r.close();
    };
  };
*/

	// .NET HTML parsing hooks
	public static void FormatHTMLFromFile(String FileName, OutputStream os) throws Exception
	{
		HTMLDocument doc = HTMLParser.ParseFile(FileName);

		doc.accept(new HTMLCollector());
		doc.accept(new HTMLScrubber(HTMLScrubber.DEFAULT_OPTIONS | HTMLScrubber.TRIM_SPACES));
		doc.accept(new HTMLFormatter(os));
	}
	
	public static void FormatHTMLFromSource(String HTMLSource, OutputStream os) throws Exception
	{
		HTMLDocument doc = HTMLParser.ParseSource(HTMLSource);

		doc.accept(new HTMLCollector());
		doc.accept(new HTMLScrubber(HTMLScrubber.DEFAULT_OPTIONS | HTMLScrubber.TRIM_SPACES));
		doc.accept(new HTMLFormatter(os));
	}

}


/**
 * Utility class, used by HTMLFormatter, which adds some word-wrapping
 * and hanging indent functionality to a PrintWriter.
 */

class MarginWriter
{
  protected int tabStop;
  protected int curPosition;
  protected int leftMargin;
  protected int rightMargin;
  protected java.io.PrintWriter out;
  protected char[] spaces = new char[256];
  {
    for (int i=0; i<spaces.length; i++)
      spaces[i] =  ' ';
  };

  public MarginWriter(java.io.PrintWriter out) {
    this.out = out;
  }

  public void flush() {
    out.flush();
  };

  public void close() {
    out.close();
  };

  public void print(String s) {
    if (curPosition == 0 && leftMargin > 0) {
      out.write(spaces, 0, leftMargin);
      curPosition = leftMargin;
    };
    out.print(s);
    curPosition += s.length();
  }

  public void printAutoWrap(String s) {
    if (curPosition > leftMargin
        && curPosition + s.length() > rightMargin)
      println();
    print(s);
  };

  public void printAutoWrap(String s, int hanging) {
    if (curPosition > leftMargin
        && curPosition + s.length() > rightMargin) {
      println();
      out.write(spaces, 0, hanging+leftMargin);
      curPosition = leftMargin + hanging;
    };
    print(s);
  };

  public void println() {
    curPosition = 0;
    out.println();
  };

  public void printlnSoft() {
    if (curPosition > 0)
      println();
  };

  public void setLeftMargin(int leftMargin) {
    this.leftMargin = leftMargin;
  };
  public int getLeftMargin() {
    return leftMargin;
  };

  public void setRightMargin(int rightMargin) {
    this.rightMargin = rightMargin;
  };
  public int getRightMargin() {
    return rightMargin;
  };

  public int getCurPosition() {
    return (curPosition == 0 ? leftMargin : curPosition);
  };
}

/**
 * Utility class, used by HTMLFormatter, which tentatively tries to format
 * the contents of an HTMLDocument.TagBlock to see if the entire block can
 * fit on the rest of the line.  If it cannot, it gives up and indicates
 * failure through the hasBlownTarget method; if it can, the contents can
 * be retrieved through the getString method.
 */

class TagBlockRenderer extends HTMLVisitor {
  protected String s;
  protected boolean multiLine;
  protected boolean blownTarget;
  protected int targetWidth=80;

  public void start() {
    s = "";
    multiLine = false;
    blownTarget = false;
  }
  public void finish() { };

  public void setTargetWidth(int w) { targetWidth = w; }

  public String getString()         { return s; }
  public boolean isMultiLine()      { return multiLine; }
  public boolean hasBlownTarget()   { return blownTarget; }

  public void visit(HTMLDocument.Tag t)     {
    if (s.length() < targetWidth)
      s += t.toString();
    else
      blownTarget = true;
  }

  public void visit(HTMLDocument.EndTag t) {
    if (s.length() < targetWidth)
      s += t.toString();
    else
      blownTarget = true;
  }

  public void visit(HTMLDocument.Comment c) {
    if (s.length() < targetWidth)
      s += c.toString();
    else
      blownTarget = true;
  }

  public void visit(HTMLDocument.Text t)    {
    if (s.length() < targetWidth)
      s += t.toString();
    else
      blownTarget = true;
  }

  public void visit(HTMLDocument.Newline n) {
    multiLine = true;
    s += " ";
  }
}


