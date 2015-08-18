<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html>
	<head>
		<title>Sparky - The PMU-Data Search Engine - Search Results</title>
		<style>
			h3
			{
				font-size: 14px;
				color: #454545;
			}
			
			.horizontal_rule
			{
				border-bottom: 1px solid #cccccc;
				font-size: 13px;
				margin: 0px 6px 12px 0px;
				padding: 0px 6px 0px 0px;
				width: 90%;
			}
			
			.search_terms
			{
				color: #9999aa;
				font-weight: 600;
			}
			
			.result_graph
			{
				background-color: #CCCCCC;
				width: 200px;
				height: 80px;
				border: 1px solid #bbbbbb;
				float: left;
				margin-right: 20px;
			}
			
			.sequence_result
			{
				padding: 6px 6px 6px 0px;
				margin: 6px 6px 6px 0px;
				border-top: 1px solid #aaaaaa;
				width: 90%;
				height: 100px;
			}
		</style>
	</head>

	<body style="font-family: Tahoma; font-size: 11px; color: #333333; padding-left: 10px; margin-left: 10px;">

		<form method="get" action="/jsp/servlets/SearchQuery">
			<span style="font-size: 15px; font-weight: 600;">Ask Sparky:</span>
			<input type="text" name="search_string" autocomplete="off" maxlength="2048" size="55" title="point_id: <ID> time_range: <start time> - <end time>" />
			<input type="submit" name="btn_search" value="Ask Sparky" />
		</form>

		<div class="horizontal_rule">&nbsp;</div><br/>
		
		<div class="horizontal_rule">
			Results listed in order of relevance for sequence '<%--TMPL_VAR:search_string--%>'
		</div>

		<%--TMPL_VAR:search_results--%>

		<div class="horizontal_rule">&nbsp;</div>

		<form method="get" action="/jsp/servlets/SearchQuery">
			<input type="text" name="search_string" autocomplete="off" maxlength="2048" size="55" title="point_id: <ID> time_range: <start time> - <end time>" />
			<input type="submit" name="btn_search" value="Ask Sparky" />
		</form>

		<div class="horizontal_rule">&nbsp;</div>

	</body>
</html>