/*
 * HTMLDocument.java -- classes to represent HTML documents as parse trees
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
 * Represents an HTML document as a sequence of elements.  The defined
 * element types are: Tag, EndTag, TagBlock (matched tag..end tag, with the
 * intervening elements), Comment, Text, Newline, and Annotation.
 *
 * <P> The various element types are defined as nested classes within
 * HTMLDocument.  Many have fields defined as public instead of properties
 * for efficient access.
 *
 * @author Brian Goetz, Quiotix
 * @see com.quiotix.html.HTMLVisitor
 */

public class HTMLDocument {
  ElementSequence elements;

  public Object UserData;

  public HTMLDocument(ElementSequence s) { elements = s; }

  public void accept(HTMLVisitor v) { v.visit(this); }

  // The various elements of the HTMLDocument (Tag, EndTag, etc) are included
  // as nested subclasses largely for reasons of namespace control.
  // The following subclasses of HTMLElement exist: Tag, EndTag, Text, Comment,
  // Newline, Annotation, TagBlock.  Also, the additional classes
  // ElementSequence, Attribute, and AttributeList are defined here as well.

  // Each subclass of HTMLElement should have a visit() method in the
  // HTMLVisitor class.

  /** Abstract class for HTML elements.  Enforces support for Visitors.  */
  public static abstract class HTMLElement {
    public abstract void accept(HTMLVisitor v);
  };

  /** HTML start tag.  Stores the tag name and a list of tag attributes.  */
  public static class Tag extends HTMLElement {
    public String tagName;
    public AttributeList attributeList;
    public boolean emptyTag = false;

    public Tag(String t, AttributeList a) { tagName = t; attributeList = a; }

    public void setEmpty(boolean b) { emptyTag = b; }

    public void accept(HTMLVisitor v) { v.visit(this); }

    /** @property */
	public int get_Length() {
      int length = 0;
      for (Enumeration ae=attributeList.attributes.elements();
           ae.hasMoreElements(); )
        length += 1 + ((Attribute) ae.nextElement()).get_Length();
      return length + tagName.length() + 2 + (emptyTag? 1 : 0);
    }

    public String toString() {
      StringBuffer s = new StringBuffer();
      s.append("<");
      s.append(tagName);
      for (Enumeration ae=attributeList.attributes.elements();
           ae.hasMoreElements(); ) {
        s.append(" ");
        s.append(((Attribute) ae.nextElement()).toString());
      }
      if (emptyTag) s.append("/");
      s.append(">");
      return s.toString();
    }
  }

  /** HTML end tag.  Stores only the tag name. */
  public static class EndTag extends HTMLElement {
    public String tagName;

    public EndTag(String t) { tagName = t; }
    public void accept(HTMLVisitor v) { v.visit(this); }
    /** @property */
	public int get_Length() { return 3 + tagName.length(); }
    public String toString() {
      return "</" + tagName + ">";
    }
  }

  /** A tag block is a composite structure consisting of a start tag
   * a sequence of HTML elements, and a matching end tag.  */
  public static class TagBlock extends HTMLElement {
    public Tag startTag;
    public EndTag endTag;
    public ElementSequence body;

    public TagBlock(String name, AttributeList aList, ElementSequence b) {
      startTag = new Tag(name, aList);
      endTag   = new EndTag(name);
      body = b;
    }
    public void accept(HTMLVisitor v) { v.visit(this); }
  }

  /** HTML comments. */
  public static class Comment extends HTMLElement {
    public String comment;

    public Comment(String c) { comment = c; }
    public void accept(HTMLVisitor v) { v.visit(this); }
    /** @property */
	public int get_Length() { return 3 + comment.length(); }
    public String toString() { return "<!" + comment + ">"; }
  }

  /** Plain text */
  public static class Text extends HTMLElement {
    public String text;

    public Text(String t) { text = t; }
    public void accept(HTMLVisitor v) { v.visit(this); }
    /** @property */
	public int get_Length() { return text.length(); }
    public String toString() { return text; }
  }

  /** End of line indicator. */
  public static class Newline extends HTMLElement {
    public static final String NL = System.Environment.get_NewLine();
    public void accept(HTMLVisitor v) { v.visit(this); }
    /** @property */
	public int get_Length() { return NL.length(); }
    public String toString() { return NL; }
  }

  /** A sequence of HTML elements.  */
  public static class ElementSequence {
    protected Vector elements;

    public ElementSequence(int n) { elements = new Vector(n); }
    public ElementSequence()      { elements = new Vector(); }

    public void addElement(HTMLElement o) { elements.addElement(o); }
    public Enumeration elements() { return elements.elements(); }
  }

  /** Annotations.  These are not part of the HTML document, but
   * provide a way for HTML-processing applications to insert
   * annotations into the document.  These annotations can be used by
   * other programs or can be brought to the user's attention at a
   * later time.  For example, the HTMLCollector might insert an
   * annotation to indicate that there is no corresponding start tag
   * for an end tag.  */
  public static class Annotation extends HTMLElement {
    String type, text;

    public Annotation(String type, String text) {
      this.type = type;
      this.text = text;
    }

    public void accept(HTMLVisitor v) { v.visit(this); }
    /** @property */
	public int get_Length() { return 14 + type.length() + text.length(); }
    public String toString() { return "<!--NOTE(" + type + ") " + text + "-->"; }
  }

  public static class Attribute {
    public String name, value;
    public boolean hasValue;

    public Attribute(String n) { name = n; hasValue = false; }
    public Attribute(String n, String v) {
      name = n;
      value = v;
      hasValue = true;
    }

    /** @property */
	public int get_Length() {
      return (hasValue? name.length() + 1 + value.length() : name.length());
    }
    public String toString() {
      return (hasValue? name+"="+value : name);
    }
  }

  public static class AttributeList {
    public Vector attributes;

    public AttributeList() {
      attributes = new Vector();
    }
    public void addAttribute(Attribute a) { attributes.addElement(a); }
  }
}



