package TVA.Hadoop.MapReduce.Datamining.SAX;

import edu.hawaii.jmotif.logic.sax.SAXFactory;
import weka.core.EuclideanDistance;
import weka.core.Instance;
import weka.core.Instances;
import weka.core.TechnicalInformationHandler;
import weka.core.neighboursearch.PerformanceStats;

/**
 * Euclidean distance function for use with SAX and WEKA's ballTree
 * 
 * @author Josh Patterson
 * @version 0.1.0
 */
public class SAXEuclideanDistance extends EuclideanDistance implements Cloneable, TechnicalInformationHandler {

	Instances oInstances;
	
	public SAXEuclideanDistance( Instances oI ) {
		
		super( oI );
		this.oInstances = oI;
		
	}


	public double distance(Instance first, Instance second) {
		
		//System.out.println( ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> custom distance function 1" );
		  
		//  first.setDataset(oInstances);
		  
		  //	System.out.println( "sax1: " + first.stringValue(0) + " - " + second.stringValue(0) );
		  	
		  //	System.out.println( "sax2: " + second.stringValue(0) ); 
		   
		//return SAXFactory.strDistance( first.stringValue(0).toCharArray(), second.stringValue(0).toCharArray() );
		
		return Math.sqrt(distance(first, second, Double.POSITIVE_INFINITY));
		
	}
	
	 public double distance(Instance first, Instance second, PerformanceStats stats) { //debug method pls remove after use
	//	    return Math.sqrt(distance(first, second, Double.POSITIVE_INFINITY, stats));
		 System.out.println( ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> custom distance function 2" );
		 return 0;
	 }	
	 
	  public double distance(Instance first, Instance second, double cutOffValue) {
		  
		 // System.out.println( ">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>> custom distance function 3" );
		  
		  first.setDataset(oInstances);
		  
		  //	System.out.println( "sax1: " + first.stringValue(0) + " - " + second.stringValue(0) );
		  	
		  //	System.out.println( "sax2: " + second.stringValue(0) ); 
		   
		//	return SAXFactory.strDistance( first.stringValue(0).toCharArray(), second.stringValue(0).toCharArray() );

		  
		return distance(first, second, cutOffValue, null);
	  }	 
	 
	  
	  
	  
	  public double distance(Instance first, Instance second, double cutOffValue, PerformanceStats stats) {
		    double distance = 0;
		    int firstI, secondI;
		    int firstNumValues = first.numValues();
		    int secondNumValues = second.numValues();
		    int numAttributes = m_Data.numAttributes();
		    int classIndex = m_Data.classIndex();
		    
		    first.setDataset(oInstances);
		    
		    validate();
		    
		    boolean bSkip = false;
		    
		    //System.out.println( "Distance > Len > " + len );
      
		    if ( first == null ) {
		    	bSkip = true;
		    	System.out.println( "Distance > first was null!" );
		    }
		    
		    if ( first.stringValue(0) == null ) {
		    	bSkip = true;
		    	System.out.println( "Distance > first.stringValue(0) was null! ----- firstNumValues = " + firstNumValues + ", numAttributes: " + numAttributes + ", classIndex: " + classIndex + ", m_Data: " + m_Data.numInstances() );
		    }
		    
		    if ( second == null || second.stringValue(0) == null ) {
		    	bSkip = true;
		    	System.out.println( "Distance > second was null!" );
		    }
	
		    int len = 0;
		    try {
		    	
		    	len = first.stringValue(0).toCharArray().length;
		    } catch (Exception e) {
		    	bSkip = true;
		    	System.out.println( e );
		    }
		    
		    double diff = 0; //SAXFactory.strDistance( first.stringValue(0).toCharArray(), second.stringValue(0).toCharArray() );
		    if (!bSkip) {
		    	try {
		    		diff = SAXFactory.strDistance( first.stringValue(0).toCharArray(), second.stringValue(0).toCharArray() );
		    	} catch (Exception e) {
		    		System.out.println( "SAX Dist Function FAIL ----------------------" );
		    	}
		    	
		    } 
		    
		    
		    
		      distance = updateDistance(distance, diff);
		      if (distance > cutOffValue)
		        return Double.POSITIVE_INFINITY;

		    return distance;
		  }	  
	  
	  
	  
	  
	  
	/**
	 * @param args
	 */
	public static void main(String[] args) {
		// TODO Auto-generated method stub
		
	//	SAXEuclideanDistance d = new SAXEuclideanDistance();
	//	System.out.println( d.globalInfo() );

	}

}
