#ifndef _HTML_LINKS
#define _HTML_LINKS

#include "GVector.h"
#include "GString.h"

class HtmlLink{

private:  
  double Xmin;
  double Ymin;
  double Xmax;
  double Ymax;
  GString* dest;

public:
  HtmlLink(){dest=NULL;}
  HtmlLink(const HtmlLink& x);
  HtmlLink& operator=(const HtmlLink& x);
  HtmlLink(double xmin,double ymin,double xmax,double ymax,GString *_dest);
  ~HtmlLink();
  GBool HtmlLink::isEqualDest(const HtmlLink& x) const;
  GString *getDest(){return new GString(dest);}
  double getX1() const {return Xmin;}
  double getX2() const {return Xmax;}
  double getY1() const {return Ymin;}
  double getY2() const {return Ymax;}
  GBool inLink(double xmin,double ymin,double xmax,double ymax) const ;
  //GString *Link(GString *content);
  GString* getLinkStart();
  
};

class HtmlLinks{
private:
 GVector<HtmlLink> *accu;
public:
 HtmlLinks();
 ~HtmlLinks();
 void AddLink(const HtmlLink& x) {accu->push_back(x);}
 GBool inLink(double xmin,double ymin,double xmax,double ymax,int& p) const;
 HtmlLink* getLink(int i) const;

};

#endif
   
