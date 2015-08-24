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

package TVA.Parsing.HTMLParser;

import java.io.*;

/**
 * Simple HTMLVisitor which dumps out the document to the specified
 * output stream.
 *
 * @author Brian Goetz, Quiotix
 */

public class HTMLDumper extends HTMLVisitor {
  protected PrintWriter out;

  public HTMLDumper(OutputStream os)     { out = new PrintWriter(os); }

  public HTMLDumper(OutputStream os, String encoding)
    throws UnsupportedEncodingException {
    out = new PrintWriter( new OutputStreamWriter(os, encoding) );
  }

  public void finish()                   { out.flush();               }

  public void visit(HTMLDocument.Tag t)        { out.print(t);   }
  public void visit(HTMLDocument.EndTag t)     { out.print(t);   }
  public void visit(HTMLDocument.Comment c)    { out.print(c);   }
  public void visit(HTMLDocument.Text t)       { out.print(t);   }
  public void visit(HTMLDocument.Newline n)    { out.println();  }
  public void visit(HTMLDocument.Annotation a) { out.print(a);   }
}

