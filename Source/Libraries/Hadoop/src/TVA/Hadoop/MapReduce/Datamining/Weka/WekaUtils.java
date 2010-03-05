package TVA.Hadoop.MapReduce.Datamining.Weka;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.LinkedList;

import org.apache.hadoop.conf.Configuration;
import org.apache.hadoop.fs.FSDataInputStream;
import org.apache.hadoop.fs.FileSystem;
import org.apache.hadoop.fs.Path;

import TVA.Hadoop.MapReduce.Datamining.SAX.SAXEuclideanDistance;
import TVA.Hadoop.MapReduce.Datamining.SAX.SAXInstance;
import TVA.Hadoop.MapReduce.Historian.File.StandardPointFile;

import edu.hawaii.jmotif.lib.ts.TSException;
import edu.hawaii.jmotif.lib.ts.Timeseries;
import edu.hawaii.jmotif.logic.sax.SAXFactory;
import edu.hawaii.jmotif.logic.sax.alphabet.Alphabet;
import edu.hawaii.jmotif.logic.sax.alphabet.NormalAlphabet;


import weka.core.Attribute;
import weka.core.FastVector;
import weka.core.Instance;
import weka.core.Instances;
import weka.core.neighboursearch.BallTree;


/**
 * Weka Interop utils to use Weka classes (Instance, BallTree, etc) with other code such as Hadoop
 * 
 * @author Josh Patterson
 * @version 0.1.0
 */
public class WekaUtils {


	public static SAXInstance GenerateInstanceFromPointWindow( LinkedList<StandardPointFile> oPoints, int iPaaSize, int iAlphabetSize ) {
		
		SAXInstance newTS = new SAXInstance();
		int iSamplePointCount = oPoints.size();
		Alphabet normalAlphabet = new NormalAlphabet();

		double[] values = new double[ iSamplePointCount ];
		long[] tstamps = new long[ iSamplePointCount ];
		int i = 0;

		for ( int x = 0; x < oPoints.size(); x++ ) {
		
			values[x] = Double.valueOf( oPoints.get(x).Value );
			tstamps[x] = oPoints.get(x).GetCalendar().getTimeInMillis();
			
		} // for
		
		try {
			
			newTS.oTimeSeries = new Timeseries( values, tstamps );
			newTS.SAX_Representation = SAXFactory.ts2string( newTS.oTimeSeries, iPaaSize, normalAlphabet, iAlphabetSize );
			
		} catch (TSException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			
			System.out.println( "\n\nWekaUtils > GenerateInstanceFromPointWindow > Bad Instance Data!!!!!!!" );
			
			return null;
			
		}
		

		return newTS;
	}		

	public static SAXInstance GenerateInstanceFromPointWindow( String csv_line, int iPaaSize, int iAlphabetSize ) {
		
		SAXInstance newTS = new SAXInstance();
		int iCSVLineLength = 21;
		Alphabet normalAlphabet = new NormalAlphabet();
		
		String[] ar_strValues = csv_line.split( "," );
		double[] values = new double[ iCSVLineLength ];
		long[] tstamps = new long[ iCSVLineLength ];
		int i = 0;
		
		for ( int x = 0; x < iCSVLineLength; x++ ) {
		
			values[x] = Double.valueOf( ar_strValues[ x ] );
			tstamps[x] = (long) x;
			
		} // for
		
		// class designation
			
		//newTS.line_index = i;
		newTS.ClassID = ar_strValues[ iCSVLineLength ];
		
		try {
			
			newTS.oTimeSeries = new Timeseries( values, tstamps );
			// dimensions == iPaaSize
			// cardinality == iAlphabetSize
			newTS.SAX_Representation = SAXFactory.ts2string( newTS.oTimeSeries, iPaaSize, normalAlphabet, iAlphabetSize );
			
		} catch (TSException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			
			System.out.println( "\n\nWekaUtils > GenerateInstanceFromPointWindow > Bad Instance Data!!!!!!!" );
			
			return null;
			
		}
		
		return newTS;
	}	
	
	
	
	
	public static Instance GenerateInstanceFromPointWindow( String csv_line, FastVector fvWekaAttributes, Instances oWekaTestInstances, StringBuffer sbInstanceClass ) {
		
		SAXInstance newTS = new SAXInstance();
		int iCSVLineLength = 21;
		Alphabet normalAlphabet = new NormalAlphabet();
		
		String[] ar_strValues = csv_line.split( "," );
		double[] values = new double[ iCSVLineLength ];
		long[] tstamps = new long[ iCSVLineLength ];
		int i = 0;
		
		for ( int x = 0; x < iCSVLineLength; x++ ) {
		
			values[x] = Double.valueOf( ar_strValues[ x ] );
			tstamps[x] = (long) x;
			
		} // for
		
		// class designation
			
		//newTS.line_index = i;
		newTS.ClassID = ar_strValues[ iCSVLineLength ];
		sbInstanceClass.delete( 0 , sbInstanceClass.length() );
		sbInstanceClass.append(newTS.ClassID);
		
		
		try {
			
			newTS.oTimeSeries = new Timeseries( values, tstamps );
			newTS.SAX_Representation = SAXFactory.ts2string( newTS.oTimeSeries, iCSVLineLength, normalAlphabet, 11 );
			
		} catch (TSException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			
			System.out.println( "\n\nWekaUtils > GenerateInstanceFromPointWindow > Bad Instance Data!!!!!!!" );
			
			return null;
			
		}
		
		
		
		
		Instance iExample = new Instance( 2 );
		
		iExample.setValue( (Attribute)fvWekaAttributes.elementAt(0), newTS.SAX_Representation );      
		iExample.setValue( (Attribute)fvWekaAttributes.elementAt(1), newTS.ClassID );
	
		iExample.setDataset( oWekaTestInstances );
		
		oWekaTestInstances.add( iExample );
	
	
		return iExample;		
	}
	
	public static ArrayList<SAXInstance> LoadTrainingInstancesCSV_FromHDFS( String strHDFSPath, int iDimensions, int iCardinality, int iSamplesPerRawInstance ) throws TSException, NumberFormatException, IOException {
		
		ArrayList<SAXInstance> arInstances = new ArrayList<SAXInstance>();
		
    	FileSystem hdfs = FileSystem.get(new Configuration());

    	Path path = new Path( strHDFSPath );

    	//reading 
    	FSDataInputStream dis = hdfs.open(path); 
    	BufferedReader oFileReader = new BufferedReader(new InputStreamReader(dis));
		  
		String line = null;

		double[] values = new double[ iSamplesPerRawInstance ];
		long[] tstamps = new long[ iSamplesPerRawInstance ];
		int i = 0;
		
		Alphabet normalAlphabet = new NormalAlphabet();
		    
		while ( (line = oFileReader.readLine()) != null ) {
			
			SAXInstance newTS = new SAXInstance();
			
			String[] ar_strValues = line.split( "," );
			
			for ( int x = 0; x < iSamplesPerRawInstance; x++ ) {
			
				values[x] = Double.valueOf( ar_strValues[ x ] );
				tstamps[x] = (long) x;
				
			} // for
			
			// class designation
				
			newTS.line_index = i;
			newTS.ClassID = ar_strValues[ iSamplesPerRawInstance ];
			newTS.oTimeSeries = new Timeseries( values, tstamps );
			newTS.SAX_Representation = SAXFactory.ts2string( newTS.oTimeSeries, iDimensions, normalAlphabet, iCardinality );

			arInstances.add( newTS );
			
		} // while		
		
		System.out.println( "BallTree > Training Instances Loaded: " + arInstances.size() );
		
		oFileReader.close();
    	//dis.close();

    	//hdfs.close(); 	
    	
    	return arInstances;
		
	}			
	

	
	
	
	
	public static BallTree ConstructBallTreeClassifier_ForHDFS( String strHDFS_TrainingCSV_Path, int iDimensions, int iCardinality, int iSamplesPerRawInstance  ) {
		
		BallTree bt = null;
		ArrayList<SAXInstance> arSAXTrainingInstances = null;
		
		try {
			arSAXTrainingInstances = LoadTrainingInstancesCSV_FromHDFS( strHDFS_TrainingCSV_Path, iDimensions, iCardinality, iSamplesPerRawInstance );
		} catch (NumberFormatException e1) {
			// TODO Auto-generated catch block
			e1.printStackTrace();
		} catch (TSException e1) {
			// TODO Auto-generated catch block
			e1.printStackTrace();
		} catch (IOException e1) {
			// TODO Auto-generated catch block
			e1.printStackTrace();
		}
		
		FastVector fvWekaSetup = Generate_MapReduce_Timeseries_WekaSetup();
		
		Instances oWekaTrainingInstances = new Instances( "Training", fvWekaSetup, 200 );
			oWekaTrainingInstances.setClassIndex( 1 );
		
		BuildWekaInstances( fvWekaSetup, arSAXTrainingInstances, oWekaTrainingInstances );
		
		// now build ball tree
		
		try {
			bt = BuildBallTree( oWekaTrainingInstances );
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		return bt;
		
	}
	
	
	
	
	public static BallTree BuildBallTree( Instances oTrainingInstances ) throws Exception {
		
		
		SAXEuclideanDistance d = new SAXEuclideanDistance( oTrainingInstances );
	
		BallTree bt = new BallTree();
		bt.setDistanceFunction( d );
		bt.setInstances( oTrainingInstances );
		
		return bt;
		
	}

	public static FastVector Generate_MapReduce_Timeseries_WekaSetup() {
		
		
		Attribute oAtt_TimeSeries = new Attribute( "sax_timeseries", (FastVector)null );
		 
		 // Declare the class attribute along with its values
		 FastVector fvClassVal = new FastVector( 3 );
		 	fvClassVal.addElement( "0" );
		 	fvClassVal.addElement( "1" );
		 	fvClassVal.addElement( "2" );
		 
		 Attribute ClassAttribute = new Attribute( "theClass", fvClassVal );
		
		 // Declare the feature vector
		 FastVector fvWekaAttributes = new FastVector( 2 );
		 	fvWekaAttributes.addElement( oAtt_TimeSeries );    
		 	fvWekaAttributes.addElement( ClassAttribute );		

		 	return fvWekaAttributes;
		
	}	
	
	public static void BuildWekaInstances( FastVector fvWekaAttributes, ArrayList<SAXInstance> arTrainingInstances, Instances oWekaTrainingInstances ) {
		
		for ( int x = 0; x < arTrainingInstances.size(); x++ ) {

			
			Instance inst = AddSAXToCollection( fvWekaAttributes, arTrainingInstances.get( x ), oWekaTrainingInstances );
			inst.setDataset( oWekaTrainingInstances );
			oWekaTrainingInstances.add(inst);	
			
		} // for				
		
	}
	
	
	public static Instance AddSAXToCollection( FastVector fvWekaAttributes, SAXInstance oSAX, Instances oInstances ) {
		
		Instance iExample = new Instance( 2 );
			iExample.setValue( (Attribute)fvWekaAttributes.elementAt(0), oSAX.SAX_Representation );      
			iExample.setValue( (Attribute)fvWekaAttributes.elementAt(1), oSAX.ClassID );
		
		return iExample;
		
	}	
	
}
