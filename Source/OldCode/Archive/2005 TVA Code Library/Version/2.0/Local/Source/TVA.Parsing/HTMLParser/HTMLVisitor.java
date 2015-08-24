/*
 * HTMLVisitor.java
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

import java.util.*;

/**
 * Abstract class implementing Visitor pattern for HTMLDocument objects.
 *
 * @author Brian Goetz, Quiotix
 */

public abstract class HTMLVisitor {
  public void visit(HTMLDocument.Tag t)         { }
  public void visit(HTMLDocument.EndTag t)      { }
  public void visit(HTMLDocument.Comment c)     { }
  public void visit(HTMLDocument.Text t)        { }
  public void visit(HTMLDocument.Newline n)     { }
  public void visit(HTMLDocument.Annotation a)  { }
  public void visit(HTMLDocument.TagBlock bl) {
    bl.startTag.accept(this);
    visit(bl.body);
    bl.endTag.accept(this);
  }

  public void visit(HTMLDocument.ElementSequence s) {
    for (Enumeration e = s.elements();
         e.hasMoreElements(); )
      ((HTMLDocument.HTMLElement)e.nextElement()).accept(this);
  }

  public void visit(HTMLDocument d) {
    start();
    visit(d.elements);
    finish();
  }

  public void start()  { }
  public void finish() { }
}

