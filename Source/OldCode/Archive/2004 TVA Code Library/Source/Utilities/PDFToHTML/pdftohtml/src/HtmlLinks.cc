#include "HtmlLinks.h"

HtmlLink::HtmlLink(const HtmlLink& x){
  Xmin=x.Xmin;
  Ymin=x.Ymin;
  Xmax=x.Xmax;
  Ymax=x.Ymax;
  dest=new GString(x.dest);
}

HtmlLink::HtmlLink(double xmin,double ymin,double xmax,double ymax,GString * _dest)
{
   if (xmin < xmax) {
    Xmin=xmin;
    Xmax=xmax;
  } else {
    Xmin=xmax;
    Xmax=xmin;
  }
  if (ymin < ymax) {
    Ymin=ymin;
    Ymax=ymax;
  } else {
    Ymin=ymax;
    Ymax=ymin;
  }                    
  dest=new GString(_dest);
}

HtmlLink::~HtmlLink(){
 if (dest) delete dest;
}

GBool HtmlLink::isEqualDest(const HtmlLink& x) const{
  return (!strcmp(dest->getCString(), x.dest->getCString()));
}

GBool HtmlLink::inLink(double xmin,double ymin,double xmax,double ymax) const {
  double y=(ymin+ymax)/2;
  if (y>Ymax) return gFalse;
  return (y>Ymin)&&(xmin<Xmax)&&(xmax>Xmin);
 }
  

HtmlLink& HtmlLink::operator=(const HtmlLink& x){
  if (this==&x) return *this;
  if (dest) {delete dest;dest=NULL;} 
  Xmin=x.Xmin;
  Ymin=x.Ymin;
  Xmax=x.Xmax;
  Ymax=x.Ymax;
  dest=new GString(x.dest);
  return *this;
} 

GString* HtmlLink::getLinkStart() {
  GString *res = new GString("<A href=\"");
  res->append(dest);
  res->append("\">");
  return res;
}

/*GString* HtmlLink::Link(GString* content){
  //GString* _dest=new GString(dest);
  GString *tmp=new GString("<a href=\"");
  tmp->append(dest);
  tmp->append("\">");
  tmp->append(content);
  tmp->append("</a>");
  //delete _dest;
  return tmp;
  }*/

   

HtmlLinks::HtmlLinks(){
  accu=new GVector<HtmlLink>();
}

HtmlLinks::~HtmlLinks(){
  delete accu;
  accu=NULL; 
}

GBool HtmlLinks::inLink(double xmin,double ymin,double xmax,double ymax,int& p)const {
  
  for(GVector<HtmlLink>::iterator i=accu->begin();i!=accu->end();i++){
    if (i->inLink(xmin,ymin,xmax,ymax)) {
        p=(i - accu->begin());
        return 1;
    }
   }
  return 0;
}

HtmlLink* HtmlLinks::getLink(int i) const{
  GVector<HtmlLink>::iterator g=accu->begin();
  g+=i; 
  return g;
}

