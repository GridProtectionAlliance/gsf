using System.Diagnostics;
using System.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

// 02/14/2007

namespace TVA.Web
{
	namespace Services
	{
		
		public interface IBusinessObjectsAdapter
		{
			
			void Initialize(params object[] itemList);
			string BuildMessage();
			
		}
		
	}
}
