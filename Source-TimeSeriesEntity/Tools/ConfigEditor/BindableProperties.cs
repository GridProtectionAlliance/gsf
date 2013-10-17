//******************************************************************************************************
//  BindableProperties.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  01/26/2010 - Mehulbhai P. Thakkar
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.ComponentModel;

namespace ConfigEditor
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public class BindableProperties : Component, ICustomTypeDescriptor
	{
	    readonly PropertyDescriptorCollection m_propertyCollection;

		public BindableProperties()
		{
			m_propertyCollection = new PropertyDescriptorCollection(new PropertyDescriptor[] { });
		}

		public void AddProperty(string propName, object propValue, string propDesc,
			string propCat, Type propType, bool isReadOnly, bool isExpandable)
		{
			DynamicProperty p = new DynamicProperty(propName, propValue, propDesc, propCat,
				propType, isReadOnly, isExpandable);
			m_propertyCollection.Add(p);
		}

		public DynamicProperty this[int index]
		{
			get { return (DynamicProperty)m_propertyCollection[index]; }
		}

		public DynamicProperty this[string name]
		{
			get { return (DynamicProperty)m_propertyCollection[name]; }
		}

		#region ICustomTypeDescriptor Members

		public AttributeCollection GetAttributes()
		{
			return (TypeDescriptor.GetAttributes(this, true));
		}

		public string GetClassName()
		{
			return (TypeDescriptor.GetClassName(this, true));
		}

		public string GetComponentName()
		{
			return (TypeDescriptor.GetComponentName(this, true));
		}

		public TypeConverter GetConverter()
		{
			return (TypeDescriptor.GetConverter(this, true));
		}

		public EventDescriptor GetDefaultEvent()
		{
			return (TypeDescriptor.GetDefaultEvent(this, true));
		}

		public PropertyDescriptor GetDefaultProperty()
		{
			PropertyDescriptorCollection props = GetAllProperties();

			if (props.Count > 0)
				return (props[0]);
			else
				return (null);
		}

		public object GetEditor(Type editorBaseType)
		{
			return (TypeDescriptor.GetEditor(this, editorBaseType, true));
		}

		public EventDescriptorCollection GetEvents(Attribute[] attributes)
		{
			return (TypeDescriptor.GetEvents(this, attributes, true));
		}

		public EventDescriptorCollection GetEvents()
		{
			return (TypeDescriptor.GetEvents(this, true));
		}

		public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
		{
			return (GetAllProperties());
		}

		public PropertyDescriptorCollection GetProperties()
		{
			return (GetAllProperties());
		}

		public object GetPropertyOwner(PropertyDescriptor pd)
		{
			return (this);
		}

		#endregion

		private PropertyDescriptorCollection GetAllProperties()
		{
			return m_propertyCollection;
		}
	}

	public class DynamicProperty : PropertyDescriptor
	{
		private string m_propName;
		private object m_propValue;
		private readonly string m_propDescription;
		private readonly string m_propCategory;
		private readonly Type m_propType;
		private readonly bool m_isReadOnly;
		private bool m_isExpandable;

		public DynamicProperty(string pName, object pValue, string pDesc, string pCat, Type pType, bool readOnly, bool expandable)
			: base(pName, new Attribute[] { })
		{
			m_propName = pName;
			m_propValue = pValue;
			m_propDescription = pDesc;
			m_propCategory = pCat;
			m_propType = pType;
			m_isReadOnly = readOnly;
			m_isExpandable = expandable;
		}

		public override Type ComponentType
		{
			get
			{
				return null;
			}
		}

		public override string Category
		{
			get
			{
				return m_propCategory;
			}
		}

		public override bool IsReadOnly
		{
			get
			{
				return m_isReadOnly;
			}
		}

		public override Type PropertyType
		{
			get
			{
				return m_propType;
			}
		}

		public override bool CanResetValue(object component)
		{
			return true;
		}

		public override object GetValue(object component)
		{
			return m_propValue;
		}

		public override void SetValue(object component, object value)
		{
			m_propValue = value;
		}

		public override void ResetValue(object component)
		{
			m_propValue = null;
		}

		public override bool ShouldSerializeValue(object component)
		{
			return false;
		}

		public override string Description
		{
			get
			{
				return m_propDescription;
			}
		}
	}
}
