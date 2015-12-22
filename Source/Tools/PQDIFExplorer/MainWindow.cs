//******************************************************************************************************
//  MainWindow.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/14/2015 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using GSF.PQDIF;
using GSF.PQDIF.Logical;
using GSF.PQDIF.Physical;
using PQDIFExplorer.Properties;

namespace PQDIFExplorer
{
    /// <summary>
    /// The main window of the PQDIF Explorer application.
    /// </summary>
    public partial class MainWindow : Form
    {
        private List<DetailsWindow> m_detailsWindows;

        /// <summary>
        /// Creates a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        // Opens the given file for exploration.
        private void OpenFile(string fileName)
        {
            Record record;
            ContainerRecord containerRecord;

            // Clear out existing items in the tree view
            RecordTree.Nodes.Clear();

            // Use the physical parser to more closely display the structure of the file
            using (PhysicalParser parser = new PhysicalParser(fileName))
            {
                parser.Open();

                while (parser.HasNextRecord())
                {
                    // Parse the next record
                    record = parser.NextRecord();

                    // The compression algorithm and compression style are necessary for properly parsing the file
                    // and must be obtained by parsing the logical structure of the container record
                    if (record.Header.TypeOfRecord == RecordType.Container)
                    {
                        containerRecord = ContainerRecord.CreateContainerRecord(record);
                        parser.CompressionAlgorithm = containerRecord.CompressionAlgorithm;
                        parser.CompressionStyle = containerRecord.CompressionStyle;
                    }

                    // Convert this record and all its child elements to a node for
                    // the tree view and add the node to the root level of the tree
                    RecordTree.Nodes.Add(ToTreeNode(record));
                }
            }
        }

        private TreeNode ToTreeNode(Record record)
        {
            // Use the type of the record to identify it in the tree view
            TreeNode node = new TreeNode(record.Header.TypeOfRecord.ToString());

            // Set the tag of the node so that we can quickly associate
            // a selected node with the record that spawned it
            node.Tag = record;

            // Determine the icon to apply to this
            // node based on the record type
            switch (record.Header.TypeOfRecord)
            {
                case RecordType.Container:
                    node.ImageIndex = 1;
                    node.SelectedImageIndex = 1;
                    break;

                case RecordType.DataSource:
                    node.ImageIndex = 2;
                    node.SelectedImageIndex = 2;
                    break;

                case RecordType.MonitorSettings:
                    node.ImageIndex = 3;
                    node.SelectedImageIndex = 3;
                    break;

                case RecordType.Observation:
                    node.ImageIndex = 4;
                    node.SelectedImageIndex = 4;
                    break;
            }

            if ((object)record.Body != null)
            {
                // Convert each of the elements in the record body's
                // collection into child nodes of the record's tree node
                foreach (Element element in record.Body.Collection.Elements)
                    node.Nodes.Add(ToTreeNode(element));
            }

            // Return the node that represents the record
            return node;
        }
        
        private TreeNode ToTreeNode(Element element)
        {
            TreeNode node;
            CollectionElement collection;
            Tag tag;

            IEnumerable<List<TreeNode>> childNodes;

            // Look up the tag and create a leaf node identified by
            // the tag name or the tag itself if no name is available
            tag = GSF.PQDIF.Tag.GetTag(element.TagOfElement);

            if ((object)tag != null)
                node = new TreeNode(tag.Name);
            else
                node = new TreeNode(element.TagOfElement.ToString());

            // Set the image of the node based on the type of the element
            switch (element.TypeOfElement)
            {
                case ElementType.Collection:
                    node.ImageIndex = 5;
                    node.SelectedImageIndex = 5;
                    break;

                case ElementType.Vector:
                    node.ImageIndex = 6;
                    node.SelectedImageIndex = 6;
                    break;

                case ElementType.Scalar:
                    node.ImageIndex = 7;
                    node.SelectedImageIndex = 7;
                    break;
            }

            // Set the tag of the node so that we can quickly associate
            // a selected node with the element that spawned it
            node.Tag = element;

            // If the element is a collection element,
            // convert each of the elements in the collection
            // into child nodes of this element's tree node
            collection = element as CollectionElement;

            if ((object)collection != null)
            {
                foreach (Element child in collection.Elements)
                    node.Nodes.Add(ToTreeNode(child));

                // If the collection element has more than
                // one child with the same tag, add an index
                // to the nodes to aid navigation
                childNodes = node.Nodes
                    .Cast<TreeNode>()
                    .GroupBy(childNode => ((Element)childNode.Tag).TagOfElement)
                    .Select(grouping => grouping.ToList())
                    .Where(list => list.Count > 1);

                foreach (List<TreeNode> list in childNodes)
                {
                    for (int i = 0; i < list.Count; i++)
                        list[i].Text = $"[{i}] {list[i].Text}";
                }
            }

            // Return the node that represents the element
            return node;
        }

        // Gets detailed information about the given record.
        private string GetDetails(Record record)
        {
            StringBuilder details = new StringBuilder();

            // Display the fields in the record's header
            details.AppendLine($"  Signature: {record.Header.RecordSignature}");
            details.AppendLine($"       Type: {record.Header.TypeOfRecord} ({record.Header.RecordTypeTag})");
            details.AppendLine($"Header Size: {record.Header.HeaderSize}");
            details.AppendLine($"  Body Size: {record.Header.BodySize}");
            details.AppendLine($"   Checksum: {record.Header.Checksum}");

            return details.ToString();
        }

        // Gets detailed information about the given element.
        private string GetDetails(Element element)
        {
            StringBuilder details = new StringBuilder();
            Tag tag;

            // Display the value if the element is a vector or a scalar
            if (element is ScalarElement)
                details.AppendLine($"        Value: {ValueAsString((ScalarElement)element)}").AppendLine();
            else if (element is VectorElement)
                details.AppendLine($"        Value: {ValueAsString((VectorElement)element)}").AppendLine();

            // Display the tag of the element and the actual type of the
            // element and its value as defined by the data in the file
            details.AppendLine($"          Tag: {element.TagOfElement}");
            details.AppendLine($" Element type: {element.TypeOfElement}");
            details.AppendLine($"Physical type: {element.TypeOfValue}");

            // Look up the element's tag to display detailed information about the element as defined
            // by its tag as well as the expected type of the element and its value based on the tag
            tag = GSF.PQDIF.Tag.GetTag(element.TagOfElement);

            if ((object)tag != null)
            {
                details.AppendLine();
                details.AppendLine($"-- Tag details --");
                details.AppendLine($"           Name: {tag.Name}");
                details.AppendLine($"  Standard Name: {tag.StandardName}");
                details.AppendLine($"    Description: {tag.Description}");
                details.AppendLine($"   Element type: {tag.ElementType}");
                details.AppendLine($"  Physical type: {tag.PhysicalType}");
                details.AppendLine($"       Required: {(tag.Required ? "Yes" : "No")}");
            }

            return details.ToString();
        }

        // Converts the value of the given element to a string.
        private string ValueAsString(ScalarElement element)
        {
            string identifierName;
            string valueString;

            object value;

            Tag tag;
            IReadOnlyCollection<Identifier> identifiers;
            List<Identifier> bitFields;

            uint bitSet;
            List<string> setBits;

            // Get the value of the element
            // parsed from the PQDIF file
            value = element.Get();

            // Get the tag definition for the element being displayed
            tag = GSF.PQDIF.Tag.GetTag(element.TagOfElement);

            // Use the format string specified by the tag
            // or a default format string if not specified
            if (element.TypeOfValue == PhysicalType.Timestamp)
                valueString = string.Format(tag.FormatString ?? "{0:yyyy-MM-dd HH:mm:ss.fffffff}", value);
            else
                valueString = string.Format(tag.FormatString ?? "{0}", value);

            // Determine whether the tag definition contains
            // a list of identifiers which can be used to
            // display the value in a more readable format
            identifiers = tag.ValidIdentifiers;

            // Some identifier collections define a set of bitfields which can be
            // combined to represent a collection of states rather than a single value
            // and these are identified by the values being represented in hexadecimal
            bitFields = identifiers.Where(id => id.Value.StartsWith("0x")).ToList();

            if (bitFields.Count > 0)
            {
                // If the value is not convertible,
                // it cannot be converted to an
                // integer to check for bit states
                if (!(value is IConvertible))
                    return valueString;

                // Convert the value to an integer which can
                // then be checked for the state of its bits
                bitSet = Convert.ToUInt32(value);

                // Get the names of the bitfields in the
                // collection of bitfields that are set
                setBits = bitFields
                    .Select(id => new { Name = id.Name, Value = Convert.ToUInt32(id.Value, 16) })
                    .Where(id => bitSet == id.Value || (bitSet & id.Value) > 0u)
                    .Select(id => id.Name)
                    .ToList();

                // If none of the bitfields are set,
                // show just the value by itself
                if (setBits.Count == 0)
                    return valueString;

                // If any of the bitfields are set,
                // display them as a comma-separated
                // list alongside the value
                identifierName = string.Join(", ", setBits);

                return $"{{ {identifierName} }} ({valueString})";
            }

            // Determine if there are any identifiers whose value exactly
            // matches the string representation of the element's value
            identifierName = identifiers.SingleOrDefault(id => id.Value == valueString)?.Name;

            if ((object)identifierName != null)
                return $"{identifierName} ({element.Get()})";

            // If the tag could not be recognized as
            // one that can be displayed in a more
            // readable form, display the value by itself
            return valueString;
        }

        // Converts the value of the given element to a string.
        private string ValueAsString(VectorElement element)
        {
            Tag tag;
            IEnumerable<string> values;
            string format;
            string join;

            // The physical types Char1 and Char2 indicate the value is a string
            if (element.TypeOfValue == PhysicalType.Char1)
                return Encoding.ASCII.GetString(element.GetValues()).Trim((char)0);

            if (element.TypeOfValue == PhysicalType.Char2)
                return Encoding.Unicode.GetString(element.GetValues()).Trim((char)0);

            // Get the tag definition of the element being displayed
            tag = GSF.PQDIF.Tag.GetTag(element.TagOfElement);

            // Determine the format in which to display the values
            // based on the tag definition and the type of the value
            if (element.TypeOfValue == PhysicalType.Timestamp)
                format = tag.FormatString ?? "{0:yyyy-MM-dd HH:mm:ss.fffffff}";
            else
                format = tag.FormatString ?? "{0}";

            // Convert the values to their string representations
            values = Enumerable.Range(0, element.Size)
                .Select(index => string.Format(format, element.Get(index)));

            // Join the values in the collection
            // to a single, comma-separated string
            join = string.Join(", ", values);

            // Wrap the string in curly braces and return
            return $"{{ {join} }}";
        }

        // Fixes the scroll bars in the details view
        // based on whether the text fits inside the text box.
        private void FixScrollBars()
        {
            Size textSize = TextRenderer.MeasureText(DetailsTextBox.Text, DetailsTextBox.Font);
            bool showVertical = DetailsTextBox.ClientSize.Height < textSize.Height + Convert.ToInt32(DetailsTextBox.Font.Size);
            bool showHorizontal = DetailsTextBox.ClientSize.Width < textSize.Width;

            if (showVertical && showHorizontal)
                DetailsTextBox.ScrollBars = ScrollBars.Both;
            else if (showVertical)
                DetailsTextBox.ScrollBars = ScrollBars.Vertical;
            else if (showHorizontal)
                DetailsTextBox.ScrollBars = ScrollBars.Horizontal;
            else
                DetailsTextBox.ScrollBars = ScrollBars.None;
        }

        // Cancels the next expand or collapse operation in the tree view.
        // This is used to suppress the behavior where double-clicking
        // causes a node to expand or collapse.
        private void CancelExpandCollapseOnce()
        {
            TreeViewCancelEventHandler expandCollapseHandler = null;

            expandCollapseHandler = (expandSender, args) =>
            {
                args.Cancel = true;
                RecordTree.BeforeExpand -= expandCollapseHandler;
                RecordTree.BeforeCollapse -= expandCollapseHandler;
            };

            RecordTree.BeforeExpand += expandCollapseHandler;
            RecordTree.BeforeCollapse += expandCollapseHandler;
        }

        // Handler called when the window loads.
        private void MainWindow_Load(object sender, EventArgs e)
        {
            m_detailsWindows = new List<DetailsWindow>();

            // Set the icon used by the main window
            Icon = new Icon(typeof(MainWindow), "Icons.explorer.ico");

            // Create the list of images to be displayed in the tree view
            RecordTree.ImageList = new ImageList();
            RecordTree.ImageList.Images.Add(Image.FromStream(typeof(MainWindow).Assembly.GetManifestResourceStream("PQDIFExplorer.Icons.default.png")));
            RecordTree.ImageList.Images.Add(Image.FromStream(typeof(MainWindow).Assembly.GetManifestResourceStream("PQDIFExplorer.Icons.container.png")));
            RecordTree.ImageList.Images.Add(Image.FromStream(typeof(MainWindow).Assembly.GetManifestResourceStream("PQDIFExplorer.Icons.datasource.png")));
            RecordTree.ImageList.Images.Add(Image.FromStream(typeof(MainWindow).Assembly.GetManifestResourceStream("PQDIFExplorer.Icons.monitorsettings.png")));
            RecordTree.ImageList.Images.Add(Image.FromStream(typeof(MainWindow).Assembly.GetManifestResourceStream("PQDIFExplorer.Icons.observation.png")));
            RecordTree.ImageList.Images.Add(Image.FromStream(typeof(MainWindow).Assembly.GetManifestResourceStream("PQDIFExplorer.Icons.collection.png")));
            RecordTree.ImageList.Images.Add(Image.FromStream(typeof(MainWindow).Assembly.GetManifestResourceStream("PQDIFExplorer.Icons.vector.png")));
            RecordTree.ImageList.Images.Add(Image.FromStream(typeof(MainWindow).Assembly.GetManifestResourceStream("PQDIFExplorer.Icons.scalar.png")));

            // Set initial size of the form
            if (Settings.Default.WindowSize != null)
                Size = Settings.Default.WindowSize;
        }

        // Handler called when the user selects the option to open a PQDIF file.
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.DefaultExt = ".pqd";
                openFileDialog.Filter = "PQDIF Files|*.pqd|All Files|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    OpenFile(openFileDialog.FileName);
            }
        }

        // Handler called after the user selects a node in the tree view.
        private void RecordTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            object tag;
            Record record;
            Element element;

            string details;

            if ((object)e.Node == null)
                return;

            tag = e.Node.Tag;
            record = tag as Record;
            element = tag as Element;

            // Get details about the record or
            // element selected in the tree view
            if ((object)record != null)
                details = GetDetails(record);
            else if ((object)element != null)
                details = GetDetails(element);
            else
                return;

            // Updates the details text box with
            // information about the selected item
            DetailsTextBox.Clear();
            DetailsTextBox.AppendText(details);
        }

        // Handler called when the user clicks on the tree view.
        private void RecordTree_MouseDown(object sender, MouseEventArgs e)
        {
            TreeNode node;

            object tag;
            Record record;
            Element element;

            string details;
            
            // Only handle mouse down events here if the user
            // double-clicks with the left mouse button
            if (e.Button != MouseButtons.Left || e.Clicks != 2)
                return;

            // Figure out which node the user double-clicked on
            node = RecordTree.GetNodeAt(RecordTree.PointToClient(MousePosition));

            if ((object)node == null || !node.Bounds.Contains(RecordTree.PointToClient(MousePosition)))
                return;

            tag = node.Tag;
            record = tag as Record;
            element = tag as Element;

            // Get details about the record or
            // element the user double-clicked on
            if ((object)record != null)
                details = GetDetails(record);
            else if ((object)element != null)
                details = GetDetails(element);
            else
                return;

            // Creates a new window in which to display the details
            BeginInvoke(new Action(() =>
            {
                DetailsWindow detailsWindow = new DetailsWindow();
                detailsWindow.SetText(details);
                detailsWindow.Show();
                detailsWindow.FormClosing += (obj, args) => m_detailsWindows.Remove(detailsWindow);
                m_detailsWindows.Add(detailsWindow);
            }));

            // Cancel expand or collapse once to suppress the
            // default behavior of expand/collapse on double-click
            if (node.Nodes.Count > 0)
                CancelExpandCollapseOnce();
        }

        // Handler called when the text is changed in the details text box.
        private void DetailsTextBox_TextChanged(object sender, EventArgs e)
        {
            FixScrollBars();
        }

        // Handler called when the details text box is resized.
        private void DetailsTextBox_Resize(object sender, EventArgs e)
        {
            if (IsHandleCreated)
                BeginInvoke(new Action(FixScrollBars));
        }

        // Handler called when the user selects the Exit button in the toolbar menu.
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        // Handler called when the main window is closing so we can clean up
        // any details windows that were spawned while the application was running.
        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            List<DetailsWindow> detailsWindows = new List<DetailsWindow>(m_detailsWindows);

            foreach (DetailsWindow detailsWindow in detailsWindows)
                detailsWindow.Close();

            if (WindowState == FormWindowState.Normal)
                Settings.Default.WindowSize = Size;
            else
                Settings.Default.WindowSize = RestoreBounds.Size;

            Settings.Default.Save();
        }
    }
}
