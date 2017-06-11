using PlanYourWeek.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace PlanYourWeek.Helpers
{
    public class YourWeekTemplateSelector : DataTemplateSelector
    {
        public DataTemplate PlaceholderTemplate { get; set; }
        public DataTemplate ActivityTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var act = item as PlannedActivity;
            var selectorItem = container as SelectorItem;

            selectorItem.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            selectorItem.MinHeight = 0;

            if (act?.ProposedActivity == null)
            {
                selectorItem.IsHitTestVisible = false;
                return PlaceholderTemplate;
            }
            return ActivityTemplate;
        }
    }
}
