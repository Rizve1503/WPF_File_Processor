using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Get_file_into_texts
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            foreach (object item in fileTypesListBox.Items)
            {
                fileTypesListBox.SelectedItems.Add(item);
            }
        }


        private void selectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                folderPathTextBox.Text = dialog.SelectedPath;
            }
        }

        private void selectOutputButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog { Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*", DefaultExt = ".txt" };
            if (dialog.ShowDialog() == true)
            {
                outputFileTextBox.Text = dialog.FileName;
            }
        }

        private void processButton_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = folderPathTextBox.Text;
            string outputFile = outputFileTextBox.Text;
            List<string> fileTypes = fileTypesListBox.SelectedItems.Cast<ListBoxItem>() 
                                                        .Select(item => item.Content.ToString().ToLower())
                                                        .ToList();
            string excludePattern = @"\\bin\\|\\obj\\|\\JS\\|Migrations|Imports|Tools|Scripts|node_modules|packages";

            try
            {
                if (string.IsNullOrEmpty(folderPath) || string.IsNullOrEmpty(outputFile) || fileTypes.Count == 0)
                {
                    MessageBox.Show("Please select a folder, output file, and at least one file type.");
                    return;
                }

                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                    .Where(f => fileTypes.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) && !Regex.IsMatch(f, excludePattern))
                    .ToList();
                if (File.Exists(outputFile)) File.Delete(outputFile);
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("==== Project root: " + folderPath + " ====");
                foreach (string file in files)
                {
                    sb.AppendLine(); 
                    sb.AppendLine("--------------Path:" + file.Replace(folderPath, "") + "----------------");
                    try
                    {
                        var txt = Regex.Replace(File.ReadAllText(file), "[\\t ]{2,}", " ");
                        txt = Regex.Replace(txt, "(\\r\\n){2,}", "\n");
                        txt = Regex.Replace(txt, "( \\r\\n){2,}", "\n");
                        txt = Regex.Replace(txt, "\\n \\r", "");
                        sb.Append(txt);
                        sb.AppendLine(); 
                        logTextBox.AppendText($"{file}{Environment.NewLine}");

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing file {file}: {ex.Message}");
                    }
                    Console.WriteLine(file); 
                }
                string output = sb.ToString();
                File.WriteAllText(outputFile, output);
                
                File.WriteAllText(outputFile, sb.ToString());
                MessageBox.Show("File processing complete!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}