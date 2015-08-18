<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html>
	<head>
		<title>Sparky Web Service</title>
	</head>
	<body>
		<div style="font-family: Tahoma; font-size: 14px; width: 80%;">
			<p>
				<h3>Search Interface</h3>
				<p>
					- point_id: &lt;ID&gt;<br />
					Returns the REST URI and approximations of number of blocks, points, and size on disk.  If no time range is specified, an approximation of a time span is also returned.
				</p>
				<p>
					- time_range: &lt;start time&gt; - &lt;end time&gt;<br />
					Returns the REST URI and approximations of number of blocks, points, and size on disk.  This parameter must be used in conjunction with the point_id parameter.
				</p>
				<p>
					- debug_index_bucket: &lt;ID&gt;<br />
					Returns debug information including point type ID, bucket base timestamp, pointer count, total pointers, duplicate pointers, and bad pointers.  If this parameter is included, all other input is ignored.
				</p>
			</p>
			<br />
			<p>
				<h3>REST Time-series Web Service</h3>
				<p>
					- /historian/timeseriesdata/read/current/&lt;one or more ID delimited by comma&gt;/[xml|json]<br />
					Returns the latest time-series data for the measurement IDs specified in the comma-delimited list.
				</p>
				<p>
					- /historian/timeseriesdata/read/current/&lt;starting ID in the range>-&lt;ending ID in the range&gt;/[xml|json]<br />
					Returns the latest time-series data for the measurement IDs specified in the range.
				<p>
					- /historian/timeseriesdata/read/historic/&lt;one or more ID delimited by comma&gt;/&lt;start time&gt;/&lt;end time&gt;/[xml|json]<br />
					Returns historic time series data for the measurement IDs specified in the comma-delimited list for the specified GMT time span. The time can be absolute time (Example: 09-21-09 23:00:01 for Sep 21, 09 11:00:01 pm) or relative to the current time (Example: * for now or *-1m for 1 minute ago where s = seconds, m = minutes, h = hours and d = days).
				<p>
					- /historian/timeseriesdata/read/historic/&lt;starting ID in the range&gt;-&lt;ending ID in the range&gt;/&lt;start time&gt;/&lt;end time&gt;/[xml|json]<br />
					Returns historic time series data for the measurement IDs specified in the range for the specified GMT time span (time format is same as above).
				</p>
			</p>
		</div>
	</body>
</html>