using System;
using System.Runtime;
using System.Runtime.CompilerServices;
namespace System.Collections.Specialized
{
    /// <summary>Describes the action that caused a <see cref="E:System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged" /> event. </summary>
    [__DynamicallyInvokable]
    public enum NotifyCollectionChangedAction
    {
        /// <summary>One or more items were added to the collection.</summary>
        Add,
        /// <summary>One or more items were removed from the collection.</summary>
        Remove,
        /// <summary>One or more items were replaced in the collection.</summary>
        Replace,
        /// <summary>One or more items were moved within the collection.</summary>
        Move,
        /// <summary>The content of the collection changed dramatically.</summary>
        Reset
    }    
    /// <summary>Provides data for the <see cref="E:System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged" /> event.</summary>
    [__DynamicallyInvokable]
    public class NotifyCollectionChangedEventArgs : EventArgs
    {
        private NotifyCollectionChangedAction _action;
        private IList _newItems;
        private IList _oldItems;
        private int _newStartingIndex = -1;
        private int _oldStartingIndex = -1;
        /// <summary>Gets the action that caused the event. </summary>
        /// <returns>A <see cref="T:System.Collections.Specialized.NotifyCollectionChangedAction" /> value that describes the action that caused the event.</returns>
        [__DynamicallyInvokable]
        public NotifyCollectionChangedAction Action
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._action;
            }
        }
        /// <summary>Gets the list of new items involved in the change.</summary>
        /// <returns>The list of new items involved in the change.</returns>
        [__DynamicallyInvokable]
        public IList NewItems
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._newItems;
            }
        }
        /// <summary>Gets the list of items affected by a <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" />, Remove, or Move action.</summary>
        /// <returns>The list of items affected by a <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" />, Remove, or Move action.</returns>
        [__DynamicallyInvokable]
        public IList OldItems
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._oldItems;
            }
        }
        /// <summary>Gets the index at which the change occurred.</summary>
        /// <returns>The zero-based index at which the change occurred.</returns>
        [__DynamicallyInvokable]
        public int NewStartingIndex
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._newStartingIndex;
            }
        }
        /// <summary>Gets the index at which a <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Move" />, Remove, or Replace action occurred.</summary>
        /// <returns>The zero-based index at which a <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Move" />, Remove, or Replace action occurred.</returns>
        [__DynamicallyInvokable]
        public int OldStartingIndex
        {
            [__DynamicallyInvokable, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._oldStartingIndex;
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" /> change.</summary>
        /// <param name="action">The action that caused the event. This must be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" />.</param>
        [__DynamicallyInvokable]
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action)
        {
            if (action != NotifyCollectionChangedAction.Reset)
            {
                throw new ArgumentException("WrongActionForCtor");
            }
            this.InitializeAdd(action, null, -1);
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a one-item change.</summary>
        /// <param name="action">The action that caused the event. This can be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" />, <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Add" />, or <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Remove" />.</param>
        /// <param name="changedItem">The item that is affected by the change.</param>
        /// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Reset, Add, or Remove, or if <paramref name="action" /> is Reset and <paramref name="changedItem" /> is not null.</exception>
        [__DynamicallyInvokable]
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem)
        {
            if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove && action != NotifyCollectionChangedAction.Reset)
            {
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor");
            }
            if (action != NotifyCollectionChangedAction.Reset)
            {
                this.InitializeAddOrRemove(action, new object[]
				{
					changedItem
				}, -1);
                return;
            }
            if (changedItem != null)
            {
                throw new ArgumentException("ResetActionRequiresNullItem");
            }
            this.InitializeAdd(action, null, -1);
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a one-item change.</summary>
        /// <param name="action">The action that caused the event. This can be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" />, <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Add" />, or <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Remove" />.</param>
        /// <param name="changedItem">The item that is affected by the change.</param>
        /// <param name="index">The index where the change occurred.</param>
        /// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Reset, Add, or Remove, or if <paramref name="action" /> is Reset and either <paramref name="changedItems" /> is not null or <paramref name="index" /> is not -1.</exception>
        [__DynamicallyInvokable]
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index)
        {
            if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove && action != NotifyCollectionChangedAction.Reset)
            {
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor");
            }
            if (action != NotifyCollectionChangedAction.Reset)
            {
                this.InitializeAddOrRemove(action, new object[]
				{
					changedItem
				}, index);
                return;
            }
            if (changedItem != null)
            {
                throw new ArgumentException("ResetActionRequiresNullItem");
            }
            if (index != -1)
            {
                throw new ArgumentException("ResetActionRequiresIndexMinus1");
            }
            this.InitializeAdd(action, null, -1);
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a multi-item change.</summary>
        /// <param name="action">The action that caused the event. This can be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" />, <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Add" />, or <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Remove" />.</param>
        /// <param name="changedItems">The items that are affected by the change.</param>
        [__DynamicallyInvokable]
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems)
        {
            if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove && action != NotifyCollectionChangedAction.Reset)
            {
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor");
            }
            if (action == NotifyCollectionChangedAction.Reset)
            {
                if (changedItems != null)
                {
                    throw new ArgumentException("ResetActionRequiresNullItem");
                }
                this.InitializeAdd(action, null, -1);
                return;
            }
            else
            {
                if (changedItems == null)
                {
                    throw new ArgumentNullException("changedItems");
                }
                this.InitializeAddOrRemove(action, changedItems, -1);
                return;
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a multi-item change or a <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" /> change.</summary>
        /// <param name="action">The action that caused the event. This can be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Reset" />, <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Add" />, or <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Remove" />.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="startingIndex">The index where the change occurred.</param>
        /// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Reset, Add, or Remove, if <paramref name="action" /> is Reset and either <paramref name="changedItems" /> is not null or <paramref name="startingIndex" /> is not -1, or if action is Add or Remove and <paramref name="startingIndex" /> is less than -1.</exception>
        /// <exception cref="T:System.ArgumentNullException">If <paramref name="action" /> is Add or Remove and <paramref name="changedItems" /> is null.</exception>
        [__DynamicallyInvokable]
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
        {
            if (action != NotifyCollectionChangedAction.Add && action != NotifyCollectionChangedAction.Remove && action != NotifyCollectionChangedAction.Reset)
            {
                throw new ArgumentException("MustBeResetAddOrRemoveActionForCtor");
            }
            if (action == NotifyCollectionChangedAction.Reset)
            {
                if (changedItems != null)
                {
                    throw new ArgumentException("ResetActionRequiresNullItem");
                }
                if (startingIndex != -1)
                {
                    throw new ArgumentException("ResetActionRequiresIndexMinus1");
                }
                this.InitializeAdd(action, null, -1);
                return;
            }
            else
            {
                if (changedItems == null)
                {
                    throw new ArgumentNullException("changedItems");
                }
                if (startingIndex < -1)
                {
                    throw new ArgumentException("IndexCannotBeNegative");
                }
                this.InitializeAddOrRemove(action, changedItems, startingIndex);
                return;
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a one-item <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" /> change.</summary>
        /// <param name="action">The action that caused the event. This can only be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" />.</param>
        /// <param name="newItem">The new item that is replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        /// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Replace.</exception>
        [__DynamicallyInvokable]
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem)
        {
            if (action != NotifyCollectionChangedAction.Replace)
            {
                throw new ArgumentException("WrongActionForCtor");
            }
            this.InitializeMoveOrReplace(action, new object[]
			{
				newItem
			}, new object[]
			{
				oldItem
			}, -1, -1);
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a one-item <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" /> change.</summary>
        /// <param name="action">The action that caused the event. This can be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" />.</param>
        /// <param name="newItem">The new item that is replacing the original item.</param>
        /// <param name="oldItem">The original item that is replaced.</param>
        /// <param name="index">The index of the item being replaced.</param>
        /// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Replace.</exception>
        [__DynamicallyInvokable]
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
        {
            if (action != NotifyCollectionChangedAction.Replace)
            {
                throw new ArgumentException("WrongActionForCtor");
            }
            this.InitializeMoveOrReplace(action, new object[]
			{
				newItem
			}, new object[]
			{
				oldItem
			}, index, index);
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a multi-item <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" /> change.</summary>
        /// <param name="action">The action that caused the event. This can only be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" />.</param>
        /// <param name="newItems">The new items that are replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        /// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Replace.</exception>
        /// <exception cref="T:System.ArgumentNullException">If <paramref name="oldItems" /> or <paramref name="newItems" /> is null.</exception>
        [__DynamicallyInvokable]
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems)
        {
            if (action != NotifyCollectionChangedAction.Replace)
            {
                throw new ArgumentException("WrongActionForCtor");
            }
            if (newItems == null)
            {
                throw new ArgumentNullException("newItems");
            }
            if (oldItems == null)
            {
                throw new ArgumentNullException("oldItems");
            }
            this.InitializeMoveOrReplace(action, newItems, oldItems, -1, -1);
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a multi-item <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" /> change.</summary>
        /// <param name="action">The action that caused the event. This can only be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Replace" />.</param>
        /// <param name="newItems">The new items that are replacing the original items.</param>
        /// <param name="oldItems">The original items that are replaced.</param>
        /// <param name="startingIndex">The index of the first item of the items that are being replaced.</param>
        /// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Replace.</exception>
        /// <exception cref="T:System.ArgumentNullException">If <paramref name="oldItems" /> or <paramref name="newItems" /> is null.</exception>
        [__DynamicallyInvokable]
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex)
        {
            if (action != NotifyCollectionChangedAction.Replace)
            {
                throw new ArgumentException("WrongActionForCtor");
            }
            if (newItems == null)
            {
                throw new ArgumentNullException("newItems");
            }
            if (oldItems == null)
            {
                throw new ArgumentNullException("oldItems");
            }
            this.InitializeMoveOrReplace(action, newItems, oldItems, startingIndex, startingIndex);
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a one-item <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Move" /> change.</summary>
        /// <param name="action">The action that caused the event. This can only be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Move" />.</param>
        /// <param name="changedItem">The item affected by the change.</param>
        /// <param name="index">The new index for the changed item.</param>
        /// <param name="oldIndex">The old index for the changed item.</param>
        /// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Move or <paramref name="index" /> is less than 0.</exception>
        [__DynamicallyInvokable]
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex)
        {
            if (action != NotifyCollectionChangedAction.Move)
            {
                throw new ArgumentException("WrongActionForCtor");
            }
            if (index < 0)
            {
                throw new ArgumentException("IndexCannotBeNegative");
            }
            object[] array = new object[]
			{
				changedItem
			};
            this.InitializeMoveOrReplace(action, array, array, index, oldIndex);
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs" /> class that describes a multi-item <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Move" /> change.</summary>
        /// <param name="action">The action that caused the event. This can only be set to <see cref="F:System.Collections.Specialized.NotifyCollectionChangedAction.Move" />.</param>
        /// <param name="changedItems">The items affected by the change.</param>
        /// <param name="index">The new index for the changed items.</param>
        /// <param name="oldIndex">The old index for the changed items.</param>
        /// <exception cref="T:System.ArgumentException">If <paramref name="action" /> is not Move or <paramref name="index" /> is less than 0.</exception>
        [__DynamicallyInvokable]
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex)
        {
            if (action != NotifyCollectionChangedAction.Move)
            {
                throw new ArgumentException("WrongActionForCtor");
            }
            if (index < 0)
            {
                throw new ArgumentException("IndexCannotBeNegative");
            }
            this.InitializeMoveOrReplace(action, changedItems, changedItems, index, oldIndex);
        }
        internal NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int newIndex, int oldIndex)
        {
            this._action = action;
            this._newItems = ((newItems == null) ? null : ArrayList.ReadOnly(newItems));
            this._oldItems = ((oldItems == null) ? null : ArrayList.ReadOnly(oldItems));
            this._newStartingIndex = newIndex;
            this._oldStartingIndex = oldIndex;
        }
        private void InitializeAddOrRemove(NotifyCollectionChangedAction action, IList changedItems, int startingIndex)
        {
            if (action == NotifyCollectionChangedAction.Add)
            {
                this.InitializeAdd(action, changedItems, startingIndex);
                return;
            }
            if (action == NotifyCollectionChangedAction.Remove)
            {
                this.InitializeRemove(action, changedItems, startingIndex);
            }
        }
        private void InitializeAdd(NotifyCollectionChangedAction action, IList newItems, int newStartingIndex)
        {
            this._action = action;
            this._newItems = ((newItems == null) ? null : ArrayList.ReadOnly(newItems));
            this._newStartingIndex = newStartingIndex;
        }
        private void InitializeRemove(NotifyCollectionChangedAction action, IList oldItems, int oldStartingIndex)
        {
            this._action = action;
            this._oldItems = ((oldItems == null) ? null : ArrayList.ReadOnly(oldItems));
            this._oldStartingIndex = oldStartingIndex;
        }
        private void InitializeMoveOrReplace(NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex, int oldStartingIndex)
        {
            this.InitializeAdd(action, newItems, startingIndex);
            this.InitializeRemove(action, oldItems, oldStartingIndex);
        }
    }
    
    /// <summary>Represents the method that handles the <see cref="E:System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged" /> event. </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">Information about the event.</param>
    [__DynamicallyInvokable]
    public delegate void NotifyCollectionChangedEventHandler(object sender, NotifyCollectionChangedEventArgs e);

    /// <summary>Notifies listeners of dynamic changes, such as when items get added and removed or the whole list is refreshed.</summary>
    [__DynamicallyInvokable]
    public interface INotifyCollectionChanged
    {
        /// <summary>Occurs when the collection changes.</summary>
        [__DynamicallyInvokable]
        event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}
