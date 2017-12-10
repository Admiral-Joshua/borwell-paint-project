//while (loopCounter2 < roomPlan1.Controls.Count)
//            {
//                Control SelectedElement = roomPlan1.Controls[loopCounter2];
//                if (SelectedElement.Name.ToString().Contains("RoomElement"))
//                {
//                    string ElementID = SelectedElement.Name.ToString().Replace("RoomElement", "");
//Control imageObject = SelectedElement.Controls["RoomImage" + ElementID];

//PictureBox ElementImage = imageObject as PictureBox;

//GroupBox ElementGroup = new GroupBox
//{
//    Width = 130,
//    Height = 150
//};
//PictureBox ElementIcon = new PictureBox
//{
//    Width = 90,
//    Height = 90,
//    SizeMode = PictureBoxSizeMode.Zoom,
//    Location = new Point(20, 15)
//};

//ElementIcon.Image = ElementImage.Image;
//                    if (ElementImage.Tag.ToString() == "square")
//                    {
//                        ElementGroup.Text = "Floor";
//                    }
//                    else if (ElementImage.Tag.ToString() == "wall")
//                    {
//                        WallCounter++;
//                        ElementGroup.Text = "Wall" + WallCounter.ToString();
//                    }

//                    Label ElementInfo = new Label
//                    {
//                        Width = 118,
//                        Height = 35,
//                        Location = new Point(6, 108)
//                    };
//ElementInfo.TextAlign = ContentAlignment.MiddleCenter;

//                    Control ElementWidthReadout = SelectedElement.Controls["WidthReadout" + ElementID];
//Control ElementHeightReadout = SelectedElement.Controls["HeightReadout" + ElementID];

//ElementInfo.Text = "Width: " + ElementWidthReadout.Text.ToString() + Environment.NewLine + "Height: " + ElementHeightReadout.Text.ToString();

//                    ElementGroup.Name = SelectedElement.Name.ToString() + "_InfoReadout";
//                    ElementInfo.Name = SelectedElement.Name.ToString() + "_InfoText";

//                    ElementGroup.Controls.Add(ElementIcon);
//                    ElementGroup.Controls.Add(ElementInfo);

//                    RoomOverviewPanel.Controls.Add(ElementGroup);

//                    ElementGroup.Click += (sender2, e2) => ShowElement(SelectedElement.Name.ToString());
//ElementIcon.Click += (sender2, e2) => ShowElement(SelectedElement.Name.ToString());

//                    if (loopCounter2 > roomPlan1.Controls.Count)
//                    {
//                        MessageBox.Show("Something odd Occurred! There were more loops than controls...??");
//                    }
//                }
//                loopCounter2++;
//            }