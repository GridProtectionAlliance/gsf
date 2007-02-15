/*
 * HTMLDumper.java -- Dumps an HTML document tree.
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

/**
 * Simple HTMLVisitor which dumps out the document to the specified
 * output stream.
 *
 * @author Brian Goetz, Quiotix
 */

public class HTMLDebugDumper extends HTMLVisitor {
  protected PrintWriter out;

  public HTMLDebugDumper(OutputStream os) { out = new PrintWriter(os); }

  public void finish()                    { out.flush();               }

  public void visit(HTMLDocument.Tag t)        {
    out.print("Tag(" + t + ")");
  }
  public void visit(HTMLDocument.EndTag t)     {
    out.print("Tag(" + t + ")");
  }
  public void visit(HTMLDocument.Comment c)    {
    out.print("Comment(" + c + ")");
  }
  public void visit(HTMLDocument.Text t)       {
    out.print(t);
  }
  public void visit(HTMLDocument.Newline n)    {
    out.println("-NL-");
  }
  public void visit(HTMLDocument.Annotation a) {
    out.print(a);
  }
  public void visit(HTMLDocument.TagBlock bl)  {
    out.print("<BLOCK>");
    visit(bl.startTag);
    visit(bl.body);
    visit(bl.endTag);
    out.print("</BLOCK>");
  }

  public static void main(String[] args) throws ParseException {
    HTMLParser parser = new HTMLParser(System.in);
    HTMLDocument doc = parser.HTMLDocument();
    doc.accept(new HTMLDebugDumper(System.out));
  }
}






