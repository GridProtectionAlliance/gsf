Notes:

When adding CSS scripts to the shared resources, carefully review the code for "url(...)" references.
References are typically relative and need to be converted to explicit GSF resource paths, for example:

	the following URL path:
		url("./images/ui-image.png")

	would need to be replaced with:
		url("/@GSF/Web/Shared/Content/Images/ui-image.png")

Referenced URL materials need to be added as embedded resources - ideally under the Content
folder, e.g., Images or Fonts, since they are for use by the style sheets.

Note that the paths are case-sensitive.