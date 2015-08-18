<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html>
	<head>
		<title>Sparky - The PMU-Data Search Engine</title>
	</head>
	<body style="font-family: Times New Roman; text-align: center;" onload="document.search_form.search_string.focus();">
		<div style="float: right;">
			<a href="help.jsp">Help</a>
		</div>
		<div style="padding-top: 35px;">
			<h2 style="padding-bottom: 25px;">Sparky!</h2>
			<p>
				<form method="get" action="/jsp/servlets/SearchQuery">
					<input type="text" name="search_string" autocomplete="off" maxlength="2048" size="55" title="point_id: <ID> time_range: <start time> - <end time>" /><br />
					<input type="submit" name="btn_search" value="Ask Sparky" />
				</form>
			</p>
			<p style="font-size: 13px;">
				Sparky is a prototype search appliance for PMU data processed by Phaedo.<br />
				With Sparky we aim to allow engineers the ability to find relevant data patterns within the larger corpus of PMU data.
			</p>
		</div>
	</body>
</html>