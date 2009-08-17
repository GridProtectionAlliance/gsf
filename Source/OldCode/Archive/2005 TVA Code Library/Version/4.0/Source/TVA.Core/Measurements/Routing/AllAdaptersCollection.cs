//*******************************************************************************************************
//  AllAdaptersCollection.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R. Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  05/13/2009 - James R. Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

using System;
using System.ComponentModel;
using System.Data;

namespace TVA.Measurements.Routing
{
    /// <summary>
    /// Represents a collection of all <see cref="IAdapterCollection"/> implementations (i.e., a collection of <see cref="IAdapterCollection"/>'s).
    /// </summary>
    /// <remarks>
    /// This collection allows all <see cref="IAdapterCollection"/> implementations to be managed as a group.
    /// </remarks>
    [CLSCompliant(false)]
    public class AllAdaptersCollection : AdapterCollectionBase<IAdapterCollection>
    {
        #region [ Constructors ]

        /// <summary>
        /// Constructs a new instance of the <see cref="AllAdaptersCollection"/>.
        /// </summary>
        public AllAdaptersCollection()
        {
            base.Name = "All Adapters Collection";
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets flag that detemines if <see cref="IAdapter"/> implementations are automatically initialized
        /// when they are added to the collection.
        /// </summary>
        /// <remarks>
        /// We don't auto-initialize collections added to the <see cref="AllAdaptersCollection"/> since no data source
        /// will be available when the collections are being created.
        /// </remarks>
        protected override bool AutoInitialize
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes each <see cref="IAdapterCollection"/> implementation in this <see cref="AllAdaptersCollection"/>.
        /// </summary>
        public override void Initialize()
        {
            foreach (IAdapterCollection item in this)
            {
                item.Initialize();
            }
        }

        /// <summary>
        /// This method is not implemented in <see cref="AllAdaptersCollection"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool TryCreateAdapter(DataRow adapterRow, out IAdapterCollection adapter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented in <see cref="AllAdaptersCollection"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool TryGetAdapterByID(uint ID, out IAdapterCollection adapter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method is not implemented in <see cref="AllAdaptersCollection"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool TryInitializeAdapterByID(uint id)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}