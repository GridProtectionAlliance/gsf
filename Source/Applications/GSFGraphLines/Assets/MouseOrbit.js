var target : Transform;
var distance = 10.0;
var xSpeed = 300.0;
var ySpeed = 300.0;
var zoomRate:int = 50;
var mouseDownFrames:int = 10;
var minX = -9999.0;

private var x = 0.0;
private var y = 0.0;
private var buttonDown:boolean;
private var rot:boolean;
private var downCount:int = 0;

function Start ()
{
    var angles = transform.eulerAngles;
    x = angles.y;
    y = angles.x;
}

function Update ()
{
	if (target)
	{	
    	// Handle pinch gesture
        if (Input.touchCount >= 2)
        {
            var touch0 = Input.GetTouch(0);
            var touch1 = Input.GetTouch(1);

            var curDist = touch0.position - touch1.position;
            var prevDist = (touch0.position - touch0.deltaPosition) - (touch1.position - touch1.deltaPosition);
  
            var delta = (curDist.magnitude - prevDist.magnitude) / (zoomRate / 5.0);
            distance -= delta;
        }
        else        
        {
        	// Mouse button functions also work for touch input
			if (Input.GetMouseButtonDown(0))
			{
				buttonDown = true;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				buttonDown = false;
			}
			
			// Only start rotation after a few frames - this allows human
			// mutitouch interaction a moment to enage since it's rare that
			// two fingers will actually hit the screen at the exact same time
			if (buttonDown)
			{
				downCount++;
				rot = (downCount >= mouseDownFrames);
			}
			else
			{
				downCount = 0;
				rot = false;
			}
			
			if (rot)
			{
				if (Input.mousePosition.x > minX)
				{
					x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
		        	y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
				}
	     	}
	     	
	       	distance += -Input.GetAxis("Mouse ScrollWheel")* Time.deltaTime * zoomRate * Mathf.Abs(distance);
        }
       		       
        var rotation = Quaternion.Euler(y, x, 0);
        var position = rotation * Vector3(0.0, 0.0, -distance) + target.position;
        
        transform.rotation = rotation;
        transform.position = position;                 
    }	
}