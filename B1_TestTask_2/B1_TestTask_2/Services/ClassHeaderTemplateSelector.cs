using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using B1_TestTask_2.ViewModels;

namespace B1_TestTask_2.Services
{
    // Класс для выбора шаблона отображения в зависимости от типа данных.
    public class ClassHeaderTemplateSelector : DataTemplateSelector
    {
        // Шаблон для заголовка класса.
        public DataTemplate ClassHeaderTemplate { get; set; }
        // Обычный шаблон для элементов, не являющихся заголовками класса.
        public DataTemplate NormalTemplate { get; set; }

        // Метод для выбора шаблона на основе переданного объекта.
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // Приведение объекта к типу AccountDisplayModel.
            var record = item as AccountDisplayModel;
            // Если объект не null и помечен как заголовок класса, используем ClassHeaderTemplate.
            if (record != null && record.IsClassHeader)
            {
                return ClassHeaderTemplate;
            }
            // В противном случае используем NormalTemplate.
            else
            {
                return NormalTemplate;
            }
        }
    }

}
