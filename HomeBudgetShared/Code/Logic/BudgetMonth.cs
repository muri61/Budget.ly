﻿using HomeBudget.Code.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProtoBuf;
using System.Linq;

namespace HomeBudget.Code
{
    [ProtoContract]
    public class BudgetMonth
	{
        [ProtoMember(1)]
        public BudgetPlanned BudgetPlanned { get; set; }
        [ProtoMember(2)]
        public BudgetReal BudgetReal { get; set; }
        [ProtoMember(3)]
        public int Month { get; set; }
        [ProtoMember(4)]
        public int Year { get; set; }

        public event Action onBudgetPlannedChanged;

        public class BudgetChartData
        {
            public string CategoryName { get; set; }
            public double ExpensesSum { get; set; }

            public BudgetChartData(string name, double sum)
            {
                CategoryName = name;
                ExpensesSum = sum;
            }
        }

		public static BudgetMonth Create(List<BudgetCategoryTemplate> categories, DateTime date)
		{
			var month = new BudgetMonth();
			month.SetupCategories(categories);
            month.SetupDate(date);

			return month;
		}

        public void AddExpense(double value, DateTime date, int categoryID, int subcatID)
        {
            BudgetReal.AddExpense(value, date, categoryID, subcatID);
        }

        public void AddIncome(double value, DateTime date, int incomeCategoryID)
        {
            BudgetReal.AddIncome(value, date, incomeCategoryID);
        }

		private BudgetMonth()
		{
            BudgetReal = new BudgetReal();
            BudgetPlanned = new BudgetPlanned();
        }

        private void OnBudgetPlannedChanged()
        {
            onBudgetPlannedChanged?.Invoke();
        }

        private void SetupDate(DateTime date)
		{
			Month = date.Month;
			Year = date.Year;
		}

		private void SetupCategories(List<BudgetCategoryTemplate> categoriesDesc)
		{
            BudgetPlanned.Setup(categoriesDesc);
            BudgetReal.Setup(categoriesDesc);
		}

        public double GetTotalIncomeReal()
        {
            return BudgetReal.GetTotalIncome();
        }

        public double GetTotalExpenseReal()
        {
            return BudgetReal.GetTotalExpenses();
        }

        public double GetTotalExpensesPlanned()
        {
            return BudgetPlanned.GetTotalExpenses();
        }

        public double GetTotalIncomePlanned()
        {
            return BudgetPlanned.GetTotalIncome();
        }

        public void UpdatePlannedBudget(BudgetPlanned newBudgetPlanned)
        {
            BudgetPlanned = new BudgetPlanned(newBudgetPlanned);
        }

        public void Setup()
        {
            BudgetPlanned.Prepare();
            BudgetReal.Prepare();
        }

        public List<BudgetCategoryForEdit> GetBudgetTemplateEdit()
        {
            var result = BudgetReal.Categories.Select(category =>
            {
                var item = new BudgetCategoryForEdit { Name = category.Name, Id = category.Id };
                var subcats = category.subcats.Select(subcat => new BudgetSubcatEdit { Name = subcat.Name, Id = subcat.Id });
                foreach(var subcat in subcats)
                {
                    item.Add(subcat);
                }
                return item;
            }).ToList();

            return result;
        }

        public void UpdateBudgetCategories(List<BudgetCategoryForEdit> categoriesUpdated)
        {
            foreach(var category in categoriesUpdated)
            {
                var categoryReal = BudgetReal.GetBudgetCategory(category.Id);
                var categoryPlan = BudgetPlanned.GetBudgetCategory(category.Id);
                foreach(var subcat in category)
                {
                    var subcatPlan = categoryPlan.GetSubcat(subcat.Id);
                    var subcatReal = categoryReal.GetSubcat(subcat.Id);
                    if (subcatPlan != null && subcatReal != null)
                    {
                        subcatPlan.Name = subcat.Name;
                        subcatReal.Name = subcat.Name;
                    }
                    else
                    {
                        //add new
                        categoryReal.subcats.Add(new RealSubcat { Id = subcat.Id, Name = subcat.Name, Value = 0 });
                        categoryPlan.subcats.Add(new PlannedSubcat { Id = subcat.Id, Name = subcat.Name, Value = 0 });
                    }
                    //TODO
                    //remove subcat
                }
            }
        }
    }

    public class BudgetSubcatEdit
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public class BudgetCategoryForEdit : ObservableCollection<BudgetSubcatEdit>
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }
}
