//*******************************************************************************************************
//  IdentifiableItem.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  03/12/2007 - Pinal C. Patel
//      Generated original version of source code.
//  09/08/2008 - J. Ritchie Carroll
//      Converted to C#.
//  11/05/2008 - Pinal C. Patel
//      Edited code comments.
//
//*******************************************************************************************************


namespace PCS
{
    /// <summary>
    /// A class that can be used to assign an identifier to an item for the purpose of identification.
    /// </summary>
    /// <typeparam name="TId">Type of the identifier to be used for identification.</typeparam>
    /// <typeparam name="TItem">Type of the item that is to be made identifiable.</typeparam>
    public class IdentifiableItem<TId, TItem>
    {
        #region [ Members ]

        // Fields
        private TId m_id;
        private TItem m_item;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentifiableItem{TId, TItem}"/> class.
        /// </summary>
        /// <param name="id">The identifier of the <paramref name="item"/>.</param>
        /// <param name="item">The item being assigned the <paramref name="id"/> to make it identifiable.</param>
        public IdentifiableItem(TId id, TItem item)
        {
            this.ID = id;
            this.Item = item;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the identifier of the <see cref="Item"/>.
        /// </summary>
        public TId ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        /// <summary>
        /// Gets or sets the item being made identifiable by assigning it an <see cref="ID"/>.
        /// </summary>
        public TItem Item
        {
            get
            {
                return m_item;
            }
            set
            {
                m_item = value;
            }
        }

        #endregion
    }
}