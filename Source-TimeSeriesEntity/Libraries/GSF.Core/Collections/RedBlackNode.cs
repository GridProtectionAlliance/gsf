//******************************************************************************************************
//  RedBlackNode.cs - Gbtc
//
//  Copyright © 2013, Grid Protection Alliance.  All Rights Reserved.
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
//  12/06/2013 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;

namespace GSF.Collections
{
    /// <summary>
    /// Represents a node in a red-black tree.
    /// </summary>
    /// <typeparam name="T">The type of objects stored in the tree.</typeparam>
    public class RedBlackNode<T>
    {
        #region [ Members ]

        // Constants
        private const bool Red = false;
        private const bool Black = true;

        // Fields 
        private RedBlackNode<T> m_parent;
        private RedBlackNode<T> m_left;
        private RedBlackNode<T> m_right;
        private bool m_color;

        private T m_item;
        private IComparer<T> m_itemComparer;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="RedBlackNode{T}"/> class.
        /// </summary>
        /// <param name="item">The item encapsulated by this node.</param>
        public RedBlackNode(T item)
            : this(item, Comparer<T>.Default)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RedBlackNode{T}"/> class.
        /// </summary>
        /// <param name="item">The item encapsulated by this node.</param>
        /// <param name="itemComparer">The comparer used to compare items in the tree.</param>
        public RedBlackNode(T item, IComparer<T> itemComparer)
        {
            if ((object)item == null)
                throw new ArgumentNullException("item");

            if ((object)itemComparer == null)
                throw new ArgumentNullException("itemComparer");

            m_item = item;
            m_itemComparer = itemComparer;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the item encapsulated by this node.
        /// </summary>
        public T Item
        {
            get
            {
                return m_item;
            }
        }

        /// <summary>
        /// Gets the root node of the tree that this node resides in.
        /// </summary>
        public RedBlackNode<T> Root
        {
            get
            {
                return ((object)Parent != null) ? Parent.Root : this;
            }
        }

        /// <summary>
        /// Gets the parent node.
        /// </summary>
        public RedBlackNode<T> Parent
        {
            get
            {
                return m_parent;
            }
        }

        /// <summary>
        /// Gets the left child node.
        /// </summary>
        public RedBlackNode<T> Left
        {
            get
            {
                return m_left;
            }
        }

        /// <summary>
        /// Gets the right child node.
        /// </summary>
        public RedBlackNode<T> Right
        {
            get
            {
                return m_right;
            }
        }

        /// <summary>
        /// Gets the <see cref="Parent"/> of this node's parent.
        /// </summary>
        public RedBlackNode<T> Grandparent
        {
            get
            {
                return ((object)Parent != null) ? Parent.Parent : null;
            }
        }

        /// <summary>
        /// Gets the child of this node's <see cref="Grandparent"/>
        /// which is not this node's <see cref="Parent"/>.
        /// </summary>
        public RedBlackNode<T> Uncle
        {
            get
            {
                RedBlackNode<T> grandparent = Grandparent;

                if ((object)grandparent == null)
                    return null;

                if (Parent == grandparent.Left)
                    return grandparent.Right;

                return grandparent.Left;
            }
        }

        /// <summary>
        /// Gets the child of this node's <see cref="Parent"/> which is not this node.
        /// </summary>
        public RedBlackNode<T> Sibling
        {
            get
            {
                if ((object)Parent != null)
                    return (this != Parent.Left) ? Parent.Left : Parent.Right;

                return null;
            }
        }

        /// <summary>
        /// Gets the first node in an inorder traversal
        /// of the tree that this node resides in.
        /// </summary>
        public RedBlackNode<T> First
        {
            get
            {
                RedBlackNode<T> first = Root;

                while ((object)first.Left != null)
                    first = first.Left;

                return first;
            }
        }

        /// <summary>
        /// Gets the next node in an inorder traversal
        /// of the tree that this node resides in.
        /// </summary>
        public RedBlackNode<T> Next
        {
            get
            {
                RedBlackNode<T> next;

                if ((object)Right != null)
                {
                    next = Right;

                    while ((object)next.Left != null)
                        next = next.Left;
                }
                else
                {
                    next = this;

                    while ((object)next.Parent != null && next != next.Parent.Left)
                        next = next.Parent;

                    next = next.Parent;
                }

                return next;
            }
        }

        /// <summary>
        /// Gets the previous node in an inorder traversal
        /// of the tree that this node resides in.
        /// </summary>
        public RedBlackNode<T> Previous
        {
            get
            {
                RedBlackNode<T> previous;

                if ((object)Left != null)
                {
                    previous = Left;

                    while ((object)previous.Right != null)
                        previous = previous.Right;
                }
                else
                {
                    previous = this;

                    while ((object)previous.Parent != null && previous != previous.Parent.Right)
                        previous = previous.Parent;

                    previous = previous.Parent;
                }

                return previous;
            }
        }

        /// <summary>
        /// Gets the last node in an inorder traversal
        /// of the tree that this node resides in.
        /// </summary>
        public RedBlackNode<T> Last
        {
            get
            {
                RedBlackNode<T> last = Root;

                while ((object)last.Right != null)
                    last = last.Right;

                return last;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Inserts the given item into the tree in which this node resides, if the item is not already present.
        /// </summary>
        /// <param name="node">The item to be inserted into the tree.</param>
        /// <returns>True if this item was inserted into the tree; false otherwise.</returns>
        /// <remarks>Inserting an item into a branch node of a tree is equivalent to inserting into the root node of that tree.</remarks>
        public bool Insert(RedBlackNode<T> node)
        {
            // Ensure that the inserted node uses the same
            // comparer as the other nodes in the tree
            node.m_itemComparer = m_itemComparer;

            // Attempt to insert the node, and rebalance
            // the tree if insertion is successful
            if (Root.InsertNode(node))
            {
                node.BalanceAfterInsert();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes this node from the tree.
        /// </summary>
        public void Remove()
        {
            // When removing nodes from a binary tree, the easiest thing to do is swap that node
            // with a leaf node and then remove the leaf node. When doing so, there are actually
            // two valid leaf nodes that can be swapped without breaking the tree. This algorithm
            // attempts to pick the one for which rebalancing the tree after the swap is trivial.
            RedBlackNode<T> leftSwapNode = null;
            RedBlackNode<T> rightSwapNode = null;
            RedBlackNode<T> swapNode;

            if ((object)Left != null)
            {
                // Find the largest leaf node in the left subtree
                leftSwapNode = Left;

                while ((object)leftSwapNode.Right != null)
                    leftSwapNode = leftSwapNode.Right;
            }

            if ((object)Right != null)
            {
                // If the swapping with the left swap node is nontrivial, find the largest leaf node in the right subtree
                if ((object)leftSwapNode == null || (leftSwapNode.m_color == Black && IsBlack(leftSwapNode.Left)))
                {
                    rightSwapNode = Right;

                    while ((object)rightSwapNode.Left != null)
                        rightSwapNode = rightSwapNode.Left;
                }
            }

            // If the right swap node is not null,
            // the left swap node is nontrivial to swap,
            // so prefer the right swap node
            swapNode = rightSwapNode ?? leftSwapNode;

            if ((object)swapNode == null)
            {
                // No swap node means this
                // is already a leaf node
                Delete();
            }
            else
            {
                // Swapping with the leaf node
                // makes this node a leaf node
                Swap(swapNode);
                Delete();
            }
        }

        /// <summary>
        /// Called when a node is inserted into the tree as a child of this node.
        /// </summary>
        protected virtual void ChildInserted()
        {
        }

        /// <summary>
        /// Called when a node that is a child of this node is removed from the tree.
        /// </summary>
        protected virtual void ChildRemoved()
        {
        }

        /// <summary>
        /// Called when this node is rotated left.
        /// </summary>
        protected virtual void RotatedLeft()
        {
        }

        /// <summary>
        /// Called when this node is rotated right.
        /// </summary>
        protected virtual void RotatedRight()
        {
        }

        /// <summary>
        /// Called when this node is swapped with another node.
        /// </summary>
        protected virtual void Swapped(RedBlackNode<T> swappedNode)
        {
        }

        // Inserts the given node into the tree in its sorted position.
        // Returns false if this node is equivalent to another node in the tree.
        private bool InsertNode(RedBlackNode<T> node)
        {
            int comparison = m_itemComparer.Compare(node.Item, Item);

            if (comparison < 0)
            {
                if ((object)Left != null)
                    return Left.InsertNode(node);

                m_left = node;
            }
            else if (comparison > 0)
            {
                if ((object)Right != null)
                    return Right.InsertNode(node);

                m_right = node;
            }

            if (comparison != 0)
            {
                ChildInserted();
                return true;
            }

            return false;
        }

        // Rebalances the tree after inserting a node into it.
        private void BalanceAfterInsert()
        {
            RedBlackNode<T> grandparent;
            RedBlackNode<T> uncle;
            RedBlackNode<T> child;

            if ((object)Parent == null)
            {
                // Root node is always black
                m_color = Black;
            }
            else if (Parent.m_color == Red)
            {
                // This node and its parent can't both be red...
                grandparent = Grandparent;
                uncle = Uncle;

                if ((object)uncle != null && uncle.m_color == Red)
                {
                    // If both parent and uncle are red, we can alternate the colors of
                    // the grandparent and its children, then recursively attempt to
                    // rebalance the tree as if we inserted the grandparent
                    Parent.m_color = Black;
                    Uncle.m_color = Black;
                    grandparent.m_color = Red;
                    grandparent.BalanceAfterInsert();
                }
                else
                {
                    // Assume that after the first rotation,
                    // this node's parent will become its child
                    child = Parent;

                    // Rotate the parent node if this node does not have the same
                    // relationship to its parent as its parent has to its grandparent.
                    // If no rotation occurs, this node is still the child node
                    if (this == Parent.Right && Parent == grandparent.Left)
                        Parent.RotateLeft();
                    else if (this == Parent.Left && Parent == grandparent.Right)
                        Parent.RotateRight();
                    else
                        child = this;

                    // Change the parent to black and the grandparent to red,
                    // then rotate the grandparent to rebalance the tree
                    child.Parent.m_color = Black;
                    grandparent.m_color = Red;

                    if (child == child.Parent.Left)
                        grandparent.RotateRight();
                    else
                        grandparent.RotateLeft();
                }
            }
        }

        // Deletes this node from the tree and ensures that the tree stays balanced.
        // Precondition: This node must have at most one child. (Therefore, the child's
        //               color will not match this node's color due to the properties
        //               of a red-black tree).
        private void Delete()
        {
            RedBlackNode<T> child = Left ?? Right;

            if ((object)child != null)
            {
                // Set the child's parent to the removed node's parent
                child.m_parent = Parent;

                // If this node was black, setting its child to black will make up for removing it.
                // If this node was red, its child is already black so this is inconsequential
                child.m_color = Black;
            }
            else if (m_color == Black)
            {
                // If this node is black and it has no children,
                // then we will need to rebalance the tree
                BalanceBeforeDelete();
            }

            if ((object)Parent != null)
            {
                // Set the parent's reference to this node
                // to point to this node's child instead
                if (this == Parent.Left)
                    Parent.m_left = child;
                else
                    Parent.m_right = child;
            }

            // Remove all references to the tree
            m_parent = null;
            m_left = null;
            m_right = null;
        }

        // Rebalances the tree after removing a node from it.
        private void BalanceBeforeDelete()
        {
            RedBlackNode<T> sibling;

            // If this is the root node, then we've already balanced
            // everything under the root node so we are done
            if ((object)Parent != null)
            {
                sibling = Sibling;

                // If sibling is red, swap the colors of the parent and sibling,
                // then rotate the parent so that this node is now the sibling
                // of a black node and the child of a red node
                if (sibling.m_color == Red)
                {
                    Parent.m_color = Red;
                    sibling.m_color = Black;

                    if (this == Parent.Left)
                        Parent.RotateLeft();
                    else
                        Parent.RotateRight();

                    sibling = Sibling;
                }

                // If both of sibling's children are black,
                // we only have two additional cases to deal with
                if (IsBlack(sibling.Left) && IsBlack(sibling.Right))
                {
                    // If parent is black, change sibling to red and rebalance the parent.
                    // If parent is red, swap parent's color with sibling's, and the rebalance is finished
                    if (Parent.m_color == Black)
                    {
                        sibling.m_color = Red;
                        Parent.BalanceBeforeDelete();
                    }
                    else
                    {
                        Parent.m_color = Black;
                        sibling.m_color = Red;
                    }
                }
                else
                {
                    // At least one of the sibling's children are red. Perform a rotation such that the sibling's
                    // relationship to its red child is opposite this node's relationship to its parent
                    if (this == Parent.Left && IsBlack(sibling.Right))
                    {
                        sibling.m_color = Red;
                        sibling.Left.m_color = Black;
                        sibling.RotateRight();
                        sibling = Sibling;
                    }
                    else if (this == Parent.Right && IsBlack(sibling.Left))
                    {
                        sibling.m_color = Red;
                        sibling.Right.m_color = Black;
                        sibling.RotateLeft();
                        sibling = Sibling;
                    }

                    // Swap the parent's color with its sibling's color,
                    // change the sibling's red child to black,
                    // and then rotate so that the parent becomes the sibling's child
                    sibling.m_color = Parent.m_color;
                    Parent.m_color = Black;

                    if (this == Parent.Left)
                    {
                        sibling.Right.m_color = Black;
                        Parent.RotateLeft();
                    }
                    else
                    {
                        sibling.Left.m_color = Black;
                        Parent.RotateRight();
                    }
                }
            }
        }

        /// <summary>
        /// Rotates this node to the left so that it becomes the left
        /// child of the node that is currently its right child.
        /// </summary>
        private void RotateLeft()
        {
            RedBlackNode<T> right = Right;

            // Fix parent's child references
            if ((object)Parent != null)
            {
                if (this == Parent.Left)
                    Parent.m_left = right;
                else
                    Parent.m_right = right;
            }

            // Fix parent references
            right.m_parent = Parent;
            m_parent = right;

            // Fix child references
            m_right = right.Left;
            right.m_left = this;

            RotatedLeft();
        }

        /// <summary>
        /// Rotates this node to the right so that it becomes the right
        /// child of the node that is currently its left child.
        /// </summary>
        private void RotateRight()
        {
            RedBlackNode<T> left = Left;

            // Fix parent's child references
            if ((object)Parent != null)
            {
                if (this == Parent.Left)
                    Parent.m_left = left;
                else
                    Parent.m_right = left;
            }

            // Fix parent references
            left.m_parent = Parent;
            m_parent = left;

            // Fix child references
            m_left = left.Right;
            left.m_right = this;

            RotatedRight();
        }

        // Swaps two of the nodes in a tree.
        private void Swap(RedBlackNode<T> swapNode)
        {
            RedBlackNode<T> parent = Parent;
            RedBlackNode<T> left = Left;
            RedBlackNode<T> right = Right;
            bool color = m_color;

            m_parent = swapNode.Parent;
            m_left = swapNode.Left;
            m_right = swapNode.Right;
            m_color = swapNode.m_color;

            swapNode.m_parent = parent;
            swapNode.m_left = left;
            swapNode.m_right = right;
            swapNode.m_color = color;

            Swapped(swapNode);
            swapNode.Swapped(this);
        }

        #endregion

        #region [ Static ]

        // Static Methods

        // Determines if the node is black when the node may or may not be null.
        // Nodes which are null are always black.
        private static bool IsBlack(RedBlackNode<T> node)
        {
            return ((object)node == null) || (node.m_color == Black);
        }

        #endregion

    }
}
