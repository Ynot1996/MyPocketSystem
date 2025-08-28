using MyPocket.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPocket.Shared.ViewModels.Categories
{
    public class UserCategoryViewModel
    {
        public List<Category> DefaultIncomeCategories { get; set; } = new();
        public List<Category> DefaultExpenseCategories { get; set; } = new();
        public List<Category> UserIncomeCategories { get; set; } = new();
        public List<Category> UserExpenseCategories { get; set; } = new();
        public string NewCategoryName { get; set; }
        public string NewCategoryType { get; set; } 
    }
}
