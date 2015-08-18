package TVA.Hadoop.MapReduce.Datamining.SAX;


import java.util.LinkedList;
import java.util.PriorityQueue;

//import TVA.Hadoop.MapReduce.Datamining.SAX.UnderflowException;
import TVA.Hadoop.MapReduce.Historian.File.StandardPointFile;

/**
 * Sliding window for reducer buckets
 * 
 * @author Josh Patterson
 * @version 0.1.0
 */
public class BucketSlidingWindow {

	

	
	//LinkedList<StandardPointFile> oDebugWindow;
	
	LinkedList<StandardPointFile> oCurrentWindow; // = new LinkedList<Integer>();

	PriorityQueue<StandardPointFile> oPointHeapNew; // = new PriorityQueue<StandardPointFile>();
	
	long _lWindowSize;
	long _lSlideIncrement;
	//long _lPointsRemaining;
	//long _lDebugCount;
	
	public BucketSlidingWindow( long WindowSizeInMS, long SlideIncrement ) {
	
		this._lWindowSize = WindowSizeInMS;
		this._lSlideIncrement = SlideIncrement;
		this.oCurrentWindow = new LinkedList<StandardPointFile>();
		
		this.oPointHeapNew = new PriorityQueue<StandardPointFile>();
		
	}
	
	public void AddPoint( StandardPointFile point ) {

		this.oPointHeapNew.add(point);
		
	}
	
	public int GetHeapPointCount() {
		
		return this.oPointHeapNew.size();
		
	}
	
	private void Init() throws Exception {
		
		System.out.println( "Init > Current Window Size: " + this.oCurrentWindow.size() + ", Point Heap Size: " + this.oPointHeapNew.size() );
		
		// prime the first window queue with n ms of data
		if ( this.oCurrentWindow.size() < 2 && this.oPointHeapNew.size() > 2 ) {
			
			// get at least a front and back point to work with
			this.oCurrentWindow.add( this.oPointHeapNew.remove() );
			this.oCurrentWindow.add( this.oPointHeapNew.remove() );
			
		} else {
			throw new Exception( "Not enough heap data to intialize into window!" );
		}

		System.out.println( "Init > Window Delta Test: " +  this.oCurrentWindow.getFirst().CalcDeltaInMS( this.oCurrentWindow.getLast() ) + " < " + this._lWindowSize );
/*
		System.out.println( "Init > Adding 10 points to Window" );
		
		for ( int x = 0; x < 10; x++ ) {
			
			this.oCurrentWindow.add( this.oPointHeapNew.remove() );
			
		}

		*/
		
		
		
		while ( this.oCurrentWindow.getFirst().CalcDeltaInMS( this.oCurrentWindow.getLast() ) < this._lWindowSize && this.oPointHeapNew.size() > 0 ) {
			
			this.oCurrentWindow.add( this.oPointHeapNew.remove() );
	
		}				

		System.out.println( "Init > Window Delta Test: " +  this.oCurrentWindow.getFirst().CalcDeltaInMS( this.oCurrentWindow.getLast() ) + " < " + this._lWindowSize );		
		
		
	}
	
	public void SlideWindowForward() throws Exception {
		
		long lCurrentFrontTS = 0;
		long lNewFrontPointTSMin = 0;
		
		// if there is no data in the 

		if ( this.oCurrentWindow.size() < 1) {
			
			// init the window
			System.out.println( "SlideWindowForward > Initializing the sliding window" );
			this.Init();
			
		}
		
		lCurrentFrontTS = this.oCurrentWindow.getFirst().GetCalendar().getTimeInMillis();
		lNewFrontPointTSMin = lCurrentFrontTS + this._lSlideIncrement;
		
	//	System.out.println( "SlideWindowForward > lCurrentFrontTS: " + lCurrentFrontTS + ", lNewFrontPointTSMin: " + lNewFrontPointTSMin + ", current front: " + this.oCurrentWindow.getFirst().GetCalendar().getTimeInMillis() );
		
		while ( this.oCurrentWindow.getFirst().GetCalendar().getTimeInMillis() < lNewFrontPointTSMin ) {
			
			//this.oCurrentWindow.add( this.oPointHeapNew.remove() );
			this.oCurrentWindow.removeFirst();
	
		}				
		
	}

	public LinkedList<StandardPointFile> GetCurrentWindow() throws Exception {
	
		if ( this.oCurrentWindow.size() < 2 && this.oPointHeapNew.size() > 2 ) {
			
			System.out.println( "GetCurrentWindow > pre init (shouldnt hit this)" );
			
			// get at least a front and back point to work with
			this.oCurrentWindow.add( this.oPointHeapNew.remove() );
			this.oCurrentWindow.add( this.oPointHeapNew.remove() );
						
			
		} else if ( this.oCurrentWindow.size() < 2 && this.oPointHeapNew.size() < 2 ) {
			
			// nothing left?
			return null;
			
		}
		
	//	System.out.println( "GetCurrentWindow > generating current window" );
		
		while ( this.oCurrentWindow.getFirst().CalcDeltaInMS( this.oCurrentWindow.getLast() ) < this._lWindowSize && this.oPointHeapNew.size() > 0 ) {
			
			if ( this.oPointHeapNew.size() < 1) {
				throw new Exception( "Window points exhausted" );
			}
			
			this.oCurrentWindow.add( this.oPointHeapNew.remove() );
	
		}		
		
	//	System.out.println( "GetCurrentWindow > window length: " + this.oCurrentWindow.getFirst().CalcDeltaInMS( this.oCurrentWindow.getLast() ) );
		
		
		return this.oCurrentWindow;
		
	}

	
	
	public boolean hasNext() {

		if ( this.oPointHeapNew.size() > 2 ) {
			return true;
		}
	
		return false;
		
	}
	


	
	
	

	public static void main(String[] args) {
		

		
	}	
		
	
	
	
		
	
	
}
