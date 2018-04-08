﻿using FFImageLoading.Forms;
using FFImageLoading.Transformations;
using FFImageLoading.Work;
using HomeBudget.Code;
using HomeBudget.Code.Logic;
using HomeBudget.Utils;
using Syncfusion.SfDataGrid.XForms;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Xamarin.Forms;

namespace HomeBudget.Pages.Utils
{
    public class BudgetSummaryDataViewModel : INotifyPropertyChanged
    {
        public string CategoryName => CategoryReal.Name;

        public BaseBudgetCategory CategoryPlanned
        {
            get;
            set;
        }

        private BaseBudgetCategory categoryReal;
        public BaseBudgetCategory CategoryReal
        {
            get { return categoryReal; }
            set
            {
                categoryReal = value;
                categoryReal.PropertyChanged += OnCategoryChanged;
            }
        }

        private void OnCategoryChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CategoryReal.TotalValues"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SpendPercentage"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SpendPercentageInt"));
        }

        private string icon;
        public string IconFile
        {
            get => "ms-appx:///" + icon;
            set => icon = value;
        }
        public double SpendPercentage
        {
            get
            {
                //Random rand = new Random();
                return CategoryPlanned.TotalValues > 0 ? (CategoryReal.TotalValues / CategoryPlanned.TotalValues) : 0; //rand.NextDouble();
            }
        }

        public int SpendPercentageInt
        {
            get
            {
                return (int)(SpendPercentage * 100);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public BudgetSummaryDataViewModel()
        {

        }
    }

    public class BudgetSummaryListView
    {
        private SfDataGrid dataGrid;
        private string circleColor = "#6BBE8B";
        private Color progressBgColor;
        private Color progressColor;

        public void Setup(SfDataGrid _dataGrid)
        {
            progressBgColor = Color.FromHex("E3CC00");
            progressColor = Color.FromHex("12B0AE");
            dataGrid = _dataGrid;
         

            dataGrid.HeaderRowHeight = 0;
            dataGrid.GridStyle = new BudgetDataGridStyle();
            dataGrid.ItemsSource = GetBudgetSummaryData();

            dataGrid.Columns.Add(CreateIconColumn());

            /*listView.Columns.Add(new GridTextColumn()
            {
                MappingName = "CategoryReal.Name",
                HeaderText = "Kategoria",
                ColumnSizer = ColumnSizer.Auto
            });*/

            dataGrid.Columns.Add(CreateTotalValueColumn());
            dataGrid.Columns.Add(CreateProgressColumn());
            dataGrid.Columns.Add(CreateSpendPercentageColumn());
        }

        private ObservableCollection<BudgetSummaryDataViewModel> GetBudgetSummaryData()
        {
            var budgetSummaryCollection = new ObservableCollection<BudgetSummaryDataViewModel>();
            var budgetReal = MainBudget.Instance.GetCurrentMonthData().BudgetReal;
            var categoriesDesc = MainBudget.Instance.BudgetDescription.Categories;
            var budgetPlanned = MainBudget.Instance.GetCurrentMonthData().BudgetPlanned;
            for (int i = 0; i < budgetReal.Categories.Count; i++)
            {
                if (!budgetReal.Categories[i].IsIncome)
                {
                    var budgetSummaryData = new BudgetSummaryDataViewModel
                    {
                        CategoryReal = budgetReal.Categories[i],
                        CategoryPlanned = budgetPlanned.Categories[i],
                        IconFile = "Assets/Categories/" + categoriesDesc[i].IconFileName
                    };

                    budgetSummaryCollection.Add(budgetSummaryData);
                }
            }

            return budgetSummaryCollection;
        }

        private GridColumn CreateIconColumn()
        {
            var iconColumn = new GridTemplateColumn
            {
                MappingName = "CategoryReal",
                ColumnSizer = ColumnSizer.Auto,
                CellTemplate = CreateCategoryIconDataTemplate()
            };

            return iconColumn;
        }

        private DataTemplate CreateCategoryIconDataTemplate()
        {
            var dataTemplate = new DataTemplate(() =>
            {
                var grid = CreateGrid(1, 1);

                var background = new CachedImage()
                {
                    Source = "Assets/circle.png",
                    Transformations = new List<ITransformation> {new TintTransformation {
                    HexColor = circleColor,
                    EnableSolidColor = true
                    }
                    }
                };
                var icon = new Image()
                {
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                };
                icon.SetBinding(Image.SourceProperty, "IconFile");

                AddElementToGrid(grid, background, 0, 0);
                AddElementToGrid(grid, icon, 0, 0);

                return grid;
            });

            return dataTemplate;
        }

        private GridColumn CreateTotalValueColumn()
        {
            return new GridTextColumn()
            {
                MappingName = "CategoryReal.TotalValues",
                HeaderText = "Razem",
                Format = "C",
                CultureInfo = new CultureInfo("pl-PL"),
                ColumnSizer = ColumnSizer.Auto,
                RecordFont = "Cambria"
            };
        }

        private GridColumn CreateSpendPercentageColumn()
        {
            return new GridTextColumn()
            {
                MappingName = "SpendPercentage",
                HeaderText = "Procent",
                Format = "P0",
                RecordFont = "Cambria",
                ColumnSizer = ColumnSizer.Auto
            };
        }

        private GridColumn CreateProgressColumn()
        {
            GridTemplateColumn progressColumn = new GridTemplateColumn()
            {
                MappingName = "SpendPercentage",
                Width = 110,
                ColumnSizer = ColumnSizer.Auto
            };
            progressColumn.CellTemplate = CreateProgressColumnTemplate();

            return progressColumn;
        }

        private DataTemplate CreateProgressColumnTemplate()
        {
            var dataTemplate = new DataTemplate(() =>
            {
                Grid grid = CreateGrid(1, 1);

                var background = new BoxView
                {
                    Color = progressBgColor,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start,
                    WidthRequest = 100,
                    HeightRequest = 20
                };

                var progress = new BoxView
                {
                    Color = progressColor,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Start,
                    HeightRequest = 20
                };

                progress.SetBinding(BoxView.WidthRequestProperty, "SpendPercentageInt");

                AddElementToGrid(grid, background, 0, 0);
                AddElementToGrid(grid, progress, 0, 0);

                return grid;
            });

            return dataTemplate;
        }

        private Grid CreateGrid(int rowsNum, int colsNum)
        {
            Grid grid = new Grid();
            for (int i = 0; i < colsNum; i++)
            {
                ColumnDefinition col = new ColumnDefinition()
                {
                    Width = new GridLength(1, GridUnitType.Star)
                };
                grid.ColumnDefinitions.Add(col);
            }

            for (int i = 0; i < rowsNum; i++)
            {
                RowDefinition row = new RowDefinition()
                {
                    Height = new GridLength(1, GridUnitType.Star)
                };

                grid.RowDefinitions.Add(row);
            }

            return grid;
        }

        private void AddElementToGrid(Grid grid, View element, int row, int col)
        {
            Grid.SetColumn(element, col);
            Grid.SetRow(element, row);

            grid.Children.Add(element);
        }
    }
}
