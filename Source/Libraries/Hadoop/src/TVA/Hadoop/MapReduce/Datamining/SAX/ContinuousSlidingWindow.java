package TVA.Hadoop.MapReduce.Datamining.SAX;


import java.util.LinkedList;
import TVA.Hadoop.MapReduce.Historian.File.StandardPointFile;

public class ContinuousSlidingWindow {

		
		LinkedList<StandardPointFile> oCurrentWindow; // = new LinkedList<Integer>();
		
		long _lWindowSize;
		long _lSlideIncrement;
		long _lCurrentTime;
		
		public ContinuousSlidingWindow( long WindowSizeInMS, long SlideIncrement ) {
		
			this._lWindowSize = WindowSizeInMS;
			this._lSlideIncrement = SlideIncrement;
			this._lCurrentTime = 0;
			this.oCurrentWindow = new LinkedList<StandardPointFile>();
			
		}
		
		public boolean WindowIsFull() {
			
			if ( this.GetWindowDelta() >= this._lWindowSize ) {
				return true;
			}
			
			return false;
			
		}
		
		public long GetWindowDelta() {
			
			if ( this.oCurrentWindow.size() > 0 ) {
				return this.oCurrentWindow.getFirst().CalcDeltaInMS( this.oCurrentWindow.getLast() );
			}
			
			return 0;
			
		}
		
		public void AddPoint( StandardPointFile point ) {

			this.oCurrentWindow.add( point );
			
		}
		
		public int GetNumberPointsInWindow() {
			
			return this.oCurrentWindow.size();
			
		}

		/**
		 * Slide the window forward
		 * - burn off the first half of the window
		 * - still must re-add more points from the Reduce iterator
		 * @throws Exception
		 */
		public void SlideWindowForward() {
			
			long lCurrentFrontTS = this.oCurrentWindow.getFirst().GetCalendar().getTimeInMillis();
			this._lCurrentTime = lCurrentFrontTS + this._lSlideIncrement;
			
			// now burn off the tail
			
			while ( this.oCurrentWindow.getFirst().GetCalendar().getTimeInMillis() < this._lCurrentTime ) {

				this.oCurrentWindow.removeFirst();
		
			}				
			
		}

		public LinkedList<StandardPointFile> GetCurrentWindow() {
			
			return this.oCurrentWindow;
			
		}

	

	

}
