using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using unvell.ReoGrid.DataFormat;

namespace FileRename_Move_Copy
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int current_row = 0;
        int current_path_length = 0;
        int current_mode = 0;
        string current_folder = "";

        public MainWindow()
        {
            InitializeComponent();
            grid.CurrentWorksheet.ColumnHeaders[0].Text = "기존 파일 또는 폴더명";
            grid.CurrentWorksheet.ColumnHeaders[1].Text = "기존 확장자명";
            grid.CurrentWorksheet.ColumnHeaders[2].Text = "변경할 파일 또는 폴더명";
            grid.CurrentWorksheet.ColumnHeaders[3].Text = "변경할 확장자명";
            grid.CurrentWorksheet.ColumnHeaders[4].Text = "처리 결과";
            grid.CurrentWorksheet.SetColumnsWidth(0, 1, 150);
            grid.CurrentWorksheet.SetColumnsWidth(1, 1, 100);
            grid.CurrentWorksheet.SetColumnsWidth(2, 1, 150);
            grid.CurrentWorksheet.SetColumnsWidth(3, 1, 100);
            grid.CurrentWorksheet.SetColumnsWidth(4, 1, 100);
            grid.CurrentWorksheet.SetRangeDataFormat(0, 0, 200, 5, CellDataFormatFlag.Text, null);
        }

        private void btnSetPath_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    editPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void btnFileList_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(editPath.Text))
            {
                current_folder = editPath.Text;
                grid.CurrentWorksheet.ClearRangeContent("A:E", unvell.ReoGrid.CellElementFlag.Data);
                current_row = 0;
                current_path_length = current_folder.Length;
                ProcessDirectory(current_folder, true, (chkFindSub.IsChecked ?? false));
                grid.CurrentWorksheet.AutoFitColumnWidth(0, false);
                grid.CurrentWorksheet.AutoFitColumnWidth(2, false);
                current_mode = 1;
                MessageBox.Show(string.Format("{0}개의 파일목록을 뽑아왔습니다.", current_row), "파일리스트 뽑기", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("폴더 경로를 확인하세요.", "폴더 경로 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ProcessDirectory(string targetDirectory, bool is_file, bool find_sub)
        {
            if (is_file)
            {
                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(targetDirectory);
                foreach (string fileName in fileEntries)
                    ProcessFile(fileName);
            }

            if (find_sub || (find_sub == false && is_file == false))
            {
                try
                {                // Recurse into subdirectories of this directory.
                    string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
                    foreach (string subdirectory in subdirectoryEntries)
                    {
                        if (find_sub)
                            ProcessDirectory(subdirectory, is_file, find_sub);

                        if (is_file == false)
                        {
                            grid.CurrentWorksheet[current_row, 0] = subdirectory.Substring(current_path_length + 1, subdirectory.Length - current_path_length - 1);
                            grid.CurrentWorksheet[current_row, 1] = "";
                            grid.CurrentWorksheet[current_row, 2] = "변경할 폴더명을 입력하세요.";
                            current_row++;
                            if (current_row >= grid.CurrentWorksheet.Rows)
                            {
                                grid.CurrentWorksheet.Rows += 200;
                                grid.CurrentWorksheet.SetRangeDataFormat(0, 0, grid.CurrentWorksheet.Rows, 5, CellDataFormatFlag.Text, null);
                           }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        // Insert logic for processing found files here.
        public void ProcessFile(string path)
        {
            var sheet = grid.CurrentWorksheet;
            // set cells data
            string extension = System.IO.Path.GetExtension(path);
            if (extension != "")
            {
                sheet[current_row, 0] = path.Substring(current_path_length + 1, path.Length - current_path_length - 1 - extension.Length);
                sheet[current_row, 1] = extension.Substring(1, extension.Length - 1);
                sheet[current_row, 3] = extension.Substring(1, extension.Length - 1);
                sheet[current_row, 2] = "변경할 파일명을 입력하세요.";
            }
            else
            {
                sheet[current_row, 0] = path.Substring(current_path_length + 1, path.Length - current_path_length - 1);
                sheet[current_row, 1] = "";
                sheet[current_row, 3] = "";
                sheet[current_row, 2] = "변경할 파일명을 입력하세요.";
            }
            current_row++;
            if (current_row >= grid.CurrentWorksheet.Rows)
            {
                grid.CurrentWorksheet.Rows += 200;
            }
        }

        private void btnFolderList_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(editPath.Text))
            {
                current_folder = editPath.Text;
                grid.CurrentWorksheet.ClearRangeContent("A:E", unvell.ReoGrid.CellElementFlag.Data);
                current_row = 0;
                current_path_length = current_folder.Length;
                ProcessDirectory(current_folder, false, (chkFindSub.IsChecked ?? false));
                grid.CurrentWorksheet.AutoFitColumnWidth(0, false);
                grid.CurrentWorksheet.AutoFitColumnWidth(2, false);
                current_mode = 2;
                MessageBox.Show(string.Format("{0}개의 폴더목록을 뽑아왔습니다.", current_row), "폴더리스트 뽑기", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("폴더 경로를 확인하세요.", "폴더 경로 오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            var sheet = grid.CurrentWorksheet;
            if (current_mode == 1)   //   file mode
            {
                int row_count = sheet.Rows;
                for (int i = 0; i < row_count; i++)
                {
                    if (sheet[i, 0] != null)
                    {
                        string current_file_name = current_folder + "/" + sheet[i, 0].ToString();
                        if (sheet[i, 1] != null && sheet[i, 1].ToString() != "")
                        {
                            current_file_name += ("." + sheet[i, 1].ToString());
                        }

                        if (File.Exists(current_file_name))
                        {
                            if (sheet[i, 2] != null)
                            {
                                string new_file_name = current_folder + "/" + sheet[i, 2].ToString();
                                if (sheet[i, 3] != null && sheet[i, 3].ToString() != "")
                                {
                                    new_file_name += ("." + sheet[i, 3].ToString());
                                }
                                try
                                {
                                    File.Move(current_file_name, new_file_name);
                                    sheet[i, 4] = "완료";
                                }
                                catch (System.IO.IOException ee)
                                {
                                    sheet[i, 4] = ee.ToString();
                                }
                            }
                        }
                    }
                }
            }
            else if (current_mode == 2)  //  folder mode
            {
                int row_count = sheet.Rows;
                for (int i = 0; i < row_count; i++)
                {
                    if (sheet[i, 0] != null)
                    {
                        string current_folder_name = current_folder + "/" + sheet[i, 0].ToString();

                        if (Directory.Exists(current_folder_name))
                        {
                            if (sheet[i, 2] != null)
                            {
                                string new_folder_name = current_folder + "/" + sheet[i, 2].ToString();
                                try
                                {
                                    Directory.Move(current_folder_name, new_folder_name);
                                    sheet[i, 4] = "완료";
                                }
                                catch (System.IO.IOException ee)
                                {
                                    sheet[i, 4] = ee.ToString();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
